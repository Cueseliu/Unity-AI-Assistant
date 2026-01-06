using System.Collections.Generic;

namespace AIOperator.LLM
{
    /// <summary>
    /// LLM 响应的统一模型，支持文本和工具调用
    /// 注意：避免与 Models.LLMResponse 冲突，使用不同的类名
    /// </summary>
    public class ToolCallResponse
    {
        /// <summary>
        /// 是否包含工具调用
        /// </summary>
        public bool HasToolCalls => ToolCalls != null && ToolCalls.Count > 0;

        /// <summary>
        /// 文本响应内容（可能为空）
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 工具调用列表
        /// </summary>
        public List<ToolCall> ToolCalls { get; set; }

        /// <summary>
        /// 停止原因: "end_turn", "tool_use", "max_tokens"
        /// </summary>
        public string StopReason { get; set; }

        /// <summary>
        /// 原始响应（用于调试）
        /// </summary>
        public string RawResponse { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public ToolCallResponse()
        {
            ToolCalls = new List<ToolCall>();
        }

        /// <summary>
        /// 创建文本响应
        /// </summary>
        public static ToolCallResponse FromText(string text)
        {
            return new ToolCallResponse
            {
                Text = text,
                StopReason = "end_turn"
            };
        }

        /// <summary>
        /// 创建工具调用响应
        /// </summary>
        public static ToolCallResponse FromToolCalls(List<ToolCall> calls, string text = null)
        {
            return new ToolCallResponse
            {
                ToolCalls = calls ?? new List<ToolCall>(),
                Text = text,
                StopReason = "tool_use"
            };
        }

        /// <summary>
        /// 创建错误响应
        /// </summary>
        public static ToolCallResponse FromError(string error)
        {
            return new ToolCallResponse
            {
                Text = $"错误: {error}",
                StopReason = "error"
            };
        }

        /// <summary>
        /// 是否是最终响应（没有工具调用，对话结束）
        /// </summary>
        public bool IsFinalResponse => !HasToolCalls && StopReason != "tool_use";

        public override string ToString()
        {
            if (HasToolCalls)
            {
                var toolNames = new List<string>();
                foreach (var call in ToolCalls)
                {
                    toolNames.Add(call.Name);
                }
                return $"[ToolCallResponse] Tools: {string.Join(", ", toolNames)}";
            }
            else
            {
                var preview = Text?.Length > 50 ? Text.Substring(0, 47) + "..." : Text;
                return $"[ToolCallResponse] Text: {preview}";
            }
        }
    }
}
