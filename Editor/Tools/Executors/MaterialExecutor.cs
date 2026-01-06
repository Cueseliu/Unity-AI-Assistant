using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// Material 执行器 - 处理材质操作
    /// </summary>
    public class MaterialExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "set_material_color"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "set_material_color":
                    return SetMaterialColor(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 设置材质颜色
        /// </summary>
        private ToolResult SetMaterialColor(Dictionary<string, object> args)
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

            // 获取 Renderer
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return ToolResult.Fail($"'{targetName}' 没有 Renderer 组件，无法设置颜色");
            }

            // 解析颜色
            Color color;
            var colorValue = args.ContainsKey("color") ? args["color"] : null;

            if (colorValue is string colorString)
            {
                if (!ColorHelper.TryParseColor(colorString, out color))
                {
                    var colorNames = string.Join(", ", ColorHelper.GetColorNames());
                    return ToolResult.InvalidParameter("color",
                        $"无法解析颜色 '{colorString}'。支持的颜色名称: {colorNames}，或使用 hex 格式如 #FF0000");
                }
            }
            else if (args.TryGetColor("color", out color))
            {
                // OK
            }
            else
            {
                return ToolResult.MissingParameter("color");
            }

            // 创建新材质实例（避免修改共享材质）
            var originalMaterial = renderer.sharedMaterial;
            var newMaterial = new Material(originalMaterial);
            newMaterial.name = $"{originalMaterial.name}_{targetName}_Instance";

            // 设置颜色
            // 尝试不同的颜色属性名
            bool colorSet = false;

            // 标准颜色属性
            if (newMaterial.HasProperty("_Color"))
            {
                newMaterial.SetColor("_Color", color);
                colorSet = true;
            }

            // URP/HDRP BaseColor
            if (newMaterial.HasProperty("_BaseColor"))
            {
                newMaterial.SetColor("_BaseColor", color);
                colorSet = true;
            }

            // 发射颜色（如果是发光材质）
            if (newMaterial.HasProperty("_EmissionColor") && color != Color.black)
            {
                // 可选：设置发射颜色
            }

            if (!colorSet)
            {
                // 强制设置主颜色
                newMaterial.color = color;
                colorSet = true;
            }

            // 记录 Undo
            Undo.RecordObject(renderer, $"Set Color of {targetName}");

            // 应用新材质
            renderer.material = newMaterial;

            var colorName = ColorHelper.ColorToString(color);
            Log($"设置颜色: {targetName} -> {colorName}");

            return ToolResult.Ok($"已将 '{targetName}' 的颜色设为 {colorName}");
        }
    }
}
