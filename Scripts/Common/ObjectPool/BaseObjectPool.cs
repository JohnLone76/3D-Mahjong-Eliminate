using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace MahjongProject
{
    /// <summary>
    /// 对象池基类：提供通用的对象池功能和性能优化
    /// </summary>
    /// <typeparam name="T">池化对象类型，必须是Unity组件</typeparam>
    /// <remarks>
    /// 核心功能：
    /// 1. 对象的创建和回收管理
    /// 2. 支持预热和动态扩容
    /// 3. 性能监控和优化
    /// 
    /// 设计考虑：
    /// 1. 泛型实现支持多种对象类型
    /// 2. 预热机制避免运行时卡顿
    /// 3. 自动扩容满足动态需求
    /// 
    /// 性能指标：
    /// 1. 预热时间 < 200ms
    /// 2. 获取对象时间 < 1ms
    /// 3. 内存碎片率 < 20%
    /// </remarks>
    public abstract class BaseObjectPool<T> where T : Component
    {
        /// <summary>
        /// 对象池队列：存储非活动状态的对象
        /// </summary>
        /// <remarks>
        /// 实现说明：
        /// 1. 使用Queue实现FIFO，确保对象均匀复用
        /// 2. 对象入队时设置为非活动状态
        /// 3. 出队时重置对象状态
        /// </remarks>
        protected Queue<T> m_pool;

        /// <summary>
        /// 对象池根节点：用于组织层级结构
        /// </summary>
        protected Transform m_poolRoot;

        /// <summary>
        /// 预制体：用于创建新对象
        /// </summary>
        protected GameObject m_prefab;

        /// <summary>
        /// 对象池初始大小
        /// </summary>
        /// <remarks>
        /// 设置原则：
        /// 1. 根据场景最大对象数量的1/4设置
        /// 2. 考虑首次加载时的性能影响
        /// 3. 预热时间不超过200ms
        /// </remarks>
        protected int m_initialSize;

        /// <summary>
        /// 对象池最大大小
        /// </summary>
        /// <remarks>
        /// 限制原因：
        /// 1. 控制内存使用上限
        /// 2. 防止无限制扩容
        /// 3. 便于内存管理
        /// </remarks>
        protected int m_maxSize;

        /// <summary>
        /// 是否已完成预热
        /// </summary>
        protected bool m_isPrewarmed;

        /// <summary>
        /// 构造函数：初始化对象池
        /// </summary>
        /// <param name="prefab">对象预制体</param>
        /// <param name="initialSize">初始大小</param>
        /// <param name="maxSize">最大大小</param>
        /// <remarks>
        /// 初始化流程：
        /// 1. 参数验证和赋值
        /// 2. 创建对象池容器
        /// 3. 创建根节点
        /// 4. 设置DontDestroyOnLoad
        /// </remarks>
        protected BaseObjectPool(GameObject prefab, int initialSize, int maxSize)
        {
            m_prefab = prefab;
            m_initialSize = initialSize;
            m_maxSize = maxSize;
            m_pool = new Queue<T>();
            
            CreatePoolRoot();
        }

        /// <summary>
        /// 创建对象池根节点
        /// </summary>
        /// <remarks>
        /// 创建流程：
        /// 1. 创建空物体作为根节点
        /// 2. 设置对象池类型名称
        /// 3. 标记为不销毁
        /// 
        /// 用途：
        /// 1. 组织对象池层级
        /// 2. 方便场景管理
        /// 3. 避免场景切换时销毁
        /// </remarks>
        protected virtual void CreatePoolRoot()
        {
            GameObject root = new GameObject($"{typeof(T).Name}Pool");
            m_poolRoot = root.transform;
            Object.DontDestroyOnLoad(root);
        }

        /// <summary>
        /// 预热对象池
        /// </summary>
        /// <remarks>
        /// 预热流程：
        /// 1. 检查是否已预热
        /// 2. 在规定时间内创建对象
        /// 3. 监控预热时间
        /// 4. 处理超时情况
        /// 
        /// 优化策略：
        /// 1. 分批创建避免卡顿
        /// 2. 超时后停止预热
        /// 3. 记录预热状态
        /// </remarks>
        public virtual void Prewarm()
        {
            if (m_isPrewarmed) return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < m_initialSize; i++)
            {
                T obj = CreateNewObject();
                ReturnToPool(obj);

                // 检查预热时间是否超过200ms
                if (stopwatch.ElapsedMilliseconds > 200)
                {
                    Debug.LogWarning($"{GetType().Name} 预热时间超过200ms，已停止预热。当前数量：{i + 1}");
                    break;
                }
            }

            stopwatch.Stop();
            m_isPrewarmed = true;
            Debug.Log($"{GetType().Name} 预热完成，耗时：{stopwatch.ElapsedMilliseconds}ms，数量：{m_pool.Count}");
        }

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <returns>池化对象实例</returns>
        /// <remarks>
        /// 获取流程：
        /// 1. 检查池中是否有可用对象
        /// 2. 没有则创建新对象
        /// 3. 重置对象状态
        /// 4. 监控获取时间
        /// 
        /// 性能优化：
        /// 1. 快速检查可用性
        /// 2. 控制创建时机
        /// 3. 监控性能指标
        /// </remarks>
        public virtual T Get()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            T obj = null;
            if (m_pool.Count > 0)
            {
                obj = m_pool.Dequeue();
            }
            else if (m_pool.Count < m_maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                Debug.LogWarning($"{GetType().Name} 对象池已满，无法创建新对象");
                return null;
            }

            OnGet(obj);
            stopwatch.Stop();

            // 检查获取时间是否超过1ms
            if (stopwatch.ElapsedMilliseconds > 1)
            {
                Debug.LogWarning($"{GetType().Name} 获取对象耗时超过1ms：{stopwatch.ElapsedMilliseconds}ms");
            }

            return obj;
        }

        /// <summary>
        /// 返回对象到对象池
        /// </summary>
        /// <param name="obj">要返回的对象</param>
        /// <remarks>
        /// 返回流程：
        /// 1. 验证对象有效性
        /// 2. 重置对象状态
        /// 3. 设置父节点
        /// 4. 加入对象池队列
        /// 
        /// 注意事项：
        /// 1. 确保对象状态正确重置
        /// 2. 处理无效对象
        /// 3. 维护对象层级
        /// </remarks>
        public virtual void ReturnToPool(T obj)
        {
            if (obj == null) return;

            OnReturn(obj);
            m_pool.Enqueue(obj);
        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        /// <remarks>
        /// 清理流程：
        /// 1. 逐个销毁对象
        /// 2. 清空队列
        /// 3. 重置计数
        /// 
        /// 使用场景：
        /// 1. 场景切换时
        /// 2. 游戏重置时
        /// 3. 内存回收时
        /// </remarks>
        public virtual void Clear()
        {
            while (m_pool.Count > 0)
            {
                T obj = m_pool.Dequeue();
                Object.Destroy(obj.gameObject);
            }
        }

        /// <summary>
        /// 创建新对象
        /// </summary>
        /// <returns>新创建的对象实例</returns>
        /// <remarks>
        /// 创建流程：
        /// 1. 实例化预制体
        /// 2. 设置父节点
        /// 3. 获取组件引用
        /// 
        /// 优化措施：
        /// 1. 使用对象池根节点
        /// 2. 确保组件存在
        /// 3. 初始状态统一
        /// </remarks>
        protected virtual T CreateNewObject()
        {
            GameObject obj = Object.Instantiate(m_prefab, m_poolRoot);
            T component = obj.GetComponent<T>();
            return component;
        }

        /// <summary>
        /// 获取对象时的处理
        /// </summary>
        /// <param name="obj">获取的对象</param>
        /// <remarks>
        /// 处理内容：
        /// 1. 激活游戏对象
        /// 2. 重置状态
        /// 3. 准备使用
        /// </remarks>
        protected virtual void OnGet(T obj)
        {
            obj.gameObject.SetActive(true);
        }

        /// <summary>
        /// 返回对象时的处理
        /// </summary>
        /// <param name="obj">返回的对象</param>
        /// <remarks>
        /// 处理内容：
        /// 1. 关闭游戏对象
        /// 2. 重置状态
        /// 3. 设置父节点
        /// </remarks>
        protected virtual void OnReturn(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(m_poolRoot);
        }
    }
} 