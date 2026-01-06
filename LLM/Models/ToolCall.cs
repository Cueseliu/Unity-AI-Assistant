using System.Collections.Generic;

namespace AIOperator.LLM
{
    /// <summary>
    /// 工具调用数据模型
    /// 表示 LLM 请求执行的一次工具调用
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// 工具调用 ID（用于匹配 tool_result）
        /// Claude 格式: "toolu_01xxx"
        /// OpenAI 格式: "call_xxx"
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 工具名称
        /// 如: "create_primitive", "set_material_color"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 工具参数（键值对）
        /// </summary>
        public Dictionary<string, object> Arguments { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ToolCall()
        {
            Arguments = new Dictionary<string, object>();
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public ToolCall(string id, string name, Dictionary<string, object> arguments)
        {
            Id = id;
            Name = name;
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// 创建无 ID 的工具调用（用于测试）
        /// </summary>
        public static ToolCall Create(string name, Dictionary<string, object> arguments = null)
        {
            return new ToolCall
            {
                Id = System.Guid.NewGuid().ToString("N").Substring(0, 16),
                Name = name,
                Arguments = arguments ?? new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// 获取参数的简短描述（用于日志）
        /// </summary>
        public string GetArgumentsSummary()
        {
            if (Arguments == null || Arguments.Count == 0)
            {
                return "{}";
            }

            var parts = new List<string>();
            foreach (var pair in Arguments)
            {
                string value = pair.Value?.ToString() ?? "null";
                if (value.Length > 20)
                {
                    value = value.Substring(0, 17) + "...";
                }
                parts.Add($"{pair.Key}={value}");
            }
            return "{" + string.Join(", ", parts) + "}";
        }

        public override string ToString()
        {
            return $"{Name}({GetArgumentsSummary()})";
        }
    }
}
