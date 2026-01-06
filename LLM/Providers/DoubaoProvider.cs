using UnityEngine;
using AIOperator.Models;

namespace AIOperator.LLM.Providers
{
    /// <summary>
    /// 豆包 API 提供者
    /// 字节跳动火山引擎 Ark 服务
    /// </summary>
    public class DoubaoProvider : OpenAICompatibleProvider
    {
        // 火山引擎 Ark OpenAI 兼容模式
        private const string DOUBAO_API_URL = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";

        protected override string ApiUrl => DOUBAO_API_URL;
        protected override string ProviderDisplayName => "Doubao";
        protected override string ApiKeyName => "火山引擎 Ark API Key";

        public DoubaoProvider(LLMConfig config, MonoBehaviour coroutineRunner)
            : base(config, coroutineRunner)
        {
        }
    }
}
