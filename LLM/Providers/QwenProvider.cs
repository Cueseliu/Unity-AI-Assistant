using UnityEngine;
using AIOperator.Models;

namespace AIOperator.LLM.Providers
{
    /// <summary>
    /// 通义千问 (Qwen) API 提供者
    /// 阿里云 DashScope 服务
    /// </summary>
    public class QwenProvider : OpenAICompatibleProvider
    {
        // 阿里云 DashScope OpenAI 兼容模式
        private const string QWEN_API_URL = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";

        protected override string ApiUrl => QWEN_API_URL;
        protected override string ProviderDisplayName => "Qwen";
        protected override string ApiKeyName => "阿里云 DashScope API Key";

        public QwenProvider(LLMConfig config, MonoBehaviour coroutineRunner)
            : base(config, coroutineRunner)
        {
        }
    }
}
