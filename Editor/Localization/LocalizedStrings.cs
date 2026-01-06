namespace AIOperator.Editor.Localization
{
    /// <summary>
    /// 本地化字符串定义 - 所有 UI 文本的中英文对照
    /// </summary>
    public static class L
    {
        // ========== 通用 ==========
        public static string Ok => Loc.Get("确定", "OK");
        public static string Cancel => Loc.Get("取消", "Cancel");
        public static string Save => Loc.Get("保存", "Save");
        public static string Close => Loc.Get("关闭", "Close");
        public static string Error => Loc.Get("错误", "Error");
        public static string Warning => Loc.Get("警告", "Warning");
        public static string Success => Loc.Get("成功", "Success");
        public static string Failed => Loc.Get("失败", "Failed");
        public static string Loading => Loc.Get("加载中...", "Loading...");
        public static string Thinking => Loc.Get("思考中...", "Thinking...");

        // ========== Chat Window ==========
        public static class Chat
        {
            public static string WindowTitle => Loc.Get("AI Operator", "AI Operator");
            public static string HeaderTitle => Loc.Get("AI Operator - Unity 智能助手", "AI Operator - Unity AI Assistant");
            public static string Send => Loc.Get("发送", "Send");
            public static string Stop => Loc.Get("中断", "Stop");
            public static string Clear => Loc.Get("清空", "Clear");
            public static string Settings => Loc.Get("设置", "Settings");
            public static string Copy => Loc.Get("复制", "Copy");
            public static string CopyAll => Loc.Get("复制全部", "Copy All");
            public static string You => Loc.Get("你", "You");
            public static string AI => Loc.Get("AI", "AI");
            public static string User => Loc.Get("用户", "User");

            // 状态
            public static string Copied => Loc.Get("已复制到剪贴板", "Copied to clipboard");
            public static string AllCopied => Loc.Get("已复制全部聊天记录到剪贴板", "All chat history copied to clipboard");
            public static string Interrupted => Loc.Get("操作已被用户中断", "Operation interrupted by user");
            public static string ChatCleared => Loc.Get("聊天记录已清空。有什么可以帮你的吗？", "Chat history cleared. How can I help you?");
            public static string ToolExecutionLog => Loc.Get("工具执行日志:", "Tool Execution Log:");

            // 欢迎消息
            public static string WelcomeConfigured(string provider, string model) => Loc.Get(
                $"你好！我是 AI Operator，可以帮你操作 Unity。\n\n" +
                $"当前使用: {provider} - {model}\n\n" +
                $"我可以直接帮你操作 Unity，试试说：\n" +
                $"• 创建一个红色的 Cube\n" +
                $"• 把 Cube 移动到 (0, 5, 0)\n" +
                $"• 给选中的物体添加 Rigidbody\n" +
                $"• 场景里有什么物体\n" +
                $"• 复制选中的物体",
                $"Hello! I'm AI Operator, here to help you work with Unity.\n\n" +
                $"Current: {provider} - {model}\n\n" +
                $"I can directly operate Unity for you. Try saying:\n" +
                $"• Create a red Cube\n" +
                $"• Move Cube to (0, 5, 0)\n" +
                $"• Add Rigidbody to selected object\n" +
                $"• What objects are in the scene\n" +
                $"• Duplicate selected object"
            );

            public static string WelcomeNotConfigured => Loc.Get(
                "欢迎使用 AI Operator！\n\n" +
                "⚠️ 你还没有配置 API Key\n\n" +
                "请点击右上角的【设置】按钮：\n" +
                "1. 选择 AI 服务（推荐 Claude）\n" +
                "2. 获取并填写 API Key\n" +
                "3. 测试连接并保存\n\n" +
                "配置完成后就可以开始使用了！",
                "Welcome to AI Operator!\n\n" +
                "⚠️ API Key not configured yet\n\n" +
                "Click the [Settings] button in the top right:\n" +
                "1. Select an AI service (Claude recommended)\n" +
                "2. Get and enter your API Key\n" +
                "3. Test connection and save\n\n" +
                "You can start using it after configuration!"
            );

            // 错误消息
            public static string ErrorPrefix => Loc.Get("❌ 错误: ", "❌ Error: ");
            public static string ErrorCheckBedrock => Loc.Get(
                "\n\n请检查：\n1. AWS 凭证是否正确\n2. Region 是否正确\n3. 模型 ID 是否正确",
                "\n\nPlease check:\n1. AWS credentials are correct\n2. Region is correct\n3. Model ID is correct"
            );
            public static string ErrorCheckAPI => Loc.Get(
                "\n\n请检查：\n1. API Key 是否正确\n2. 网络连接是否正常",
                "\n\nPlease check:\n1. API Key is correct\n2. Network connection is working"
            );

            // 新增 UI 功能
            public static string QuickCommands => Loc.Get("快捷", "Quick");
            public static string TryCommands => Loc.Get("试试:", "Try:");
            public static string History => Loc.Get("历史", "Hist");
            public static string MoreHistory => Loc.Get("更多历史", "more");
            public static string ShortcutHint => Loc.Get("Enter 发送 | Shift+Enter 换行 | Ctrl+↑↓ 历史", "Enter to send | Shift+Enter newline | Ctrl+↑↓ history");
        }

        // ========== Settings Window ==========
        public static class Settings
        {
            public static string WindowTitle => Loc.Get("AI Operator 设置", "AI Operator Settings");
            public static string Title => Loc.Get("AI Operator 设置", "AI Operator Settings");

            // 语言设置
            public static string LanguageSection => Loc.Get("语言 / Language", "Language");
            public static string LanguageLabel => Loc.Get("界面语言", "Interface Language");

            // Provider 选择
            public static string ProviderSection => Loc.Get("1. 选择 AI 服务", "1. Select AI Service");
            public static string ProviderHint => Loc.Get("选择你想使用的 AI 服务提供商", "Choose your preferred AI service provider");
            public static string ProviderLabel => Loc.Get("AI 服务", "AI Service");

            // API Key 配置
            public static string APIKeySection => Loc.Get("2. 配置 API Key", "2. Configure API Key");
            public static string APIKeyLabel => Loc.Get("API Key", "API Key");
            public static string GetAPIKey => Loc.Get("获取 API Key →", "Get API Key →");

            // AWS Bedrock 配置
            public static string AWSSection => Loc.Get("2. 配置 AWS 凭证", "2. Configure AWS Credentials");
            public static string AWSRegionLabel => Loc.Get("AWS Region", "AWS Region");
            public static string AWSAuthMethod => Loc.Get("认证方式（选择其一）：", "Authentication (choose one):");
            public static string AWSBearerMethod => Loc.Get("方式 1: Bearer Token（推荐 - 简单快速）", "Method 1: Bearer Token (Recommended - Simple & Fast)");
            public static string AWSBearerLabel => Loc.Get("Bearer Token", "Bearer Token");
            public static string AWSOr => Loc.Get("或", "or");
            public static string AWSAccessKeyMethod => Loc.Get("方式 2: Access Key + Secret（传统方式）", "Method 2: Access Key + Secret (Traditional)");
            public static string AWSAccessKeyLabel => Loc.Get("AWS Access Key ID", "AWS Access Key ID");
            public static string AWSSecretKeyLabel => Loc.Get("AWS Secret Access Key", "AWS Secret Access Key");
            public static string GetAWSCredentials => Loc.Get("获取 AWS 凭证 →", "Get AWS Credentials →");

            // 模型选择
            public static string ModelSection => Loc.Get("3. 选择模型", "3. Select Model");
            public static string ModelLabel => Loc.Get("模型", "Model");

            // 测试连接
            public static string TestSection => Loc.Get("4. 测试连接", "4. Test Connection");
            public static string TestButton => Loc.Get("测试 API 连接", "Test API Connection");
            public static string Testing => Loc.Get("测试中...", "Testing...");

            // 保存
            public static string SaveButton => Loc.Get("保存设置", "Save Settings");
            public static string SaveSuccess => Loc.Get("保存成功", "Save Successful");
            public static string SaveSuccessMessage => Loc.Get(
                "设置已保存！\n\n请重新打开 AI Operator 窗口以应用新配置。",
                "Settings saved!\n\nPlease reopen AI Operator window to apply new configuration."
            );

            // 帮助
            public static string HelpSection => Loc.Get("帮助信息", "Help");
            public static string HelpContent => Loc.Get(
                "使用步骤：\n" +
                "1. 选择你想使用的 AI 服务\n" +
                "2. 点击【获取 API Key】按钮，注册并获取密钥\n" +
                "3. 将 API Key 粘贴到上面的输入框\n" +
                "4. 选择合适的模型（推荐使用默认）\n" +
                "5. 点击【测试连接】确保配置正确\n" +
                "6. 保存设置并开始使用！",
                "Steps:\n" +
                "1. Select your preferred AI service\n" +
                "2. Click [Get API Key] to register and get your key\n" +
                "3. Paste the API Key in the input field above\n" +
                "4. Select a model (default recommended)\n" +
                "5. Click [Test Connection] to verify\n" +
                "6. Save settings and start using!"
            );

            // 配置管理
            public static string ConfigSection => Loc.Get("配置管理", "Configuration");
            public static string ResetDefault => Loc.Get("恢复默认", "Reset Default");
            public static string ExportConfig => Loc.Get("导出配置", "Export");
            public static string ImportConfig => Loc.Get("导入配置", "Import");
            public static string ResetConfirm => Loc.Get("确定恢复默认设置？", "Reset to default settings?");
            public static string ResetConfirmMessage => Loc.Get(
                "这将清除所有配置（包括 API Key）。\n确定要继续吗？",
                "This will clear all settings (including API Key).\nAre you sure?"
            );
            public static string ConfigExported => Loc.Get("配置已导出", "Configuration Exported");
            public static string ConfigExportedMessage => Loc.Get("配置已导出到:\n", "Configuration exported to:\n");
            public static string ConfigImported => Loc.Get("配置已导入", "Configuration Imported");
            public static string ConfigImportedMessage => Loc.Get("配置导入成功！请重新打开窗口以应用。", "Configuration imported! Please reopen window to apply.");
            public static string ConfigImportFailed => Loc.Get("导入失败", "Import Failed");
            public static string ConfigImportFailedMessage => Loc.Get("无法读取配置文件或文件格式错误。", "Cannot read config file or invalid format.");

            // Provider 帮助文本
            public static string ClaudeHelp => Loc.Get(
                "Claude API 是 Anthropic 提供的 AI 服务，擅长代码和 Unity 开发。\n新用户有免费额度可以试用。",
                "Claude API is an AI service by Anthropic, excellent for coding and Unity development.\nNew users get free trial credits."
            );
            public static string BedrockHelp => Loc.Get(
                "AWS Bedrock 是通过 AWS 调用 Claude 的服务。\n需要 AWS 账号和 IAM 凭证。\n请确保你的 AWS 账号已开通 Bedrock 服务权限。",
                "AWS Bedrock provides Claude through AWS.\nRequires AWS account and IAM credentials.\nMake sure Bedrock service is enabled in your AWS account."
            );
            public static string OpenAIHelp => Loc.Get(
                "OpenAI 是 ChatGPT 的提供商，提供强大的 AI 能力。\n需要注册并充值才能使用。",
                "OpenAI is the provider of ChatGPT with powerful AI capabilities.\nRegistration and payment required."
            );
            public static string DeepSeekHelp => Loc.Get(
                "DeepSeek 是国内 AI 服务，价格便宜，支持支付宝充值。\n1 元可以使用很久。",
                "DeepSeek is a Chinese AI service with affordable pricing.\nSupports Alipay payment."
            );
            public static string QwenHelp => Loc.Get(
                "通义千问是阿里云提供的大语言模型服务。\n需要注册阿里云并开通 DashScope 服务。\n新用户有免费额度。",
                "Qwen is Alibaba Cloud's LLM service.\nRequires Alibaba Cloud account with DashScope enabled.\nNew users get free credits."
            );
            public static string GLM4Help => Loc.Get(
                "智谱清言 GLM-4 是智谱 AI 提供的国产大模型。\n注册后有免费额度，支持微信支付充值。",
                "GLM-4 is a Chinese LLM by Zhipu AI.\nFree credits after registration, supports WeChat Pay."
            );
            public static string DoubaoHelp => Loc.Get(
                "豆包是字节跳动火山引擎提供的 AI 服务。\n需要注册火山引擎并开通方舟大模型服务。\n支持支付宝充值，新用户有免费额度。",
                "Doubao is ByteDance's Volcengine AI service.\nRequires Volcengine account with Ark model enabled.\nSupports Alipay, new users get free credits."
            );

            // 模型描述
            public static string ModelSonnet => Loc.Get(
                "Sonnet - 平衡性能和速度，最适合 Unity 开发（推荐）",
                "Sonnet - Balanced performance and speed, best for Unity development (Recommended)"
            );
            public static string ModelHaiku => Loc.Get(
                "Haiku - 最快最便宜，适合简单任务",
                "Haiku - Fastest and cheapest, good for simple tasks"
            );
            public static string ModelOpus => Loc.Get(
                "Opus - 最强大，适合复杂任务（较贵）",
                "Opus - Most powerful, for complex tasks (expensive)"
            );
            public static string ModelGPT4oMini => Loc.Get(
                "GPT-4o Mini - 快速便宜，适合日常使用（推荐）",
                "GPT-4o Mini - Fast and affordable for daily use (Recommended)"
            );
            public static string ModelGPT4o => Loc.Get(
                "GPT-4o - OpenAI 最新模型，能力强大",
                "GPT-4o - OpenAI's latest model, very capable"
            );
            public static string ModelDeepSeekChat => Loc.Get(
                "DeepSeek Chat - 通用对话模型（推荐）",
                "DeepSeek Chat - General conversation model (Recommended)"
            );
            public static string ModelDeepSeekCoder => Loc.Get(
                "DeepSeek Coder - 专门的代码模型",
                "DeepSeek Coder - Specialized coding model"
            );
            public static string ModelQwenPlus => Loc.Get(
                "Qwen Plus - 效果最好，推荐日常使用",
                "Qwen Plus - Best performance, recommended for daily use"
            );
            public static string ModelQwenTurbo => Loc.Get(
                "Qwen Turbo - 速度最快，适合简单任务",
                "Qwen Turbo - Fastest, good for simple tasks"
            );
            public static string ModelQwenMax => Loc.Get(
                "Qwen Max - 最强能力，适合复杂任务",
                "Qwen Max - Most capable, for complex tasks"
            );
            public static string ModelGLM4Plus => Loc.Get(
                "GLM-4 Plus - 效果最好（推荐）",
                "GLM-4 Plus - Best performance (Recommended)"
            );
            public static string ModelGLM4Flash => Loc.Get(
                "GLM-4 Flash - 速度最快，适合简单任务",
                "GLM-4 Flash - Fastest, good for simple tasks"
            );
            public static string ModelGLM4 => Loc.Get(
                "GLM-4 - 标准版本，性能均衡",
                "GLM-4 - Standard version, balanced performance"
            );
            public static string ModelDoubaoPro => Loc.Get(
                "豆包 1.5 Pro - 最新版本，效果最好（推荐）",
                "Doubao 1.5 Pro - Latest version, best performance (Recommended)"
            );
            public static string ModelDoubaoLite => Loc.Get(
                "豆包 1.5 Lite - 轻量版，速度快",
                "Doubao 1.5 Lite - Lightweight, fast"
            );
            public static string ModelDoubaoSeed => Loc.Get(
                "豆包 Seed - 基础版本，适合简单任务",
                "Doubao Seed - Basic version, for simple tasks"
            );
            public static string ModelDefault => Loc.Get(
                "当前选择的模型",
                "Currently selected model"
            );
        }

        // ========== Tool Results ==========
        public static class Tool
        {
            // 通用
            public static string NotFound(string name) => Loc.Get($"未找到: '{name}'", $"Not found: '{name}'");
            public static string MissingParam(string param) => Loc.Get($"缺少必填参数: {param}", $"Missing required parameter: {param}");
            public static string InvalidParam(string param, string reason) => Loc.Get($"参数 '{param}' 无效: {reason}", $"Invalid parameter '{param}': {reason}");

            // GameObject
            public static string Created(string name, string type) => Loc.Get($"已创建 {type}: '{name}'", $"Created {type}: '{name}'");
            public static string Deleted(string name) => Loc.Get($"已删除: '{name}'", $"Deleted: '{name}'");
            public static string Duplicated(string original, string copy) => Loc.Get($"已复制 '{original}' 为 '{copy}'", $"Duplicated '{original}' as '{copy}'");

            // Transform
            public static string TransformUpdated(string name) => Loc.Get($"已更新 '{name}' 的 Transform", $"Updated Transform of '{name}'");

            // Selection
            public static string NoSelection => Loc.Get("当前没有选中任何物体", "No objects currently selected");
            public static string Selected(string name) => Loc.Get($"已选中物体: '{name}'", $"Selected: '{name}'");
            public static string SelectionCount(int count) => Loc.Get($"当前选中 {count} 个物体:", $"Currently selected {count} object(s):");

            // Hierarchy
            public static string ParentSet(string child, string parent) => Loc.Get($"已将 '{child}' 设为 '{parent}' 的子物体", $"Set '{child}' as child of '{parent}'");
            public static string ParentCleared(string name) => Loc.Get($"已将 '{name}' 移到根层级", $"Moved '{name}' to root level");
            public static string NoChildren(string name) => Loc.Get($"'{name}' 没有子物体", $"'{name}' has no children");

            // Component
            public static string ComponentAdded(string target, string component) => Loc.Get($"已为 '{target}' 添加 {component} 组件", $"Added {component} component to '{target}'");
            public static string ComponentRemoved(string target, string component) => Loc.Get($"已从 '{target}' 移除 {component} 组件", $"Removed {component} component from '{target}'");
            public static string ComponentExists(string target, string component) => Loc.Get($"'{target}' 上已存在 {component} 组件", $"'{target}' already has {component} component");
            public static string ComponentNotFound(string target, string component) => Loc.Get($"在 '{target}' 上未找到 {component} 组件", $"Component {component} not found on '{target}'");
            public static string CannotRemoveTransform => Loc.Get("不能移除 Transform 组件", "Cannot remove Transform component");
            public static string NoRenderer(string name) => Loc.Get($"'{name}' 没有 Renderer 组件，无法设置颜色", $"'{name}' has no Renderer component, cannot set color");

            // Script
            public static string ScriptCreated(string path) => Loc.Get($"已创建脚本: '{path}'", $"Created script: '{path}'");
            public static string ScriptAttached(string script, string target) => Loc.Get($"已将脚本 '{script}' 挂载到 '{target}'", $"Attached script '{script}' to '{target}'");
            public static string ScriptNotFound(string name) => Loc.Get($"未找到脚本 '{name}'", $"Script '{name}' not found");
            public static string ScriptExists(string path) => Loc.Get($"脚本已存在: {path}", $"Script already exists: {path}");

            // Prefab
            public static string PrefabSaved(string path) => Loc.Get($"已保存 Prefab: '{path}'", $"Saved Prefab: '{path}'");
            public static string PrefabUpdated(string path) => Loc.Get($"已更新 Prefab: '{path}'", $"Updated Prefab: '{path}'");
            public static string PrefabInstantiated(string name) => Loc.Get($"已实例化 Prefab: '{name}'", $"Instantiated Prefab: '{name}'");

            // Scene
            public static string NoValidScene => Loc.Get("当前没有有效的场景", "No valid scene currently");
            public static string SceneInfo(string name) => Loc.Get($"场景信息:", $"Scene Info:");
            public static string SceneAnalysisTitle(string name) => Loc.Get($"=== 场景分析报告: {name} ===", $"=== Scene Analysis Report: {name} ===");
            public static string SceneHealthPassed => Loc.Get("✅ 场景健康检查通过！没有发现问题。", "✅ Scene health check passed! No issues found.");

            // Console
            public static string NoLogs => Loc.Get("控制台没有日志", "Console has no logs");
            public static string ConsoleCleared => Loc.Get("已清空控制台", "Console cleared");

            // UI
            public static string CanvasCreated => Loc.Get("已创建 Canvas 和 EventSystem", "Created Canvas and EventSystem");
            public static string CanvasExists => Loc.Get("场景中已存在 Canvas", "Canvas already exists in scene");
            public static string UIElementCreated(string type, string name) => Loc.Get($"已创建 UI {type}: '{name}'", $"Created UI {type}: '{name}'");

            // Batch
            public static string BatchCreated(int count, string type) => Loc.Get($"已批量创建 {count} 个 {type}", $"Batch created {count} {type}(s)");

            // 循环引用
            public static string CircularReference(string parent, string child) => Loc.Get(
                $"不能将 '{parent}' 设为 '{child}' 的父物体，因为这会造成循环引用",
                $"Cannot set '{parent}' as parent of '{child}' as it would create a circular reference"
            );
        }
    }
}
