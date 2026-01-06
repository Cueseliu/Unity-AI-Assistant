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
    /// Selection 执行器 - 处理 Unity 编辑器选择操作
    /// </summary>
    public class SelectionExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "get_selection",
            "select_gameobject"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "get_selection":
                    return GetSelection(args);
                case "select_gameobject":
                    return SelectGameObject(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 获取当前选中的物体
        /// </summary>
        private ToolResult GetSelection(Dictionary<string, object> args)
        {
            var selectedObjects = Selection.gameObjects;

            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                return ToolResult.Ok("当前没有选中任何物体");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"当前选中 {selectedObjects.Length} 个物体:");

            foreach (var go in selectedObjects)
            {
                if (go == null) continue;

                sb.AppendLine($"\n【{go.name}】");
                sb.AppendLine($"  - 位置: ({go.transform.position.x:F2}, {go.transform.position.y:F2}, {go.transform.position.z:F2})");
                sb.AppendLine($"  - 激活: {go.activeSelf}");

                // 列出主要组件
                var components = go.GetComponents<Component>();
                if (components.Length > 1) // 排除 Transform
                {
                    var compNames = new List<string>();
                    foreach (var comp in components)
                    {
                        if (comp != null && !(comp is Transform))
                        {
                            compNames.Add(comp.GetType().Name);
                        }
                    }
                    if (compNames.Count > 0)
                    {
                        sb.AppendLine($"  - 组件: {string.Join(", ", compNames)}");
                    }
                }
            }

            // 如果有活动选中对象
            if (Selection.activeGameObject != null)
            {
                sb.AppendLine($"\n活动选中对象: {Selection.activeGameObject.name}");
            }

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 选中指定的物体
        /// </summary>
        private ToolResult SelectGameObject(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            // 尝试精确匹配
            var go = GameObject.Find(name);

            if (go == null)
            {
                // 尝试在所有场景物体中搜索（包括非激活的）
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name == name && obj.scene.isLoaded)
                    {
                        go = obj;
                        break;
                    }
                }
            }

            if (go == null)
            {
                // 尝试部分匹配
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                var nameLower = name.ToLower();
                foreach (var obj in allObjects)
                {
                    if (obj.name.ToLower().Contains(nameLower) && obj.scene.isLoaded)
                    {
                        go = obj;
                        Log($"部分匹配: '{name}' -> '{obj.name}'");
                        break;
                    }
                }
            }

            if (go == null)
            {
                return ToolResult.NotFound(name);
            }

            // 选中物体
            Selection.activeGameObject = go;

            // 聚焦到 Scene 视图
            SceneView.lastActiveSceneView?.FrameSelected();

            Log($"选中物体: {go.name}");

            return ToolResult.Ok($"已选中物体: '{go.name}'");
        }
    }
}
