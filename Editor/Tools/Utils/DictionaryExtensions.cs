using System;
using System.Collections.Generic;
using UnityEngine;

namespace AIOperator.Editor.Tools.Utils
{
    /// <summary>
    /// Dictionary 参数解析扩展方法
    /// 用于从 LLM 返回的工具参数中安全地提取各种类型的值
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 获取字符串值
        /// </summary>
        public static string GetString(this Dictionary<string, object> dict, string key, string defaultValue = null)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
            {
                return value.ToString();
            }
            return defaultValue;
        }

        /// <summary>
        /// 尝试获取字符串值
        /// </summary>
        public static bool TryGetString(this Dictionary<string, object> dict, string key, out string result)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
            {
                result = value.ToString();
                return true;
            }
            result = null;
            return false;
        }

        /// <summary>
        /// 获取整数值
        /// </summary>
        public static int GetInt(this Dictionary<string, object> dict, string key, int defaultValue = 0)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is int i) return i;
                if (value is long l) return (int)l;
                if (value is double d) return (int)d;
                if (value is float f) return (int)f;
                if (int.TryParse(value?.ToString(), out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取浮点数值
        /// </summary>
        public static float GetFloat(this Dictionary<string, object> dict, string key, float defaultValue = 0f)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is float f) return f;
                if (value is double d) return (float)d;
                if (value is int i) return i;
                if (value is long l) return l;
                if (float.TryParse(value?.ToString(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed)) return parsed;
            }
            return defaultValue;
        }

        /// <summary>
        /// 获取布尔值
        /// </summary>
        public static bool GetBool(this Dictionary<string, object> dict, string key, bool defaultValue = false)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is bool b) return b;
                if (value is string s)
                {
                    if (s.ToLower() == "true") return true;
                    if (s.ToLower() == "false") return false;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 尝试获取 Vector3 值
        /// 支持数组格式 [x, y, z]
        /// </summary>
        public static bool TryGetVector3(this Dictionary<string, object> dict, string key, out Vector3 result)
        {
            result = Vector3.zero;

            if (!dict.TryGetValue(key, out var value)) return false;

            // 支持数组格式 [x, y, z]
            if (value is List<object> list && list.Count >= 3)
            {
                try
                {
                    result = new Vector3(
                        Convert.ToSingle(list[0]),
                        Convert.ToSingle(list[1]),
                        Convert.ToSingle(list[2])
                    );
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            // 支持字符串格式 "x,y,z" 或 "(x,y,z)"
            if (value is string str)
            {
                str = str.Trim('(', ')', '[', ']', ' ');
                var parts = str.Split(',');
                if (parts.Length >= 3)
                {
                    try
                    {
                        result = new Vector3(
                            float.Parse(parts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture),
                            float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture)
                        );
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取 Vector3 值，如果不存在返回默认值
        /// </summary>
        public static Vector3 GetVector3(this Dictionary<string, object> dict, string key, Vector3 defaultValue = default)
        {
            if (TryGetVector3(dict, key, out var result))
            {
                return result;
            }
            return defaultValue;
        }

        /// <summary>
        /// 尝试获取 Color 值
        /// 支持颜色名称、数组格式 [r, g, b] 或 [r, g, b, a]、hex 格式
        /// </summary>
        public static bool TryGetColor(this Dictionary<string, object> dict, string key, out Color result)
        {
            result = Color.white;

            if (!dict.TryGetValue(key, out var value)) return false;

            // 支持颜色名称或 hex
            if (value is string colorString)
            {
                return ColorHelper.TryParseColor(colorString, out result);
            }

            // 支持数组格式 [r, g, b] 或 [r, g, b, a]
            if (value is List<object> list && list.Count >= 3)
            {
                try
                {
                    result = new Color(
                        Convert.ToSingle(list[0]),
                        Convert.ToSingle(list[1]),
                        Convert.ToSingle(list[2]),
                        list.Count >= 4 ? Convert.ToSingle(list[3]) : 1f
                    );
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取 Color 值，如果不存在返回默认值
        /// </summary>
        public static Color GetColor(this Dictionary<string, object> dict, string key, Color defaultValue = default)
        {
            if (TryGetColor(dict, key, out var result))
            {
                return result;
            }
            return defaultValue == default ? Color.white : defaultValue;
        }

        /// <summary>
        /// 获取字符串数组
        /// </summary>
        public static string[] GetStringArray(this Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is List<object> list)
                {
                    var result = new string[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        result[i] = list[i]?.ToString() ?? "";
                    }
                    return result;
                }
            }
            return new string[0];
        }

        /// <summary>
        /// 获取浮点数数组
        /// </summary>
        public static float[] GetFloatArray(this Dictionary<string, object> dict, string key)
        {
            if (dict.TryGetValue(key, out var value))
            {
                if (value is List<object> list)
                {
                    var result = new float[list.Count];
                    for (int i = 0; i < list.Count; i++)
                    {
                        try
                        {
                            result[i] = Convert.ToSingle(list[i]);
                        }
                        catch
                        {
                            result[i] = 0f;
                        }
                    }
                    return result;
                }
            }
            return null;
        }

        /// <summary>
        /// 检查是否包含指定的键
        /// </summary>
        public static bool HasKey(this Dictionary<string, object> dict, string key)
        {
            return dict.ContainsKey(key) && dict[key] != null;
        }
    }
}
