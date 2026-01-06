using System.Collections.Generic;
using AIOperator.LLM;

namespace AIOperator.Editor.Tools.Core
{
    /// <summary>
    /// 工具执行器接口
    /// 每个 Executor 负责一组相关的工具
    /// </summary>
    public interface IToolExecutor
    {
        /// <summary>
        /// 获取此执行器支持的工具名称列表
        /// </summary>
        string[] SupportedTools { get; }

        /// <summary>
        /// 执行指定的工具
        /// </summary>
        /// <param name="toolName">工具名称</param>
        /// <param name="arguments">工具参数</param>
        /// <returns>执行结果</returns>
        ToolResult Execute(string toolName, Dictionary<string, object> arguments);
    }

    /// <summary>
    /// 工具执行器基类
    /// 提供通用功能
    /// </summary>
    public abstract class ToolExecutorBase : IToolExecutor
    {
        /// <summary>
        /// 获取此执行器支持的工具名称列表
        /// </summary>
        public abstract string[] SupportedTools { get; }

        /// <summary>
        /// 执行指定的工具
        /// </summary>
        public abstract ToolResult Execute(string toolName, Dictionary<string, object> arguments);

        /// <summary>
        /// 检查必填参数是否存在
        /// </summary>
        protected bool HasRequiredParam(Dictionary<string, object> args, string paramName, out string error)
        {
            if (!args.ContainsKey(paramName) || args[paramName] == null)
            {
                error = $"缺少必填参数: {paramName}";
                return false;
            }
            error = null;
            return true;
        }

        /// <summary>
        /// 检查多个必填参数
        /// </summary>
        protected bool CheckRequiredParams(Dictionary<string, object> args, string[] paramNames, out string error)
        {
            foreach (var name in paramNames)
            {
                if (!HasRequiredParam(args, name, out error))
                {
                    return false;
                }
            }
            error = null;
            return true;
        }

        /// <summary>
        /// 输出调试日志
        /// </summary>
        protected void Log(string message)
        {
            UnityEngine.Debug.Log($"[{GetType().Name}] {message}");
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        protected void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"[{GetType().Name}] {message}");
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        protected void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[{GetType().Name}] {message}");
        }
    }
}
