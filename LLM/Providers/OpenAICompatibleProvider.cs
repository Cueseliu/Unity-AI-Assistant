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
    /// OpenAI 兼容 API 基类 - 支持所有 OpenAI 格式的 API
    /// 包括 DeepSeek、Qwen、GLM-4、豆包、Moonshot 等
    /// </summary>
    public abstract class OpenAICompatibleProvider : ILLMProvider
    {
        protected LLMConfig config;
        protected MonoBehaviour coroutineRunner;

        /// <summary>
        /// API 端点 URL
        /// </summary>
        protected abstract string ApiUrl { get; }

        /// <summary>
        /// Provider 显示名称
        /// </summary>
        protected abstract string ProviderDisplayName { get; }

        /// <summary>
        /// 错误提示中的 API Key 名称
        /// </summary>
        protected virtual string ApiKeyName => $"{ProviderDisplayName} API Key";

        public OpenAICompatibleProvider(LLMConfig config, MonoBehaviour coroutineRunner)
        {
            this.config = config;
            this.coroutineRunner = coroutineRunner;
        }

        public string GetProviderName()
        {
            return ProviderDisplayName;
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
                onError?.Invoke($"请先在设置中配置 {ApiKeyName}");
                yield break;
            }

            string jsonData = BuildRequestWithTools(messages, tools, systemPrompt);

            Debug.Log($"[{ProviderDisplayName}] 发送带工具的请求");

            using (UnityWebRequest www = new UnityWebRequest(ApiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                SetRequestHeaders(www);

                www.timeout = 120;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[{ProviderDisplayName}] 收到响应");

                    try
                    {
                        var response = ParseResponseWithTools(responseText);
                        response.RawResponse = responseText;
                        onResponse?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[{ProviderDisplayName}] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = ParseError(www);
                    Debug.LogError($"[{ProviderDisplayName}] {errorMsg}");
                    onError?.Invoke(errorMsg);
                }
            }
        }

        /// <summary>
        /// 设置请求头 - 子类可以重写以添加特殊头
        /// </summary>
        protected virtual void SetRequestHeaders(UnityWebRequest www)
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {config.apiKey}");
        }

        protected string BuildRequestWithTools(List<LLMMessage> messages, ToolDefinition[] tools, string systemPrompt = null)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"model\":\"{config.model}\",");

            // 添加工具定义
            if (tools != null && tools.Length > 0)
            {
                sb.Append("\"tools\":");
                sb.Append(ToolDefinitionSerializer.ToOpenAIFormat(tools));
                sb.Append(",");
            }

            // 添加消息
            sb.Append("\"messages\":[");

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
            sb.Append("\"temperature\":0.7");
            sb.Append("}");

            return sb.ToString();
        }

        protected string SerializeMessageOpenAI(LLMMessage message)
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

        protected ToolCallResponse ParseResponseWithTools(string responseText)
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

        // ========== 简单消息发送 ==========

        private IEnumerator SendMessageCoroutine(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError)
        {
            if (string.IsNullOrEmpty(config.apiKey))
            {
                onError?.Invoke($"请先在设置中配置 {ApiKeyName}");
                yield break;
            }

            string jsonData = BuildSimpleRequest(messages);

            Debug.Log($"[{ProviderDisplayName}] 发送请求到: {ApiUrl}");
            Debug.Log($"[{ProviderDisplayName}] 使用模型: {config.model}");

            using (UnityWebRequest www = new UnityWebRequest(ApiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();

                SetRequestHeaders(www);

                www.timeout = 60;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseText = www.downloadHandler.text;
                    Debug.Log($"[{ProviderDisplayName}] 收到响应");

                    try
                    {
                        var json = MiniJSON.Deserialize(responseText) as Dictionary<string, object>;
                        if (json != null && json.TryGetValue("choices", out var choicesObj))
                        {
                            var choices = choicesObj as List<object>;
                            if (choices != null && choices.Count > 0)
                            {
                                var choice = choices[0] as Dictionary<string, object>;
                                if (choice != null && choice.TryGetValue("message", out var msgObj))
                                {
                                    var msg = msgObj as Dictionary<string, object>;
                                    if (msg != null && msg.TryGetValue("content", out var content))
                                    {
                                        onResponse?.Invoke(content?.ToString() ?? "");
                                        yield break;
                                    }
                                }
                            }
                        }
                        onError?.Invoke("响应格式错误");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[{ProviderDisplayName}] 解析响应失败: {e.Message}");
                        onError?.Invoke($"解析响应失败: {e.Message}");
                    }
                }
                else
                {
                    string errorMsg = ParseError(www);
                    Debug.LogError($"[{ProviderDisplayName}] {errorMsg}");
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

            string jsonData = BuildSimpleRequest(testMessages);

            using (UnityWebRequest www = new UnityWebRequest(ApiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                SetRequestHeaders(www);
                www.timeout = 15;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(true, "连接成功！");
                }
                else
                {
                    callback?.Invoke(false, ParseError(www));
                }
            }
        }

        protected string BuildSimpleRequest(List<LLMMessage> messages)
        {
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
            sb.Append("\"temperature\":0.7");
            sb.Append("}");
            return sb.ToString();
        }

        protected string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        protected virtual string ParseError(UnityWebRequest www)
        {
            string baseError = $"请求失败: {www.error}";

            try
            {
                string responseText = www.downloadHandler.text;
                if (!string.IsNullOrEmpty(responseText))
                {
                    var json = MiniJSON.Deserialize(responseText) as Dictionary<string, object>;
                    if (json != null && json.TryGetValue("error", out var errorObj))
                    {
                        var error = errorObj as Dictionary<string, object>;
                        if (error != null)
                        {
                            string message = error.ContainsKey("message") ? error["message"]?.ToString() : "";
                            string code = error.ContainsKey("code") ? error["code"]?.ToString() : "";
                            if (!string.IsNullOrEmpty(message))
                            {
                                return $"{baseError}\n\n错误代码: {code}\n错误信息: {message}";
                            }
                        }
                    }
                }
            }
            catch
            {
                // 解析失败，返回基础错误
            }

            return $"{baseError}\n\n请检查：\n1. API Key 是否正确\n2. 网络连接是否正常\n3. 是否有足够的 API 额度";
        }
    }
}
