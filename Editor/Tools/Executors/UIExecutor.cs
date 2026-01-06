using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// UI 工具执行器 - 处理 UI 相关操作
    /// </summary>
    public class UIExecutor : ToolExecutorBase
    {
        // 缓存字体引用
        private static Font cachedFont;

        public override string[] SupportedTools => new[]
        {
            "create_canvas",
            "create_button",
            "create_text",
            "create_image",
            "create_slider",
            "create_panel"
        };

        /// <summary>
        /// 获取 UI 字体（支持中文）
        /// 优先从项目加载中文字体，回退到系统字体
        /// </summary>
        private static Font GetUIFont()
        {
            if (cachedFont != null)
                return cachedFont;

            // 方式1：尝试通过 AssetDatabase 直接加载项目中的字体
            string[] assetPaths = new[]
            {
                "Assets/Font/Noto_Sans_SC/static/NotoSansSC-Regular.ttf",
                "Assets/Font/Noto_Sans_SC/NotoSansSC-VariableFont_wght.ttf",
                "Assets/Resources/Fonts/NotoSansSC-Regular.ttf",
                "Assets/Fonts/NotoSansSC-Regular.ttf"
            };

            foreach (var path in assetPaths)
            {
                Font font = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(path);
                if (font != null)
                {
                    cachedFont = font;
                    Debug.Log($"[UIExecutor] 加载中文字体: {path}");
                    return cachedFont;
                }
            }

            // 方式2：尝试从 Resources 加载
            string[] resourceNames = new[]
            {
                "Fonts/NotoSansSC-Regular",
                "Fonts/ChineseFont"
            };

            foreach (var fontName in resourceNames)
            {
                Font font = Resources.Load<Font>(fontName);
                if (font != null)
                {
                    cachedFont = font;
                    Debug.Log($"[UIExecutor] 从 Resources 加载字体: {fontName}");
                    return cachedFont;
                }
            }

            // 方式3：回退到系统中文字体
            string[] systemFonts = new[]
            {
                "Microsoft YaHei",  // 微软雅黑 (Windows)
                "SimHei",           // 黑体 (Windows)
                "PingFang SC",      // 苹方 (macOS)
                "Heiti SC",         // 黑体-简 (macOS)
                "Noto Sans CJK SC"  // Linux
            };

            foreach (var fontName in systemFonts)
            {
                Font font = Font.CreateDynamicFontFromOSFont(fontName, 14);
                if (font != null)
                {
                    cachedFont = font;
                    Debug.Log($"[UIExecutor] 使用系统字体: {fontName}");
                    return cachedFont;
                }
            }

            // 最终回退到 Unity 内置字体
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            Debug.LogWarning("[UIExecutor] 未找到中文字体，使用 Arial（不支持中文）");
            return cachedFont;
        }

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "create_canvas":
                    return CreateCanvas(args);
                case "create_button":
                    return CreateButton(args);
                case "create_text":
                    return CreateText(args);
                case "create_image":
                    return CreateImage(args);
                case "create_slider":
                    return CreateSlider(args);
                case "create_panel":
                    return CreatePanel(args);
                default:
                    return ToolResult.Fail($"未知的 UI 工具: {toolName}");
            }
        }

        /// <summary>
        /// 创建 Canvas（如果不存在）
        /// </summary>
        private ToolResult CreateCanvas(Dictionary<string, object> args)
        {
            string name = args.GetString("name", "Canvas");

            // 检查是否已存在 Canvas
            Canvas existingCanvas = Object.FindObjectOfType<Canvas>();
            if (existingCanvas != null)
            {
                return ToolResult.Ok($"场景中已存在 Canvas: {existingCanvas.gameObject.name}");
            }

            // 创建 Canvas
            GameObject canvasObj = new GameObject(name);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建 EventSystem（如果不存在）
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            Selection.activeGameObject = canvasObj;

            return ToolResult.Ok($"已创建 Canvas: {name}，包含 CanvasScaler 和 GraphicRaycaster");
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        private ToolResult CreateButton(Dictionary<string, object> args)
        {
            string name = args.GetString("name", "Button");
            string text = args.GetString("text", "Button");
            float[] position = args.GetFloatArray("position");

            // 确保有 Canvas
            Canvas canvas = EnsureCanvas();
            if (canvas == null)
            {
                return ToolResult.Fail("无法创建或找到 Canvas");
            }

            // 创建按钮
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(canvas.transform, false);

            // 添加 RectTransform
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 40);

            if (position != null && position.Length >= 2)
            {
                rectTransform.anchoredPosition = new Vector2(position[0], position[1]);
            }

            // 添加 Image 组件
            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 添加 Button 组件
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            button.colors = colors;

            // 创建文本子物体
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = GetUIFont();
            textComponent.fontSize = 18;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;

            Undo.RegisterCreatedObjectUndo(buttonObj, "Create Button");
            Selection.activeGameObject = buttonObj;

            return ToolResult.Ok($"已创建按钮: {name}，文字: \"{text}\"");
        }

        /// <summary>
        /// 创建文本
        /// </summary>
        private ToolResult CreateText(Dictionary<string, object> args)
        {
            string name = args.GetString("name", "Text");
            string content = args.GetString("content", "New Text");
            int fontSize = args.GetInt("font_size", 24);
            float[] position = args.GetFloatArray("position");

            Canvas canvas = EnsureCanvas();
            if (canvas == null)
            {
                return ToolResult.Fail("无法创建或找到 Canvas");
            }

            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 50);

            if (position != null && position.Length >= 2)
            {
                rectTransform.anchoredPosition = new Vector2(position[0], position[1]);
            }

            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = content;
            textComponent.font = GetUIFont();
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;

            Undo.RegisterCreatedObjectUndo(textObj, "Create Text");
            Selection.activeGameObject = textObj;

            return ToolResult.Ok($"已创建文本: {name}，内容: \"{content}\"");
        }

        /// <summary>
        /// 创建图片
        /// </summary>
        private ToolResult CreateImage(Dictionary<string, object> args)
        {
            string name = args.GetString("name", "Image");
            string colorStr = args.GetString("color", "white");
            float[] size = args.GetFloatArray("size");
            float[] position = args.GetFloatArray("position");

            Canvas canvas = EnsureCanvas();
            if (canvas == null)
            {
                return ToolResult.Fail("无法创建或找到 Canvas");
            }

            GameObject imageObj = new GameObject(name);
            imageObj.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = imageObj.AddComponent<RectTransform>();

            if (size != null && size.Length >= 2)
            {
                rectTransform.sizeDelta = new Vector2(size[0], size[1]);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(100, 100);
            }

            if (position != null && position.Length >= 2)
            {
                rectTransform.anchoredPosition = new Vector2(position[0], position[1]);
            }

            Image image = imageObj.AddComponent<Image>();
            if (Utils.ColorHelper.TryParseColor(colorStr, out Color parsedColor))
            {
                image.color = parsedColor;
            }
            else
            {
                image.color = Color.white;
            }

            Undo.RegisterCreatedObjectUndo(imageObj, "Create Image");
            Selection.activeGameObject = imageObj;

            return ToolResult.Ok($"已创建图片: {name}，颜色: {colorStr}");
        }

        /// <summary>
        /// 创建滑动条（可用作血条）
        /// </summary>
        private ToolResult CreateSlider(Dictionary<string, object> args)
        {
            string name = args.GetString("name", "Slider");
            string sliderType = args.GetString("type", "default"); // "default" or "health_bar"
            float[] position = args.GetFloatArray("position");
            float value = args.GetFloat("value", 1f);

            Canvas canvas = EnsureCanvas();
            if (canvas == null)
            {
                return ToolResult.Fail("无法创建或找到 Canvas");
            }

            // 创建 Slider 根物体
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(canvas.transform, false);

            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.sizeDelta = new Vector2(200, 20);

            if (position != null && position.Length >= 2)
            {
                sliderRect.anchoredPosition = new Vector2(position[0], position[1]);
            }

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = value;

            // 创建背景
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // 创建填充区域
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);

            // 创建填充
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();

            // 根据类型设置颜色
            if (sliderType == "health_bar")
            {
                fillImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // 红色血条
                slider.interactable = false; // 血条不可交互
            }
            else
            {
                fillImage.color = new Color(0.3f, 0.6f, 0.9f, 1f); // 蓝色默认
            }

            // 配置 Slider
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;

            Undo.RegisterCreatedObjectUndo(sliderObj, "Create Slider");
            Selection.activeGameObject = sliderObj;

            string typeDesc = sliderType == "health_bar" ? "血条" : "滑动条";
            return ToolResult.Ok($"已创建{typeDesc}: {name}，当前值: {value}");
        }

        /// <summary>
        /// 创建面板
        /// </summary>
        private ToolResult CreatePanel(Dictionary<string, object> args)
        {
            string name = args.GetString("name", "Panel");
            string colorStr = args.GetString("color", "gray");
            float[] size = args.GetFloatArray("size");
            float[] position = args.GetFloatArray("position");

            Canvas canvas = EnsureCanvas();
            if (canvas == null)
            {
                return ToolResult.Fail("无法创建或找到 Canvas");
            }

            GameObject panelObj = new GameObject(name);
            panelObj.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();

            if (size != null && size.Length >= 2)
            {
                rectTransform.sizeDelta = new Vector2(size[0], size[1]);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(300, 200);
            }

            if (position != null && position.Length >= 2)
            {
                rectTransform.anchoredPosition = new Vector2(position[0], position[1]);
            }

            Image image = panelObj.AddComponent<Image>();
            Color color;
            if (Utils.ColorHelper.TryParseColor(colorStr, out color))
            {
                color.a = 0.8f; // 半透明
            }
            else
            {
                color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 默认灰色半透明
            }
            image.color = color;

            Undo.RegisterCreatedObjectUndo(panelObj, "Create Panel");
            Selection.activeGameObject = panelObj;

            return ToolResult.Ok($"已创建面板: {name}，尺寸: {rectTransform.sizeDelta}");
        }

        /// <summary>
        /// 确保场景中有 Canvas
        /// </summary>
        private Canvas EnsureCanvas()
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }

            // 创建新的 Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 创建 EventSystem
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            Debug.Log("[UIExecutor] 自动创建了 Canvas 和 EventSystem");

            return canvas;
        }
    }
}
