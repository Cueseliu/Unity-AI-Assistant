using System.Collections.Generic;

namespace AIOperator.LLM
{
    /// <summary>
    /// 工具执行结果
    /// </summary>
    public class ToolResult
    {
        /// <summary>
        /// 是否执行成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 结果消息（发送给 LLM 的内容）
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误信息（如果失败）
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// 额外数据（可选）
        /// </summary>
        public Dictionary<string, object> Data { get; set; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private ToolResult()
        {
            Data = new Dictionary<string, object>();
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static ToolResult Ok(string message)
        {
            return new ToolResult
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// 创建成功结果（带额外数据）
        /// </summary>
        public static ToolResult Ok(string message, Dictionary<string, object> data)
        {
            return new ToolResult
            {
                Success = true,
                Message = message,
                Data = data ?? new Dictionary<string, object>()
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        public static ToolResult Fail(string error)
        {
            return new ToolResult
            {
                Success = false,
                Error = error,
                Message = $"Error: {error}"
            };
        }

        /// <summary>
        /// 创建 "未找到物体" 错误
        /// </summary>
        public static ToolResult NotFound(string objectName)
        {
            return Fail($"Not found: '{objectName}'");
        }

        /// <summary>
        /// 创建 "缺少参数" 错误
        /// </summary>
        public static ToolResult MissingParameter(string paramName)
        {
            return Fail($"Missing required parameter: {paramName}");
        }

        /// <summary>
        /// 创建 "无效参数" 错误
        /// </summary>
        public static ToolResult InvalidParameter(string paramName, string reason)
        {
            return Fail($"Invalid parameter '{paramName}': {reason}");
        }

        /// <summary>
        /// 将结果转换为发送给 LLM 的字符串
        /// </summary>
        public string ToContent()
        {
            return Message ?? (Success ? "Operation successful" : Error ?? "Unknown error");
        }

        public override string ToString()
        {
            return Success ? $"[OK] {Message}" : $"[FAIL] {Error}";
        }
    }

    /// <summary>
    /// 工具结果消息（用于发送回 LLM）
    /// </summary>
    public class ToolResultMessage
    {
        /// <summary>
        /// 对应的工具调用 ID
        /// </summary>
        public string ToolUseId { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public ToolResult Result { get; set; }

        /// <summary>
        /// 是否为错误结果
        /// </summary>
        public bool IsError => !Result.Success;

        public ToolResultMessage(string toolUseId, ToolResult result)
        {
            ToolUseId = toolUseId;
            Result = result;
        }
    }
}
