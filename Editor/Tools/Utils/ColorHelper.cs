using System.Collections.Generic;
using UnityEngine;

namespace AIOperator.Editor.Tools.Utils
{
    /// <summary>
    /// 颜色解析辅助类
    /// 支持颜色名称、hex 格式等
    /// </summary>
    public static class ColorHelper
    {
        /// <summary>
        /// 预定义的颜色名称映射
        /// </summary>
        private static readonly Dictionary<string, Color> ColorNames = new Dictionary<string, Color>
        {
            // 基础颜色
            { "red", Color.red },
            { "green", Color.green },
            { "blue", Color.blue },
            { "yellow", Color.yellow },
            { "white", Color.white },
            { "black", Color.black },
            { "gray", Color.gray },
            { "grey", Color.grey },
            { "cyan", Color.cyan },
            { "magenta", Color.magenta },
            { "clear", Color.clear },

            // 扩展颜色
            { "orange", new Color(1f, 0.5f, 0f) },
            { "purple", new Color(0.5f, 0f, 0.5f) },
            { "pink", new Color(1f, 0.75f, 0.8f) },
            { "brown", new Color(0.6f, 0.3f, 0f) },
            { "gold", new Color(1f, 0.84f, 0f) },
            { "silver", new Color(0.75f, 0.75f, 0.75f) },
            { "navy", new Color(0f, 0f, 0.5f) },
            { "teal", new Color(0f, 0.5f, 0.5f) },
            { "olive", new Color(0.5f, 0.5f, 0f) },
            { "maroon", new Color(0.5f, 0f, 0f) },
            { "lime", new Color(0f, 1f, 0f) },
            { "aqua", new Color(0f, 1f, 1f) },
            { "fuchsia", new Color(1f, 0f, 1f) },

            // 中文颜色名
            { "红", Color.red },
            { "红色", Color.red },
            { "绿", Color.green },
            { "绿色", Color.green },
            { "蓝", Color.blue },
            { "蓝色", Color.blue },
            { "黄", Color.yellow },
            { "黄色", Color.yellow },
            { "白", Color.white },
            { "白色", Color.white },
            { "黑", Color.black },
            { "黑色", Color.black },
            { "灰", Color.gray },
            { "灰色", Color.gray },
            { "橙", new Color(1f, 0.5f, 0f) },
            { "橙色", new Color(1f, 0.5f, 0f) },
            { "紫", new Color(0.5f, 0f, 0.5f) },
            { "紫色", new Color(0.5f, 0f, 0.5f) },
            { "粉", new Color(1f, 0.75f, 0.8f) },
            { "粉色", new Color(1f, 0.75f, 0.8f) },
            { "棕", new Color(0.6f, 0.3f, 0f) },
            { "棕色", new Color(0.6f, 0.3f, 0f) },
            { "金", new Color(1f, 0.84f, 0f) },
            { "金色", new Color(1f, 0.84f, 0f) },
        };

        /// <summary>
        /// 尝试解析颜色字符串
        /// </summary>
        /// <param name="colorString">颜色名称或 hex 格式</param>
        /// <param name="result">解析结果</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseColor(string colorString, out Color result)
        {
            result = Color.white;

            if (string.IsNullOrEmpty(colorString))
            {
                return false;
            }

            colorString = colorString.Trim().ToLower();

            // 尝试颜色名称
            if (ColorNames.TryGetValue(colorString, out result))
            {
                return true;
            }

            // 尝试解析 hex 格式 (#RRGGBB 或 #RRGGBBAA)
            if (ColorUtility.TryParseHtmlString(colorString, out result))
            {
                return true;
            }

            // 如果没有 # 前缀，尝试添加后解析
            if (!colorString.StartsWith("#"))
            {
                if (ColorUtility.TryParseHtmlString("#" + colorString, out result))
                {
                    return true;
                }
            }

            // 尝试解析 rgb(r, g, b) 或 rgba(r, g, b, a) 格式
            if (TryParseRgbFormat(colorString, out result))
            {
                return true;
            }

            result = Color.white;
            return false;
        }

        /// <summary>
        /// 尝试解析 rgb/rgba 格式
        /// </summary>
        private static bool TryParseRgbFormat(string colorString, out Color result)
        {
            result = Color.white;

            // 检查 rgb( 或 rgba( 格式
            if (colorString.StartsWith("rgb"))
            {
                int start = colorString.IndexOf('(');
                int end = colorString.IndexOf(')');
                if (start >= 0 && end > start)
                {
                    string values = colorString.Substring(start + 1, end - start - 1);
                    string[] parts = values.Split(',');

                    if (parts.Length >= 3)
                    {
                        try
                        {
                            float r = ParseColorComponent(parts[0].Trim());
                            float g = ParseColorComponent(parts[1].Trim());
                            float b = ParseColorComponent(parts[2].Trim());
                            float a = parts.Length >= 4 ? ParseColorComponent(parts[3].Trim()) : 1f;

                            result = new Color(r, g, b, a);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 解析颜色分量（0-255 或 0-1）
        /// </summary>
        private static float ParseColorComponent(string value)
        {
            float f = float.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
            // 如果大于 1，假设是 0-255 范围
            if (f > 1f)
            {
                f /= 255f;
            }
            return Mathf.Clamp01(f);
        }

        /// <summary>
        /// 获取所有支持的颜色名称
        /// </summary>
        public static string[] GetColorNames()
        {
            var names = new List<string>();
            foreach (var key in ColorNames.Keys)
            {
                // 只返回英文名称
                if (!ContainsChinese(key))
                {
                    names.Add(key);
                }
            }
            return names.ToArray();
        }

        /// <summary>
        /// 检查字符串是否包含中文
        /// </summary>
        private static bool ContainsChinese(string str)
        {
            foreach (char c in str)
            {
                if (c >= 0x4E00 && c <= 0x9FA5)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 将 Color 转换为友好的字符串表示
        /// </summary>
        public static string ColorToString(Color color)
        {
            // 检查是否是已知颜色
            foreach (var pair in ColorNames)
            {
                if (!ContainsChinese(pair.Key) &&
                    Mathf.Approximately(pair.Value.r, color.r) &&
                    Mathf.Approximately(pair.Value.g, color.g) &&
                    Mathf.Approximately(pair.Value.b, color.b))
                {
                    return pair.Key;
                }
            }

            // 返回 hex 格式
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
