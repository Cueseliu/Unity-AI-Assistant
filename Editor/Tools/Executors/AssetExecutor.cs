using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// Asset 执行器 - 处理 Prefab 保存和实例化
    /// </summary>
    public class AssetExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "save_as_prefab",
            "instantiate_prefab"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "save_as_prefab":
                    return SaveAsPrefab(args);
                case "instantiate_prefab":
                    return InstantiatePrefab(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 将 GameObject 保存为 Prefab
        /// </summary>
        private ToolResult SaveAsPrefab(Dictionary<string, object> args)
        {
            var targetName = args.GetString("target");
            if (string.IsNullOrEmpty(targetName))
            {
                return ToolResult.MissingParameter("target");
            }

            var path = args.GetString("path");
            if (string.IsNullOrEmpty(path))
            {
                return ToolResult.MissingParameter("path");
            }

            // 确保路径以 .prefab 结尾
            if (!path.EndsWith(".prefab"))
            {
                path += ".prefab";
            }

            // 确保路径以 Assets/ 开头
            if (!path.StartsWith("Assets/") && !path.StartsWith("Assets\\"))
            {
                path = "Assets/" + path;
            }

            // 查找目标物体
            var go = GameObject.Find(targetName);
            if (go == null)
            {
                return ToolResult.NotFound(targetName);
            }

            // 确保目录存在
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                // 递归创建目录
                CreateFolderRecursive(directory);
            }

            // 检查是否已存在同名 Prefab
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            bool isUpdate = existingPrefab != null;

            try
            {
                GameObject prefab;
                if (isUpdate)
                {
                    // 更新现有 Prefab
                    prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                    Log($"更新 Prefab: {path}");
                }
                else
                {
                    // 创建新 Prefab
                    prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
                    Log($"创建 Prefab: {path}");
                }

                if (prefab == null)
                {
                    return ToolResult.Fail("保存 Prefab 失败");
                }

                // 注意：不调用 AssetDatabase.Refresh()
                // PrefabUtility.SaveAsPrefabAsset 已经会自动更新资产数据库
                // 额外的 Refresh 可能导致不必要的延迟

                var action = isUpdate ? "更新" : "创建";
                return ToolResult.Ok($"已{action} Prefab: '{path}'\n源物体: '{targetName}'");
            }
            catch (System.Exception e)
            {
                LogError($"保存 Prefab 失败: {e}");
                return ToolResult.Fail($"保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 实例化 Prefab
        /// </summary>
        private ToolResult InstantiatePrefab(Dictionary<string, object> args)
        {
            var path = args.GetString("path");
            if (string.IsNullOrEmpty(path))
            {
                return ToolResult.MissingParameter("path");
            }

            // 确保路径以 Assets/ 开头
            if (!path.StartsWith("Assets/") && !path.StartsWith("Assets\\"))
            {
                path = "Assets/" + path;
            }

            // 确保路径以 .prefab 结尾
            if (!path.EndsWith(".prefab"))
            {
                path += ".prefab";
            }

            // 加载 Prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                return ToolResult.NotFound($"Prefab: {path}");
            }

            // 获取位置
            var position = args.GetVector3("position", Vector3.zero);

            // 获取实例名称
            var instanceName = args.GetString("name");

            // 实例化
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (instance == null)
            {
                return ToolResult.Fail("实例化 Prefab 失败");
            }

            instance.transform.position = position;

            if (!string.IsNullOrEmpty(instanceName))
            {
                instance.name = instanceName;
            }

            // 注册 Undo
            Undo.RegisterCreatedObjectUndo(instance, $"Instantiate {prefab.name}");

            // 选中新实例
            Selection.activeGameObject = instance;

            Log($"实例化 Prefab: {path} -> {instance.name}");

            var sb = new StringBuilder();
            sb.AppendLine($"已实例化 Prefab: '{prefab.name}'");
            sb.AppendLine($"- 实例名称: {instance.name}");
            sb.AppendLine($"- 位置: ({position.x}, {position.y}, {position.z})");
            sb.AppendLine($"- Prefab 路径: {path}");

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 递归创建文件夹
        /// </summary>
        private void CreateFolderRecursive(string path)
        {
            // 规范化路径
            path = path.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parentPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parentPath))
            {
                parentPath = parentPath.Replace("\\", "/");
                CreateFolderRecursive(parentPath);
            }

            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(parentPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}
