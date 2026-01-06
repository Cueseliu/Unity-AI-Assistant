# User Guide / 使用指南

[中文](#中文) | [English](#english)

---

## 中文

### 基础对话

打开聊天窗口后，直接用自然语言告诉 AI 你想做什么即可。

#### 创建物体
```
创建一个 Cube
创建一个红色的 Sphere
在 (0, 5, 0) 创建一个 Cylinder
```

#### 操作物体
```
把 Cube 移动到 (10, 0, 0)
把 Player 旋转 90 度
放大 Enemy 到 2 倍
```

#### 查询信息
```
Player 在哪里
场景里有什么物体
选中了什么
```

### 工作流示例

#### 创建玩家角色

只需说:
```
创建一个玩家角色，可以 WASD 移动
```

AI 会自动:
1. 创建 Player GameObject
2. 添加 Rigidbody 和 Collider
3. 生成 PlayerController 脚本
4. 挂载脚本
5. 保存为 Prefab

#### 创建 UI 系统

```
创建一个游戏主菜单，包含开始游戏和退出按钮
```

AI 会自动:
1. 创建 Canvas 和 EventSystem
2. 创建背景面板
3. 创建标题文本
4. 创建按钮

#### 场景分析

```
分析一下当前场景，看看有什么问题
```

AI 会检查:
- 是否有 Camera
- 是否有光源
- 是否有丢失脚本
- 物理组件配置是否正确
- UI 系统是否完整

### 快捷键

| 快捷键 | 功能 |
|--------|------|
| Enter | 发送消息 |
| Shift + Enter | 换行（多行输入）|
| Ctrl + ↑ | 上一条历史命令 |
| Ctrl + ↓ | 下一条历史命令 |
| Escape | 中断当前操作 |

### 快捷命令栏

首次使用时会显示快捷命令栏，包含常用命令按钮。点击即可直接执行。

可以通过标题栏的"快捷"按钮显示/隐藏。

### 历史命令

点击输入框旁边的"历史"按钮可查看最近使用的命令，点击即可重新填入。

也可以使用 Ctrl+↑/↓ 快速浏览历史。

### 批量操作

```
创建 10 个 Cube 排成一排
给所有 Cube 添加 Rigidbody
把所有 Enemy 设为红色
删除所有 Temp 开头的物体
```

### 高级技巧

#### 组合命令
可以在一条消息中包含多个操作:
```
创建一个 Cube，移动到 (0, 5, 0)，设为红色，添加 Rigidbody
```

#### 相对操作
```
把 Player 向上移动 5 米
把选中物体旋转 45 度
```

#### 使用选中物体
```
给选中的物体添加 Collider
复制选中的物体
删除选中的物体
```

### 常见问题处理

#### API Key 错误
检查 API Key 是否正确，是否有足够余额。

#### 网络超时
检查网络连接，尝试重新发送。

#### 工具执行失败
查看工具执行日志了解详细错误信息。

---

## English

### Basic Conversation

After opening the chat window, simply tell the AI what you want to do in natural language.

#### Creating Objects
```
Create a Cube
Create a red Sphere
Create a Cylinder at (0, 5, 0)
```

#### Manipulating Objects
```
Move Cube to (10, 0, 0)
Rotate Player 90 degrees
Scale Enemy to 2x
```

#### Querying Information
```
Where is Player
What objects are in the scene
What's selected
```

### Workflow Examples

#### Creating Player Character

Just say:
```
Create a player character with WASD movement
```

AI will automatically:
1. Create Player GameObject
2. Add Rigidbody and Collider
3. Generate PlayerController script
4. Attach script
5. Save as Prefab

#### Creating UI System

```
Create a main menu with start and exit buttons
```

AI will automatically:
1. Create Canvas and EventSystem
2. Create background panel
3. Create title text
4. Create buttons

#### Scene Analysis

```
Analyze the current scene for issues
```

AI will check:
- Camera presence
- Light sources
- Missing scripts
- Physics component configuration
- UI system completeness

### Keyboard Shortcuts

| Shortcut | Function |
|----------|----------|
| Enter | Send message |
| Shift + Enter | New line (multi-line input) |
| Ctrl + ↑ | Previous history command |
| Ctrl + ↓ | Next history command |
| Escape | Cancel current operation |

### Quick Commands Bar

The quick commands bar appears on first use with common command buttons. Click to execute directly.

Toggle visibility with the "Quick" button in the title bar.

### Command History

Click the "History" button next to the input box to view recent commands. Click to fill in again.

You can also use Ctrl+↑/↓ to quickly browse history.

### Batch Operations

```
Create 10 Cubes in a row
Add Rigidbody to all Cubes
Set all Enemies to red
Delete all objects starting with Temp
```

### Advanced Tips

#### Combined Commands
Include multiple operations in one message:
```
Create a Cube, move to (0, 5, 0), set to red, add Rigidbody
```

#### Relative Operations
```
Move Player up 5 units
Rotate selected object 45 degrees
```

#### Using Selected Objects
```
Add Collider to selected object
Duplicate selected object
Delete selected object
```

### Troubleshooting

#### API Key Error
Check if API Key is correct and has sufficient balance.

#### Network Timeout
Check network connection and try resending.

#### Tool Execution Failed
Check the tool execution log for detailed error information.
