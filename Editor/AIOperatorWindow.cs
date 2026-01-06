using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AIOperator.Models;
using AIOperator.LLM;
using AIOperator.LLM.Providers;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Localization;

namespace AIOperator.Editor
{
    /// <summary>
    /// AI Operator 主窗口 - Unity Editor 内置 AI 聊天面板
    /// 支持工具调用和多轮对话
    /// </summary>
    public class AIOperatorWindow : EditorWindow
    {
        // 聊天消息列表
        private List<ChatMessage> chatHistory = new List<ChatMessage>();

        // 用户输入框内容
        private string userInput = "";

        // UI 滚动位置
        private Vector2 scrollPosition;

        // 样式
        private GUIStyle userMessageStyle;
        private GUIStyle assistantMessageStyle;
        private GUIStyle timestampStyle;
        private GUIStyle toolCallStyle;
        private GUIStyle quickCommandStyle;
        private GUIStyle inputAreaStyle;

        // LLM 相关
        private ILLMProvider llmProvider;
        private LLMConfig llmConfig;
        private bool isWaitingResponse = false;

        // 工具调用管理器
        private ToolCallManager toolCallManager;
        private string currentStatus = "";
        private List<string> toolExecutionLog = new List<string>();

        // 历史命令
        private List<string> commandHistory = new List<string>();
        private int historyIndex = -1;
        private const int MAX_HISTORY = 50;

        // 快捷命令
        private static readonly string[] QuickCommands = new string[]
        {
            "Create a Cube",
            "Create a player character",
            "Create a Canvas with Button",
            "Analyze scene",
            "Get scene hierarchy"
        };

        // 首次使用标记
        private const string FIRST_USE_KEY = "AIOperator_FirstUse";
        private bool showQuickCommands = false;

        /// <summary>
        /// 打开窗口的菜单项
        /// </summary>
        [MenuItem("Window/AI Operator/Chat Window", false, 1)]
        public static void ShowWindow()
        {
            AIOperatorWindow window = GetWindow<AIOperatorWindow>();
            window.titleContent = new GUIContent(L.Chat.WindowTitle);
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            // 初始化
            if (chatHistory == null)
            {
                chatHistory = new List<ChatMessage>();
            }

            // 加载 LLM 配置
            llmConfig = LLMConfig.Load();

            // 初始化 LLM Provider
            InitializeLLMProvider();

            // 检查是否首次使用
            bool isFirstUse = !EditorPrefs.GetBool(FIRST_USE_KEY, false);
            if (isFirstUse)
            {
                showQuickCommands = true;
                EditorPrefs.SetBool(FIRST_USE_KEY, true);
            }

            // 欢迎消息
            if (chatHistory.Count == 0)
            {
                string welcomeMessage = GetWelcomeMessage();
                chatHistory.Add(new ChatMessage("assistant", welcomeMessage));
            }

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
            titleContent = new GUIContent(L.Chat.WindowTitle);
            Repaint();
        }

        private void InitializeLLMProvider()
        {
            switch (llmConfig.provider)
            {
                case "Claude":
                    llmProvider = new ClaudeProvider(llmConfig, CoroutineRunner.Instance);
                    break;
                case "DeepSeek":
                    llmProvider = new DeepSeekProvider(llmConfig, CoroutineRunner.Instance);
                    break;
                case "Bedrock":
                    llmProvider = new BedrockProvider(llmConfig, CoroutineRunner.Instance);
                    break;
                case "Qwen":
                    llmProvider = new QwenProvider(llmConfig, CoroutineRunner.Instance);
                    break;
                case "GLM-4":
                    llmProvider = new GLM4Provider(llmConfig, CoroutineRunner.Instance);
                    break;
                case "Doubao":
                    llmProvider = new DoubaoProvider(llmConfig, CoroutineRunner.Instance);
                    break;
                default:
                    llmProvider = new ClaudeProvider(llmConfig, CoroutineRunner.Instance);
                    break;
            }

            // 初始化工具调用管理器
            toolCallManager = new ToolCallManager(llmProvider);
            toolCallManager.OnStatusUpdate = (status) =>
            {
                currentStatus = status;
                Repaint();
            };
            toolCallManager.OnToolExecuted = (call, result) =>
            {
                string logEntry = $"[{System.DateTime.Now:HH:mm:ss}] {call.Name}: {(result.Success ? "✓" : "✗")} {result.Message}";
                toolExecutionLog.Add(logEntry);
                if (toolExecutionLog.Count > 20)
                {
                    toolExecutionLog.RemoveAt(0);
                }
                Repaint();
            };
        }

