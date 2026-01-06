# Asset Preparation Guide / 素材准备指南

本文档说明 Unity Asset Store 上架所需的全部素材。

---

## 1. 截图 Screenshots (必须 5 张以上)

### 截图规格
- **分辨率**: 1920x1080 (Full HD)
- **格式**: PNG (推荐) 或 JPG
- **命名**: `screenshot_01.png`, `screenshot_02.png`, ...

### 必须截图列表

#### Screenshot 1: 主界面展示
**文件名**: `screenshot_01_main_interface.png`
**内容**:
- 完整的 Chat Window 界面
- 显示几条对话消息（用户问题 + AI 回复）
- 显示快捷命令栏
- 右侧可见 Unity Scene 视图有几个创建的物体

**操作步骤**:
1. 打开 AI Operator Chat Window
2. 发送命令："Create a red Cube"
3. 发送命令："Create a blue Sphere at (2, 0, 0)"
4. 展开快捷命令栏
5. 调整窗口大小，截图

#### Screenshot 2: 设置界面
**文件名**: `screenshot_02_settings.png`
**内容**:
- 设置窗口完整展示
- 显示 Provider 选择下拉框
- 显示语言选择
- 显示配置管理按钮

**操作步骤**:
1. 打开 Settings 窗口
2. 展示所有配置选项
3. 截图

#### Screenshot 3: 批量操作演示
**文件名**: `screenshot_03_batch_create.png`
**内容**:
- 展示批量创建物体的结果
- 显示"创建 10 个 Cube 排成一排"的对话
- Scene 视图显示创建的物体

**操作步骤**:
1. 清空场景
2. 发送命令："Create 10 Cubes in a row"
3. 截图展示结果

#### Screenshot 4: UI 创建演示
**文件名**: `screenshot_04_ui_creation.png`
**内容**:
- 展示创建的 UI 元素
- Canvas、Button、Text 等
- Game 视图显示 UI 效果

**操作步骤**:
1. 发送命令："Create a Canvas with a Start Game button"
2. 切换到 Game 视图
3. 截图

#### Screenshot 5: 场景分析演示
**文件名**: `screenshot_05_scene_analysis.png`
**内容**:
- 展示 analyze_scene 工具的输出
- 显示场景健康检查结果
- 问题列表和建议

**操作步骤**:
1. 创建一个有潜在问题的场景（如缺少 Light）
2. 发送命令："Analyze scene"
3. 截图显示分析结果

#### Screenshot 6 (可选): 工作流演示
**文件名**: `screenshot_06_workflow.png`
**内容**:
- 展示复杂工作流的结果
- 如"创建玩家角色"后的完整物体结构

---

## 2. 封面图 Key Images

### Asset Store 主封面
**文件名**: `cover_1200x630.jpg`
**规格**: 1200 x 630 像素, JPG 格式
**内容建议**:
- 产品名称 "AI Operator"
- 副标题 "Unity AI Assistant"
- 展示界面截图或抽象的 AI + Unity 图形
- 清晰的品牌标识

### 产品图标
**文件名**: `icon_512x512.png`
**规格**: 512 x 512 像素, PNG 透明背景
**内容建议**:
- 简洁的图标设计
- AI 或聊天气泡元素
- Unity 风格的配色

### 社交媒体封面
**文件名**: `social_1200x630.jpg`
**规格**: 1200 x 630 像素
**用途**: Twitter, Reddit, Discord 分享

---

## 3. 演示视频 Demo Videos

### 快速演示视频 (60秒)
**文件名**: `demo_60s.mp4`
**规格**: 1920x1080, 60fps, MP4 格式
**内容**:
```
0-10s: 产品 Logo + 名称
10-25s: 基础对话演示（创建物体）
25-40s: 批量操作演示
40-55s: UI 创建演示
55-60s: 结束画面 + 下载链接
```

### 完整功能视频 (3-5分钟)
**文件名**: `demo_full.mp4`
**规格**: 1920x1080, 30fps, MP4 格式
**内容**:
```
0-30s: 介绍和安装
30s-1m: 配置 API Key
1m-2m: 基础对话功能
2m-3m: 工作流演示
3m-4m: UI 创建
4m-5m: 高级功能和总结
```

### 安装教程视频
**文件名**: `tutorial_install.mp4`
**规格**: 1920x1080
**内容**: 从导入到配置的完整流程

---

## 4. GIF 动图

### 基础对话 GIF
**文件名**: `gif_basic_chat.gif`
**规格**: 800x600 或 640x480
**内容**: 输入命令 -> AI 响应 -> 物体创建
**时长**: 5-10 秒循环

### 批量创建 GIF
**文件名**: `gif_batch_create.gif`
**内容**: 一句话创建多个物体的动画效果

### UI 创建 GIF
**文件名**: `gif_ui_creation.gif`
**内容**: 创建按钮和 UI 元素的过程

---

## 5. 素材清单 Checklist

### 必须准备
- [ ] screenshot_01_main_interface.png
- [ ] screenshot_02_settings.png
- [ ] screenshot_03_batch_create.png
- [ ] screenshot_04_ui_creation.png
- [ ] screenshot_05_scene_analysis.png
- [ ] cover_1200x630.jpg
- [ ] icon_512x512.png
- [ ] demo_60s.mp4

### 推荐准备
- [ ] screenshot_06_workflow.png
- [ ] social_1200x630.jpg
- [ ] demo_full.mp4
- [ ] tutorial_install.mp4
- [ ] gif_basic_chat.gif
- [ ] gif_batch_create.gif
- [ ] gif_ui_creation.gif

---

## 6. 录制工具推荐

### 截图工具
- Windows: Snipping Tool, ShareX
- Mac: Screenshot (Cmd+Shift+4)
- Unity: Game View > 右键 > Screenshot

### 视频录制
- OBS Studio (免费，推荐)
- Camtasia
- ScreenFlow (Mac)

### GIF 制作
- ScreenToGif (Windows, 推荐)
- LICEcap
- Gifox (Mac)

### 图片编辑
- Photoshop
- GIMP (免费)
- Canva (在线)

---

## 7. 设计规范

### 品牌颜色
```
主色: #4A90D9 (蓝色 - 代表 AI/科技)
辅色: #2C3E50 (深灰 - Unity 风格)
强调: #27AE60 (绿色 - 成功/完成)
背景: #1E1E1E (深色 - Unity 编辑器)
```

### 字体建议
- 标题: Inter, Roboto, 或 Unity 默认字体
- 正文: 系统默认字体

### 设计原则
1. 保持简洁，突出产品功能
2. 使用 Unity 编辑器风格配色
3. 展示实际使用场景
4. 包含中英文双语元素（可选）
