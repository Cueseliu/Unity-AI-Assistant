using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEditor;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// Console 执行器 - 处理 Unity 控制台日志操作
    /// </summary>
    public class ConsoleExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "get_console_logs",
            "clear_console"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "get_console_logs":
                    return GetConsoleLogs(args);
                case "clear_console":
                    return ClearConsole(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 获取控制台日志
        /// </summary>
        private ToolResult GetConsoleLogs(Dictionary<string, object> args)
        {
            var count = args.GetInt("count", 10);
            count = Mathf.Clamp(count, 1, 100);

            var typeFilter = args.GetString("type", "all").ToLower();

            try
            {
                // 使用反射访问 LogEntries 内部类
                var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor");
                if (logEntriesType == null)
                {
                    return ToolResult.Fail("无法访问 Unity 日志系统");
                }

                // 获取日志条目数量
                var getCountMethod = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public);
                if (getCountMethod == null)
                {
                    return ToolResult.Fail("无法获取日志数量方法");
                }

                int totalCount = (int)getCountMethod.Invoke(null, null);

                if (totalCount == 0)
                {
                    return ToolResult.Ok("控制台没有日志");
                }

                // 开始获取日志
                var startGettingEntriesMethod = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var endGettingEntriesMethod = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public);
                var getEntryInternalMethod = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);

                if (startGettingEntriesMethod == null || endGettingEntriesMethod == null || getEntryInternalMethod == null)
                {
                    return ToolResult.Fail("无法访问日志获取方法");
                }

                // LogEntry 类型
                var logEntryType = Type.GetType("UnityEditor.LogEntry, UnityEditor");
                if (logEntryType == null)
                {
                    return ToolResult.Fail("无法访问 LogEntry 类型");
                }

                var sb = new StringBuilder();
                sb.AppendLine($"控制台日志 (过滤: {typeFilter}):");
                sb.AppendLine("────────────────────");

                startGettingEntriesMethod.Invoke(null, null);

                var logEntry = Activator.CreateInstance(logEntryType);
                var messageField = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
                var modeField = logEntryType.GetField("mode", BindingFlags.Instance | BindingFlags.Public);

                var logs = new List<(string message, int mode)>();

                for (int i = totalCount - 1; i >= 0 && logs.Count < count * 3; i--)
                {
                    getEntryInternalMethod.Invoke(null, new object[] { i, logEntry });

                    var message = messageField.GetValue(logEntry) as string;
                    var mode = (int)modeField.GetValue(logEntry);

                    // 根据 mode 判断日志类型
                    // mode 值说明: 0-Log, 256-Warning, 512-Error
                    var logType = GetLogType(mode);

                    // 过滤日志类型
                    if (typeFilter != "all")
                    {
                        if (typeFilter == "error" && logType != "Error") continue;
                        if (typeFilter == "warning" && logType != "Warning") continue;
                        if (typeFilter == "log" && logType != "Log") continue;
                    }

                    logs.Add((message, mode));
                    if (logs.Count >= count) break;
                }

                endGettingEntriesMethod.Invoke(null, null);

                if (logs.Count == 0)
                {
                    return ToolResult.Ok($"没有找到类型为 '{typeFilter}' 的日志");
                }

                // 反转以时间顺序显示（最新的在最后）
                logs.Reverse();

                int index = 1;
                foreach (var (message, mode) in logs)
                {
                    var logType = GetLogType(mode);
                    var icon = logType switch
                    {
                        "Error" => "❌",
                        "Warning" => "⚠️",
                        _ => "ℹ️"
                    };

                    // 截断过长的消息
                    var displayMessage = message;
                    if (displayMessage.Length > 200)
                    {
                        displayMessage = displayMessage.Substring(0, 200) + "...";
                    }

                    // 去除换行
                    displayMessage = displayMessage.Replace("\n", " ").Replace("\r", "");

                    sb.AppendLine($"{index}. {icon} [{logType}] {displayMessage}");
                    index++;
                }

                sb.AppendLine("────────────────────");
                sb.AppendLine($"显示 {logs.Count} 条，共 {totalCount} 条日志");

                Log($"获取控制台日志，数量: {logs.Count}");

                return ToolResult.Ok(sb.ToString());
            }
            catch (Exception e)
            {
                LogError($"获取控制台日志失败: {e}");
                return ToolResult.Fail($"获取日志失败: {e.Message}");
            }
        }

        /// <summary>
        /// 根据 mode 获取日志类型
        /// </summary>
        private string GetLogType(int mode)
        {
            // Unity Console Log Mode:
            // 错误相关: mode & 0x100 (256) 或 mode & 0x200 (512) 或特定错误标志
            // 警告相关: mode & 0x100 但不是错误

            if ((mode & 0x200) != 0) return "Error";      // Assert 和 Exception
            if ((mode & 0x100) != 0) return "Warning";    // Warning
            return "Log";                                   // Normal log
        }

        /// <summary>
        /// 清空控制台
        /// </summary>
        private ToolResult ClearConsole(Dictionary<string, object> args)
        {
            try
            {
                var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor");
                if (logEntriesType == null)
                {
                    return ToolResult.Fail("无法访问 Unity 日志系统");
                }

                var clearMethod = logEntriesType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
                if (clearMethod == null)
                {
                    return ToolResult.Fail("无法找到清空方法");
                }

                clearMethod.Invoke(null, null);

                Log("已清空控制台");

                return ToolResult.Ok("已清空控制台");
            }
            catch (Exception e)
            {
                LogError($"清空控制台失败: {e}");
                return ToolResult.Fail($"清空失败: {e.Message}");
            }
        }
    }
}
