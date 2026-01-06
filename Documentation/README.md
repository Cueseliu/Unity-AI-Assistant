# AI Operator

**Control Unity Editor with Natural Language using LLM Tool Calling**

[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black?logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/Cueseliu/Unity-AI-Assistant/pulls)

AI Operator is a Unity Editor extension that enables natural language control of the Unity Editor through LLM function calling. Instead of navigating menus or writing scripts, simply describe what you want in plain English.

![Demo](https://via.placeholder.com/800x400?text=AI+Operator+Demo+GIF)

## Features

### Natural Language to Unity Actions
```
"Create a red cube at position (0, 5, 0)"
"Add Rigidbody to all objects tagged 'Enemy'"
"Create a UI panel with Start and Quit buttons"
"Generate 10 enemies in a circle formation"
```

### 30 Built-in Tools

| Category | Tools |
|----------|-------|
| **GameObject** | create_primitive, create_empty, find, delete, duplicate, set_transform, get_transform, batch_create |
| **Selection** | get_selection, select_gameobject, set_parent, get_children |
| **Components** | add_component, remove_component, get_components |
| **Materials** | set_material_color |
| **Prefabs** | save_as_prefab, instantiate_prefab |
| **Scripts** | create_script (auto-generates C# MonoBehaviour) |
| **UI (uGUI)** | create_canvas, create_button, create_text, create_image, create_slider, create_panel |
| **Scene** | get_hierarchy, get_scene_info, analyze_scene |
| **Console** | get_console_logs, clear_console |

### Multi-Provider Architecture

Supports 6 LLM providers with a unified interface:

| Provider | Model | Best For |
|----------|-------|----------|
| **Claude** (Anthropic) | claude-3-5-sonnet | Best accuracy, recommended |
| **DeepSeek** | deepseek-chat | Cost-effective |
| **AWS Bedrock** | Claude via AWS | Enterprise / AWS users |
| **Qwen** (Alibaba) | qwen-plus | Chinese language tasks |
| **GLM-4** (Zhipu AI) | glm-4 | Alternative Chinese LLM |
| **Doubao** (ByteDance) | doubao-pro | Chinese market |

### Workflow Automation

Single commands trigger multi-step workflows:

```
User: "Create a player character that can move with WASD"

AI Operator executes:
1. create_primitive → Capsule named "Player"
2. add_component → Rigidbody (constraints set)
3. add_component → CapsuleCollider
4. create_script → PlayerController.cs with movement logic
5. add_component → PlayerController to Player
6. save_as_prefab → Assets/Prefabs/Player.prefab
```

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    AIOperatorWindow                      │
│                   (Unity EditorWindow)                   │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                   ILLMProvider                           │
│            (Abstract Provider Interface)                 │
├──────────┬──────────┬──────────┬──────────┬────────────┤
│  Claude  │ DeepSeek │ Bedrock  │  Qwen    │  GLM-4     │
│ Provider │ Provider │ Provider │ Provider │  Provider  │
└──────────┴──────────┴──────────┴──────────┴────────────┘
                          │
                          │ Tool Calling (Function Calling)
                          ▼
┌─────────────────────────────────────────────────────────┐
│                    ToolRegistry                          │
│              (30 Registered Tool Definitions)            │
└─────────────────────────┬───────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────┐
│                   Tool Executors                         │
│  GameObjectTools │ UITools │ SceneTools │ ScriptTools   │
└─────────────────────────────────────────────────────────┘
```

## Technical Highlights

- **Provider Abstraction**: Clean interface (`ILLMProvider`) allows easy addition of new LLM providers
- **Tool Definition System**: JSON Schema-based tool definitions compatible with OpenAI function calling spec
- **Async/Await Pattern**: Non-blocking API calls with proper Unity main thread dispatching
- **Localization System**: Runtime language switching (EN/CN) with `Localization.cs`
- **Config Management**: Import/Export settings as JSON for team sharing
- **Error Recovery**: Graceful handling of API failures and invalid tool calls

## Installation

### Option 1: Clone Repository
```bash
git clone https://github.com/Cueseliu/Unity-AI-Assistant.git
```
Copy the `AIOperator` folder to your Unity project's `Assets/` directory.

### Option 2: Unity Package
Download the `.unitypackage` from [Releases](https://github.com/Cueseliu/Unity-AI-Assistant/releases) and import into Unity.

## Quick Start

1. **Open Settings**: `Window > AI Operator > Settings`
2. **Select Provider**: Choose your preferred LLM service
3. **Enter API Key**: Get one from your provider's dashboard
4. **Test Connection**: Click "Test" to verify
5. **Start Chatting**: `Window > AI Operator > Chat Window`

## Example Commands

### Basic Operations
```
Create a Sphere
Delete all Cubes in the scene
Select the Main Camera
Move Player to (10, 0, 5)
Rotate Enemy by 45 degrees on Y axis
```

### Component Management
```
Add Rigidbody to Player
Remove all AudioSource components from selected
What components does the Main Camera have?
```

### UI Creation
```
Create a Canvas with a title text
Add a health bar slider to the UI
Create a settings panel with 3 buttons
```

### Scene Analysis
```
Show me the scene hierarchy
How many GameObjects are in this scene?
Analyze scene for potential issues
```

### Batch Operations
```
Create 5 cubes in a row
Spawn 10 enemies in a circle around Player
Duplicate selected object 3 times
```

## Configuration

Settings are stored in Unity EditorPrefs and can be exported/imported as JSON:

```json
{
  "provider": "Claude",
  "apiKey": "sk-...",
  "model": "claude-3-5-sonnet-20241022",
  "language": "en"
}
```

## Adding New Tools

1. Define tool in `ToolDefinitions.cs`:
```csharp
new ToolDefinition {
    Name = "my_custom_tool",
    Description = "Does something useful",
    Parameters = new {
        type = "object",
        properties = new {
            param1 = new { type = "string", description = "..." }
        },
        required = new[] { "param1" }
    }
}
```

2. Implement executor in `ToolExecutors/`:
```csharp
public static ToolResult ExecuteMyCustomTool(Dictionary<string, object> args) {
    // Implementation
    return ToolResult.Success("Done!");
}
```

3. Register in `ToolRegistry.cs`

## Adding New LLM Providers

1. Implement `ILLMProvider` interface:
```csharp
public class MyProvider : ILLMProvider {
    public string Name => "MyProvider";
    public async Task<LLMResponse> SendMessageAsync(string message, List<ToolDefinition> tools) {
        // API call implementation
    }
}
```

2. Register in `ProviderFactory.cs`

## Project Structure

```
AIOperator/
├── Editor/
│   ├── AIOperatorWindow.cs      # Main chat window
│   ├── AIOperatorSettings.cs    # Settings window
│   ├── Localization/
│   │   ├── Localization.cs      # Language manager
│   │   └── LocalizedStrings.cs  # String definitions
│   └── Tools/
│       ├── Core/
│       │   ├── ToolDefinition.cs
│       │   ├── ToolRegistry.cs
│       │   └── ToolResult.cs
│       └── Executors/
│           ├── GameObjectTools.cs
│           ├── UITools.cs
│           ├── SceneTools.cs
│           └── ...
├── LLM/
│   ├── ILLMProvider.cs          # Provider interface
│   ├── SystemPromptBuilder.cs   # Prompt construction
│   ├── Providers/
│   │   ├── ClaudeProvider.cs
│   │   ├── DeepSeekProvider.cs
│   │   └── ...
│   └── Models/
│       ├── LLMRequest.cs
│       └── LLMResponse.cs
├── Documentation/
└── package.json
```

## Requirements

- Unity 2021.3 LTS or newer
- Internet connection
- API key from supported provider

## Roadmap

- [ ] Animation system tools
- [ ] Asset import/management tools
- [ ] Custom tool plugin system
- [ ] Conversation history persistence
- [ ] Voice input support

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

MIT License - see [LICENSE](LICENSE) for details.

## Author

Built by [Cueseliu](https://github.com/Cueseliu) - Unity + AI Integration Specialist

---

**Keywords**: Unity, AI, LLM, Claude, GPT, Natural Language, Editor Extension, Tool Calling, Function Calling, Automation
