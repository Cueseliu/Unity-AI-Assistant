using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AIOperator.Models;
using AIOperator.LLM;
using AIOperator.Editor.Tools.Executors;

namespace AIOperator.Editor.Tools.Core
{
    /// <summary>
    /// 工具调用管理器 - 管理工具调用循环，维护对话状态
    /// </summary>
    public class ToolCallManager
    {
        private const int MAX_TOOL_ROUNDS = 10;  // 防止无限循环

        private ILLMProvider provider;
        private List<LLMMessage> conversationHistory;
        private bool executorsRegistered = false;
        private string systemPrompt;

        /// <summary>
        /// 当前状态回调
        /// </summary>
        public Action<string> OnStatusUpdate;

        /// <summary>
        /// 工具执行回调
        /// </summary>
        public Action<ToolCall, ToolResult> OnToolExecuted;

        public ToolCallManager(ILLMProvider provider)
        {
            this.provider = provider;
            this.conversationHistory = new List<LLMMessage>();

            // 构建系统提示词
            this.systemPrompt = SystemPromptBuilder.Build();

            // 确保执行器已注册
            EnsureExecutorsRegistered();
        }

        /// <summary>
        /// 确保所有执行器已注册
        /// </summary>
        private void EnsureExecutorsRegistered()
        {
            if (executorsRegistered) return;

            var registry = ToolRegistry.Instance;

            // 注册所有执行器
            registry.RegisterExecutor(new GameObjectExecutor());
            registry.RegisterExecutor(new SelectionExecutor());
            registry.RegisterExecutor(new HierarchyExecutor());
            registry.RegisterExecutor(new ComponentExecutor());
            registry.RegisterExecutor(new MaterialExecutor());

            // Phase 2E: 高级工具
            registry.RegisterExecutor(new SceneExecutor());
            registry.RegisterExecutor(new ConsoleExecutor());
            registry.RegisterExecutor(new AssetExecutor());
            registry.RegisterExecutor(new ScriptExecutor());

            // Phase 3C: UI 工具
            registry.RegisterExecutor(new UIExecutor());

            executorsRegistered = true;
            Debug.Log($"[ToolCallManager] 已注册 {registry.GetRegisteredToolNames().Length} 个工具");
        }

        /// <summary>
        /// 处理用户消息，包括可能的多轮工具调用
        /// </summary>
        public IEnumerator ProcessMessage(
            string userMessage,
            Action<string> onComplete,
            Action<string> onError)
        {
            // 添加用户消息
            conversationHistory.Add(new LLMMessage("user", userMessage));

            int round = 0;
            bool continueLoop = true;

            while (continueLoop && round < MAX_TOOL_ROUNDS)
            {
                round++;
                OnStatusUpdate?.Invoke($"思考中... (轮次 {round})");

                // 调用 LLM（带系统提示词）
                ToolCallResponse response = null;
                string error = null;

                yield return provider.SendMessageWithToolsCoroutine(
                    conversationHistory,
                    ToolDefinitions.AllTools,
                    systemPrompt,
                    (r) => response = r,
                    (e) => error = e
                );

                if (error != null)
                {
                    onError?.Invoke(error);
                    yield break;
                }

                if (response == null)
                {
                    onError?.Invoke("未收到响应");
                    yield break;
                }

                if (response.HasToolCalls)
                {
                    // 添加 assistant 的工具调用消息到历史
                    conversationHistory.Add(LLMMessage.FromToolUse(response.ToolCalls, response.Text));

                    // 执行工具并收集结果
                    var toolResults = new List<ToolResultMessage>();

                    foreach (var toolCall in response.ToolCalls)
                    {
                        OnStatusUpdate?.Invoke($"执行: {toolCall.Name}...");

                        // 验证参数
                        var validationError = ToolRegistry.Instance.ValidateToolCall(toolCall);
                        if (validationError != null)
                        {
                            var errorResult = ToolResult.Fail(validationError);
                            toolResults.Add(new ToolResultMessage(toolCall.Id, errorResult));
                            OnToolExecuted?.Invoke(toolCall, errorResult);
                            continue;
                        }

                        // 执行工具
                        var result = ToolRegistry.Instance.ExecuteTool(toolCall);
                        toolResults.Add(new ToolResultMessage(toolCall.Id, result));
                        OnToolExecuted?.Invoke(toolCall, result);

                        Debug.Log($"[ToolCallManager] 工具 {toolCall.Name} 执行结果: {result}");
                    }

                    // 添加工具结果到历史
                    conversationHistory.Add(LLMMessage.FromToolResults(toolResults));
                }
                else
                {
                    // 没有工具调用，结束循环
                    continueLoop = false;
                    conversationHistory.Add(new LLMMessage("assistant", response.Text ?? ""));
                    onComplete?.Invoke(response.Text ?? "操作完成");
                }
            }

            if (round >= MAX_TOOL_ROUNDS)
            {
                onError?.Invoke($"工具调用轮次超限 (最大 {MAX_TOOL_ROUNDS} 轮)");
            }
        }

        /// <summary>
        /// 获取对话历史
        /// </summary>
        public List<LLMMessage> GetHistory()
        {
            return new List<LLMMessage>(conversationHistory);
        }

        /// <summary>
        /// 清空对话历史
        /// </summary>
        public void ClearHistory()
        {
            conversationHistory.Clear();
        }

        /// <summary>
        /// 设置新的 Provider
        /// </summary>
        public void SetProvider(ILLMProvider newProvider)
        {
            this.provider = newProvider;
        }
    }
}
