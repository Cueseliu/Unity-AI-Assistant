# Tool Reference / 工具参考

[中文](#中文) | [English](#english)

---

## 中文

AI Operator 包含 30 个内置工具，覆盖 Unity 开发的常见操作。

### GameObject 工具 (8个)

#### create_primitive
创建基础几何体（Cube, Sphere, Cylinder 等）

**示例:**
```
创建一个 Cube
创建一个红色的 Sphere 在 (0, 5, 0)
```

#### create_empty
创建空 GameObject

**示例:**
```
创建一个空物体叫 GameManager
```

#### find_gameobject
按名称查找 GameObject

**示例:**
```
找到名为 Player 的物体
场景里有 Cube 吗
```

#### delete_gameobject
删除 GameObject

**示例:**
```
删除 Cube
删除所有 Enemy 开头的物体
```

#### duplicate_gameobject
复制 GameObject

**示例:**
```
复制选中的物体
复制 Player 创建 Player2
```

#### set_transform
设置位置、旋转、缩放

**示例:**
```
把 Cube 移动到 (0, 5, 0)
把选中物体旋转 45 度
放大 Cube 到 2 倍
```

#### get_transform
获取 Transform 信息

**示例:**
```
Player 的位置是多少
获取 Cube 的 Transform
```

#### batch_create
批量创建物体

**示例:**
```
创建 10 个 Cube 排成一排
批量创建 5x5 的 Sphere 网格
```

### Selection 工具 (2个)

#### get_selection
获取当前选中的物体

**示例:**
```
当前选中了什么
有物体被选中吗
```

#### select_gameobject
选中指定物体

**示例:**
```
选中 Player
选中所有 Cube
```

### Hierarchy 工具 (2个)

#### set_parent
设置父子关系

**示例:**
```
把 Weapon 设为 Player 的子物体
把 UI 移到 Canvas 下
```

#### get_children
获取子物体

**示例:**
```
Player 有哪些子物体
获取 Canvas 下的所有 UI
```

### Component 工具 (3个)

#### add_component
添加组件

**示例:**
```
给 Player 添加 Rigidbody
给选中物体添加 BoxCollider
```

#### remove_component
移除组件

**示例:**
```
移除 Cube 上的 BoxCollider
删除选中物体的 Rigidbody
```

#### get_components
获取组件列表

**示例:**
```
Player 有哪些组件
查看选中物体的组件
```

### Material 工具 (1个)

#### set_material_color
设置材质颜色

**示例:**
```
把 Cube 设为红色
给 Player 设置蓝色材质
```

### Scene 工具 (2个)

#### get_scene_hierarchy
获取场景层级结构

**示例:**
```
显示场景结构
场景里有什么物体
```

#### get_scene_info
获取场景信息

**示例:**
```
当前场景信息
场景叫什么名字
```

### Console 工具 (2个)

#### get_console_logs
获取控制台日志

**示例:**
```
有什么错误
查看控制台日志
```

#### clear_console
清空控制台

**示例:**
```
清空控制台
清除日志
```

### Prefab 工具 (2个)

#### save_as_prefab
保存为 Prefab

**示例:**
```
把 Player 保存为 Prefab
保存选中物体为预制体
```

#### instantiate_prefab
实例化 Prefab

**示例:**
```
实例化 Enemy Prefab
在 (0, 0, 10) 放置 Player Prefab
```

### Script 工具 (1个)

#### create_script
创建 C# 脚本

**示例:**
```
创建一个 PlayerController 脚本
生成移动控制脚本
```

### UI 工具 (6个)

#### create_canvas
创建 Canvas 和 EventSystem

**示例:**
```
创建 Canvas
准备 UI 系统
```

#### create_button
创建按钮

**示例:**
```
创建一个开始游戏按钮
在中间添加按钮
```

#### create_text
创建文本

**示例:**
```
创建分数文本
添加标题文字
```

#### create_image
创建图片

**示例:**
```
创建背景图片
添加图标
```

#### create_slider
创建滑动条（血条）

**示例:**
```
创建血条 UI
添加进度条
```

#### create_panel
创建面板

**示例:**
```
创建菜单面板
添加背景面板
```

### 场景分析工具 (1个)

#### analyze_scene
场景健康检查

**示例:**
```
分析场景
检查场景问题
有什么需要修复的吗
```

---

## English

AI Operator includes 30 built-in tools covering common Unity operations.

### GameObject Tools (8)

#### create_primitive
Create basic geometry (Cube, Sphere, Cylinder, etc.)

**Examples:**
```
Create a Cube
Create a red Sphere at (0, 5, 0)
```

#### create_empty
Create empty GameObject

**Examples:**
```
Create an empty object called GameManager
```

#### find_gameobject
Find GameObject by name

**Examples:**
```
Find the object named Player
Is there a Cube in the scene
```

#### delete_gameobject
Delete GameObject

**Examples:**
```
Delete Cube
Delete all objects starting with Enemy
```

#### duplicate_gameobject
Duplicate GameObject

**Examples:**
```
Duplicate selected object
Duplicate Player as Player2
```

#### set_transform
Set position, rotation, scale

**Examples:**
```
Move Cube to (0, 5, 0)
Rotate selected object 45 degrees
Scale Cube to 2x
```

#### get_transform
Get Transform information

**Examples:**
```
What's Player's position
Get Cube's Transform
```

#### batch_create
Batch create objects

**Examples:**
```
Create 10 Cubes in a row
Create a 5x5 grid of Spheres
```

### Selection Tools (2)

#### get_selection
Get currently selected objects

**Examples:**
```
What's selected
Is anything selected
```

#### select_gameobject
Select specified objects

**Examples:**
```
Select Player
Select all Cubes
```

### Hierarchy Tools (2)

#### set_parent
Set parent-child relationships

**Examples:**
```
Make Weapon a child of Player
Move UI under Canvas
```

#### get_children
Get child objects

**Examples:**
```
What children does Player have
Get all UI under Canvas
```

### Component Tools (3)

#### add_component
Add component

**Examples:**
```
Add Rigidbody to Player
Add BoxCollider to selected object
```

#### remove_component
Remove component

**Examples:**
```
Remove BoxCollider from Cube
Delete Rigidbody from selected object
```

#### get_components
Get component list

**Examples:**
```
What components does Player have
List components on selected object
```

### Material Tools (1)

#### set_material_color
Set material color

**Examples:**
```
Make Cube red
Set Player to blue
```

### Scene Tools (2)

#### get_scene_hierarchy
Get scene hierarchy

**Examples:**
```
Show scene structure
What objects are in the scene
```

#### get_scene_info
Get scene information

**Examples:**
```
Current scene info
What's the scene name
```

### Console Tools (2)

#### get_console_logs
Get console logs

**Examples:**
```
Any errors
Show console logs
```

#### clear_console
Clear console

**Examples:**
```
Clear console
Clear logs
```

### Prefab Tools (2)

#### save_as_prefab
Save as Prefab

**Examples:**
```
Save Player as Prefab
Save selected as prefab
```

#### instantiate_prefab
Instantiate Prefab

**Examples:**
```
Instantiate Enemy Prefab
Place Player Prefab at (0, 0, 10)
```

### Script Tools (1)

#### create_script
Create C# script

**Examples:**
```
Create a PlayerController script
Generate movement script
```

### UI Tools (6)

#### create_canvas
Create Canvas and EventSystem

**Examples:**
```
Create Canvas
Set up UI system
```

#### create_button
Create button

**Examples:**
```
Create a start game button
Add button in center
```

#### create_text
Create text

**Examples:**
```
Create score text
Add title text
```

#### create_image
Create image

**Examples:**
```
Create background image
Add icon
```

#### create_slider
Create slider (health bar)

**Examples:**
```
Create health bar UI
Add progress bar
```

#### create_panel
Create panel

**Examples:**
```
Create menu panel
Add background panel
```

### Scene Analysis Tools (1)

#### analyze_scene
Scene health check

**Examples:**
```
Analyze scene
Check scene issues
What needs to be fixed
```
