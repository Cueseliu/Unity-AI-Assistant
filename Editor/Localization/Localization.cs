using System;
using UnityEditor;

namespace AIOperator.Editor.Localization
{
    /// <summary>
    /// 支持的语言
    /// </summary>
    public enum Language
    {
        Chinese,  // 中文
        English   // 英文
    }

    /// <summary>
    /// 本地化管理器 - 处理中英文切换
    /// </summary>
    public static class Loc
    {
        private const string PREF_KEY = "AIOperator.Language";

        private static Language _currentLanguage = Language.Chinese;
        private static bool _initialized = false;

        /// <summary>
        /// 语言变更事件
        /// </summary>
        public static event Action OnLanguageChanged;

        /// <summary>
        /// 当前语言
        /// </summary>
        public static Language CurrentLanguage
        {
            get
            {
                if (!_initialized)
                {
                    Load();
                }
                return _currentLanguage;
            }
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    Save();
                    OnLanguageChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// 是否为中文
        /// </summary>
        public static bool IsChinese => CurrentLanguage == Language.Chinese;

        /// <summary>
        /// 是否为英文
        /// </summary>
        public static bool IsEnglish => CurrentLanguage == Language.English;

        /// <summary>
        /// 从 EditorPrefs 加载语言设置
        /// </summary>
        private static void Load()
        {
            int savedValue = EditorPrefs.GetInt(PREF_KEY, 0);
            _currentLanguage = (Language)savedValue;
            _initialized = true;
        }

        /// <summary>
        /// 保存语言设置到 EditorPrefs
        /// </summary>
        private static void Save()
        {
            EditorPrefs.SetInt(PREF_KEY, (int)_currentLanguage);
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="chinese">中文文本</param>
        /// <param name="english">英文文本</param>
        /// <returns>当前语言对应的文本</returns>
        public static string Get(string chinese, string english)
        {
            return IsChinese ? chinese : english;
        }

        /// <summary>
        /// 获取语言显示名称
        /// </summary>
        public static string[] GetLanguageNames()
        {
            return new[] { "中文", "English" };
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        public static void ToggleLanguage()
        {
            CurrentLanguage = IsChinese ? Language.English : Language.Chinese;
        }
    }
}
