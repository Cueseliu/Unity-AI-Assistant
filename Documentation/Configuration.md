# Configuration Guide / 配置指南

[中文](#中文) | [English](#english)

---

## 中文

### 打开设置窗口

菜单栏: `Window > AI Operator > Settings`

### Claude API 配置

Claude 是 Anthropic 公司开发的 AI，效果最好，推荐使用。

1. 访问 [console.anthropic.com](https://console.anthropic.com/settings/keys)
2. 注册账号（新用户有免费额度）
3. 创建 API Key
4. 在设置窗口中:
   - 选择 Provider: `Claude`
   - 粘贴 API Key
   - 选择模型（推荐 `claude-sonnet-4-20250514`）
5. 点击"测试连接"验证
6. 保存设置

**模型说明:**
- **Sonnet** - 平衡性能和速度，最适合 Unity 开发（推荐）
- **Haiku** - 最快最便宜，适合简单任务
- **Opus** - 最强大，适合复杂任务（较贵）

### DeepSeek API 配置

DeepSeek 是国产 AI 服务，价格便宜，支持支付宝。

1. 访问 [platform.deepseek.com](https://platform.deepseek.com/api_keys)
2. 注册并充值（1 元可用很久）
3. 创建 API Key
4. 在设置窗口中配置

**模型说明:**
- **deepseek-chat** - 通用对话模型（推荐）
- **deepseek-coder** - 专门的代码模型

### AWS Bedrock 配置

AWS Bedrock 是通过 AWS 调用 Claude，适合企业用户。

支持两种认证方式：

#### 方式 1: Bearer Token（推荐）
1. 登录 AWS Console
2. 获取 Bedrock Bearer Token
3. 在设置窗口填入

#### 方式 2: Access Key
1. 访问 [AWS IAM](https://console.aws.amazon.com/iam/home#/security_credentials)
2. 创建 Access Key
3. 在设置窗口填入 Access Key ID 和 Secret Access Key

**注意:** 确保你的 AWS 账号已开通 Bedrock 服务权限。

### 通义千问配置

阿里云提供的 AI 服务。

1. 访问 [dashscope.console.aliyun.com](https://dashscope.console.aliyun.com/apiKey)
2. 开通 DashScope 服务
3. 创建 API Key
4. 在设置窗口配置

### 智谱 GLM-4 配置

国产大模型，注册有免费额度。

1. 访问 [open.bigmodel.cn](https://open.bigmodel.cn/usercenter/apikeys)
2. 注册账号
3. 创建 API Key
4. 在设置窗口配置

### 豆包配置

字节跳动的 AI 服务。

1. 访问 [console.volcengine.com](https://console.volcengine.com/ark/region:ark+cn-beijing/apiKey)
2. 开通方舟大模型服务
3. 创建 API Key
4. 在设置窗口配置

### 配置管理

#### 导出配置
点击"导出配置"可将当前设置保存为 JSON 文件，方便备份或迁移到其他项目。

#### 导入配置
点击"导入配置"可从 JSON 文件恢复设置。

#### 恢复默认
点击"恢复默认"可清除所有配置，恢复初始状态。

---

## English

### Open Settings Window

Menu: `Window > AI Operator > Settings`

### Claude API Configuration

Claude by Anthropic offers the best performance and is recommended.

1. Visit [console.anthropic.com](https://console.anthropic.com/settings/keys)
2. Create an account (new users get free credits)
3. Create an API Key
4. In the settings window:
   - Select Provider: `Claude`
   - Paste API Key
   - Select model (recommend `claude-sonnet-4-20250514`)
5. Click "Test Connection" to verify
6. Save settings

**Model descriptions:**
- **Sonnet** - Balanced performance and speed, best for Unity dev (recommended)
- **Haiku** - Fastest and cheapest, for simple tasks
- **Opus** - Most powerful, for complex tasks (expensive)

### DeepSeek API Configuration

DeepSeek is an affordable Chinese AI service.

1. Visit [platform.deepseek.com](https://platform.deepseek.com/api_keys)
2. Register and top up
3. Create API Key
4. Configure in settings window

**Model descriptions:**
- **deepseek-chat** - General conversation model (recommended)
- **deepseek-coder** - Specialized coding model

### AWS Bedrock Configuration

AWS Bedrock provides Claude through AWS, suitable for enterprise users.

Two authentication methods supported:

#### Method 1: Bearer Token (Recommended)
1. Log in to AWS Console
2. Get Bedrock Bearer Token
3. Enter in settings window

#### Method 2: Access Key
1. Visit [AWS IAM](https://console.aws.amazon.com/iam/home#/security_credentials)
2. Create Access Key
3. Enter Access Key ID and Secret Access Key in settings

**Note:** Ensure your AWS account has Bedrock service enabled.

### Qwen Configuration

AI service by Alibaba Cloud.

1. Visit [dashscope.console.aliyun.com](https://dashscope.console.aliyun.com/apiKey)
2. Enable DashScope service
3. Create API Key
4. Configure in settings window

### GLM-4 Configuration

Chinese LLM with free credits for new users.

1. Visit [open.bigmodel.cn](https://open.bigmodel.cn/usercenter/apikeys)
2. Create an account
3. Create API Key
4. Configure in settings window

### Doubao Configuration

AI service by ByteDance.

1. Visit [console.volcengine.com](https://console.volcengine.com/ark/region:ark+cn-beijing/apiKey)
2. Enable Ark model service
3. Create API Key
4. Configure in settings window

### Configuration Management

#### Export Config
Click "Export" to save current settings as a JSON file for backup or migration.

#### Import Config
Click "Import" to restore settings from a JSON file.

#### Reset Default
Click "Reset Default" to clear all settings and restore to initial state.
