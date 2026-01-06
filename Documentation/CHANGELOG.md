# Changelog / 更新日志

All notable changes to AI Operator will be documented in this file.

---

## [1.0.0] - 2024-XX-XX

### Added / 新增功能

#### Core Features / 核心功能
- Natural language Unity control via AI chat interface
- 通过 AI 聊天界面用自然语言控制 Unity

#### AI Services / AI 服务支持
- Claude API (Anthropic)
- DeepSeek API
- AWS Bedrock (Claude via AWS)
- Qwen (通义千问)
- GLM-4 (智谱清言)
- Doubao (豆包)

#### Tools (30 total) / 工具 (共 30 个)

**GameObject Tools (8) / GameObject 工具**
- create_primitive - Create basic geometry / 创建基础几何体
- create_empty - Create empty GameObject / 创建空物体
- find_gameobject - Find by name / 按名称查找
- delete_gameobject - Delete objects / 删除物体
- duplicate_gameobject - Duplicate objects / 复制物体
- set_transform - Set transform / 设置 Transform
- get_transform - Get transform info / 获取 Transform 信息
- batch_create - Batch creation / 批量创建

**Selection Tools (2) / 选择工具**
- get_selection - Get selected objects / 获取选中物体
- select_gameobject - Select objects / 选中物体

**Hierarchy Tools (2) / 层级工具**
- set_parent - Set parent-child / 设置父子关系
- get_children - Get children / 获取子物体

**Component Tools (3) / 组件工具**
- add_component - Add component / 添加组件
- remove_component - Remove component / 移除组件
- get_components - List components / 获取组件列表

**Material Tools (1) / 材质工具**
- set_material_color - Set color / 设置颜色

**Scene Tools (2) / 场景工具**
- get_scene_hierarchy - Get hierarchy / 获取场景层级
- get_scene_info - Get scene info / 获取场景信息

**Console Tools (2) / 控制台工具**
- get_console_logs - Get logs / 获取日志
- clear_console - Clear console / 清空控制台

**Prefab Tools (2) / Prefab 工具**
- save_as_prefab - Save prefab / 保存为 Prefab
- instantiate_prefab - Instantiate / 实例化 Prefab

**Script Tools (1) / 脚本工具**
- create_script - Create C# script / 创建 C# 脚本

**UI Tools (6) / UI 工具**
- create_canvas - Create Canvas / 创建 Canvas
- create_button - Create button / 创建按钮
- create_text - Create text / 创建文本
- create_image - Create image / 创建图片
- create_slider - Create slider / 创建滑动条
- create_panel - Create panel / 创建面板

**Analysis Tools (1) / 分析工具**
- analyze_scene - Scene health check / 场景健康检查

#### UI Features / 界面功能
- Bilingual interface (Chinese/English) / 中英文双语界面
- Quick commands bar / 快捷命令栏
- Command history / 命令历史
- Keyboard shortcuts / 快捷键支持
- Config import/export / 配置导入导出
- Reset to default / 恢复默认设置

### Technical / 技术特性
- Unity 2021.3+ support / 支持 Unity 2021.3+
- Editor-only plugin / 纯编辑器插件
- No runtime dependencies / 无运行时依赖

---

## Future Plans / 未来计划

### Planned Features / 计划功能
- [ ] Animation system support / 动画系统支持
- [ ] Asset import/management / 资源导入管理
- [ ] Multi-scene operations / 多场景操作
- [ ] Code editing capabilities / 代码编辑功能
- [ ] Voice input support / 语音输入支持
- [ ] Plugin marketplace integration / 插件市场集成

---

## Version History / 版本历史

| Version | Date | Highlights |
|---------|------|------------|
| 1.0.0 | 2024-XX-XX | Initial release / 首次发布 |