        private string GetWelcomeMessage()
        {
            bool isConfigured;
            if (llmConfig.provider == "Bedrock")
            {
                isConfigured = !string.IsNullOrEmpty(llmConfig.awsBearerToken) ||
                              !string.IsNullOrEmpty(llmConfig.awsAccessKeyId);
            }
            else
            {
                isConfigured = !string.IsNullOrEmpty(llmConfig.apiKey);
            }

            if (!isConfigured)
            {
                return L.Chat.WelcomeNotConfigured;
            }

            return L.Chat.WelcomeConfigured(llmConfig.provider, llmConfig.model);
        }

        private void OnGUI()
        {
            InitializeStyles();
            DrawHeader();
            DrawChatArea();
            DrawInputArea();
        }

        /// <summary>
        /// 初始化样式
        /// </summary>
        private void InitializeStyles()
        {
            if (userMessageStyle == null)
            {
                userMessageStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    wordWrap = true,
                    padding = new RectOffset(12, 12, 10, 10),
                    margin = new RectOffset(40, 8, 5, 5),
                    fontSize = 12,
                    richText = true,
                    normal = { background = MakeRoundedTexture(new Color(0.25f, 0.45f, 0.75f, 0.4f)) }
                };
            }

            if (assistantMessageStyle == null)
            {
                assistantMessageStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    wordWrap = true,
                    padding = new RectOffset(12, 12, 10, 10),
                    margin = new RectOffset(8, 40, 5, 5),
                    fontSize = 12,
                    richText = true,
                    normal = { background = MakeRoundedTexture(new Color(0.25f, 0.25f, 0.28f, 0.5f)) }
                };
            }

