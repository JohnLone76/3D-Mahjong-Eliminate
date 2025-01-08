using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// View层基类，负责UI显示和用户输入
    /// </summary>
    public abstract class BaseView : MonoBehaviour
    {
        protected EventCenter m_eventCenter;

        protected virtual void Awake()
        {
            m_eventCenter = EventCenter.Instance;
            OnInit();
        }

        protected virtual void OnEnable()
        {
            RegisterEvents();
        }

        protected virtual void OnDisable()
        {
            UnregisterEvents();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 注册事件
        /// </summary>
        protected virtual void RegisterEvents() { }

        /// <summary>
        /// 注销事件
        /// </summary>
        protected virtual void UnregisterEvents() { }

        /// <summary>
        /// 发送事件
        /// </summary>
        protected void SendEvent(string eventName, object param = null)
        {
            m_eventCenter.TriggerEvent(eventName, param);
        }
    }
} 