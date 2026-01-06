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
    /// DeepSeek API 提供者 - 支持工具调用
    /// </summary>
    public class DeepSeekProvider : ILLMProvider
    {
        private LLMConfig config;
        private MonoBehaviour coroutineRunner;

        // DeepSeek API 常量
        private const string DEEPSEEK_API_URL = "https://api.deepseek.com/v1/chat/completions";

        public DeepSeekProvider(LLMConfig config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        public string GetProviderName()
        {
            return "DeepSeek";
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
            if (string.IsNullOrEmpty(config.apiKey))
            {
                onError?.Invoke("请先在设置中配置 DeepSeek API Key");
                yield break;
            }

            // 构建带工具的请求
            string jsonData = BuildDeepSeekRequestWithTools(messages, tools, systemPrompt);

            Debug.Log($"[DeepSeek] 发送带工具的请求");

            using (UnityWebRequest www = new UnityWebRequest(DEEPSEEK_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");

                www.timeout = 120;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[DeepSeek] 收到响应");

                    try
                    {
                        var response = ParseDeepSeekResponseWithTools(responseText);
                        response.RawResponse = responseText;
                        onResponse?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DeepSeek] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = ParseDeepSeekError(www);
                    Debug.LogError($"[DeepSeek] {errorMsg}");
                    onError?.Invoke(errorMsg);
                }
            }
        }

        private string BuildDeepSeekRequestWithTools(List<LLMMessage> messages, ToolDefinition[] tools, string systemPrompt = null)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{config.model}\",");

            // 添加工具定义（OpenAI 格式）
            if (tools != null && tools.Length > 0)
            {
                sb.Append("\"tools\":");
                sb.Append(ToolDefinitionSerializer.ToOpenAIFormat(tools));
                sb.Append(",");
            }

            // 添加消息（OpenAI 格式使用 system role 的消息）
            sb.Append("\"messages\":[");

            // 如果有系统提示词，作为第一条 system 消息
            bool hasMessages = false;
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                sb.Append("{\"role\":\"system\",\"content\":\"");
                sb.Append(EscapeJson(systemPrompt));
                sb.Append("\"}");
                hasMessages = true;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                if (hasMessages || i > 0) sb.Append(",");
                hasMessages = true;
                sb.Append(SerializeMessageOpenAI(messages[i]));
            }
            sb.Append("],");

            sb.Append("\"max_tokens\":4096,");
            sb.Append("\"temperature\":1");
            sb.Append("}");

            return sb.ToString();
        }

        private string SerializeMessageOpenAI(LLMMessage message)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"role\":\"{message.role}\",");

            switch (message.contentType)
            {
                case MessageContentType.ToolUse:
                    sb.Append($"\"content\":{(string.IsNullOrEmpty(message.content) ? "null" : $"\"{EscapeJson(message.content)}\"")},");
                    sb.Append("\"tool_calls\":[");
                    if (message.toolCalls != null)
                    {
                        for (int i = 0; i < message.toolCalls.Count; i++)
                        {
                            if (i > 0) sb.Append(",");
                            var call = message.toolCalls[i];
                            sb.Append("{");
                            sb.Append($"\"id\":\"{call.Id}\",");
                            sb.Append("\"type\":\"function\",");
                            sb.Append("\"function\":{");
                            sb.Append($"\"name\":\"{call.Name}\",");
                            sb.Append($"\"arguments\":\"{EscapeJson(MiniJSON.Serialize(call.Arguments))}\"");
                            sb.Append("}}");
                        }
                    }
                    sb.Append("]");
                    break;

                case MessageContentType.ToolResult:
                    if (message.toolResults != null && message.toolResults.Count > 0)
                    {
                        var result = message.toolResults[0];
                        sb.Clear();
                        sb.Append("{");
                        sb.Append("\"role\":\"tool\",");
                        sb.Append($"\"tool_call_id\":\"{result.ToolUseId}\",");
                        sb.Append($"\"content\":\"{EscapeJson(result.Result.ToContent())}\"");
                        sb.Append("}");

                        for (int i = 1; i < message.toolResults.Count; i++)
                        {
                            var r = message.toolResults[i];
                            sb.Append(",{");
                            sb.Append("\"role\":\"tool\",");
                            sb.Append($"\"tool_call_id\":\"{r.ToolUseId}\",");
                            sb.Append($"\"content\":\"{EscapeJson(r.Result.ToContent())}\"");
                            sb.Append("}");
                        }
                        return sb.ToString();
                    }
                    sb.Append($"\"content\":\"\"");
                    break;

                default:
                    sb.Append($"\"content\":\"{EscapeJson(message.content ?? "")}\"");
                    break;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private ToolCallResponse ParseDeepSeekResponseWithTools(string responseText)
        {
            var json = MiniJSON.Deserialize(responseText) as Dictionary<string, object>;
            if (json == null)
            {
                throw new Exception("无法解析 JSON 响应");
            }

            var response = new ToolCallResponse();

            if (json.TryGetValue("choices", out var choicesObj) && choicesObj is List<object> choices && choices.Count > 0)
            {
                var choice = choices[0] as Dictionary<string, object>;
                if (choice != null)
                {
                    if (choice.TryGetValue("finish_reason", out var finishReason))
                    {
                        response.StopReason = finishReason?.ToString();
                    }

                    if (choice.TryGetValue("message", out var messageObj) && messageObj is Dictionary<string, object> message)
                    {
                        if (message.TryGetValue("content", out var content) && content != null)
                        {
                            response.Text = content.ToString();
                        }

                        if (message.TryGetValue("tool_calls", out var toolCallsObj) && toolCallsObj is List<object> toolCallsList)
                        {
                            var toolCalls = new List<ToolCall>();
                            foreach (var tcObj in toolCallsList)
                            {
                                if (tcObj is Dictionary<string, object> tc)
                                {
                                    var toolCall = new ToolCall();
                                    toolCall.Id = tc.ContainsKey("id") ? tc["id"]?.ToString() : "";

                                    if (tc.TryGetValue("function", out var funcObj) && funcObj is Dictionary<string, object> func)
                                    {
                                        toolCall.Name = func.ContainsKey("name") ? func["name"]?.ToString() : "";

                                        if (func.TryGetValue("arguments", out var argsObj))
                                        {
                                            var argsStr = argsObj?.ToString() ?? "{}";
                                            var args = MiniJSON.Deserialize(argsStr) as Dictionary<string, object>;
                                            toolCall.Arguments = args ?? new Dictionary<string, object>();
                                        }
                                    }

                                    toolCalls.Add(toolCall);
                                }
                            }

                            if (toolCalls.Count > 0)
                            {
                                response.ToolCalls = toolCalls;
                            }
                        }
                    }
                }
            }

            return response;
        }

        // ========== 原有方法 ==========

        private IEnumerator SendMessageCoroutine(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError)
        {
            // 检查 API Key
            if (string.IsNullOrEmpty(config.apiKey))
            {
                onError?.Invoke("请先在设置中配置 DeepSeek API Key");
                yield break;
            }

            // 构建请求体（OpenAI 兼容格式）
            string jsonData = BuildDeepSeekRequest(messages);

            Debug.Log($"[DeepSeek] 发送请求到: {DEEPSEEK_API_URL}");
            Debug.Log($"[DeepSeek] 使用模型: {config.model}");

            // 创建 UnityWebRequest
            using (UnityWebRequest www = new UnityWebRequest(DEEPSEEK_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                // 设置 DeepSeek API 所需的请求头
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");

                www.timeout = 60;

                // 发送请求
                yield return www.SendWebRequest();

                // 处理响应
                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[DeepSeek] 收到响应");

                    try
                    {
                        DeepSeekResponse response = JsonUtility.FromJson<DeepSeekResponse>(responseText);
                        if (response != null && response.choices != null && response.choices.Length > 0)
                        {
                            onResponse?.Invoke(response.choices[0].message.content);
                        }
                        else
                        {
                            onError?.Invoke("响应格式错误");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[DeepSeek] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}\n\n响应内容: {responseText}");
                    }
                }
                else
                {
                    string errorMsg = ParseDeepSeekError(www);
                    Debug.LogError($"[DeepSeek] {errorMsg}");
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

            // 发送一个简单的测试消息
            List<LLMMessage> testMessages = new List<LLMMessage>
            {
                new LLMMessage("user", "Hi")
            };

            string jsonData = BuildDeepSeekRequest(testMessages);

            using (UnityWebRequest www = new UnityWebRequest(DEEPSEEK_API_URL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");
                www.timeout = 10;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, "连接成功！");
                }
                else
                {
                    callback?.Invoke(false, ParseDeepSeekError(www));
                }
            }
        }

        /// <summary>
        /// 构建 DeepSeek API 请求 JSON (OpenAI 兼容格式)
        /// </summary>
        private string BuildDeepSeekRequest(List<LLMMessage> messages)
        {
            // OpenAI 兼容的格式
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{config.model}\",");
            sb.Append("\"messages\":[");

            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.Append($"\"role\":\"{messages[i].role}\",");
                sb.Append($"\"content\":\"{EscapeJson(messages[i].content)}\"");
                sb.Append("}");
            }

            sb.Append("],");
            sb.Append("\"max_tokens\":4096,");
            sb.Append("\"temperature\":1");
            sb.Append("}");
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
        /// 解析 DeepSeek 错误信息
        /// </summary>
        private string ParseDeepSeekError(UnityWebRequest www)
        {
            string baseError = $"请求失败: {www.error}";

            try
            {
                string responseText = www.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    // 尝试解析错误信息
                    DeepSeekErrorResponse error = JsonUtility.FromJson<DeepSeekErrorResponse>(responseText);
                    if (error != null && error.error != null)
                    {
                        return $"{baseError}\n\n错误类型: {error.error.type}\n错误信息: {error.error.message}";
                    }
                }
            }
            catch
            {
                // 如果解析失败，返回基础错误
            }

            return $"{baseError}\n\n请检查：\n1. API Key 是否正确\n2. 网络连接是否正常\n3. 是否有足够的 API 额度";
        }
    }

    // DeepSeek API 响应数据结构 (OpenAI 兼容)
    [Serializable]
    public class DeepSeekResponse
    {
        public string id;
        public string @object;
        public long created;
        public string model;
        public DeepSeekChoice[] choices;
        public DeepSeekUsage usage;

        [Serializable]
        public class DeepSeekChoice
        {
            public int index;
            public DeepSeekMessage message;
            public string finish_reason;
        }

        [Serializable]
        public class DeepSeekMessage
        {
            public string role;
            public string content;
        }

        [Serializable]
        public class DeepSeekUsage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }

    // DeepSeek API 错误响应
    [Serializable]
    public class DeepSeekErrorResponse
    {
        public DeepSeekError error;

        [Serializable]
        public class DeepSeekError
        {
            public string type;
            public string message;
            public string code;
        }
    }
}
