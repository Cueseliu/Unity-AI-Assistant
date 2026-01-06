using System;
using System.Collections;
using System.Collections.Generic;
using AIOperator.Models;

namespace AIOperator.LLM
{
    /// <summary>
    /// LLM 提供者接口 - 用于支持不同的 AI 模型
    /// </summary>
    public interface ILLMProvider
    {
        /// <summary>
        /// 获取提供者名称
        /// </summary>
        string GetProviderName();

        /// <summary>
        /// 测试连接是否正常
        /// </summary>
        void TestConnection(Action<bool, string> callback);

        // ========== 原有方法（保持兼容）==========

        /// <summary>
        /// 发送消息并获取回复（纯文本，无工具）
        /// </summary>
        /// <param name="messages">对话历史</param>
        /// <param name="onResponse">收到回复时的回调</param>
        /// <param name="onError">出错时的回调</param>
        void SendMessage(List<LLMMessage> messages, Action<string> onResponse, Action<string> onError);

        // ========== 新增方法（支持工具调用）==========

        /// <summary>
        /// 发送消息（带工具定义），返回统一的 ToolCallResponse
        /// </summary>
        /// <param name="messages">对话历史（支持工具消息）</param>
        /// <param name="tools">工具定义数组</param>
        /// <param name="onResponse">收到响应时的回调</param>
        /// <param name="onError">出错时的回调</param>
        void SendMessageWithTools(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            Action<ToolCallResponse> onResponse,
            Action<string> onError
        );

        /// <summary>
        /// 协程版本（用于 EditorWindow）
        /// </summary>
        IEnumerator SendMessageWithToolsCoroutine(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            Action<ToolCallResponse> onResponse,
            Action<string> onError
        );

        /// <summary>
        /// 带系统提示词的工具调用版本
        /// </summary>
        IEnumerator SendMessageWithToolsCoroutine(
            List<LLMMessage> messages,
            ToolDefinition[] tools,
            string systemPrompt,
            Action<ToolCallResponse> onResponse,
            Action<string> onError
        );
    }
}