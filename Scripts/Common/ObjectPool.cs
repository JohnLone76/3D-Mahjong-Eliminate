using System.Collections.Generic;
using UnityEngine;

namespace MahjongEliminate.Common
{
    /// <summary>
    /// 通用对象池类
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        // 对象池容器
        private readonly Queue<T> m_pool;
        
        // 对象池预制体
        private readonly T m_prefab;
        
        // 对象池父节点
        private readonly Transform m_parent;
        
        // 对象池容量
        private readonly int m_maxSize;

        public ObjectPool(T prefab, Transform parent, int initialSize, int maxSize)
        {
            m_pool = new Queue<T>();
            m_prefab = prefab;
            m_parent = parent;
            m_maxSize = maxSize;

            // 预热对象池
            for (int i = 0; i < initialSize; i++)
            {
                CreateObject();
            }
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        public T Get()
        {
            T obj;
            if (m_pool.Count > 0)
            {
                obj = m_pool.Dequeue();
            }
            else if (m_pool.Count < m_maxSize)
            {
                obj = CreateObject();
            }
            else
            {
                Debug.LogWarning("Object pool has reached its maximum size!");
                return null;
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(m_parent);
            
            if (m_pool.Count < m_maxSize)
            {
                m_pool.Enqueue(obj);
            }
            else
            {
                Object.Destroy(obj.gameObject);
            }
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        private T CreateObject()
        {
            T obj = Object.Instantiate(m_prefab, m_parent);
            obj.gameObject.SetActive(false);
            m_pool.Enqueue(obj);
            return obj;
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public void Clear()
        {
            while (m_pool.Count > 0)
            {
                T obj = m_pool.Dequeue();
                Object.Destroy(obj.gameObject);
            }
        }
    }
} 