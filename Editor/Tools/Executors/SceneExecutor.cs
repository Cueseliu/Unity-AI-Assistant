using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// Scene 执行器 - 处理场景层级结构和信息查询
    /// </summary>
    public class SceneExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "get_scene_hierarchy",
            "get_scene_info",
            "analyze_scene"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "get_scene_hierarchy":
                    return GetSceneHierarchy(args);
                case "get_scene_info":
                    return GetSceneInfo(args);
                case "analyze_scene":
                    return AnalyzeScene(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 获取场景层级结构
        /// </summary>
        private ToolResult GetSceneHierarchy(Dictionary<string, object> args)
        {
            var maxDepth = args.GetInt("max_depth", 3);
            maxDepth = Mathf.Clamp(maxDepth, 1, 10);

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return ToolResult.Fail("当前没有有效的场景");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"场景: {scene.name}");
            sb.AppendLine("层级结构:");
            sb.AppendLine("────────────────────");

            var rootObjects = scene.GetRootGameObjects();
            int totalObjects = 0;

            foreach (var root in rootObjects)
            {
                totalObjects += BuildHierarchyTree(sb, root, 0, maxDepth, ref totalObjects);
            }

            sb.AppendLine("────────────────────");
            sb.AppendLine($"总计: {rootObjects.Length} 个根物体");

            Log($"获取场景层级结构，深度: {maxDepth}");

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 递归构建层级树
        /// </summary>
        private int BuildHierarchyTree(StringBuilder sb, GameObject go, int depth, int maxDepth, ref int count)
        {
            if (depth > maxDepth) return 0;

            count++;
            var indent = new string(' ', depth * 2);
            var hasChildren = go.transform.childCount > 0;
            var prefix = hasChildren ? "▼ " : "  ";

            // 构建物体信息
            var info = new StringBuilder();
            info.Append($"{indent}{prefix}{go.name}");

            // 添加关键组件标记
            var components = go.GetComponents<Component>();
            var markers = new List<string>();

            foreach (var comp in components)
            {
                if (comp == null) continue;
                var typeName = comp.GetType().Name;

                // 只标记重要组件
                if (typeName == "Camera") markers.Add("[Camera]");
                else if (typeName == "Light") markers.Add("[Light]");
                else if (typeName == "AudioSource") markers.Add("[Audio]");
                else if (typeName == "Canvas") markers.Add("[Canvas]");
                else if (typeName == "Animator") markers.Add("[Animator]");
                else if (typeName == "Rigidbody") markers.Add("[Rigidbody]");
                else if (typeName == "CharacterController") markers.Add("[CharCtrl]");
            }

            if (markers.Count > 0)
            {
                info.Append($" {string.Join(" ", markers)}");
            }

            // 如果物体未激活，标记
            if (!go.activeSelf)
            {
                info.Append(" (inactive)");
            }

            sb.AppendLine(info.ToString());

            // 递归处理子物体
            int childCount = 0;
            if (depth < maxDepth)
            {
                for (int i = 0; i < go.transform.childCount; i++)
                {
                    childCount += BuildHierarchyTree(sb, go.transform.GetChild(i).gameObject, depth + 1, maxDepth, ref count);
                }
            }
            else if (go.transform.childCount > 0)
            {
                // 超过最大深度，显示子物体数量
                var childIndent = new string(' ', (depth + 1) * 2);
                sb.AppendLine($"{childIndent}... ({go.transform.childCount} 个子物体)");
            }

            return 1 + childCount;
        }

        /// <summary>
        /// 获取场景基本信息
        /// </summary>
        private ToolResult GetSceneInfo(Dictionary<string, object> args)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return ToolResult.Fail("当前没有有效的场景");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"场景信息:");
            sb.AppendLine($"- 名称: {scene.name}");
            sb.AppendLine($"- 路径: {scene.path}");
            sb.AppendLine($"- 已加载: {scene.isLoaded}");
            sb.AppendLine($"- 已修改: {scene.isDirty}");
            sb.AppendLine($"- Build Index: {scene.buildIndex}");

            // 统计物体数量
            var rootObjects = scene.GetRootGameObjects();
            int totalObjects = CountAllObjects(rootObjects);

            sb.AppendLine($"- 根物体数量: {rootObjects.Length}");
            sb.AppendLine($"- 总物体数量: {totalObjects}");

            // 统计各类组件
            var componentCounts = new Dictionary<string, int>();
            foreach (var root in rootObjects)
            {
                CountComponents(root, componentCounts);
            }

            if (componentCounts.Count > 0)
            {
                sb.AppendLine("- 组件统计:");

                // 按数量排序，显示前 10 个
                var sorted = new List<KeyValuePair<string, int>>(componentCounts);
                sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

                int shown = 0;
                foreach (var kvp in sorted)
                {
                    if (shown >= 10) break;
                    sb.AppendLine($"  - {kvp.Key}: {kvp.Value}");
                    shown++;
                }

                if (sorted.Count > 10)
                {
                    sb.AppendLine($"  ... 还有 {sorted.Count - 10} 种组件");
                }
            }

            // 查找相机
            var cameras = Object.FindObjectsOfType<Camera>();
            if (cameras.Length > 0)
            {
                sb.AppendLine($"- 相机 ({cameras.Length} 个):");
                foreach (var cam in cameras)
                {
                    sb.AppendLine($"  - {cam.name} (depth: {cam.depth})");
                }
            }

            // 查找灯光
            var lights = Object.FindObjectsOfType<Light>();
            if (lights.Length > 0)
            {
                sb.AppendLine($"- 灯光 ({lights.Length} 个):");
                foreach (var light in lights)
                {
                    sb.AppendLine($"  - {light.name} ({light.type})");
                }
            }

            Log("获取场景信息");

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 递归统计所有物体
        /// </summary>
        private int CountAllObjects(GameObject[] roots)
        {
            int count = 0;
            foreach (var root in roots)
            {
                count += CountObjectsRecursive(root);
            }
            return count;
        }

        private int CountObjectsRecursive(GameObject go)
        {
            int count = 1;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                count += CountObjectsRecursive(go.transform.GetChild(i).gameObject);
            }
            return count;
        }

        /// <summary>
        /// 递归统计组件
        /// </summary>
        private void CountComponents(GameObject go, Dictionary<string, int> counts)
        {
            var components = go.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null) continue;
                var typeName = comp.GetType().Name;

                if (counts.ContainsKey(typeName))
                {
                    counts[typeName]++;
                }
                else
                {
                    counts[typeName] = 1;
                }
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                CountComponents(go.transform.GetChild(i).gameObject, counts);
            }
        }

        /// <summary>
        /// 分析场景并进行健康检查
        /// </summary>
        private ToolResult AnalyzeScene(Dictionary<string, object> args)
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return ToolResult.Fail("当前没有有效的场景");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"=== 场景分析报告: {scene.name} ===");
            sb.AppendLine();

            var warnings = new List<string>();
            var suggestions = new List<string>();
            var errors = new List<string>();

            // 1. 检查相机
            var cameras = Object.FindObjectsOfType<Camera>();
            if (cameras.Length == 0)
            {
                errors.Add("场景中没有相机！游戏运行时将无法看到任何内容。");
                suggestions.Add("使用 create_empty 创建空物体，然后添加 Camera 组件，或创建一个 Cube 并添加 Camera。");
            }
            else
            {
                sb.AppendLine($"相机: {cameras.Length} 个");
                foreach (var cam in cameras)
                {
                    sb.AppendLine($"  - {cam.name} (depth: {cam.depth}, tag: {cam.tag})");
                }
            }

            // 2. 检查灯光
            var lights = Object.FindObjectsOfType<Light>();
            if (lights.Length == 0)
            {
                warnings.Add("场景中没有灯光，物体可能显示为黑色。");
                suggestions.Add("考虑添加 Directional Light 作为主光源。");
            }
            else
            {
                sb.AppendLine($"灯光: {lights.Length} 个");
                bool hasDirectional = false;
                foreach (var light in lights)
                {
                    sb.AppendLine($"  - {light.name} ({light.type}, intensity: {light.intensity})");
                    if (light.type == LightType.Directional) hasDirectional = true;
                }
                if (!hasDirectional)
                {
                    warnings.Add("没有 Directional Light，整体照明可能不均匀。");
                }
            }

            // 3. 检查丢失的脚本
            var rootObjects = scene.GetRootGameObjects();
            var missingScripts = new List<string>();
            CheckMissingScripts(rootObjects, missingScripts);
            if (missingScripts.Count > 0)
            {
                errors.Add($"发现 {missingScripts.Count} 个物体有丢失的脚本引用！");
                foreach (var name in missingScripts)
                {
                    sb.AppendLine($"  - 丢失脚本: {name}");
                }
                suggestions.Add("检查这些物体并移除丢失的脚本组件，或重新添加正确的脚本。");
            }

            // 4. 检查未激活的重要物体
            var inactiveCameras = new List<string>();
            var inactiveLights = new List<string>();
            foreach (var cam in cameras)
            {
                if (!cam.gameObject.activeInHierarchy)
                {
                    inactiveCameras.Add(cam.name);
                }
            }
            foreach (var light in lights)
            {
                if (!light.gameObject.activeInHierarchy)
                {
                    inactiveLights.Add(light.name);
                }
            }

            if (inactiveCameras.Count > 0)
            {
                warnings.Add($"有 {inactiveCameras.Count} 个相机未激活: {string.Join(", ", inactiveCameras)}");
            }
            if (inactiveLights.Count > 0)
            {
                warnings.Add($"有 {inactiveLights.Count} 个灯光未激活: {string.Join(", ", inactiveLights)}");
            }

            // 5. 检查 EventSystem（如果有 Canvas）
            var canvases = Object.FindObjectsOfType<UnityEngine.Canvas>();
            if (canvases.Length > 0)
            {
                sb.AppendLine($"UI Canvas: {canvases.Length} 个");
                var eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem == null)
                {
                    warnings.Add("场景有 Canvas 但没有 EventSystem，UI 交互将无法工作！");
                    suggestions.Add("创建 EventSystem 物体并添加 EventSystem 和 StandaloneInputModule 组件。");
                }
            }

            // 6. 检查 Rigidbody 但没有 Collider
            var rigidbodies = Object.FindObjectsOfType<Rigidbody>();
            foreach (var rb in rigidbodies)
            {
                var collider = rb.GetComponent<Collider>();
                if (collider == null)
                {
                    warnings.Add($"物体 '{rb.name}' 有 Rigidbody 但没有 Collider，可能导致物理行为异常。");
                }
            }

            // 7. 统计信息
            int totalObjects = CountAllObjects(rootObjects);
            sb.AppendLine();
            sb.AppendLine($"统计:");
            sb.AppendLine($"  - 总物体数: {totalObjects}");
            sb.AppendLine($"  - 根物体数: {rootObjects.Length}");

            // 输出问题列表
            sb.AppendLine();
            if (errors.Count > 0)
            {
                sb.AppendLine("=== 错误 ===");
                foreach (var error in errors)
                {
                    sb.AppendLine($"❌ {error}");
                }
            }

            if (warnings.Count > 0)
            {
                sb.AppendLine("=== 警告 ===");
                foreach (var warning in warnings)
                {
                    sb.AppendLine($"⚠️ {warning}");
                }
            }

            if (suggestions.Count > 0)
            {
                sb.AppendLine("=== 建议 ===");
                foreach (var suggestion in suggestions)
                {
                    sb.AppendLine($"💡 {suggestion}");
                }
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                sb.AppendLine("✅ 场景健康检查通过！没有发现问题。");
            }

            Log("场景分析完成");
            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 检查丢失的脚本
        /// </summary>
        private void CheckMissingScripts(GameObject[] roots, List<string> missingList)
        {
            foreach (var root in roots)
            {
                CheckMissingScriptsRecursive(root, missingList);
            }
        }

        private void CheckMissingScriptsRecursive(GameObject go, List<string> missingList)
        {
            var components = go.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp == null)
                {
                    missingList.Add(go.name);
                    break; // 每个物体只记录一次
                }
            }

            for (int i = 0; i < go.transform.childCount; i++)
            {
                CheckMissingScriptsRecursive(go.transform.GetChild(i).gameObject, missingList);
            }
        }
    }
}
