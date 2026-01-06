using System;
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
    /// GameObject 执行器 - 处理 GameObject 的创建、查找、删除、复制和 Transform 操作
    /// </summary>
    public class GameObjectExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "create_primitive",
            "create_empty",
            "find_gameobject",
            "delete_gameobject",
            "duplicate_gameobject",
            "set_transform",
            "get_transform",
            "batch_create"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "create_primitive":
                    return CreatePrimitive(args);
                case "create_empty":
                    return CreateEmpty(args);
                case "find_gameobject":
                    return FindGameObject(args);
                case "delete_gameobject":
                    return DeleteGameObject(args);
                case "duplicate_gameobject":
                    return DuplicateGameObject(args);
                case "set_transform":
                    return SetTransform(args);
                case "get_transform":
                    return GetTransform(args);
                case "batch_create":
                    return BatchCreate(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 创建基础几何体
        /// </summary>
        private ToolResult CreatePrimitive(Dictionary<string, object> args)
        {
            // 获取参数
            var primitiveTypeStr = args.GetString("primitive_type");
            if (string.IsNullOrEmpty(primitiveTypeStr))
            {
                return ToolResult.MissingParameter("primitive_type");
            }

            // 解析 PrimitiveType
            if (!Enum.TryParse<PrimitiveType>(primitiveTypeStr, true, out var primitiveType))
            {
                return ToolResult.InvalidParameter("primitive_type",
                    $"无效类型 '{primitiveTypeStr}'，可选: Cube, Sphere, Capsule, Cylinder, Plane, Quad");
            }

            // 获取名称和位置
            var name = args.GetString("name", primitiveTypeStr);
            var position = args.GetVector3("position", Vector3.zero);

            // 创建物体
            var go = GameObject.CreatePrimitive(primitiveType);
            go.name = name;
            go.transform.position = position;

            // 注册 Undo
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");

            // 选中新创建的物体
            Selection.activeGameObject = go;

            Log($"创建 {primitiveType}: {name} 位置 {position}");

            return ToolResult.Ok($"已创建 {primitiveType}: '{name}'，位置 ({position.x}, {position.y}, {position.z})");
        }

        /// <summary>
        /// 创建空物体
        /// </summary>
        private ToolResult CreateEmpty(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            var position = args.GetVector3("position", Vector3.zero);

            var go = new GameObject(name);
            go.transform.position = position;

            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            Selection.activeGameObject = go;

            Log($"创建空物体: {name} 位置 {position}");

            return ToolResult.Ok($"已创建空物体: '{name}'，位置 ({position.x}, {position.y}, {position.z})");
        }

        /// <summary>
        /// 查找 GameObject
        /// </summary>
        private ToolResult FindGameObject(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            var go = GameObject.Find(name);
            if (go == null)
            {
                return ToolResult.NotFound(name);
            }

            // 构建信息
            var sb = new StringBuilder();
            sb.AppendLine($"找到物体: '{go.name}'");
            sb.AppendLine($"- 位置: ({go.transform.position.x:F2}, {go.transform.position.y:F2}, {go.transform.position.z:F2})");
            sb.AppendLine($"- 旋转: ({go.transform.eulerAngles.x:F2}, {go.transform.eulerAngles.y:F2}, {go.transform.eulerAngles.z:F2})");
            sb.AppendLine($"- 缩放: ({go.transform.localScale.x:F2}, {go.transform.localScale.y:F2}, {go.transform.localScale.z:F2})");
            sb.AppendLine($"- 激活状态: {go.activeSelf}");
            sb.AppendLine($"- 标签: {go.tag}");
            sb.AppendLine($"- 层级: {LayerMask.LayerToName(go.layer)}");

            // 组件列表
            var components = go.GetComponents<Component>();
            sb.AppendLine($"- 组件 ({components.Length}个):");
            foreach (var comp in components)
            {
                if (comp != null)
                {
                    sb.AppendLine($"  - {comp.GetType().Name}");
                }
            }

            // 子物体
            if (go.transform.childCount > 0)
            {
                sb.AppendLine($"- 子物体 ({go.transform.childCount}个):");
                for (int i = 0; i < Mathf.Min(go.transform.childCount, 10); i++)
                {
                    sb.AppendLine($"  - {go.transform.GetChild(i).name}");
                }
                if (go.transform.childCount > 10)
                {
                    sb.AppendLine($"  ... 还有 {go.transform.childCount - 10} 个");
                }
            }

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 删除 GameObject
        /// </summary>
        private ToolResult DeleteGameObject(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            var go = GameObject.Find(name);
            if (go == null)
            {
                return ToolResult.NotFound(name);
            }

            Undo.DestroyObjectImmediate(go);

            Log($"删除物体: {name}");

            return ToolResult.Ok($"已删除物体: '{name}'");
        }

        /// <summary>
        /// 复制 GameObject
        /// </summary>
        private ToolResult DuplicateGameObject(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            var original = GameObject.Find(name);
            if (original == null)
            {
                return ToolResult.NotFound(name);
            }

            var newName = args.GetString("new_name", name + "_Copy");

            var copy = UnityEngine.Object.Instantiate(original);
            copy.name = newName;

            Undo.RegisterCreatedObjectUndo(copy, $"Duplicate {name}");
            Selection.activeGameObject = copy;

            Log($"复制物体: {name} -> {newName}");

            return ToolResult.Ok($"已复制物体: '{name}' -> '{newName}'");
        }

        /// <summary>
        /// 设置 Transform
        /// </summary>
        private ToolResult SetTransform(Dictionary<string, object> args)
        {
            var target = args.GetString("target");
            if (string.IsNullOrEmpty(target))
            {
                return ToolResult.MissingParameter("target");
            }

            var go = GameObject.Find(target);
            if (go == null)
            {
                return ToolResult.NotFound(target);
            }

            Undo.RecordObject(go.transform, $"Set Transform {target}");

            var changed = new List<string>();

            // 设置位置
            if (args.TryGetVector3("position", out var position))
            {
                go.transform.position = position;
                changed.Add($"位置 ({position.x}, {position.y}, {position.z})");
            }

            // 设置旋转
            if (args.TryGetVector3("rotation", out var rotation))
            {
                go.transform.eulerAngles = rotation;
                changed.Add($"旋转 ({rotation.x}, {rotation.y}, {rotation.z})");
            }

            // 设置缩放
            if (args.TryGetVector3("scale", out var scale))
            {
                go.transform.localScale = scale;
                changed.Add($"缩放 ({scale.x}, {scale.y}, {scale.z})");
            }

            if (changed.Count == 0)
            {
                return ToolResult.Fail("没有指定要修改的属性 (position/rotation/scale)");
            }

            Log($"设置 Transform: {target} - {string.Join(", ", changed)}");

            return ToolResult.Ok($"已更新 '{target}' 的 Transform:\n- " + string.Join("\n- ", changed));
        }

        /// <summary>
        /// 获取 Transform 信息
        /// </summary>
        private ToolResult GetTransform(Dictionary<string, object> args)
        {
            var target = args.GetString("target");
            if (string.IsNullOrEmpty(target))
            {
                return ToolResult.MissingParameter("target");
            }

            var go = GameObject.Find(target);
            if (go == null)
            {
                return ToolResult.NotFound(target);
            }

            var t = go.transform;
            var sb = new StringBuilder();
            sb.AppendLine($"'{target}' 的 Transform:");
            sb.AppendLine($"- 位置 (World): ({t.position.x:F2}, {t.position.y:F2}, {t.position.z:F2})");
            sb.AppendLine($"- 位置 (Local): ({t.localPosition.x:F2}, {t.localPosition.y:F2}, {t.localPosition.z:F2})");
            sb.AppendLine($"- 旋转 (Euler): ({t.eulerAngles.x:F2}, {t.eulerAngles.y:F2}, {t.eulerAngles.z:F2})");
            sb.AppendLine($"- 缩放 (Local): ({t.localScale.x:F2}, {t.localScale.y:F2}, {t.localScale.z:F2})");

            if (t.parent != null)
            {
                sb.AppendLine($"- 父物体: {t.parent.name}");
            }

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 批量创建
        /// </summary>
        private ToolResult BatchCreate(Dictionary<string, object> args)
        {
            // 获取参数
            var primitiveTypeStr = args.GetString("primitive_type");
            if (string.IsNullOrEmpty(primitiveTypeStr))
            {
                return ToolResult.MissingParameter("primitive_type");
            }

            if (!Enum.TryParse<PrimitiveType>(primitiveTypeStr, true, out var primitiveType))
            {
                return ToolResult.InvalidParameter("primitive_type", $"无效类型 '{primitiveTypeStr}'");
            }

            var count = args.GetInt("count", 1);
            if (count <= 0 || count > 100)
            {
                return ToolResult.InvalidParameter("count", "数量必须在 1-100 之间");
            }

            var spacing = args.GetVector3("spacing", new Vector3(2, 0, 0));
            var startPosition = args.GetVector3("start_position", Vector3.zero);
            var namePrefix = args.GetString("name_prefix", primitiveTypeStr);

            // 创建父物体
            var parent = new GameObject($"{namePrefix}_Group");
            parent.transform.position = startPosition;
            Undo.RegisterCreatedObjectUndo(parent, $"Create {namePrefix} Group");

            var createdNames = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(primitiveType);
                var objName = $"{namePrefix}_{i + 1}";
                go.name = objName;
                go.transform.SetParent(parent.transform);
                go.transform.localPosition = spacing * i;

                Undo.RegisterCreatedObjectUndo(go, $"Create {objName}");
                createdNames.Add(objName);
            }

            Selection.activeGameObject = parent;

            Log($"批量创建 {count} 个 {primitiveType}");

            return ToolResult.Ok($"已创建 {count} 个 {primitiveType}，父物体: '{parent.name}'\n创建的物体: {string.Join(", ", createdNames)}");
        }
    }
}
