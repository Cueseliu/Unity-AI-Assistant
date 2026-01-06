# FAQ / 常见问题

[中文](#中文) | [English](#english)

---

## 中文

### 安装和配置

#### Q: 支持哪些 Unity 版本？
A: 支持 Unity 2021.3 及以上版本。

#### Q: 需要互联网连接吗？
A: 是的，需要连接 AI 服务的 API。

#### Q: 推荐使用哪个 AI 服务？
A: 推荐使用 Claude（效果最好）或 DeepSeek（便宜实惠）。

#### Q: API Key 安全吗？
A: API Key 保存在本地 EditorPrefs 中，不会上传到任何服务器。可以导出配置时选择不包含敏感信息。

### 使用问题

#### Q: AI 不理解我的意思怎么办？
A: 尝试用更具体的描述，比如:
- 不要说："创建个东西"
- 而是说："创建一个 Cube"

#### Q: 可以撤销 AI 的操作吗？
A: 可以使用 Unity 的 Ctrl+Z 撤销大多数操作。

#### Q: AI 创建的脚本在哪里？
A: 默认保存在 `Assets/Scripts/` 目录下。

#### Q: 可以同时对多个物体操作吗？
A: 可以，比如"给所有 Cube 添加 Rigidbody"。

#### Q: 支持中文物体名吗？
A: 支持，可以创建中文名的 GameObject。

### 性能问题

#### Q: 响应很慢怎么办？
A:
1. 检查网络连接
2. 尝试使用更快的模型（如 Haiku）
3. 简化请求内容

#### Q: API 费用高吗？
A:
- Claude: 新用户有免费额度，之后按使用量计费
- DeepSeek: 非常便宜，1 元可用很久

### 错误排查

#### Q: "API Key not configured" 错误
A: 打开设置窗口，填写正确的 API Key。

#### Q: "Network error" 错误
A: 检查网络连接，确保能访问 API 服务。

#### Q: "Rate limit exceeded" 错误
A: API 请求频率超限，等待一会再试。

#### Q: 工具执行失败
A: 查看 Unity Console 中的详细错误信息。

### 功能限制

#### Q: 能直接运行游戏吗？
A: 暂不支持，但可以让 AI 准备好运行所需的一切。

#### Q: 能编辑已有脚本吗？
A: 目前只支持创建新脚本，不支持编辑已有脚本。

#### Q: 能导入外部资源吗？
A: 暂不支持，需要手动导入后才能操作。

---

## English

### Installation and Configuration

#### Q: Which Unity versions are supported?
A: Unity 2021.3 and above are supported.

#### Q: Is internet connection required?
A: Yes, to connect to AI service APIs.

#### Q: Which AI service is recommended?
A: Claude (best performance) or DeepSeek (affordable) are recommended.

#### Q: Is the API Key secure?
A: API Key is stored locally in EditorPrefs and never uploaded to any server. You can export config without sensitive information.

### Usage Questions

#### Q: What if AI doesn't understand me?
A: Try more specific descriptions, like:
- Don't say: "Create something"
- Say instead: "Create a Cube"

#### Q: Can I undo AI's operations?
A: Yes, use Unity's Ctrl+Z to undo most operations.

#### Q: Where are AI-created scripts saved?
A: By default in the `Assets/Scripts/` directory.

#### Q: Can I operate on multiple objects at once?
A: Yes, like "Add Rigidbody to all Cubes".

#### Q: Are Chinese object names supported?
A: Yes, you can create GameObjects with Chinese names.

### Performance Issues

#### Q: Why is the response slow?
A:
1. Check network connection
2. Try a faster model (like Haiku)
3. Simplify your request

#### Q: Is the API expensive?
A:
- Claude: Free credits for new users, then pay-per-use
- DeepSeek: Very cheap, $1 lasts a long time

### Troubleshooting

#### Q: "API Key not configured" error
A: Open settings window and enter correct API Key.

#### Q: "Network error" error
A: Check network connection and ensure API service is accessible.

#### Q: "Rate limit exceeded" error
A: API request rate exceeded, wait and try again.

#### Q: Tool execution failed
A: Check detailed error in Unity Console.

### Feature Limitations

#### Q: Can it run the game directly?
A: Not supported yet, but AI can prepare everything needed for running.

#### Q: Can it edit existing scripts?
A: Currently only supports creating new scripts, not editing existing ones.

#### Q: Can it import external assets?
A: Not supported yet, assets need to be manually imported first.
