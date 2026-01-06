using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AIOperator.LLM;

namespace AIOperator.Editor.Tools.Core
{
    /// <summary>
    /// 工具注册表 - 管理工具执行器和参数验证
    /// 单例模式
    /// </summary>
    public class ToolRegistry
    {
        private static ToolRegistry _instance;
        public static ToolRegistry Instance => _instance ??= new ToolRegistry();

        // 工具名称 -> 执行器映射
        private Dictionary<string, IToolExecutor> executors = new Dictionary<string, IToolExecutor>();

        // 工具定义映射
        private Dictionary<string, ToolDefinition> toolDefinitions = new Dictionary<string, ToolDefinition>();

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private ToolRegistry()
        {
            // 注册工具定义
            foreach (var tool in ToolDefinitions.AllTools)
            {
                toolDefinitions[tool.Name] = tool;
            }
        }

        /// <summary>
        /// 注册执行器
        /// </summary>
        public void RegisterExecutor(IToolExecutor executor)
        {
            if (executor == null) return;

            foreach (var toolName in executor.SupportedTools)
            {
                if (executors.ContainsKey(toolName))
                {
                    Debug.LogWarning($"[ToolRegistry] 工具 '{toolName}' 已被注册，将被覆盖");
                }
                executors[toolName] = executor;
            }
        }

        /// <summary>
        /// 验证工具调用参数
        /// </summary>
        /// <returns>错误信息，null 表示验证通过</returns>
        public string ValidateToolCall(ToolCall call)
        {
            if (call == null)
            {
                return "工具调用为空";
            }

            if (string.IsNullOrEmpty(call.Name))
            {
                return "工具名称为空";
            }

            // 检查工具是否存在
            if (!toolDefinitions.TryGetValue(call.Name, out var tool))
            {
                var availableTools = string.Join(", ", toolDefinitions.Keys.Take(10));
                return $"未知工具: {call.Name}。可用工具包括: {availableTools}...";
            }

            // 检查必填参数
            if (tool.Parameters?.Required != null)
            {
                foreach (var required in tool.Parameters.Required)
                {
                    if (!call.Arguments.ContainsKey(required) || call.Arguments[required] == null)
                    {
                        return $"缺少必填参数: {required}";
                    }
                }
            }

            return null; // 验证通过
        }

        /// <summary>
        /// 执行工具调用
        /// </summary>
        public ToolResult ExecuteTool(ToolCall call)
        {
            if (call == null)
            {
                return ToolResult.Fail("工具调用为空");
            }

            // 验证参数
            var validationError = ValidateToolCall(call);
            if (validationError != null)
            {
                return ToolResult.Fail(validationError);
            }

            // 查找执行器
            if (!executors.TryGetValue(call.Name, out var executor))
            {
                return ToolResult.Fail($"未找到工具 '{call.Name}' 的执行器，请确保已注册对应的 Executor");
            }

            try
            {
                Debug.Log($"[ToolRegistry] 执行工具: {call}");
                var result = executor.Execute(call.Name, call.Arguments);
                Debug.Log($"[ToolRegistry] 执行结果: {result}");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ToolRegistry] 执行工具 {call.Name} 失败: {e}");
                return ToolResult.Fail($"执行失败: {e.Message}");
            }
        }

        /// <summary>
        /// 批量执行多个工具调用
        /// </summary>
        public List<ToolResult> ExecuteTools(List<ToolCall> calls)
        {
            var results = new List<ToolResult>();
            foreach (var call in calls)
            {
                results.Add(ExecuteTool(call));
            }
            return results;
        }

        /// <summary>
        /// 检查工具是否已注册
        /// </summary>
        public bool IsToolRegistered(string toolName)
        {
            return executors.ContainsKey(toolName);
        }

        /// <summary>
        /// 获取所有已注册的工具名称
        /// </summary>
        public string[] GetRegisteredToolNames()
        {
            return executors.Keys.ToArray();
        }

        /// <summary>
        /// 获取所有定义的工具名称
        /// </summary>
        public string[] GetAllToolNames()
        {
            return toolDefinitions.Keys.ToArray();
        }

        /// <summary>
        /// 获取工具定义
        /// </summary>
        public ToolDefinition GetToolDefinition(string toolName)
        {
            toolDefinitions.TryGetValue(toolName, out var tool);
            return tool;
        }

        /// <summary>
        /// 获取所有工具定义
        /// </summary>
        public ToolDefinition[] GetAllToolDefinitions()
        {
            return toolDefinitions.Values.ToArray();
        }

        /// <summary>
        /// 清除所有执行器（用于测试）
        /// </summary>
        public void ClearExecutors()
        {
            executors.Clear();
        }

        /// <summary>
        /// 重置单例（用于测试）
        /// </summary>
        public static void Reset()
        {
            _instance = null;
        }
    }
}
