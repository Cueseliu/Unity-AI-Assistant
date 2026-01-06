using System;

namespace AIOperator.Models
{
    /// <summary>
    /// 聊天消息数据模型
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// 消息角色：user(用户) 或 assistant(AI)
        /// </summary>
        public string role;
        
        /// <summary>
        /// 消息内容
        /// </summary>
        public string content;
        
        /// <summary>
        /// 消息时间戳
        /// </summary>
        public string timestamp;
        
        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
            this.timestamp = DateTime.Now.ToString("HH:mm:ss");
        }
    }
}