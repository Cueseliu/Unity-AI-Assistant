# AI Operator - Unity AI Assistant

[中文](#中文) | [English](#english)

---

## 中文

### 简介

AI Operator 是一个强大的 Unity Editor 插件，让你可以通过自然语言与 AI 对话来操作 Unity。无需记忆复杂的 API，只需告诉 AI 你想做什么，它就会帮你完成。

### 功能特性

- **自然语言操作** - 用中文或英文描述你想做的事，AI 自动完成
- **30+ 内置工具** - 涵盖 GameObject、组件、材质、UI、场景管理等
- **多 AI 服务支持** - 支持 Claude、DeepSeek、AWS Bedrock、通义千问、智谱 GLM-4、豆包等
- **工作流自动化** - "创建一个玩家角色" 一句话完成完整流程
- **中英文双语界面** - 随时切换界面语言
- **配置导入导出** - 轻松备份和迁移设置

### 快速开始

#### 1. 导入插件
将 AIOperator 文件夹拖入 Unity 项目的 Assets 目录

#### 2. 打开设置
菜单栏: `Window > AI Operator > Settings`

#### 3. 配置 API
1. 选择 AI 服务 (推荐 Claude 或 DeepSeek)
2. 获取并填写 API Key
3. 点击"测试连接"验证
4. 保存设置

#### 4. 开始使用
菜单栏: `Window > AI Operator > Chat Window`

### 示例命令

```
创建一个红色的 Cube
把 Cube 移动到 (0, 5, 0)
给选中的物体添加 Rigidbody
创建一个玩家角色
创建一个开始游戏按钮
分析一下当前场景
```

### 支持的 AI 服务

| 服务 | 推荐度 | 说明 |
|------|--------|------|
| Claude | ⭐⭐⭐⭐⭐ | Anthropic 出品，效果最好 |
| DeepSeek | ⭐⭐⭐⭐ | 国产，便宜好用 |
| 通义千问 | ⭐⭐⭐⭐ | 阿里云，新用户免费 |
| 智谱 GLM-4 | ⭐⭐⭐ | 国产大模型 |
| 豆包 | ⭐⭐⭐ | 字节跳动出品 |
| AWS Bedrock | ⭐⭐⭐⭐ | 企业级，通过 AWS 调用 Claude |

### 系统要求

- Unity 2021.3 或更高版本
- 互联网连接
- AI 服务 API Key

### 文档

- [配置指南](Configuration.md)
- [使用指南](UserGuide.md)
- [工具参考](ToolReference.md)
- [常见问题](FAQ.md)
- [更新日志](CHANGELOG.md)

### 技术支持

如有问题，请通过以下方式联系：
- GitHub Issues
- Email: support@example.com

---

## English

### Introduction

AI Operator is a powerful Unity Editor plugin that lets you operate Unity through natural language conversations with AI. No need to memorize complex APIs - just tell the AI what you want to do.

### Features

- **Natural Language Control** - Describe what you want in plain English or Chinese
- **30+ Built-in Tools** - GameObject, components, materials, UI, scene management, etc.
- **Multiple AI Services** - Claude, DeepSeek, AWS Bedrock, Qwen, GLM-4, Doubao
- **Workflow Automation** - "Create a player character" completes the full workflow
- **Bilingual Interface** - Switch between Chinese and English anytime
- **Config Import/Export** - Easy backup and migration

### Quick Start

#### 1. Import Plugin
Drag the AIOperator folder into your Unity project's Assets directory

#### 2. Open Settings
Menu: `Window > AI Operator > Settings`

#### 3. Configure API
1. Select AI service (Claude or DeepSeek recommended)
2. Get and enter your API Key
3. Click "Test Connection" to verify
4. Save settings

#### 4. Start Using
Menu: `Window > AI Operator > Chat Window`

### Example Commands

```
Create a red Cube
Move Cube to (0, 5, 0)
Add Rigidbody to selected object
Create a player character
Create a start game button
Analyze current scene
```

### Supported AI Services

| Service | Rating | Notes |
|---------|--------|-------|
| Claude | ⭐⭐⭐⭐⭐ | Best performance by Anthropic |
| DeepSeek | ⭐⭐⭐⭐ | Chinese service, affordable |
| Qwen | ⭐⭐⭐⭐ | Alibaba Cloud, free tier available |
| GLM-4 | ⭐⭐⭐ | Chinese LLM by Zhipu AI |
| Doubao | ⭐⭐⭐ | ByteDance service |
| AWS Bedrock | ⭐⭐⭐⭐ | Enterprise-grade, Claude via AWS |

### System Requirements

- Unity 2021.3 or higher
- Internet connection
- AI service API Key

### Documentation

- [Configuration Guide](Configuration.md)
- [User Guide](UserGuide.md)
- [Tool Reference](ToolReference.md)
- [FAQ](FAQ.md)
- [Changelog](CHANGELOG.md)

### Support

For issues, please contact:
- GitHub Issues
- Email: support@example.com

---

## License

Copyright (c) 2024. All rights reserved.
