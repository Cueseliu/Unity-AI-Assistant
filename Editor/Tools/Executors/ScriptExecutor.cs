using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Core;
using AIOperator.Editor.Tools.Utils;

namespace AIOperator.Editor.Tools.Executors
{
    /// <summary>
    /// Script 执行器 - 处理 C# 脚本创建
    /// </summary>
    public class ScriptExecutor : ToolExecutorBase
    {
        public override string[] SupportedTools => new string[]
        {
            "create_script",
            "read_script"
        };

        public override ToolResult Execute(string toolName, Dictionary<string, object> args)
        {
            switch (toolName)
            {
                case "create_script":
                    return CreateScript(args);
                case "read_script":
                    return ReadScript(args);
                default:
                    return ToolResult.Fail($"未知工具: {toolName}");
            }
        }

        /// <summary>
        /// 创建 C# 脚本
        /// </summary>
        private ToolResult CreateScript(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            var code = args.GetString("code");
            if (string.IsNullOrEmpty(code))
            {
                return ToolResult.MissingParameter("code");
            }

            // 清理脚本名称
            name = SanitizeScriptName(name);
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.InvalidParameter("name", "脚本名称无效");
            }

            var scriptType = args.GetString("script_type", "MonoBehaviour");
            var folder = args.GetString("folder", "Assets/Scripts");

            // 确保文件夹以 Assets 开头
            if (!folder.StartsWith("Assets/") && !folder.StartsWith("Assets\\"))
            {
                folder = "Assets/" + folder;
            }

            // 规范化路径
            folder = folder.Replace("\\", "/");

            // 确保目录存在
            if (!AssetDatabase.IsValidFolder(folder))
            {
                CreateFolderRecursive(folder);
            }

            // 构建完整路径
            var fileName = name.EndsWith(".cs") ? name : name + ".cs";
            var fullPath = Path.Combine(folder, fileName).Replace("\\", "/");

            // 检查是否已存在
            if (File.Exists(fullPath))
            {
                return ToolResult.Fail($"脚本已存在: {fullPath}");
            }

            // 如果没有提供完整代码，使用模板
            if (!code.Contains("class ") && !code.Contains("interface "))
            {
                code = GenerateScriptFromTemplate(name.Replace(".cs", ""), scriptType, code);
            }

            try
            {
                // 写入文件
                File.WriteAllText(fullPath, code, System.Text.Encoding.UTF8);

                // 注意：不调用 AssetDatabase.Refresh()
                // 因为刷新会触发 Domain Reload，中断当前协程
                // Unity 会自动检测到新文件并在后台编译

                Log($"创建脚本: {fullPath}");

                var sb = new StringBuilder();
                sb.AppendLine($"已创建脚本: '{fullPath}'");
                sb.AppendLine($"- 脚本名称: {name}");
                sb.AppendLine($"- 脚本类型: {scriptType}");
                sb.AppendLine($"- 文件夹: {folder}");
                sb.AppendLine("\nUnity 将自动检测并编译此脚本，稍后即可使用该组件。");

                return ToolResult.Ok(sb.ToString());
            }
            catch (System.Exception e)
            {
                LogError($"创建脚本失败: {e}");
                return ToolResult.Fail($"创建失败: {e.Message}");
            }
        }

        /// <summary>
        /// 清理脚本名称
        /// </summary>
        private string SanitizeScriptName(string name)
        {
            // 移除 .cs 后缀（稍后会添加回来）
            if (name.EndsWith(".cs"))
            {
                name = name.Substring(0, name.Length - 3);
            }

            // 移除非法字符
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var c in invalidChars)
            {
                name = name.Replace(c.ToString(), "");
            }

            // 确保以字母开头
            if (name.Length > 0 && !char.IsLetter(name[0]))
            {
                name = "_" + name;
            }