            if (timestampStyle == null)
            {
                timestampStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    fontSize = 9,
                    normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
                };
            }

            if (toolCallStyle == null)
            {
                toolCallStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    wordWrap = true,
                    fontSize = 10,
                    padding = new RectOffset(10, 10, 6, 6),
                    margin = new RectOffset(50, 15, 3, 3),
                    richText = true,
                    normal = { background = MakeRoundedTexture(new Color(0.15f, 0.35f, 0.25f, 0.4f)) }
                };
            }

            if (quickCommandStyle == null)
            {
                quickCommandStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    padding = new RectOffset(10, 10, 5, 5),
                    margin = new RectOffset(5, 5, 2, 2),
                    fontSize = 10
                };
            }

            if (inputAreaStyle == null)
            {
                inputAreaStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    margin = new RectOffset(0, 0, 5, 5)
                };
            }
        }

        /// <summary>
        /// 绘制标题栏
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(L.Chat.HeaderTitle, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            // 快捷命令切换按钮
            string quickCmdLabel = showQuickCommands ? "▼ " + L.Chat.QuickCommands : "▶ " + L.Chat.QuickCommands;
            if (GUILayout.Button(quickCmdLabel, EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                showQuickCommands = !showQuickCommands;
            }

            if (GUILayout.Button(L.Chat.CopyAll, EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                CopyAllMessages();
            }

            if (GUILayout.Button(L.Chat.Clear, EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ClearChat();
            }

            if (GUILayout.Button(L.Chat.Settings, EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                AIOperatorSettings.ShowWindow();
            }

            EditorGUILayout.EndHorizontal();

            // 快捷命令栏
            if (showQuickCommands)
            {
                DrawQuickCommands();
            }
        }

        /// <summary>
        /// 绘制快捷命令栏
        /// </summary>
        private void DrawQuickCommands()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(L.Chat.TryCommands, EditorStyles.miniLabel, GUILayout.Width(50));

            foreach (string cmd in QuickCommands)
            {
                if (GUILayout.Button(cmd, quickCommandStyle))
                {
                    if (!isWaitingResponse)
                    {
                        SendMessage(cmd);
                    }
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 复制所有聊天记录
        /// </summary>
        private void CopyAllMessages()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var message in chatHistory)
            {
                string roleLabel = message.role == "user" ? L.Chat.User : L.Chat.AI;
                sb.AppendLine($"[{message.timestamp}] {roleLabel}:");
                sb.AppendLine(message.content);
                sb.AppendLine();
            }
            EditorGUIUtility.systemCopyBuffer = sb.ToString();
            Debug.Log($"[AI Operator] {L.Chat.AllCopied}");
        }

        /// <summary>
        /// 绘制聊天区域
        /// </summary>
        private void DrawChatArea()
        {
            // 聊天历史区域
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

            foreach (var message in chatHistory)
            {
                DrawMessage(message);
            }

            // 显示当前状态
            if (isWaitingResponse && !string.IsNullOrEmpty(currentStatus))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(60);
                EditorGUILayout.LabelField($"⏳ {currentStatus}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            // 显示最近的工具执行日志
            if (toolExecutionLog.Count > 0 && isWaitingResponse)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginVertical(toolCallStyle);
                EditorGUILayout.LabelField(L.Chat.ToolExecutionLog, EditorStyles.miniBoldLabel);
                int startIdx = Mathf.Max(0, toolExecutionLog.Count - 5);
                for (int i = startIdx; i < toolExecutionLog.Count; i++)
                {
                    EditorGUILayout.LabelField(toolExecutionLog[i], EditorStyles.miniLabel);
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制单条消息
        /// </summary>
        private void DrawMessage(ChatMessage message)
        {
            EditorGUILayout.BeginVertical();

            // 时间戳、角色标签和复制按钮
            EditorGUILayout.BeginHorizontal();
            string roleLabel = message.role == "user" ? L.Chat.You : L.Chat.AI;
            EditorGUILayout.LabelField($"[{message.timestamp}] {roleLabel}", timestampStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(L.Chat.Copy, EditorStyles.miniButton, GUILayout.Width(40)))
            {
                EditorGUIUtility.systemCopyBuffer = message.content;
                Debug.Log($"[AI Operator] {L.Chat.Copied}");
            }
            EditorGUILayout.EndHorizontal();

            // 消息内容（支持右键复制）
            GUIStyle style = message.role == "user" ? userMessageStyle : assistantMessageStyle;
            Rect contentRect = EditorGUILayout.GetControlRect(false, style.CalcHeight(new GUIContent(message.content), position.width - 60));
            EditorGUI.LabelField(contentRect, message.content, style);

            // 右键菜单复制
            Event e = Event.current;
            if (e.type == EventType.ContextClick && contentRect.Contains(e.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent(L.Chat.Copy), false, () => {
                    EditorGUIUtility.systemCopyBuffer = message.content;
                    Debug.Log($"[AI Operator] {L.Chat.Copied}");
                });
                menu.ShowAsContext();
                e.Use();
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        /// <summary>
        /// 绘制输入区域
        /// </summary>
        private void DrawInputArea()
        {
            // 输入区域背景
            EditorGUILayout.BeginVertical(inputAreaStyle);

            // 快捷键提示
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(L.Chat.ShortcutHint, timestampStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            // 输入框（多行）
            GUI.enabled = !isWaitingResponse;
            GUI.SetNextControlName("UserInput");
            userInput = EditorGUILayout.TextArea(userInput, GUILayout.Height(50), GUILayout.ExpandWidth(true));

            // 按钮区域
            EditorGUILayout.BeginVertical(GUILayout.Width(65));

            // 发送/中断按钮
            if (isWaitingResponse)
            {
                // 显示中断按钮
                GUI.enabled = true;
                GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f); // 红色背景
                if (GUILayout.Button(L.Chat.Stop, GUILayout.Width(60), GUILayout.Height(24)))
                {
                    CancelCurrentRequest();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                // 显示发送按钮
                GUI.backgroundColor = new Color(0.3f, 0.6f, 0.9f);
                if (GUILayout.Button(L.Chat.Send, GUILayout.Width(60), GUILayout.Height(24)))
                {
                    ExecuteSend();
                }
                GUI.backgroundColor = Color.white;
            }

            // 历史命令按钮
            GUI.enabled = commandHistory.Count > 0 && !isWaitingResponse;
            if (GUILayout.Button("↑↓ " + L.Chat.History, EditorStyles.miniButton, GUILayout.Width(60), GUILayout.Height(20)))
            {
                ShowHistoryMenu();
            }
            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // 处理键盘事件
            HandleKeyboardInput();
        }

        /// <summary>
        /// 执行发送
        /// </summary>
        private void ExecuteSend()
        {
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                // 添加到历史记录
                AddToHistory(userInput);

                SendMessage(userInput);
                userInput = "";
                historyIndex = -1;
                GUI.FocusControl("UserInput");
            }
        }

        /// <summary>
        /// 添加到历史记录
        /// </summary>
        private void AddToHistory(string command)
        {
            // 避免重复添加
            if (commandHistory.Count > 0 && commandHistory[commandHistory.Count - 1] == command)
                return;

            commandHistory.Add(command);

            // 限制历史记录数量
            if (commandHistory.Count > MAX_HISTORY)
            {
                commandHistory.RemoveAt(0);
            }
        }

        /// <summary>
        /// 显示历史命令菜单
        /// </summary>
        private void ShowHistoryMenu()
        {
            GenericMenu menu = new GenericMenu();

            // 倒序显示最近的命令
            for (int i = commandHistory.Count - 1; i >= 0 && i >= commandHistory.Count - 10; i--)
            {
                string cmd = commandHistory[i];
                string displayCmd = cmd.Length > 50 ? cmd.Substring(0, 47) + "..." : cmd;
                menu.AddItem(new GUIContent(displayCmd), false, () =>
                {
                    userInput = cmd;
                    Repaint();
                });
            }

            if (commandHistory.Count > 10)
            {
                menu.AddSeparator("");
                menu.AddDisabledItem(new GUIContent($"... {L.Chat.MoreHistory} ({commandHistory.Count - 10})"));
            }

            menu.ShowAsContext();
        }

        /// <summary>
        /// 处理键盘输入
        /// </summary>
        private void HandleKeyboardInput()
        {
            Event e = Event.current;
            bool isFocused = GUI.GetNameOfFocusedControl() == "UserInput";

            // Ctrl+Enter 或 Enter 发送
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return && isFocused && !isWaitingResponse)
            {
                bool ctrlPressed = e.control || e.command;

                // Ctrl+Enter 总是发送，普通 Enter 如果没有 Shift 也发送
                if (ctrlPressed || !e.shift)
                {
                    ExecuteSend();
                    e.Use();
                }
            }

            // 上下箭头浏览历史
            if (e.type == EventType.KeyDown && isFocused && !isWaitingResponse && commandHistory.Count > 0)
            {
                if (e.keyCode == KeyCode.UpArrow && e.control)
                {
                    // Ctrl+上箭头：上一条历史
                    if (historyIndex < 0)
                    {
                        historyIndex = commandHistory.Count - 1;
                    }
                    else if (historyIndex > 0)
                    {
                        historyIndex--;
                    }

                    if (historyIndex >= 0 && historyIndex < commandHistory.Count)
                    {
                        userInput = commandHistory[historyIndex];
                    }
                    e.Use();
                }
                else if (e.keyCode == KeyCode.DownArrow && e.control)
                {
                    // Ctrl+下箭头：下一条历史
                    if (historyIndex >= 0)
                    {
                        historyIndex++;
                        if (historyIndex >= commandHistory.Count)
                        {
                            historyIndex = -1;
                            userInput = "";
                        }
                        else
                        {
                            userInput = commandHistory[historyIndex];
                        }
                    }
                    e.Use();
                }
            }

            // Escape 键中断
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape && isWaitingResponse)
            {
                CancelCurrentRequest();
                e.Use();
            }
        }

        /// <summary>
        /// 取消当前请求
        /// </summary>
        private void CancelCurrentRequest()
        {
            if (!isWaitingResponse) return;

            // 停止所有协程
            CoroutineRunner.Instance.StopAllEditorCoroutines();

            // 添加中断提示消息
            chatHistory.Add(new ChatMessage("assistant", $"⚠️ {L.Chat.Interrupted}"));

            // 重置状态
            isWaitingResponse = false;
            currentStatus = "";
            toolExecutionLog.Clear();

            Debug.Log("[AI Operator] User interrupted current request");
            Repaint();
        }

        /// <summary>
        /// 发送消息（使用工具调用管理器）
        /// </summary>
        private void SendMessage(string message)
        {
            // 添加用户消息到聊天历史
            chatHistory.Add(new ChatMessage("user", message));

            // 显示加载状态
            isWaitingResponse = true;
            currentStatus = L.Thinking;
            toolExecutionLog.Clear();

            // 滚动到底部
            scrollPosition.y = float.MaxValue;
            Repaint();

            // 使用工具调用管理器处理消息
            CoroutineRunner.Instance.StartCoroutine(
                toolCallManager.ProcessMessage(
                    message,
                    // 完成回调
                    (response) =>
                    {
                        // 添加 AI 回复
                        chatHistory.Add(new ChatMessage("assistant", response));

                        isWaitingResponse = false;
                        currentStatus = "";
                        scrollPosition.y = float.MaxValue;
                        Repaint();
                    },
                    // 错误回调
                    (error) =>
                    {
                        // 添加错误消息
                        string errorMsg = $"{L.Chat.ErrorPrefix}{error}";
                        if (llmConfig.provider == "Bedrock")
                        {
                            errorMsg += L.Chat.ErrorCheckBedrock;
                        }
                        else
                        {
                            errorMsg += L.Chat.ErrorCheckAPI;
                        }
                        chatHistory.Add(new ChatMessage("assistant", errorMsg));

                        isWaitingResponse = false;
                        currentStatus = "";
                        scrollPosition.y = float.MaxValue;
                        Repaint();
                    }
                )
            );
        }

        /// <summary>
        /// 清空聊天记录
        /// </summary>
        private void ClearChat()
        {
            chatHistory.Clear();
            toolCallManager?.ClearHistory();
            toolExecutionLog.Clear();
            currentStatus = "";
            chatHistory.Add(new ChatMessage("assistant", L.Chat.ChatCleared));
            Repaint();
        }

        /// <summary>
        /// 创建纯色纹理
        /// </summary>
        private Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 创建带圆角效果的纹理（模拟）
        /// </summary>
        private Texture2D MakeRoundedTexture(Color color)
        {
            int size = 16;
            Texture2D texture = new Texture2D(size, size);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
    }
}
