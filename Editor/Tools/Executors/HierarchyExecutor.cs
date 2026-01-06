using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// Hierarchy 执行器 - 处理父子关系操作
    /// </summary>
    public class HierarchyExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "set_parent",
            "get_children"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "set_parent":
                    return SetParent(args);
                case "get_children":
                    return GetChildren(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 设置父物体
        /// </summary>
        private ToolResult SetParent(Dictionary<string, object> args)
        {
            var targetName = args.GetString("target");
            if (string.IsNullOrEmpty(targetName))
            {
                return ToolResult.MissingParameter("target");
            }

            var target = GameObject.Find(targetName);
            if (target == null)
            {
                return ToolResult.NotFound(targetName);
            }

            var parentName = args.GetString("parent");
            var worldPositionStays = args.GetBool("world_position_stays", true);

            Transform newParent = null;
            if (!string.IsNullOrEmpty(parentName))
            {
                var parentGo = GameObject.Find(parentName);
                if (parentGo == null)
                {
                    return ToolResult.Fail($"未找到父物体: '{parentName}'");
                }
                newParent = parentGo.transform;

                // 检查循环引用
                if (IsChildOf(newParent, target.transform))
                {
                    return ToolResult.Fail($"不能将 '{parentName}' 设为 '{targetName}' 的父物体，因为这会造成循环引用");
                }
            }

            Undo.SetTransformParent(target.transform, newParent, $"Set Parent of {targetName}");
            target.transform.SetParent(newParent, worldPositionStays);

            if (newParent == null)
            {
                Log($"将 {targetName} 移到根层级");
                return ToolResult.Ok($"已将 '{targetName}' 移到根层级");
            }
            else
            {
                Log($"设置父物体: {targetName} -> {parentName}");
                return ToolResult.Ok($"已将 '{targetName}' 设为 '{parentName}' 的子物体");
            }
        }

        /// <summary>
        /// 检查是否是子物体
        /// </summary>
        private bool IsChildOf(Transform potentialChild, Transform potentialParent)
        {
            var current = potentialChild;
            while (current != null)
            {
                if (current == potentialParent)
                {
                    return true;
                }
                current = current.parent;
            }
            return false;
        }

        /// <summary>
        /// 获取子物体
        /// </summary>
        private ToolResult GetChildren(Dictionary<string, object> args)
        {
            var targetName = args.GetString("target");
            if (string.IsNullOrEmpty(targetName))
            {
                return ToolResult.MissingParameter("target");
            }

            var target = GameObject.Find(targetName);
            if (target == null)
            {
                return ToolResult.NotFound(targetName);
            }

            var recursive = args.GetBool("recursive", false);

            var sb = new StringBuilder();

            if (target.transform.childCount == 0)
            {
                return ToolResult.Ok($"'{targetName}' 没有子物体");
            }

            if (recursive)
            {
                sb.AppendLine($"'{targetName}' 的所有后代物体:");
                BuildChildrenTreeRecursive(target.transform, sb, 0, 100);
            }
            else
            {
                sb.AppendLine($"'{targetName}' 的直接子物体 ({target.transform.childCount}个):");
                for (int i = 0; i < target.transform.childCount; i++)
                {
                    var child = target.transform.GetChild(i);
                    sb.AppendLine($"  - {child.name}");
                }
            }

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 递归构建子物体树
        /// </summary>
        private int BuildChildrenTreeRecursive(Transform parent, StringBuilder sb, int depth, int maxCount)
        {
            int count = 0;
            string indent = new string(' ', (depth + 1) * 2);

            for (int i = 0; i < parent.childCount && count < maxCount; i++)
            {
                var child = parent.GetChild(i);
                sb.AppendLine($"{indent}- {child.name}");
                count++;

                if (child.childCount > 0)
                {
                    count += BuildChildrenTreeRecursive(child, sb, depth + 1, maxCount - count);
                }
            }

            return count;
        }
    }
}