            return name;
        }

        /// <summary>
        /// 从模板生成脚本
        /// </summary>
        private string GenerateScriptFromTemplate(string className, string scriptType, string customCode)
        {
            var sb = new StringBuilder();

            switch (scriptType.ToLower())
            {
                case "monobehaviour":
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine();
                    sb.AppendLine($"public class {className} : MonoBehaviour");
                    sb.AppendLine("{");
                    sb.AppendLine("    void Start()");
                    sb.AppendLine("    {");
                    if (!string.IsNullOrEmpty(customCode))
                    {
                        sb.AppendLine($"        {customCode}");
                    }
                    sb.AppendLine("    }");
                    sb.AppendLine();
                    sb.AppendLine("    void Update()");
                    sb.AppendLine("    {");
                    sb.AppendLine("    }");
                    sb.AppendLine("}");
                    break;

                case "scriptableobject":
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine();
                    sb.AppendLine($"[CreateAssetMenu(fileName = \"{className}\", menuName = \"ScriptableObjects/{className}\")]");
                    sb.AppendLine($"public class {className} : ScriptableObject");
                    sb.AppendLine("{");
                    if (!string.IsNullOrEmpty(customCode))
                    {
                        sb.AppendLine($"    {customCode}");
                    }
                    sb.AppendLine("}");
                    break;

                case "editor":
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine("using UnityEditor;");
                    sb.AppendLine();
                    sb.AppendLine($"public class {className} : Editor");
                    sb.AppendLine("{");
                    sb.AppendLine("    public override void OnInspectorGUI()");
                    sb.AppendLine("    {");
                    sb.AppendLine("        DrawDefaultInspector();");
                    if (!string.IsNullOrEmpty(customCode))
                    {
                        sb.AppendLine($"        {customCode}");
                    }
                    sb.AppendLine("    }");
                    sb.AppendLine("}");
                    break;

                case "plain":
                default:
                    sb.AppendLine("using System;");
                    sb.AppendLine("using UnityEngine;");
                    sb.AppendLine();
                    sb.AppendLine($"public class {className}");
                    sb.AppendLine("{");
                    if (!string.IsNullOrEmpty(customCode))
                    {
                        sb.AppendLine($"    {customCode}");
                    }
                    sb.AppendLine("}");
                    break;
            }

            return sb.ToString();
        }

        /// <summary>
        /// 递归创建文件夹
        /// </summary>
        private void CreateFolderRecursive(string path)
        {
            path = path.Replace("\\", "/");

            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parentPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parentPath))
            {
                parentPath = parentPath.Replace("\\", "/");
                CreateFolderRecursive(parentPath);
            }

            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(folderName) && !string.IsNullOrEmpty(parentPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        /// <summary>
        /// 读取脚本内容
        /// </summary>
        private ToolResult ReadScript(Dictionary<string, object> args)
        {
            var name = args.GetString("name");
            if (string.IsNullOrEmpty(name))
            {
                return ToolResult.MissingParameter("name");
            }

            // 移除 .cs 后缀（如果有）
            if (name.EndsWith(".cs"))
            {
                name = name.Substring(0, name.Length - 3);
            }

            // 搜索路径
            string[] searchPaths = new[]
            {
                $"Assets/Scripts/{name}.cs",
                $"Assets/AIOperator/GeneratedScripts/{name}.cs",
                $"Assets/{name}.cs"
            };

            // 如果指定了文件夹，优先搜索
            var folder = args.GetString("folder");
            if (!string.IsNullOrEmpty(folder))
            {
                if (!folder.StartsWith("Assets/") && !folder.StartsWith("Assets\\"))
                {
                    folder = "Assets/" + folder;
                }
                folder = folder.Replace("\\", "/");
                searchPaths = new[] { $"{folder}/{name}.cs" };
            }

            // 查找脚本文件
            string foundPath = null;
            foreach (var searchPath in searchPaths)
            {
                if (File.Exists(searchPath))
                {
                    foundPath = searchPath;
                    break;
                }
            }

            // 如果基础路径没找到，使用 AssetDatabase 搜索
            if (foundPath == null)
            {
                var guids = AssetDatabase.FindAssets($"t:MonoScript {name}");
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var fileName = Path.GetFileNameWithoutExtension(assetPath);
                    if (fileName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
                    {
                        foundPath = assetPath;
                        break;
                    }
                }
            }

            if (foundPath == null)
            {
                return ToolResult.Fail($"未找到脚本 '{name}'。\n搜索路径: {string.Join(", ", searchPaths)}");
            }

            try
            {
                // 读取脚本内容
                var content = File.ReadAllText(foundPath, System.Text.Encoding.UTF8);

                // 获取文件信息
                var fileInfo = new FileInfo(foundPath);
                var lineCount = content.Split('\n').Length;

                var sb = new StringBuilder();
                sb.AppendLine($"脚本: {foundPath}");
                sb.AppendLine($"大小: {fileInfo.Length} bytes");
                sb.AppendLine($"行数: {lineCount}");
                sb.AppendLine($"最后修改: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();
                sb.AppendLine("========== 代码内容 ==========");
                sb.AppendLine(content);
                sb.AppendLine("========== 代码结束 ==========");

                Log($"读取脚本: {foundPath}");
                return ToolResult.Ok(sb.ToString());
            }
            catch (System.Exception e)
            {
                LogError($"读取脚本失败: {e}");
                return ToolResult.Fail($"读取失败: {e.Message}");
            }
        }
    }
}
