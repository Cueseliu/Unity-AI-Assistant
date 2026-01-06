using System;
using System.Collections.Generic;
using System.Text;
using AIOperator.LLM;

namespace AIOperator.Models
{
    /// <summary>
    /// 消息内容类型
    /// </summary>
    public enum MessageContentType
    {
        Text,       // 普通文本
        ToolUse,    // 工具调用（assistant 发出）
        ToolResult  // 工具结果（user 角色返回给 assistant）
    }

    /// <summary>
    /// LLM API 消息格式 - 扩展支持工具调用
    /// </summary>
    [Serializable]
    public class LLMMessage
    {
        public string role;
        public string content;

        // 工具调用相关
        public MessageContentType contentType = MessageContentType.Text;
        public List<ToolCall> toolCalls;           // 当 contentType 为 ToolUse 时
        public List<ToolResultMessage> toolResults; // 当 contentType 为 ToolResult 时

        /// <summary>
        /// 创建普通文本消息
        /// </summary>
        public LLMMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
            this.contentType = MessageContentType.Text;
        }

        /// <summary>
        /// 创建工具调用消息（assistant 角色）
        /// </summary>
        public static LLMMessage FromToolUse(List<ToolCall> calls, string text = null)
        {
            return new LLMMessage("assistant", text ?? "")
            {
                contentType = MessageContentType.ToolUse,
                toolCalls = calls
            };
        }

        /// <summary>
        /// 创建工具结果消息（user 角色，用于返回工具执行结果）
        /// </summary>
        public static LLMMessage FromToolResults(List<ToolResultMessage> results)
        {
            return new LLMMessage("user", "")
            {
                contentType = MessageContentType.ToolResult,
                toolResults = results
            };
        }

        /// <summary>
        /// 是否是工具调用消息
        /// </summary>
        public bool HasToolCalls => contentType == MessageContentType.ToolUse && toolCalls != null && toolCalls.Count > 0;

        /// <summary>
        /// 是否是工具结果消息
        /// </summary>
        public bool HasToolResults => contentType == MessageContentType.ToolResult && toolResults != null && toolResults.Count > 0;

