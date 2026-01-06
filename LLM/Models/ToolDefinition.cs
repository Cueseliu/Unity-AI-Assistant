using System.Collections.Generic;

namespace AIOperator.LLM
{
    /// <summary>
    /// 工具定义 - 描述工具的名称、描述和参数结构
    /// 发送给 LLM，告诉它可以调用哪些工具
    /// </summary>
    public class ToolDefinition
    {
        /// <summary>
        /// 工具名称（唯一标识）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 工具描述（告诉 LLM 这个工具做什么）
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 参数定义
        /// </summary>
        public ToolParameters Parameters { get; set; }
    }

    /// <summary>
    /// 工具参数定义
    /// </summary>
    public class ToolParameters
    {
        /// <summary>
        /// JSON Schema 类型（通常为 "object"）
        /// </summary>
        public string Type { get; set; } = "object";

        /// <summary>
        /// 参数属性定义
        /// </summary>
        public Dictionary<string, ToolProperty> Properties { get; set; }

        /// <summary>
        /// 必填参数列表
        /// </summary>
        public string[] Required { get; set; }
    }

    /// <summary>
    /// 工具参数属性定义
    /// </summary>
    public class ToolProperty
    {
        /// <summary>
        /// 参数类型: string, number, integer, boolean, array, object
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 参数描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 可选值列表（枚举）
        /// </summary>
        public string[] Enum { get; set; }

        /// <summary>
        /// 数组元素类型（当 Type 为 array 时）
        /// </summary>
        public ToolProperty Items { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object Default { get; set; }
    }
}
