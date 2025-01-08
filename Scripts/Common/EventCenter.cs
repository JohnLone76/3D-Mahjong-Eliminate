using UnityEngine;
using System.Collections.Generic;
using System;

namespace MahjongProject
{
    /// <summary>
    /// 事件中心，用于处理MVC层间通信
    /// </summary>
    public class EventCenter : MonoBehaviour
    {
        private static EventCenter m_instance;
        public static EventCenter Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("EventCenter");
                    m_instance = go.AddComponent<EventCenter>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        // 事件字典
        private Dictionary<string, Action<object>> m_eventDictionary = new Dictionary<string, Action<object>>();

        /// <summary>
        /// 添加事件监听
        /// </summary>
        public void AddListener(string eventName, Action<object> callback)
        {
            if (!m_eventDictionary.ContainsKey(eventName))
            {
                m_eventDictionary.Add(eventName, null);
            }
            m_eventDictionary[eventName] += callback;
        }

        /// <summary>
        /// 移除事件监听
        /// </summary>
        public void RemoveListener(string eventName, Action<object> callback)
        {
            if (m_eventDictionary.ContainsKey(eventName))
            {
                m_eventDictionary[eventName] -= callback;
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerEvent(string eventName, object param = null)
        {
            if (m_eventDictionary.ContainsKey(eventName))
            {
                m_eventDictionary[eventName]?.Invoke(param);
            }
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void ClearAllEvents()
        {
            m_eventDictionary.Clear();
        }
    }
} 