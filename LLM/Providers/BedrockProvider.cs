using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using AIOperator.Models;
using AIOperator.Utils;

namespace AIOperator.LLM.Providers
{
    /// <summary>
    /// AWS Bedrock Claude API 提供者 - 支持工具调用
    /// </summary>
    public class BedrockProvider : ILLMProvider
    {
        private LLMConfig config;
        private MonoBehaviour coroutineRunner;

        public BedrockProvider(LLMConfig config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        public string GetProviderName()
        {
            return "AWS Bedrock";
        }

        public void SendMessage(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError)
        {
            coroutineRunner.StartCoroutine(SendMessageCoroutine(messages, onResponse, onError));
        }

        public void TestConnection(Action<bool, string> callback)
        {
            coroutineRunner.StartCoroutine(TestConnectionCoroutine(callback));
        }

        // ========== 工具调用支持 ==========

        public void SendMessageWithTools(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            Action<ToolCallResponse> onResponse,
            Action<string> onError)
        {
            coroutineRunner.StartCoroutine(SendMessageWithToolsCoroutine(messages, tools, onResponse, onError));
        }

        public IEnumerator SendMessageWithToolsCoroutine(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            Action<ToolCallResponse> onResponse,
            Action<string> onError)
        {
            yield return SendMessageWithToolsCoroutine(messages, tools, null, onResponse, onError);
        }

        public IEnumerator SendMessageWithToolsCoroutine(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            string systemPrompt,
            Action<ToolCallResponse> onResponse,
            Action<string> onError)
        {
            if (!ValidateConfig(out string configError))
            {
                onError?.Invoke(configError);
                yield break;
            }

            string url = BuildBedrockUrl();
            string jsonData = BuildBedrockRequestWithTools(messages, tools, systemPrompt);

            Debug.Log($"[Bedrock] 发送带工具的请求到: {url}");
            Debug.Log($"[Bedrock] 请求内容: {jsonData.Substring(0, Math.Min(500, jsonData.Length))}...");

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                // 设置认证
                if (!SetupAuthentication(www, url, jsonData, out string authError))
                {
                    onError?.Invoke(authError);
                    yield break;
                }

                www.timeout = 120;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[Bedrock] 收到响应: {responseText.Substring(0, Math.Min(200, responseText.Length))}...");

                    try
                    {
                        var response = ParseBedrockResponseWithTools(responseText);
                        response.RawResponse = responseText;
                        onResponse?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Bedrock] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"[Bedrock] 请求失败 - Result: {www.result}, Code: {www.responseCode}, Error: {www.error}");
                    string responseBody = www.downloadHandler?.text ?? "无响应体";
                    Debug.LogError($"[Bedrock] 响应体: {responseBody}");
                    string errorMsg = ParseBedrockError(www);
                    onError?.Invoke(errorMsg);
                }
            }
        }

        private string BuildBedrockRequestWithTools(List<LLMMessage> messages, ToolDefinition[] tools, string systemPrompt = null)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"anthropic_version\":\"bedrock-2023-05-31\",");
            sb.Append("\"max_tokens\":4096,");

            // 添加系统提示词（Bedrock/Claude 格式）
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                sb.Append($"\"system\":\"{EscapeJson(systemPrompt)}\",");
            }

            // 添加工具定义（Bedrock 格式）
            if (tools != null && tools.Length > 0)
            {
                sb.Append("\"tools\":");
                sb.Append(ToolDefinitionSerializer.ToBedrockFormat(tools));
                sb.Append(",");
            }

            // 添加消息
            sb.Append("\"messages\":[");
            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(SerializeMessageBedrock(messages[i]));
            }
            sb.Append("]}");

            return sb.ToString();
        }

        private string SerializeMessageBedrock(LLMMessage message)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"role\":\"{message.role}\",");

            switch (message.contentType)
            {
                case MessageContentType.ToolUse:
                    sb.Append("\"content\":[");
                    bool first = true;

                    if (!string.IsNullOrEmpty(message.content))
                    {
                        sb.Append($"{{\"type\":\"text\",\"text\":\"{EscapeJson(message.content)}\"}}");
                        first = false;
                    }

                    if (message.toolCalls != null)
                    {
                        foreach (var call in message.toolCalls)
                        {
                            if (!first) sb.Append(",");
                            first = false;
                            sb.Append("{");
                            sb.Append("\"type\":\"tool_use\",");
                            sb.Append($"\"id\":\"{call.Id}\",");
                            sb.Append($"\"name\":\"{call.Name}\",");
                            sb.Append("\"input\":");
                            sb.Append(MiniJSON.Serialize(call.Arguments));
                            sb.Append("}");
                        }
                    }
                    sb.Append("]");
                    break;

                case MessageContentType.ToolResult:
                    sb.Append("\"content\":[");
                    if (message.toolResults != null)
                    {
                        for (int i = 0; i < message.toolResults.Count; i++)
                        {
                            if (i > 0) sb.Append(",");
                            var result = message.toolResults[i];
                            sb.Append("{");
                            sb.Append("\"type\":\"tool_result\",");
                            sb.Append($"\"tool_use_id\":\"{result.ToolUseId}\",");
                            sb.Append($"\"content\":\"{EscapeJson(result.Result.ToContent())}\"");
                            if (result.IsError)
                            {
                                sb.Append(",\"is_error\":true");
                            }
                            sb.Append("}");
                        }
                    }
                    sb.Append("]");
                    break;

                default:
                    // Claude/Bedrock Messages API 要求 content 为数组格式
                    sb.Append("\"content\":[{\"type\":\"text\",\"text\":\"");
                    sb.Append(EscapeJson(message.content ?? ""));
                    sb.Append("\"}]");
                    break;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private ToolCallResponse ParseBedrockResponseWithTools(string responseText)
        {
            var json = MiniJSON.Deserialize(responseText) as Dictionary<string, object>;
            if (json == null)
            {
                throw new Exception("无法解析 JSON 响应");
            }

            var response = new ToolCallResponse();

            if (json.TryGetValue("stop_reason", out var stopReason))
            {
                response.StopReason = stopReason?.ToString();
            }

            if (json.TryGetValue("content", out var contentObj) && contentObj is List<object> contentArray)
            {
                var toolCalls = new List<ToolCall>();
                var textParts = new List<string>();

                foreach (var item in contentArray)
                {
                    if (item is Dictionary<string, object> contentItem)
                    {
                        var type = contentItem.ContainsKey("type") ? contentItem["type"]?.ToString() : null;

                        if (type == "text")
                        {
                            if (contentItem.TryGetValue("text", out var text))
                            {
                                textParts.Add(text?.ToString() ?? "");
                            }
                        }
                        else if (type == "tool_use")
                        {
                            var toolCall = new ToolCall();
                            toolCall.Id = contentItem.ContainsKey("id") ? contentItem["id"]?.ToString() : "";
                            toolCall.Name = contentItem.ContainsKey("name") ? contentItem["name"]?.ToString() : "";

                            if (contentItem.TryGetValue("input", out var inputObj) && inputObj is Dictionary<string, object> input)
                            {
                                toolCall.Arguments = input;
                            }
                            else
                            {
                                toolCall.Arguments = new Dictionary<string, object>();
                            }

                            toolCalls.Add(toolCall);
                        }
                    }
                }

                if (textParts.Count > 0)
                {
                    response.Text = string.Join("\n", textParts);
                }

                if (toolCalls.Count > 0)
                {
                    response.ToolCalls = toolCalls;
                }
            }

            return response;
        }

        private bool SetupAuthentication(UnityWebRequest www, string url, string jsonData, out string error)
        {
            error = null;

            if (!string.IsNullOrEmpty(config.awsBearerToken))
            {
                www.SetRequestHeader("Authorization", $"Bearer {config.awsBearerToken}");
                www.SetRequestHeader("Content-Type", "application/json");
                return true;
            }

            try
            {
                DateTime timestamp = DateTime.UtcNow;
                var signedHeaders = AWSSignatureV4.SignRequest(
                    "POST",
                    url,
                    config.awsRegion,
                    config.awsAccessKeyId,
                    config.awsSecretAccessKey,
                    jsonData,
                    timestamp
                );

                foreach (var header in signedHeaders)
                {
                    if (header.Key.ToLower() != "host")
                    {
                        www.SetRequestHeader(header.Key, header.Value);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                error = $"AWS 签名失败: {e.Message}";
                return false;
            }
        }

        // ========== 原有方法 ==========

        private IEnumerator SendMessageCoroutine(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError)
        {
            // 检查必要配置
            if (!ValidateConfig(out string error))
            {
                onError?.Invoke(error);
                yield break;
            }

            // 构建 Bedrock API URL
            string url = BuildBedrockUrl();

            // 构建请求体（Bedrock 格式）
            string jsonData = BuildBedrockRequest(messages);

            Debug.Log($"[Bedrock] 发送请求到: {url}");
            Debug.Log($"[Bedrock] 使用模型: {config.model}");
            Debug.Log($"[Bedrock] Region: {config.awsRegion}");

            // 创建 UnityWebRequest
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                // 设置认证方式：优先使用 Bearer Token
                if (!string.IsNullOrEmpty(config.awsBearerToken))
                {
                    // 使用 Bearer Token 认证
                    Debug.Log($"[Bedrock] 使用 Bearer Token 认证");
                    www.SetRequestHeader("Authorization", $"Bearer {config.awsBearerToken}");
                    www.SetRequestHeader("Content-Type", "application/json");
                }
                else
                {
                    // 使用 AWS Signature V4 认证
                    Debug.Log($"[Bedrock] 使用 AWS SigV4 认证");
                    DateTime timestamp = DateTime.UtcNow;
                    Dictionary<string, string> signedHeaders;

                    try
                    {
                        signedHeaders = AWSSignatureV4.SignRequest(
                            "POST",
                            url,
                            config.awsRegion,
                            config.awsAccessKeyId,
                            config.awsSecretAccessKey,
                            jsonData,
                            timestamp
                        );

                        // 设置签名后的请求头，跳过 host（Unity 自动处理）
                        foreach (var header in signedHeaders)
                        {
                            if (header.Key.ToLower() == "host")
                            {
                                continue;
                            }
                            www.SetRequestHeader(header.Key, header.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Bedrock] 签名失败: {e.Message}");
                        onError?.Invoke($"AWS 签名生成失败: {e.Message}");
                        yield break;
                    }
                }

                www.timeout = 60;

                // 发送请求
                yield return www.SendWebRequest();

                // 处理响应
                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[Bedrock] 收到响应");

                    try
                    {
                        BedrockResponse response = JsonUtility.FromJson<BedrockResponse>(responseText);
                        if (response != null && response.content != null && response.content.Length > 0)
                        {
                            onResponse?.Invoke(response.content[0].text);
                        }
                        else
                        {
                            onError?.Invoke("响应格式错误");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Bedrock] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}\n\n响应内容: {responseText}");
                    }
                }
                else
                {
                    string errorMsg = ParseBedrockError(www);
                    Debug.LogError($"[Bedrock] {errorMsg}");
                    onError?.Invoke(errorMsg);
                }
            }
        }

        private IEnumerator TestConnectionCoroutine(Action<bool, string> callback)
        {
            if (!ValidateConfig(out string error))
            {
                callback?.Invoke(false, error);
                yield break;
            }

            // 发送一个简单的测试消息
            List<LLMMessage> testMessages = new List<LLMMessage>
            {
                new LLMMessage("user", "Hi")
            };

            string url = BuildBedrockUrl();
            string jsonData = BuildBedrockRequest(testMessages);

            Debug.Log($"[Bedrock Test] URL: {url}");
            Debug.Log($"[Bedrock Test] Request: {jsonData}");
            Debug.Log($"[Bedrock Test] Region: {config.awsRegion}");

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                // 设置认证方式：优先使用 Bearer Token
                if (!string.IsNullOrEmpty(config.awsBearerToken))
                {
                    // 使用 Bearer Token 认证
                    Debug.Log($"[Bedrock Test] 使用 Bearer Token 认证");
                    Debug.Log($"[Bedrock Test] Token 前缀: {config.awsBearerToken.Substring(0, Math.Min(20, config.awsBearerToken.Length))}...");
                    www.SetRequestHeader("Authorization", $"Bearer {config.awsBearerToken}");
                    www.SetRequestHeader("Content-Type", "application/json");
                }
                else
                {
                    // 使用 AWS Signature V4 认证
                    Debug.Log($"[Bedrock Test] 使用 AWS SigV4 认证");
                    Debug.Log($"[Bedrock Test] AccessKey: {config.awsAccessKeyId.Substring(0, Math.Min(8, config.awsAccessKeyId.Length))}...");

                    DateTime timestamp = DateTime.UtcNow;
                    Dictionary<string, string> signedHeaders;

                    try
                    {
                        signedHeaders = AWSSignatureV4.SignRequest(
                            "POST",
                            url,
                            config.awsRegion,
                            config.awsAccessKeyId,
                            config.awsSecretAccessKey,
                            jsonData,
                            timestamp
                        );
                        Debug.Log($"[Bedrock Test] 签名成功");

                        // 设置请求头，跳过 host
                        foreach (var header in signedHeaders)
                        {
                            if (header.Key.ToLower() == "host")
                            {
                                continue;
                            }
                            try
                            {
                                www.SetRequestHeader(header.Key, header.Value);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"[Bedrock Test] 无法设置 header {header.Key}: {e.Message}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Bedrock Test] 签名失败: {e.Message}\n{e.StackTrace}");
                        callback?.Invoke(false, $"AWS 签名失败: {e.Message}");
                        yield break;
                    }
                }

                www.timeout = 10;

                yield return www.SendWebRequest();

                Debug.Log($"[Bedrock Test] 响应状态: {www.result}, Code: {www.responseCode}");

                if (www.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, "连接成功！");
                }
                else
                {
                    string errorResponse = www.downloadHandler != null ? www.downloadHandler.text : "无响应内容";
                    Debug.LogError($"[Bedrock Test] 错误响应: {errorResponse}");
                    callback?.Invoke(false, ParseBedrockError(www));
                }
            }
        }

        /// <summary>
        /// 构建 Bedrock API URL
        /// </summary>
        private string BuildBedrockUrl()
        {
            // Bedrock Runtime API endpoint
            return $"https://bedrock-runtime.{config.awsRegion}.amazonaws.com/model/{config.model}/invoke";
        }

        /// <summary>
        /// 构建 Bedrock 请求 JSON
        /// </summary>
        private string BuildBedrockRequest(List<LLMMessage> messages)
        {
            // Bedrock 使用和 Anthropic API 相同的格式
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"anthropic_version\":\"bedrock-2023-05-31\",");
            sb.Append("\"max_tokens\":4096,");
            sb.Append("\"messages\":[");

            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.Append($"\"role\":\"{messages[i].role}\",");
                // Claude/Bedrock Messages API 要求 content 为数组格式
                sb.Append("\"content\":[{\"type\":\"text\",\"text\":\"");
                sb.Append(EscapeJson(messages[i].content ?? ""));
                sb.Append("\"}]");
                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        /// <summary>
        /// 转义 JSON 字符串
        /// </summary>
        private string EscapeJson(string text)
        {
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        private bool ValidateConfig(out string error)
        {
            // 检查是否有 Bearer Token 或 AWS 凭证
            bool hasBearerToken = !string.IsNullOrEmpty(config.awsBearerToken);
            bool hasAwsCredentials = !string.IsNullOrEmpty(config.awsAccessKeyId) &&
                                     !string.IsNullOrEmpty(config.awsSecretAccessKey);

            if (!hasBearerToken && !hasAwsCredentials)
            {
                error = "请先在设置中配置 AWS Bearer Token 或 AWS Access Key";
                return false;
            }

            if (string.IsNullOrEmpty(config.awsRegion))
            {
                error = "请先在设置中配置 AWS Region";
                return false;
            }

            if (string.IsNullOrEmpty(config.model))
            {
                error = "请先在设置中选择模型";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// 解析 Bedrock 错误信息
        /// </summary>
        private string ParseBedrockError(UnityWebRequest www)
        {
            string baseError = $"请求失败: {www.error}";

            try
            {
                string responseText = www.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    return $"{baseError}\n\n错误详情: {responseText}";
                }
            }
            catch { }

            return $"{baseError}\n\n请检查：\n1. AWS Access Key 和 Secret Key 是否正确\n2. Region 是否正确\n3. 模型 ID 是否正确\n4. AWS 账号是否有 Bedrock 访问权限\n5. 网络连接是否正常";
        }
    }

    // Bedrock API 响应数据结构（与 Claude API 相同）
    [Serializable]
    public class BedrockResponse
    {
        public string id;
        public string type;
        public string role;
        public BedrockContent[] content;
        public string model;

        [Serializable]
        public class BedrockContent
        {
            public string type;
            public string text;
        }
    }
}