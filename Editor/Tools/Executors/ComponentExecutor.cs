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
    /// Component 执行器 - 处理组件的添加、移除和查询
    /// </summary>
    public class ComponentExecutor : ToolExecutorBase
    {
        // 支持的组件类型映射
        private static readonly Dictionary<string, Type> SupportedComponents = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            // 物理组件
            { "Rigidbody", typeof(Rigidbody) },
            { "BoxCollider", typeof(BoxCollider) },
            { "SphereCollider", typeof(SphereCollider) },
            { "CapsuleCollider", typeof(CapsuleCollider) },
            { "MeshCollider", typeof(MeshCollider) },
            { "CharacterController", typeof(CharacterController) },

            // 音频组件
            { "AudioSource", typeof(AudioSource) },
            { "AudioListener", typeof(AudioListener) },

            // 渲染组件
            { "Light", typeof(Light) },
            { "Camera", typeof(Camera) },
            { "LineRenderer", typeof(LineRenderer) },
            { "TrailRenderer", typeof(TrailRenderer) },
            { "ParticleSystem", typeof(ParticleSystem) },

            // 动画组件
            { "Animator", typeof(Animator) },
            { "Animation", typeof(Animation) },

            // UI 组件（Canvas 相关）
            { "Canvas", typeof(Canvas) },
            { "CanvasGroup", typeof(CanvasGroup) },

            // 其他常用组件
            { "Rigidbody2D", typeof(Rigidbody2D) },
            { "BoxCollider2D", typeof(BoxCollider2D) },
            { "CircleCollider2D", typeof(CircleCollider2D) },
            { "SpriteRenderer", typeof(SpriteRenderer) },
        };

        public override string[] SupportedTools => new string[]
        {
            "add_component",
            "remove_component",
            "get_components",
            "attach_script"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "add_component":
                    return AddComponent(args);
                case "remove_component":
                    return RemoveComponent(args);
                case "get_components":
                    return GetComponents(args);
                case "attach_script":
                    return AttachScript(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        private ToolResult AddComponent(Dictionary<string, object> args)
        {
            var targetName = args.GetString("target");
            if (string.IsNullOrEmpty(targetName))
            {
                return ToolResult.MissingParameter("target");
            }

            var componentType = args.GetString("component_type");
            if (string.IsNullOrEmpty(componentType))
            {
                return ToolResult.MissingParameter("component_type");
            }

            var target = GameObject.Find(targetName);
            if (target == null)
            {
                return ToolResult.NotFound(targetName);
            }

            // 查找组件类型
            if (!SupportedComponents.TryGetValue(componentType, out var type))
            {
                // 尝试通过反射查找
                type = Type.GetType($"UnityEngine.{componentType}, UnityEngine");
                if (type == null)
                {
                    var supportedList = string.Join(", ", SupportedComponents.Keys);
                    return ToolResult.InvalidParameter("component_type",
                        $"不支持的组件类型 '{componentType}'。支持的类型: {supportedList}");
                }
            }

            // 检查是否已存在
            if (target.GetComponent(type) != null)
            {
                return ToolResult.Fail($"'{targetName}' 上已存在 {componentType} 组件");
            }

            // 添加组件
            var component = Undo.AddComponent(target, type);
            if (component == null)
            {
                return ToolResult.Fail($"添加 {componentType} 组件失败");
            }

            Log($"添加组件: {targetName} <- {componentType}");

            return ToolResult.Ok($"已为 '{targetName}' 添加 {componentType} 组件");
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        private ToolResult RemoveComponent(Dictionary<string, object> args)
        {
            var targetName = args.GetString("target");
            if (string.IsNullOrEmpty(targetName))
            {
                return ToolResult.MissingParameter("target");
            }

            var componentType = args.GetString("component_type");
            if (string.IsNullOrEmpty(componentType))
            {
                return ToolResult.MissingParameter("component_type");
            }

            var target = GameObject.Find(targetName);
            if (target == null)
            {
                return ToolResult.NotFound(targetName);
            }

            // 查找组件类型
            Type type = null;
            if (SupportedComponents.TryGetValue(componentType, out type))
            {
                // OK
            }
            else
            {
                // 尝试通过反射查找
                type = Type.GetType($"UnityEngine.{componentType}, UnityEngine");
            }

            if (type == null)
            {
                // 尝试按名称查找组件
                var components = target.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp != null && comp.GetType().Name.Equals(componentType, StringComparison.OrdinalIgnoreCase))
                    {
                        Undo.DestroyObjectImmediate(comp);
                        Log($"移除组件: {targetName} -> {componentType}");
                        return ToolResult.Ok($"已从 '{targetName}' 移除 {comp.GetType().Name} 组件");
                    }
                }
                return ToolResult.Fail($"在 '{targetName}' 上未找到 {componentType} 组件");
            }

            // 禁止移除 Transform
            if (type == typeof(Transform))
            {
                return ToolResult.Fail("不能移除 Transform 组件");
            }

            var component = target.GetComponent(type);
            if (component == null)
            {
                return ToolResult.Fail($"在 '{targetName}' 上未找到 {componentType} 组件");
            }

            Undo.DestroyObjectImmediate(component);

            Log($"移除组件: {targetName} -> {componentType}");

            return ToolResult.Ok($"已从 '{targetName}' 移除 {componentType} 组件");
        }

        /// <summary>
        /// 获取组件列表
        /// </summary>
        private ToolResult GetComponents(Dictionary<string, object> args)
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

            var components = target.GetComponents<Component>();

            var sb = new StringBuilder();
            sb.AppendLine($"'{targetName}' 上的组件 ({components.Length}个):");

            foreach (var comp in components)
            {
                if (comp == null)
                {
                    sb.AppendLine("  - [Missing Script]");
                    continue;
                }

                var typeName = comp.GetType().Name;
                sb.AppendLine($"\n【{typeName}】");

                // 为常见组件显示关键属性
                switch (comp)
                {
                    case Transform t:
                        sb.AppendLine($"  Position: ({t.position.x:F2}, {t.position.y:F2}, {t.position.z:F2})");
                        sb.AppendLine($"  Rotation: ({t.eulerAngles.x:F2}, {t.eulerAngles.y:F2}, {t.eulerAngles.z:F2})");
                        sb.AppendLine($"  Scale: ({t.localScale.x:F2}, {t.localScale.y:F2}, {t.localScale.z:F2})");
                        break;

                    case Rigidbody rb:
                        sb.AppendLine($"  Mass: {rb.mass}");
                        sb.AppendLine($"  Use Gravity: {rb.useGravity}");
                        sb.AppendLine($"  Is Kinematic: {rb.isKinematic}");
                        break;

                    case Collider col:
                        sb.AppendLine($"  Is Trigger: {col.isTrigger}");
                        sb.AppendLine($"  Enabled: {col.enabled}");
                        break;

                    case Renderer renderer:
                        sb.AppendLine($"  Enabled: {renderer.enabled}");
                        if (renderer.sharedMaterial != null)
                        {
                            sb.AppendLine($"  Material: {renderer.sharedMaterial.name}");
                        }
                        break;

                    case Light light:
                        sb.AppendLine($"  Type: {light.type}");
                        sb.AppendLine($"  Color: {light.color}");
                        sb.AppendLine($"  Intensity: {light.intensity}");
                        break;

                    case Camera cam:
                        sb.AppendLine($"  FOV: {cam.fieldOfView}");
                        sb.AppendLine($"  Near: {cam.nearClipPlane}, Far: {cam.farClipPlane}");
                        break;

                    case AudioSource audio:
                        sb.AppendLine($"  Playing: {audio.isPlaying}");
                        sb.AppendLine($"  Volume: {audio.volume}");
                        if (audio.clip != null)
                        {
                            sb.AppendLine($"  Clip: {audio.clip.name}");
                        }
                        break;
                }
            }

            return ToolResult.Ok(sb.ToString());
        }

        /// <summary>
        /// 挂载用户脚本到 GameObject
        /// </summary>
        private ToolResult AttachScript(Dictionary<string, object> args)
        {
            var targetName = args.GetString("target");
            if (string.IsNullOrEmpty(targetName))
            {
                return ToolResult.MissingParameter("target");
            }

            var scriptName = args.GetString("script_name");
            if (string.IsNullOrEmpty(scriptName))
            {
                return ToolResult.MissingParameter("script_name");
            }

            // 移除 .cs 后缀（如果有）
            if (scriptName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            {
                scriptName = scriptName.Substring(0, scriptName.Length - 3);
            }

            var target = GameObject.Find(targetName);
            if (target == null)
            {
                return ToolResult.NotFound(targetName);
            }

            // 查找脚本资源
            string[] searchPaths = new[]
            {
                $"Assets/Scripts/{scriptName}.cs",
                $"Assets/AIOperator/GeneratedScripts/{scriptName}.cs",
                $"Assets/{scriptName}.cs"
            };

            MonoScript monoScript = null;
            string foundPath = null;

            // 先尝试指定路径
            foreach (var path in searchPaths)
            {
                monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (monoScript != null)
                {
                    foundPath = path;
                    break;
                }
            }

            // 如果指定路径没找到，搜索整个项目
            if (monoScript == null)
            {
                string[] guids = AssetDatabase.FindAssets($"t:MonoScript {scriptName}");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script != null && script.name.Equals(scriptName, StringComparison.OrdinalIgnoreCase))
                    {
                        monoScript = script;
                        foundPath = path;
                        break;
                    }
                }
            }

            if (monoScript == null)
            {
                return ToolResult.Fail($"未找到脚本 '{scriptName}'。请确保脚本已存在于项目中（如 Assets/Scripts/{scriptName}.cs）");
            }

            // 获取脚本的类型
            Type scriptType = monoScript.GetClass();
            if (scriptType == null)
            {
                return ToolResult.Fail($"脚本 '{scriptName}' 无法获取类型。可能存在编译错误或脚本类名与文件名不匹配");
            }

            // 检查是否为 MonoBehaviour
            if (!typeof(MonoBehaviour).IsAssignableFrom(scriptType))
            {
                return ToolResult.Fail($"脚本 '{scriptName}' 不是 MonoBehaviour，无法挂载到 GameObject");
            }

            // 检查是否已存在该组件
            if (target.GetComponent(scriptType) != null)
            {
                return ToolResult.Fail($"'{targetName}' 上已存在 {scriptName} 脚本");
            }

            // 添加脚本组件
            var component = Undo.AddComponent(target, scriptType);
            if (component == null)
            {
                return ToolResult.Fail($"挂载脚本 '{scriptName}' 失败");
            }

            Log($"挂载脚本: {targetName} <- {scriptName} ({foundPath})");

            return ToolResult.Ok($"已将脚本 '{scriptName}' 挂载到 '{targetName}'（路径: {foundPath}）");
        }
    }
}
