using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// Model层基类，负责数据的存储和处理
    /// </summary>
    public abstract class BaseModel
    {
        protected EventCenter m_eventCenter;

        public BaseModel()
        {
            m_eventCenter = EventCenter.Instance;
            OnInit();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 发送事件
        /// </summary>
        protected void SendEvent(string eventName, object param = null)
        {
            m_eventCenter.TriggerEvent(eventName, param);
        }
    }
} 