        /// <summary>
        /// 获取消息的文本表示（用于调试）
        /// </summary>
        public string GetDisplayText()
        {
            switch (contentType)
            {
                case MessageContentType.ToolUse:
                    if (toolCalls == null || toolCalls.Count == 0)
                        return content ?? "";
                    var sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(content))
                    {
                        sb.AppendLine(content);
                    }
                    sb.AppendLine($"[调用 {toolCalls.Count} 个工具]");
                    foreach (var call in toolCalls)
                    {
                        sb.AppendLine($"  - {call.Name}({call.GetArgumentsSummary()})");
                    }
                    return sb.ToString();

                case MessageContentType.ToolResult:
                    if (toolResults == null || toolResults.Count == 0)
                        return "[工具结果]";
                    var sb2 = new StringBuilder();
                    sb2.AppendLine($"[{toolResults.Count} 个工具结果]");
                    foreach (var result in toolResults)
                    {
                        var status = result.Result.Success ? "成功" : "失败";
                        sb2.AppendLine($"  - {result.ToolUseId}: {status}");
                    }
                    return sb2.ToString();

                default:
                    return content ?? "";
            }
        }
    }
    
    /// <summary>
    /// LLM 请求数据
    /// </summary>
    [Serializable]
    public class LLMRequest
    {
        public string model;
        public List<LLMMessage> messages;
        public bool stream;
        
        public LLMRequest(string model, List<LLMMessage> messages, bool stream = false)
        {
            this.model = model;
            this.messages = messages;
            this.stream = stream;
        }
    }
    
    /// <summary>
    /// LLM 响应数据
    /// </summary>
    [Serializable]
    public class LLMResponse
    {
        public string model;
        public Message message;
        public bool done;
        
        [Serializable]
        public class Message
        {
            public string role;
            public string content;
        }
    }
    
    /// <summary>
    /// LLM 配置
    /// </summary>
    [Serializable]
    public class LLMConfig
    {
        public string provider = "Claude";
        public string apiUrl = "";
        public string model = "claude-3-5-sonnet-20241022";
        public string apiKey = "";

        // AWS Bedrock 专用配置
        public string awsAccessKeyId = "";
        public string awsSecretAccessKey = "";
        public string awsRegion = "us-east-1";
        public string awsBearerToken = ""; // Bearer Token 认证方式

        // 保存到 EditorPrefs 的键
        private const string PREF_PROVIDER = "AIOperator.LLM.Provider";
        private const string PREF_API_URL = "AIOperator.LLM.ApiUrl";
        private const string PREF_MODEL = "AIOperator.LLM.Model";
        private const string PREF_API_KEY = "AIOperator.LLM.ApiKey";
        private const string PREF_AWS_ACCESS_KEY_ID = "AIOperator.LLM.AWSAccessKeyId";
        private const string PREF_AWS_SECRET_ACCESS_KEY = "AIOperator.LLM.AWSSecretAccessKey";
        private const string PREF_AWS_REGION = "AIOperator.LLM.AWSRegion";
        private const string PREF_AWS_BEARER_TOKEN = "AIOperator.LLM.AWSBearerToken";

        // 预设的模型列表
        public static readonly string[] ClaudeModels = new string[]
        {
            "claude-3-5-sonnet-20241022",  // 最新 Sonnet（推荐）
            "claude-3-5-haiku-20241022"    // 最快最便宜
        };

        public static readonly string[] OpenAIModels = new string[]
        {
            "gpt-4o",       // 最新最强（推荐）
            "gpt-4o-mini"   // 快速便宜
        };

        public static readonly string[] DeepSeekModels = new string[]
        {
            "deepseek-chat",
            "deepseek-coder"
        };

        public static readonly string[] QwenModels = new string[]
        {
            "qwen-plus",        // 效果最好（推荐）
            "qwen-turbo",       // 速度最快
            "qwen-max"          // 最强能力
        };

        public static readonly string[] GLM4Models = new string[]
        {
            "glm-4-plus",       // 效果最好（推荐）
            "glm-4-flash",      // 速度最快
            "glm-4"             // 标准版
        };

        public static readonly string[] DoubaoModels = new string[]
        {
            "doubao-seed-1-8-251228",    // Seed 1.8 最新版（推荐）
            "doubao-1-5-pro-32k-250115", // 1.5 Pro 版本
            "doubao-1-5-lite-32k-250115" // 1.5 Lite 版本
        };

        public static readonly string[] BedrockModels = new string[]
        {
            "global.anthropic.claude-opus-4-5-20251101-v1:0",      // 最强大（推荐）
            "us.anthropic.claude-sonnet-4-20250514-v3:0",          // 平衡版本
            "us.anthropic.claude-haiku-4-5-20251001-v1:0",         // 最快最便宜
            "global.anthropic.claude-3-5-sonnet-20241022-v2:0"     // 经典 3.5 版本
        };

        public static readonly string[] AWSRegions = new string[]
        {
            "us-east-1",
            "us-west-2",
            "eu-west-1",
            "eu-central-1",
            "ap-southeast-1",
            "ap-northeast-1"
        };

        /// <summary>
        /// 从 EditorPrefs 加载配置
        /// </summary>
        public static LLMConfig Load()
        {
            return new LLMConfig
            {
                provider = UnityEditor.EditorPrefs.GetString(PREF_PROVIDER, "Claude"),
                apiUrl = UnityEditor.EditorPrefs.GetString(PREF_API_URL, ""),
                model = UnityEditor.EditorPrefs.GetString(PREF_MODEL, "claude-3-5-sonnet-20241022"),
                apiKey = UnityEditor.EditorPrefs.GetString(PREF_API_KEY, ""),
                awsAccessKeyId = UnityEditor.EditorPrefs.GetString(PREF_AWS_ACCESS_KEY_ID, ""),
                awsSecretAccessKey = UnityEditor.EditorPrefs.GetString(PREF_AWS_SECRET_ACCESS_KEY, ""),
                awsRegion = UnityEditor.EditorPrefs.GetString(PREF_AWS_REGION, "us-east-1"),
                awsBearerToken = UnityEditor.EditorPrefs.GetString(PREF_AWS_BEARER_TOKEN, "")
            };
        }

        /// <summary>
        /// 保存配置到 EditorPrefs
        /// </summary>
        public void Save()
        {
            UnityEditor.EditorPrefs.SetString(PREF_PROVIDER, provider);
            UnityEditor.EditorPrefs.SetString(PREF_API_URL, apiUrl);
            UnityEditor.EditorPrefs.SetString(PREF_MODEL, model);
            UnityEditor.EditorPrefs.SetString(PREF_API_KEY, apiKey);
            UnityEditor.EditorPrefs.SetString(PREF_AWS_ACCESS_KEY_ID, awsAccessKeyId);
            UnityEditor.EditorPrefs.SetString(PREF_AWS_SECRET_ACCESS_KEY, awsSecretAccessKey);
            UnityEditor.EditorPrefs.SetString(PREF_AWS_REGION, awsRegion);
            UnityEditor.EditorPrefs.SetString(PREF_AWS_BEARER_TOKEN, awsBearerToken);
        }

        /// <summary>
        /// 根据提供者获取默认模型
        /// </summary>
        public static string GetDefaultModel(string provider)
        {
            switch (provider)
            {
                case "Claude":
                    return "claude-3-5-sonnet-20241022";
                case "Bedrock":
                    return "global.anthropic.claude-opus-4-5-20251101-v1:0";
                case "OpenAI":
                    return "gpt-4o-mini";
                case "DeepSeek":
                    return "deepseek-chat";
                case "Qwen":
                    return "qwen-plus";
                case "GLM-4":
                    return "glm-4-plus";
                case "Doubao":
                    return "doubao-seed-1-8-251228";
                default:
                    return "claude-3-5-sonnet-20241022";
            }
        }

        /// <summary>
        /// 根据提供者获取模型列表
        /// </summary>
        public static string[] GetModelsForProvider(string provider)
        {
            switch (provider)
            {
                case "Claude":
                    return ClaudeModels;
                case "Bedrock":
                    return BedrockModels;
                case "OpenAI":
                    return OpenAIModels;
                case "DeepSeek":
                    return DeepSeekModels;
                case "Qwen":
                    return QwenModels;
                case "GLM-4":
                    return GLM4Models;
                case "Doubao":
                    return DoubaoModels;
                default:
                    return ClaudeModels;
            }
        }
    }
}