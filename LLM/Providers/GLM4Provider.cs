using UnityEngine;
using AIOperator.Models;

namespace AIOperator.LLM.Providers
{
    /// <summary>
    /// 智谱清言 GLM-4 API 提供者
    /// 智谱 AI BigModel 开放平台
    /// </summary>
    public class GLM4Provider : OpenAICompatibleProvider
    {
        // 智谱 AI OpenAI 兼容模式
        private const string GLM4_API_URL = "https://open.bigmodel.cn/api/paas/v4/chat/completions";

        protected override string ApiUrl => GLM4_API_URL;
        protected override string ProviderDisplayName => "GLM-4";
        protected override string ApiKeyName => "智谱 AI API Key";

        public GLM4Provider(LLMConfig config, MonoBehaviour coroutineRunner)
            : base(config, coroutineRunner)
        {
        }
    }
}
