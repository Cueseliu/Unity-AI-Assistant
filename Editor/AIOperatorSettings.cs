using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using AIOperator.Models;
using AIOperator.LLM;
using AIOperator.LLM.Providers;
using AIOperator.Editor.Localization;

namespace AIOperator.Editor
{
    /// <summary>
    /// AI Operator 设置窗口
    /// </summary>
    public class AIOperatorSettings : EditorWindow
    {
        private LLMConfig config;
        private Vector2 scrollPosition;
        private int selectedProviderIndex = 0;
        private int selectedModelIndex = 0;
        private int selectedRegionIndex = 0;
        private int selectedLanguageIndex = 0;
        private string[] providerOptions = new string[] { "Claude", "Bedrock", "OpenAI", "DeepSeek", "Qwen", "GLM-4", "Doubao" };
        private bool isTestingConnection = false;
        private string testResult = "";

        [MenuItem("Window/AI Operator/Settings", false, 2)]
        public static void ShowWindow()
        {
            AIOperatorSettings window = GetWindow<AIOperatorSettings>();
            window.titleContent = new GUIContent(L.Settings.WindowTitle);
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // 加载配置
            config = LLMConfig.Load();

            // 设置当前选择的 Provider
            selectedProviderIndex = System.Array.IndexOf(providerOptions, config.provider);
            if (selectedProviderIndex < 0) selectedProviderIndex = 0;

            // 设置当前选择的模型
            UpdateSelectedModelIndex();

            // 设置当前选择的 Region
            UpdateSelectedRegionIndex();

            // 设置当前选择的语言
            selectedLanguageIndex = (int)Loc.CurrentLanguage;

            // 订阅语言变更事件
            Loc.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDisable()
        {
            Loc.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged()
        {
            // 更新窗口标题
            titleContent = new GUIContent(L.Settings.WindowTitle);
            Repaint();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(L.Settings.Title, EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawLanguageSelection();
            EditorGUILayout.Space(15);

            DrawProviderSelection();
            EditorGUILayout.Space(15);

            DrawAPIKeySection();
            EditorGUILayout.Space(15);

            DrawModelSelection();
            EditorGUILayout.Space(15);

            DrawTestConnection();
            EditorGUILayout.Space(15);

            DrawSaveButton();
            EditorGUILayout.Space(15);

            DrawConfigManagement();
            EditorGUILayout.Space(15);

            DrawHelpSection();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制语言选择
        /// </summary>
        private void DrawLanguageSelection()
        {
            EditorGUILayout.LabelField(L.Settings.LanguageSection, EditorStyles.boldLabel);

            int newLanguageIndex = EditorGUILayout.Popup(L.Settings.LanguageLabel, selectedLanguageIndex, Loc.GetLanguageNames());

            if (newLanguageIndex != selectedLanguageIndex)
            {
                selectedLanguageIndex = newLanguageIndex;
                Loc.CurrentLanguage = (Language)newLanguageIndex;
            }
        }

        /// <summary>
        /// 绘制 Provider 选择
        /// </summary>
        private void DrawProviderSelection()
        {
            EditorGUILayout.LabelField(L.Settings.ProviderSection, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(L.Settings.ProviderHint, MessageType.Info);

            int newProviderIndex = EditorGUILayout.Popup(L.Settings.ProviderLabel, selectedProviderIndex, providerOptions);

            if (newProviderIndex != selectedProviderIndex)
            {
                selectedProviderIndex = newProviderIndex;
                config.provider = providerOptions[selectedProviderIndex];
                config.model = LLMConfig.GetDefaultModel(config.provider);
                UpdateSelectedModelIndex();
            }
        }

        /// <summary>
        /// 绘制 API Key 配置
        /// </summary>
        private void DrawAPIKeySection()
        {
            if (config.provider == "Bedrock")
            {
                // AWS Bedrock 特殊配置
                EditorGUILayout.LabelField(L.Settings.AWSSection, EditorStyles.boldLabel);

                string helpText = GetAPIKeyHelpText();
                EditorGUILayout.HelpBox(helpText, MessageType.Info);

                // Region 选择
                selectedRegionIndex = EditorGUILayout.Popup(L.Settings.AWSRegionLabel, selectedRegionIndex, LLMConfig.AWSRegions);
                config.awsRegion = LLMConfig.AWSRegions[selectedRegionIndex];

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(L.Settings.AWSAuthMethod, EditorStyles.miniLabel);

                // 方式 1: Bearer Token
                EditorGUILayout.LabelField(L.Settings.AWSBearerMethod, EditorStyles.boldLabel);
                config.awsBearerToken = EditorGUILayout.PasswordField(L.Settings.AWSBearerLabel, config.awsBearerToken);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField(L.Settings.AWSOr, EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.Space(5);

                // 方式 2: Access Key + Secret Key
                EditorGUILayout.LabelField(L.Settings.AWSAccessKeyMethod, EditorStyles.boldLabel);
                config.awsAccessKeyId = EditorGUILayout.TextField(L.Settings.AWSAccessKeyLabel, config.awsAccessKeyId);
                config.awsSecretAccessKey = EditorGUILayout.PasswordField(L.Settings.AWSSecretKeyLabel, config.awsSecretAccessKey);

                // 获取凭证链接
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button(L.Settings.GetAWSCredentials, GUILayout.Width(150)))
                {
                    Application.OpenURL("https://console.aws.amazon.com/iam/home#/security_credentials");
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // 其他 Provider 的 API Key 配置
                EditorGUILayout.LabelField(L.Settings.APIKeySection, EditorStyles.boldLabel);

                string helpText = GetAPIKeyHelpText();
                EditorGUILayout.HelpBox(helpText, MessageType.Info);

                config.apiKey = EditorGUILayout.PasswordField(L.Settings.APIKeyLabel, config.apiKey);

                // 获取 API Key 链接
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth));
                if (GUILayout.Button(L.Settings.GetAPIKey, GUILayout.Width(150)))
                {
                    Application.OpenURL(GetAPIKeyURL());
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 绘制模型选择
        /// </summary>
        private void DrawModelSelection()
        {
            EditorGUILayout.LabelField(L.Settings.ModelSection, EditorStyles.boldLabel);

            string[] models = LLMConfig.GetModelsForProvider(config.provider);
            selectedModelIndex = EditorGUILayout.Popup(L.Settings.ModelLabel, selectedModelIndex, models);
            config.model = models[selectedModelIndex];

            // 显示模型说明
            EditorGUILayout.HelpBox(GetModelDescription(config.model), MessageType.Info);
        }

        /// <summary>
        /// 绘制测试连接
        /// </summary>
        private void DrawTestConnection()
        {
            EditorGUILayout.LabelField(L.Settings.TestSection, EditorStyles.boldLabel);

            bool hasCredentials;
            if (config.provider == "Bedrock")
            {
                hasCredentials = !string.IsNullOrEmpty(config.awsBearerToken) ||
                                (!string.IsNullOrEmpty(config.awsAccessKeyId) && !string.IsNullOrEmpty(config.awsSecretAccessKey));
            }
            else
            {
                hasCredentials = !string.IsNullOrEmpty(config.apiKey);
            }

            GUI.enabled = hasCredentials && !isTestingConnection;

            string buttonText = isTestingConnection ? L.Settings.Testing : L.Settings.TestButton;
            if (GUILayout.Button(buttonText, GUILayout.Height(30)))
            {
                TestConnection();
            }

            GUI.enabled = true;

            if (!string.IsNullOrEmpty(testResult))
            {
                MessageType messageType = testResult.Contains("✓") ? MessageType.Info : MessageType.Error;
                EditorGUILayout.HelpBox(testResult, messageType);
            }
        }

        /// <summary>
        /// 绘制保存按钮
        /// </summary>
        private void DrawSaveButton()
        {
            EditorGUILayout.Space(10);

            if (GUILayout.Button(L.Settings.SaveButton, GUILayout.Height(40)))
            {
                config.Save();
                EditorUtility.DisplayDialog(L.Settings.SaveSuccess, L.Settings.SaveSuccessMessage, L.Ok);
            }
        }

        /// <summary>
        /// 绘制配置管理
        /// </summary>
        private void DrawConfigManagement()
        {
            EditorGUILayout.LabelField(L.Settings.ConfigSection, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            // 恢复默认按钮
            if (GUILayout.Button(L.Settings.ResetDefault, GUILayout.Height(25)))
            {
                ResetToDefault();
            }

            // 导出配置按钮
            if (GUILayout.Button(L.Settings.ExportConfig, GUILayout.Height(25)))
            {
                ExportConfig();
            }

            // 导入配置按钮
            if (GUILayout.Button(L.Settings.ImportConfig, GUILayout.Height(25)))
            {
                ImportConfig();
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 恢复默认设置
        /// </summary>
        private void ResetToDefault()
        {
            if (EditorUtility.DisplayDialog(L.Settings.ResetConfirm, L.Settings.ResetConfirmMessage, L.Ok, L.Cancel))
            {
                config = new LLMConfig();
                config.Save();

                // 重新加载索引
                selectedProviderIndex = 0;
                selectedModelIndex = 0;
                selectedRegionIndex = 0;
                UpdateSelectedModelIndex();
                UpdateSelectedRegionIndex();

                testResult = "";
                Repaint();
            }
        }

        /// <summary>
        /// 导出配置
        /// </summary>
        private void ExportConfig()
        {
            string defaultPath = Application.dataPath;
            string path = EditorUtility.SaveFilePanel(
                L.Settings.ExportConfig,
                defaultPath,
                "AIOperatorConfig",
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = JsonUtility.ToJson(config, true);
                    File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog(L.Settings.ConfigExported, L.Settings.ConfigExportedMessage + path, L.Ok);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AI Operator] Export config failed: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 导入配置
        /// </summary>
        private void ImportConfig()
        {
            string defaultPath = Application.dataPath;
            string path = EditorUtility.OpenFilePanel(
                L.Settings.ImportConfig,
                defaultPath,
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    LLMConfig importedConfig = JsonUtility.FromJson<LLMConfig>(json);

                    if (importedConfig != null)
                    {
                        config = importedConfig;
                        config.Save();

                        // 更新 UI
                        selectedProviderIndex = Array.IndexOf(providerOptions, config.provider);
                        if (selectedProviderIndex < 0) selectedProviderIndex = 0;
                        UpdateSelectedModelIndex();
                        UpdateSelectedRegionIndex();

                        EditorUtility.DisplayDialog(L.Settings.ConfigImported, L.Settings.ConfigImportedMessage, L.Ok);
                        Repaint();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(L.Settings.ConfigImportFailed, L.Settings.ConfigImportFailedMessage, L.Ok);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[AI Operator] Import config failed: {e.Message}");
                    EditorUtility.DisplayDialog(L.Settings.ConfigImportFailed, L.Settings.ConfigImportFailedMessage, L.Ok);
                }
            }
        }

        /// <summary>
        /// 绘制帮助信息
        /// </summary>
        private void DrawHelpSection()
        {
            EditorGUILayout.LabelField(L.Settings.HelpSection, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(L.Settings.HelpContent, MessageType.None);
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        private void TestConnection()
        {
            isTestingConnection = true;
            testResult = L.Loading;
            Repaint();

            Debug.Log($"[Settings] Testing connection - Provider: {config.provider}");

            ILLMProvider provider = null;

            switch (config.provider)
            {
                case "Claude":
                    provider = new ClaudeProvider(config, CoroutineRunner.Instance);
                    break;
                case "DeepSeek":
                    provider = new DeepSeekProvider(config, CoroutineRunner.Instance);
                    break;
                case "Bedrock":
                    provider = new BedrockProvider(config, CoroutineRunner.Instance);
                    Debug.Log($"[Settings] Bedrock config - Region: {config.awsRegion}");
                    break;
                case "Qwen":
                    provider = new QwenProvider(config, CoroutineRunner.Instance);
                    break;
                case "GLM-4":
                    provider = new GLM4Provider(config, CoroutineRunner.Instance);
                    break;
                case "Doubao":
                    provider = new DoubaoProvider(config, CoroutineRunner.Instance);
                    break;
            }

            if (provider != null)
            {
                Debug.Log($"[Settings] Provider created, calling TestConnection");

                provider.TestConnection((success, message) =>
                {
                    Debug.Log($"[Settings] TestConnection callback - Success: {success}, Message: {message}");
                    isTestingConnection = false;
                    testResult = success ? $"✓ {message}" : $"✗ {message}";
                    Repaint();
                });
            }
            else
            {
                Debug.LogError("[Settings] Provider creation failed");
                isTestingConnection = false;
                testResult = $"✗ {L.Failed}";
                Repaint();
            }
        }

        /// <summary>
        /// 更新选择的模型索引
        /// </summary>
        private void UpdateSelectedModelIndex()
        {
            string[] models = LLMConfig.GetModelsForProvider(config.provider);
            selectedModelIndex = System.Array.IndexOf(models, config.model);
            if (selectedModelIndex < 0) selectedModelIndex = 0;
        }

        /// <summary>
        /// 获取 API Key 帮助文本
        /// </summary>
        private string GetAPIKeyHelpText()
        {
            switch (config.provider)
            {
                case "Claude":
                    return L.Settings.ClaudeHelp;
                case "Bedrock":
                    return L.Settings.BedrockHelp;
                case "OpenAI":
                    return L.Settings.OpenAIHelp;
                case "DeepSeek":
                    return L.Settings.DeepSeekHelp;
                case "Qwen":
                    return L.Settings.QwenHelp;
                case "GLM-4":
                    return L.Settings.GLM4Help;
                case "Doubao":
                    return L.Settings.DoubaoHelp;
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取 API Key URL
        /// </summary>
        private string GetAPIKeyURL()
        {
            switch (config.provider)
            {
                case "Claude":
                    return "https://console.anthropic.com/settings/keys";
                case "OpenAI":
                    return "https://platform.openai.com/api-keys";
                case "DeepSeek":
                    return "https://platform.deepseek.com/api_keys";
                case "Qwen":
                    return "https://dashscope.console.aliyun.com/apiKey";
                case "GLM-4":
                    return "https://open.bigmodel.cn/usercenter/apikeys";
                case "Doubao":
                    return "https://console.volcengine.com/ark/region:ark+cn-beijing/apiKey";
                default:
                    return "https://console.anthropic.com/settings/keys";
            }
        }

        /// <summary>
        /// 获取模型描述
        /// </summary>
        private string GetModelDescription(string model)
        {
            // Claude 系列
            if (model.Contains("sonnet"))
                return L.Settings.ModelSonnet;
            if (model.Contains("haiku"))
                return L.Settings.ModelHaiku;
            if (model.Contains("opus"))
                return L.Settings.ModelOpus;

            // OpenAI 系列
            if (model.Contains("gpt-4o-mini"))
                return L.Settings.ModelGPT4oMini;
            if (model.Contains("gpt-4o"))
                return L.Settings.ModelGPT4o;

            // DeepSeek 系列
            if (model.Contains("deepseek-chat"))
                return L.Settings.ModelDeepSeekChat;
            if (model.Contains("deepseek-coder"))
                return L.Settings.ModelDeepSeekCoder;

            // Qwen 系列
            if (model.Contains("qwen-plus"))
                return L.Settings.ModelQwenPlus;
            if (model.Contains("qwen-turbo"))
                return L.Settings.ModelQwenTurbo;
            if (model.Contains("qwen-max"))
                return L.Settings.ModelQwenMax;

            // GLM-4 系列
            if (model.Contains("glm-4-plus"))
                return L.Settings.ModelGLM4Plus;
            if (model.Contains("glm-4-flash"))
                return L.Settings.ModelGLM4Flash;
            if (model == "glm-4")
                return L.Settings.ModelGLM4;

            // Doubao 系列
            if (model.Contains("doubao-1-5-pro") || model.Contains("doubao-seed"))
                return L.Settings.ModelDoubaoPro;
            if (model.Contains("doubao-1-5-lite"))
                return L.Settings.ModelDoubaoLite;

            return L.Settings.ModelDefault;
        }

        /// <summary>
        /// 更新选择的 Region 索引
        /// </summary>
        private void UpdateSelectedRegionIndex()
        {
            selectedRegionIndex = System.Array.IndexOf(LLMConfig.AWSRegions, config.awsRegion);
            if (selectedRegionIndex < 0) selectedRegionIndex = 0;
        }
    }
}
