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
    /// Claude API 提供者 - 支持工具调用
    /// </summary>
    public class ClaudeProvider : ILLMProvider
    {
        private LLMConfig config;
        private MonoBehaviour coroutineRunner;

        // Claude API 常量
        private const string CLAUDE_API_URL = "https://api.anthropic.com/v1/messages";
        private const string CLAUDE_VERSION = "2023-06-01";

        public ClaudeProvider(LLMConfig config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        public string GetProviderName()
        {
            return "Claude";
        }

        public void SendMessage(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError)
        {
            coroutineRunner.StartCoroutine(SendMessageCoroutine(messages, onResponse, onError));
        }

        public void TestConnection(Action<bool, string> callback)
        {
            coroutineRunner.StartCoroutine(TestConnectionCoroutine(callback));
        }

        // ========== 新增：工具调用支持 ==========

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
            // 无系统提示词版本
            yield return SendMessageWithToolsCoroutine(messages, tools, null, onResponse, onError);
        }

        public IEnumerator SendMessageWithToolsCoroutine(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            string systemPrompt,
            Action<ToolCallResponse> onResponse,
            Action<string> onError)
        {
            // 检查 API Key
            if (string.IsNullOrEmpty(config.apiKey))
            {
                onError?.Invoke("请先在设置中配置 Claude API Key");
                yield break;
            }

            // 构建请求体（带工具和系统提示词）
            string jsonData = BuildClaudeRequestWithTools(messages, tools, systemPrompt);

            Debug.Log($"[Claude] 发送带工具的请求");

            using (UnityWebRequest www = new UnityWebRequest(CLAUDE_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("x-api-key", config.apiKey);
                www.SetRequestHeader("anthropic-version", CLAUDE_VERSION);

                www.timeout = 120; // 工具调用可能需要更长时间

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[Claude] 收到响应: {responseText.Substring(0, Math.Min(500, responseText.Length))}...");

                    try
                    {
                        var response = ParseClaudeResponseWithTools(responseText);
                        response.RawResponse = responseText;
                        onResponse?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Claude] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = ParseClaudeError(www);
                    Debug.LogError($"[Claude] {errorMsg}");
                    onError?.Invoke(errorMsg);
                }
            }
        }

        /// <summary>
        /// 构建带工具的 Claude API 请求
        /// </summary>
        private string BuildClaudeRequestWithTools(List<LLMMessage> messages, ToolDefinition[] tools, string systemPrompt = null)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{config.model}\",");
            sb.Append("\"max_tokens\":4096,");

            // 添加系统提示词（Claude API 支持独立的 system 参数）
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                sb.Append($"\"system\":\"{EscapeJson(systemPrompt)}\",");
            }

            // 添加工具定义
            if (tools != null && tools.Length > 0)
            {
                sb.Append("\"tools\":");
                sb.Append(ToolDefinitionSerializer.ToClaudeFormat(tools));
                sb.Append(",");
            }

            // 添加消息
            sb.Append("\"messages\":[");
            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(SerializeMessage(messages[i]));
            }
            sb.Append("]}");

            return sb.ToString();
        }

        /// <summary>
        /// 序列化单条消息（支持工具调用和工具结果）
        /// </summary>
        private string SerializeMessage(LLMMessage message)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"role\":\"{message.role}\",");

            switch (message.contentType)
            {
                case MessageContentType.ToolUse:
                    // Assistant 消息包含工具调用
                    sb.Append("\"content\":[");
                    bool first = true;

                    // 添加文本内容（如果有）
                    if (!string.IsNullOrEmpty(message.content))
                    {
                        sb.Append($"{{\"type\":\"text\",\"text\":\"{EscapeJson(message.content)}\"}}");
                        first = false;
                    }

                    // 添加工具调用
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
                    // User 消息包含工具结果
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
                    // 普通文本消息
                    sb.Append($"\"content\":\"{EscapeJson(message.content ?? "")}\"");
                    break;
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 解析带工具调用的响应
        /// </summary>
        private ToolCallResponse ParseClaudeResponseWithTools(string responseText)
        {
            var json = MiniJSON.Deserialize(responseText) as Dictionary<string, object>;
            if (json == null)
            {
                throw new Exception("无法解析 JSON 响应");
            }

            var response = new ToolCallResponse();

            // 获取 stop_reason
            if (json.TryGetValue("stop_reason", out var stopReason))
            {
                response.StopReason = stopReason?.ToString();
            }

            // 解析 content 数组
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

        // ========== 原有方法 ==========

        private IEnumerator SendMessageCoroutine(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError)
        {
            if (string.IsNullOrEmpty(config.apiKey))
            {
                onError?.Invoke("请先在设置中配置 Claude API Key");
                yield break;
            }

            string jsonData = BuildClaudeRequest(messages);

            Debug.Log($"[Claude] 发送请求到: {CLAUDE_API_URL}");

            using (UnityWebRequest www = new UnityWebRequest(CLAUDE_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("x-api-key", config.apiKey);
                www.SetRequestHeader("anthropic-version", CLAUDE_VERSION);

                www.timeout = 60;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[Claude] 收到响应");

                    try
                    {
                        ClaudeResponse response = JsonUtility.FromJson<ClaudeResponse>(responseText);
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
                        Debug.LogError($"[Claude] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = ParseClaudeError(www);
                    Debug.LogError($"[Claude] {errorMsg}");
                    onError?.Invoke(errorMsg);
                }
            }
        }

        private IEnumerator TestConnectionCoroutine(Action<bool, string> callback)
        {
            if (string.IsNullOrEmpty(config.apiKey))
            {
                callback?.Invoke(false, "API Key 未配置");
                yield break;
            }

            List<LLMMessage> testMessages = new List<LLMMessage>
            {
                new LLMMessage("user", "Hi")
            };

            string jsonData = BuildClaudeRequest(testMessages);

            using (UnityWebRequest www = new UnityWebRequest(CLAUDE_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("x-api-key", config.apiKey);
                www.SetRequestHeader("anthropic-version", CLAUDE_VERSION);
                www.timeout = 10;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, "连接成功！");
                }
                else
                {
                    callback?.Invoke(false, ParseClaudeError(www));
                }
            }
        }

        private string BuildClaudeRequest(List<LLMMessage> messages)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{config.model}\",");
            sb.Append("\"max_tokens\":4096,");
            sb.Append("\"messages\":[");

            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.Append($"\"role\":\"{messages[i].role}\",");
                sb.Append($"\"content\":\"{EscapeJson(messages[i].content ?? "")}\"");
                sb.Append("}");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        private string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private string ParseClaudeError(UnityWebRequest www)
        {
            string baseError = $"请求失败: {www.error}";

            try
            {
                string responseText = www.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    ClaudeErrorResponse error = JsonUtility.FromJson<ClaudeErrorResponse>(responseText);
                    if (error != null && error.error != null)
                    {
                        return $"{baseError}\n\n错误类型: {error.error.type}\n错误信息: {error.error.message}";
                    }
                }
            }
            catch { }

            return $"{baseError}\n\n请检查：\n1. API Key 是否正确\n2. 网络连接是否正常\n3. 是否有足够的 API 额度";
        }
    }

    // Claude API 响应数据结构
    [Serializable]
    public class ClaudeResponse
    {
        public string id;
        public string type;
        public string role;
        public ClaudeContent[] content;
        public string model;

        [Serializable]
        public class ClaudeContent
        {
            public string type;
            public string text;
        }
    }

    // Claude API 错误响应
    [Serializable]
    public class ClaudeErrorResponse
    {
        public ClaudeError error;

        [Serializable]
        public class ClaudeError
        {
            public string type;
            public string message;
        }
    }
}