# Publishing Guide / 发布指南

本文档说明如何将 AI Operator 打包并发布到 Unity Asset Store。

---

## 1. 发布前检查清单

### 代码检查
- [ ] 所有编译错误已修复
- [ ] 无 Debug.Log 残留（生产环境）
- [ ] 无硬编码的测试 API Key
- [ ] 代码注释完整
- [ ] 命名空间规范 (AIOperator.*)

### 文件检查
- [ ] 删除临时文件和测试文件
- [ ] 删除 .git 目录（如有）
- [ ] 删除 IDE 配置文件 (.vs, .idea)
- [ ] 所有 .meta 文件存在
- [ ] package.json 版本号正确
- [ ] LICENSE 文件存在

### 功能检查
- [ ] 聊天窗口正常打开
- [ ] 设置窗口正常打开
- [ ] 语言切换正常
- [ ] 配置保存/加载正常
- [ ] 至少一个 Provider 测试通过

---

## 2. Unity Package 导出

### 步骤

1. **打开 Unity 项目**

2. **选择要导出的文件夹**
   ```
   Assets/AIOperator/
   ```

3. **导出 Package**
   - 菜单: `Assets > Export Package...`
   - 确保勾选所有必要文件
   - 点击 `Export...`
   - 保存为 `AIOperator_v1.0.0.unitypackage`

### 导出内容清单
```
AIOperator/
├── Documentation/
│   ├── README.md
│   ├── Configuration.md
│   ├── UserGuide.md
│   ├── ToolReference.md
│   ├── FAQ.md
│   └── CHANGELOG.md
├── Editor/
│   ├── AIOperatorWindow.cs
│   ├── AIOperatorSettings.cs
│   ├── Localization/
│   │   ├── Localization.cs
│   │   └── LocalizedStrings.cs
│   └── Tools/
│       ├── Core/
│       ├── Executors/
│       └── ...
├── LLM/
│   ├── ILLMProvider.cs
│   ├── SystemPromptBuilder.cs
│   ├── Providers/
│   └── Models/
├── Marketing/
│   ├── AssetPrepGuide.md
│   ├── AssetStoreDescription.md
│   └── PublishGuide.md
├── LICENSE
└── package.json
```

---

## 3. 新项目测试

### 测试步骤

1. **创建新 Unity 项目**
   - Unity Hub > New Project
   - 选择 Unity 2021.3 或更高版本
   - 使用 3D 模板

2. **导入 Package**
   - `Assets > Import Package > Custom Package...`
   - 选择导出的 .unitypackage
   - 点击 Import All

3. **验证导入**
   - 检查无编译错误
   - 打开 `Window > AI Operator > Settings`
   - 打开 `Window > AI Operator > Chat Window`

4. **功能测试**
   - 配置一个 API Key (如 DeepSeek)
   - 发送测试命令："Create a Cube"
   - 验证物体创建成功

---

## 4. Asset Store 提交

### 准备工作

1. **创建 Publisher 账号**
   - 访问 [publisher.unity.com](https://publisher.unity.com)
   - 注册/登录 Unity 账号
   - 完成 Publisher 认证

2. **准备素材**
   参考 `AssetPrepGuide.md` 准备：
   - 至少 5 张截图 (1920x1080)
   - 产品图标 (512x512)
   - 封面图 (1200x630)
   - 演示视频 (可选)

### 提交流程

1. **创建 Package**
   - 登录 Publisher Portal
   - 点击 "Create New Package"
   - 填写基本信息

2. **填写产品信息**

   **Title**: AI Operator - Unity AI Assistant

   **Category**: Editor Extensions > AI

   **Description**: 参考 `AssetStoreDescription.md`

   **Technical Details**:
   - Unity Version: 2021.3+
   - Platforms: Editor only
   - Dependencies: None
   - File Size: ~500KB

3. **上传素材**
   - Key Images (封面)
   - Screenshots (截图)
   - Icon (图标)

4. **上传 Package**
   - 上传 .unitypackage 文件

5. **设置定价**
   推荐定价：
   - 标准版: $29 USD
   - 或根据市场调研调整

6. **提交审核**
   - 检查所有信息
   - 点击 Submit for Review
   - 等待审核 (通常 3-10 个工作日)

---

## 5. GitHub 发布 (可选)

### 创建免费版仓库

1. **创建 GitHub 仓库**
   ```
   Repository: ai-operator-free
   Description: AI Operator Free - Control Unity with Natural Language
   ```

2. **准备免费版内容**
   - 保留基础功能
   - 限制 Provider 数量 (如只保留 DeepSeek)
   - 限制工具数量 (如保留 15 个核心工具)
   - 添加付费版广告

3. **创建 Release**
   ```
   Tag: v1.0.0-free
   Title: AI Operator Free v1.0.0
   Attach: AIOperator_Free_v1.0.0.unitypackage
   ```

---

## 6. 宣传计划

### 发布公告

#### Reddit
- r/Unity3D
- r/gamedev
- r/indiegames

**帖子模板**:
```
[Asset Release] AI Operator - Control Unity with Natural Language

Just released AI Operator on Asset Store!

Features:
- Natural language Unity control
- 30+ built-in tools
- Multiple AI services supported
- Bilingual (EN/CN)

Link: [Asset Store URL]
Demo Video: [YouTube URL]

Happy to answer any questions!
```

#### Twitter/X
```
Excited to release AI Operator - Unity AI Assistant!

Control Unity with natural language:
"Create a red Cube" ✓
"Add Rigidbody to Player" ✓
"Create 10 enemies in a circle" ✓

Asset Store: [URL]
#Unity3D #gamedev #AI #indiedev
```

#### B站/知乎 (中文社区)
```
【Unity 插件】AI Operator - 用自然语言控制 Unity

功能特点：
- 中英文双语支持
- 30+ 内置工具
- 支持 Claude、DeepSeek、通义千问等
- 一句话完成复杂工作流

Asset Store 链接：[URL]
演示视频：[B站链接]
```

---

## 7. 版本更新计划

### v1.1.0 (计划)
- [ ] 添加更多 AI 服务支持
- [ ] 动画系统工具
- [ ] 资源导入工具

### v1.2.0 (计划)
- [ ] 代码编辑功能
- [ ] 多场景支持
- [ ] 性能优化

---

## 8. 支持和维护

### 用户支持
- 邮件: support@aioperator.com
- GitHub Issues
- Asset Store 评论回复

### Bug 修复流程
1. 收到 Bug 报告
2. 复现问题
3. 修复并测试
4. 发布补丁版本 (v1.0.1, v1.0.2, ...)
5. 通知用户更新

### 更新发布
- 重大更新: 提前通知用户
- 修复更新: 尽快发布
- 保持 CHANGELOG 更新
