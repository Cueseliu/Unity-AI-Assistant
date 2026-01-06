using System.Collections.Generic;

namespace AIOperator.LLM
{
    /// <summary>
    /// 工具定义集合 - 包含所有可用工具
    /// </summary>
    public static class ToolDefinitions
    {
        /// <summary>
        /// 所有工具定义
        /// </summary>
        public static readonly ToolDefinition[] AllTools = new ToolDefinition[]
        {
            // ========== GameObject 工具 ==========
            new ToolDefinition
            {
                Name = "create_primitive",
                Description = @"创建一个基础几何体 (Cube, Sphere, Capsule 等)。
示例: create_primitive(primitive_type='Cube', name='MyCube', position=[0,2,0])
返回: 创建成功的物体名称和位置",
                Parameters = new ToolParameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["primitive_type"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new[] { "Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad" },
                            Description = "几何体类型"
                        },
                        ["name"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "物体名称 (可选，默认使用几何体类型名)"
                        },
                        ["position"] = new ToolProperty
                        {
                            Type = "array",
                            Description = "位置 [x, y, z] (可选，默认 [0,0,0])",
                            Items = new ToolProperty { Type = "number" }
                        }
                    },
                    Required = new[] { "primitive_type" }
                }
            },

            new ToolDefinition
            {
                Name = "create_empty",
                Description = @"创建一个空 GameObject。
示例: create_empty(name='MyObject')
返回: 创建成功的物体名称",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "物体名称" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y, z] (可选)" }
                    },
                    Required = new[] { "name" }
                }
            },

            new ToolDefinition
            {
                Name = "find_gameobject",
                Description = @"在场景中查找 GameObject。
示例: find_gameobject(name='Player')
返回: 物体信息（位置、组件列表等）",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "物体名称" }
                    },
                    Required = new[] { "name" }
                }
            },

            new ToolDefinition
            {
                Name = "delete_gameobject",
                Description = @"删除一个 GameObject。
示例: delete_gameobject(name='OldCube')
返回: 删除确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "要删除的物体名称" }
                    },
                    Required = new[] { "name" }
                }
            },

            new ToolDefinition
            {
                Name = "duplicate_gameobject",
                Description = @"复制一个 GameObject。
示例: duplicate_gameobject(name='Player', new_name='Player_Copy')
返回: 新物体名称",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "要复制的物体名称" },
                        ["new_name"] = new ToolProperty { Type = "string", Description = "新物体名称 (可选)" }
                    },
                    Required = new[] { "name" }
                }
            },

            // ========== Transform 工具 ==========
            new ToolDefinition
            {
                Name = "set_transform",
                Description = @"设置 GameObject 的位置、旋转或缩放。
示例: set_transform(target='Cube', position=[0,5,0], rotation=[0,45,0])
返回: 更新后的 Transform 信息",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y, z]" },
                        ["rotation"] = new ToolProperty { Type = "array", Description = "旋转 [x, y, z] (欧拉角)" },
                        ["scale"] = new ToolProperty { Type = "array", Description = "缩放 [x, y, z]" }
                    },
                    Required = new[] { "target" }
                }
            },

            new ToolDefinition
            {
                Name = "get_transform",
                Description = @"获取 GameObject 的 Transform 信息。
示例: get_transform(target='Player')
返回: 位置、旋转、缩放信息",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" }
                    },
                    Required = new[] { "target" }
                }
            },

            // ========== Selection 工具 ==========
            new ToolDefinition
            {
                Name = "get_selection",
                Description = @"获取当前在 Unity 编辑器中选中的物体。
示例: get_selection()
返回: 选中物体的列表",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>()
                }
            },

            new ToolDefinition
            {
                Name = "select_gameobject",
                Description = @"在 Unity 编辑器中选中指定的物体。
示例: select_gameobject(name='Player')
返回: 选中确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "要选中的物体名称" }
                    },
                    Required = new[] { "name" }
                }
            },

            // ========== Hierarchy 工具 ==========
            new ToolDefinition
            {
                Name = "set_parent",
                Description = @"设置 GameObject 的父物体。
示例: set_parent(target='Weapon', parent='Player')
返回: 设置确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["parent"] = new ToolProperty { Type = "string", Description = "父物体名称 (留空则移到根层级)" },
                        ["world_position_stays"] = new ToolProperty { Type = "boolean", Description = "是否保持世界坐标 (默认 true)" }
                    },
                    Required = new[] { "target" }
                }
            },

            new ToolDefinition
            {
                Name = "get_children",
                Description = @"获取 GameObject 的所有子物体。
示例: get_children(target='Player')
返回: 子物体列表",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["recursive"] = new ToolProperty { Type = "boolean", Description = "是否递归获取所有后代 (默认 false)" }
                    },
                    Required = new[] { "target" }
                }
            },

            // ========== Component 工具 ==========
            new ToolDefinition
            {
                Name = "add_component",
                Description = @"给 GameObject 添加组件。
示例: add_component(target='Player', component_type='Rigidbody')
返回: 添加确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["component_type"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new[] {
                                "Rigidbody", "BoxCollider", "SphereCollider", "CapsuleCollider", "MeshCollider",
                                "CharacterController", "AudioSource", "AudioListener",
                                "Light", "Camera", "Animator", "LineRenderer", "TrailRenderer"
                            },
                            Description = "组件类型"
                        }
                    },
                    Required = new[] { "target", "component_type" }
                }
            },

            new ToolDefinition
            {
                Name = "remove_component",
                Description = @"从 GameObject 移除组件。
示例: remove_component(target='Player', component_type='Rigidbody')
返回: 移除确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["component_type"] = new ToolProperty { Type = "string", Description = "组件类型名称" }
                    },
                    Required = new[] { "target", "component_type" }
                }
            },

            new ToolDefinition
            {
                Name = "get_components",
                Description = @"获取 GameObject 上的所有组件。
示例: get_components(target='Player')
返回: 组件列表",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" }
                    },
                    Required = new[] { "target" }
                }
            },

            new ToolDefinition
            {
                Name = "attach_script",
                Description = @"将项目中的 MonoBehaviour 脚本挂载到 GameObject 上。
注意: 脚本必须已存在于项目中且编译通过。
示例: attach_script(target='Player', script_name='PlayerController')
返回: 挂载确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["script_name"] = new ToolProperty { Type = "string", Description = "脚本名称 (不含 .cs 后缀)" }
                    },
                    Required = new[] { "target", "script_name" }
                }
            },

            // ========== Material 工具 ==========
            new ToolDefinition
            {
                Name = "set_material_color",
                Description = @"设置物体的材质颜色。会创建新材质实例。
示例: set_material_color(target='Cube', color='red') 或 set_material_color(target='Cube', color=[1,0,0,1])
支持的颜色名称: red, green, blue, yellow, white, black, gray, cyan, magenta, orange, purple, pink
返回: 设置确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["color"] = new ToolProperty
                        {
                            Type = "string",
                            Description = "颜色名称 (red/green/blue/yellow/white/black/gray/cyan/magenta/orange/purple/pink) 或 [r,g,b,a] 数组 (0-1) 或 hex (#RRGGBB)"
                        }
                    },
                    Required = new[] { "target", "color" }
                }
            },

            // ========== Scene 工具 ==========
            new ToolDefinition
            {
                Name = "get_scene_hierarchy",
                Description = @"获取当前场景的层级结构。
示例: get_scene_hierarchy(max_depth=3)
返回: 场景物体树形结构",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["max_depth"] = new ToolProperty { Type = "integer", Description = "最大深度 (默认 3)" }
                    }
                }
            },

            new ToolDefinition
            {
                Name = "get_scene_info",
                Description = @"获取当前场景的基本信息。
示例: get_scene_info()
返回: 场景名称、路径、物体数量等",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>()
                }
            },

            new ToolDefinition
            {
                Name = "analyze_scene",
                Description = @"分析当前场景并进行健康检查。
检查项目包括:
- 相机是否存在
- 灯光配置
- 丢失的脚本引用
- UI EventSystem 检查
- Rigidbody/Collider 配置
示例: analyze_scene()
返回: 场景分析报告（包含错误、警告和建议）",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>()
                }
            },

            // ========== Console 工具 ==========
            new ToolDefinition
            {
                Name = "get_console_logs",
                Description = @"获取 Unity Console 的日志。
示例: get_console_logs(count=10, type='error')
返回: 日志列表",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["count"] = new ToolProperty { Type = "integer", Description = "获取条数 (默认 10)" },
                        ["type"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new[] { "all", "error", "warning", "log" },
                            Description = "日志类型 (默认 all)"
                        }
                    }
                }
            },

            new ToolDefinition
            {
                Name = "clear_console",
                Description = @"清空 Unity Console。
示例: clear_console()
返回: 清空确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>()
                }
            },

            // ========== Prefab 工具 ==========
            new ToolDefinition
            {
                Name = "save_as_prefab",
                Description = @"将 GameObject 保存为 Prefab。
示例: save_as_prefab(target='Player', path='Assets/Prefabs/Player.prefab')
返回: Prefab 路径",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["target"] = new ToolProperty { Type = "string", Description = "目标物体名称" },
                        ["path"] = new ToolProperty { Type = "string", Description = "保存路径 (如 Assets/Prefabs/MyPrefab.prefab)" }
                    },
                    Required = new[] { "target", "path" }
                }
            },

            new ToolDefinition
            {
                Name = "instantiate_prefab",
                Description = @"实例化一个 Prefab。
示例: instantiate_prefab(path='Assets/Prefabs/Enemy.prefab', position=[5,0,0])
返回: 新实例名称",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["path"] = new ToolProperty { Type = "string", Description = "Prefab 路径" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y, z] (可选)" },
                        ["name"] = new ToolProperty { Type = "string", Description = "实例名称 (可选)" }
                    },
                    Required = new[] { "path" }
                }
            },

            // ========== Script 工具 ==========
            new ToolDefinition
            {
                Name = "create_script",
                Description = @"创建一个 C# 脚本文件。
示例: create_script(name='PlayerController', script_type='MonoBehaviour', code='...')
返回: 脚本路径",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "脚本名称 (不含 .cs)" },
                        ["script_type"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new[] { "MonoBehaviour", "ScriptableObject", "Editor", "Plain" },
                            Description = "脚本类型 (默认 MonoBehaviour)"
                        },
                        ["code"] = new ToolProperty { Type = "string", Description = "脚本代码内容" },
                        ["folder"] = new ToolProperty { Type = "string", Description = "保存文件夹 (默认 Assets/Scripts)" }
                    },
                    Required = new[] { "name", "code" }
                }
            },

            new ToolDefinition
            {
                Name = "read_script",
                Description = @"读取项目中的 C# 脚本内容，用于检查代码逻辑和诊断问题。
示例: read_script(name='PlayerController')
返回: 脚本文件信息和完整代码内容",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "脚本名称 (不含 .cs 后缀)" },
                        ["folder"] = new ToolProperty { Type = "string", Description = "脚本所在文件夹 (可选，默认搜索常见路径)" }
                    },
                    Required = new[] { "name" }
                }
            },

            // ========== 批量操作工具 ==========
            new ToolDefinition
            {
                Name = "batch_create",
                Description = @"批量创建多个相同类型的物体。
示例: batch_create(primitive_type='Cube', count=5, spacing=[2,0,0], name_prefix='Enemy')
返回: 创建的物体列表",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["primitive_type"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new[] { "Cube", "Sphere", "Capsule", "Cylinder" },
                            Description = "几何体类型"
                        },
                        ["count"] = new ToolProperty { Type = "integer", Description = "数量" },
                        ["spacing"] = new ToolProperty { Type = "array", Description = "间隔 [x, y, z]" },
                        ["start_position"] = new ToolProperty { Type = "array", Description = "起始位置 [x, y, z] (默认 [0,0,0])" },
                        ["name_prefix"] = new ToolProperty { Type = "string", Description = "名称前缀 (默认使用几何体类型)" }
                    },
                    Required = new[] { "primitive_type", "count" }
                }
            },

            // ========== UI 工具 ==========
            new ToolDefinition
            {
                Name = "create_canvas",
                Description = @"创建 UI Canvas（如果场景中不存在）。会自动创建 EventSystem。
示例: create_canvas()
返回: Canvas 创建确认或已存在提示",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "Canvas 名称 (默认 'Canvas')" }
                    }
                }
            },

            new ToolDefinition
            {
                Name = "create_button",
                Description = @"创建 UI 按钮。会自动确保 Canvas 存在。
示例: create_button(name='StartButton', text='开始游戏', position=[0, 0])
返回: 按钮创建确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "按钮名称" },
                        ["text"] = new ToolProperty { Type = "string", Description = "按钮显示文字" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y] (相对于 Canvas 中心)" }
                    },
                    Required = new[] { "text" }
                }
            },

            new ToolDefinition
            {
                Name = "create_text",
                Description = @"创建 UI 文本。会自动确保 Canvas 存在。
示例: create_text(name='ScoreText', content='Score: 0', font_size=32)
返回: 文本创建确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "文本物体名称" },
                        ["content"] = new ToolProperty { Type = "string", Description = "文本内容" },
                        ["font_size"] = new ToolProperty { Type = "integer", Description = "字体大小 (默认 24)" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y]" }
                    },
                    Required = new[] { "content" }
                }
            },

            new ToolDefinition
            {
                Name = "create_image",
                Description = @"创建 UI 图片。会自动确保 Canvas 存在。
示例: create_image(name='Background', color='blue', size=[400, 300])
返回: 图片创建确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "图片名称" },
                        ["color"] = new ToolProperty { Type = "string", Description = "颜色 (如 'red', 'blue', '#FF0000')" },
                        ["size"] = new ToolProperty { Type = "array", Description = "尺寸 [width, height]" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y]" }
                    }
                }
            },

            new ToolDefinition
            {
                Name = "create_slider",
                Description = @"创建 UI 滑动条，可用作血条、进度条等。
示例: create_slider(name='HealthBar', type='health_bar', value=0.8)
type 可选: 'default'(蓝色可交互), 'health_bar'(红色不可交互)
返回: 滑动条创建确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "滑动条名称" },
                        ["type"] = new ToolProperty
                        {
                            Type = "string",
                            Enum = new[] { "default", "health_bar" },
                            Description = "类型: default(默认), health_bar(血条)"
                        },
                        ["value"] = new ToolProperty { Type = "number", Description = "初始值 (0-1，默认 1)" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y]" }
                    }
                }
            },

            new ToolDefinition
            {
                Name = "create_panel",
                Description = @"创建 UI 面板，可作为容器或背景。
示例: create_panel(name='MenuPanel', color='gray', size=[400, 300])
返回: 面板创建确认",
                Parameters = new ToolParameters
                {
                    Properties = new Dictionary<string, ToolProperty>
                    {
                        ["name"] = new ToolProperty { Type = "string", Description = "面板名称" },
                        ["color"] = new ToolProperty { Type = "string", Description = "背景颜色" },
                        ["size"] = new ToolProperty { Type = "array", Description = "尺寸 [width, height]" },
                        ["position"] = new ToolProperty { Type = "array", Description = "位置 [x, y]" }
                    }
                }
            }
        };
    }
}
