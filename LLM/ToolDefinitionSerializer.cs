using System.Collections.Generic;
using System.Text;

namespace AIOperator.LLM
{
    /// <summary>
    /// 工具定义序列化器 - 为不同 Provider 生成对应格式的工具定义 JSON
    /// </summary>
    public static class ToolDefinitionSerializer
    {
        /// <summary>
        /// 生成 Claude API 格式的工具定义
        /// Claude 使用 input_schema 字段
        /// </summary>
        public static string ToClaudeFormat(ToolDefinition[] tools)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < tools.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{");
                sb.Append($"\"name\":\"{tools[i].Name}\",");
                sb.Append($"\"description\":\"{EscapeJson(tools[i].Description)}\",");
                sb.Append("\"input_schema\":");
                sb.Append(SerializeParameters(tools[i].Parameters));
                sb.Append("}");
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// 生成 OpenAI/DeepSeek 格式的工具定义
        /// OpenAI 使用 type: "function" 包装，参数字段为 parameters
        /// </summary>
        public static string ToOpenAIFormat(ToolDefinition[] tools)
        {
            var sb = new StringBuilder();
            sb.Append("[");

            for (int i = 0; i < tools.Length; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append("{\"type\":\"function\",\"function\":{");
                sb.Append($"\"name\":\"{tools[i].Name}\",");
                sb.Append($"\"description\":\"{EscapeJson(tools[i].Description)}\",");
                sb.Append("\"parameters\":");
                sb.Append(SerializeParameters(tools[i].Parameters));
                sb.Append("}}");
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// 生成 Bedrock API 格式的工具定义
        /// Bedrock Runtime API 使用与 Claude Messages API 相同的格式
        /// </summary>
        public static string ToBedrockFormat(ToolDefinition[] tools)
        {
            // Bedrock Runtime API (invoke) 使用与 Claude API 相同的格式
            return ToClaudeFormat(tools);
        }

        /// <summary>
        /// 序列化参数定义为 JSON Schema 格式
        /// </summary>
        private static string SerializeParameters(ToolParameters parameters)
        {
            if (parameters == null)
            {
                return "{\"type\":\"object\",\"properties\":{}}";
            }

            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"type\":\"{parameters.Type ?? "object"}\",");
            sb.Append("\"properties\":{");

            if (parameters.Properties != null)
            {
                bool first = true;
                foreach (var pair in parameters.Properties)
                {
                    if (!first) sb.Append(",");
                    first = false;

                    sb.Append($"\"{pair.Key}\":");
                    sb.Append(SerializeProperty(pair.Value));
                }
            }

            sb.Append("}");

            // 添加 required 字段
            if (parameters.Required != null && parameters.Required.Length > 0)
            {
                sb.Append(",\"required\":[");
                for (int i = 0; i < parameters.Required.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append($"\"{parameters.Required[i]}\"");
                }
                sb.Append("]");
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 序列化单个属性
        /// </summary>
        private static string SerializeProperty(ToolProperty property)
        {
            if (property == null)
            {
                return "{\"type\":\"string\"}";
            }

            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"type\":\"{property.Type ?? "string"}\"");

            if (!string.IsNullOrEmpty(property.Description))
            {
                sb.Append($",\"description\":\"{EscapeJson(property.Description)}\"");
            }

            // 枚举值
            if (property.Enum != null && property.Enum.Length > 0)
            {
                sb.Append(",\"enum\":[");
                for (int i = 0; i < property.Enum.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append($"\"{EscapeJson(property.Enum[i])}\"");
                }
                sb.Append("]");
            }

            // 数组元素类型
            if (property.Type == "array" && property.Items != null)
            {
                sb.Append(",\"items\":");
                sb.Append(SerializeProperty(property.Items));
            }

            // 默认值
            if (property.Default != null)
            {
                sb.Append(",\"default\":");
                if (property.Default is string)
                {
                    sb.Append($"\"{EscapeJson(property.Default.ToString())}\"");
                }
                else if (property.Default is bool b)
                {
                    sb.Append(b ? "true" : "false");
                }
                else
                {
                    sb.Append(property.Default.ToString());
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 转义 JSON 字符串
        /// </summary>
        public static string EscapeJson(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            return text
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>
        /// 获取指定格式的工具定义
        /// </summary>
        /// <param name="provider">Provider 名称: Claude, DeepSeek, OpenAI, Bedrock</param>
        /// <param name="tools">工具定义数组</param>
        public static string GetToolsJson(string provider, ToolDefinition[] tools)
        {
            switch (provider?.ToLower())
            {
                case "claude":
                    return ToClaudeFormat(tools);
                case "bedrock":
                    return ToBedrockFormat(tools);
                case "openai":
                case "deepseek":
                    return ToOpenAIFormat(tools);
                default:
                    return ToClaudeFormat(tools);
            }
        }
    }
}
