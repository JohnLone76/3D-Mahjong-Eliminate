using UnityEngine;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 特效管理器：负责特效的加载、播放和回收
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理特效对象池
    /// 2. 加载和实例化特效
    /// 3. 控制特效的生命周期
    /// 
    /// 设计考虑：
    /// 1. 使用对象池优化性能
    /// 2. 支持自动回收
    /// 3. 预加载常用特效
    /// </remarks>
    public class EffectManager : BaseController
    {
        private static EffectManager m_instance;
        public static EffectManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("EffectManager");
                    m_instance = go.AddComponent<EffectManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        // 特效对象池
        private Dictionary<string, BaseObjectPool<ParticleSystem>> m_effectPools;

        // 特效预制体缓存
        private Dictionary<string, GameObject> m_effectPrefabs;

        protected override void OnInit()
        {
            base.OnInit();
            m_effectPools = new Dictionary<string, BaseObjectPool<ParticleSystem>>();
            m_effectPrefabs = new Dictionary<string, GameObject>();
            PreloadEffects();
        }

        /// <summary>
        /// 预加载特效
        /// </summary>
        /// <remarks>
        /// 预加载流程：
        /// 1. 加载常用特效预制体
        /// 2. 初始化对象池
        /// 3. 预热对象池
        /// </remarks>
        private void PreloadEffects()
        {
            // 预加载点击特效
            PreloadEffect("ClickEffect", Constants.PoolConfig.INITIAL_EFFECT_POOL_SIZE);
            
            // 预加载消除特效
            PreloadEffect("DestroyEffect", Constants.PoolConfig.INITIAL_EFFECT_POOL_SIZE);
        }

        /// <summary>
        /// 预加载特效
        /// </summary>
        /// <param name="effectName">特效名称</param>
        /// <param name="preloadCount">预加载数量</param>
        private void PreloadEffect(string effectName, int preloadCount)
        {
            // 加载预制体
            GameObject prefab = ResourceLoader.LoadEffectPrefab(effectName);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load effect prefab: {effectName}");
                return;
            }

            // 缓存预制体
            m_effectPrefabs[effectName] = prefab;

            // 创建对象池
            var pool = EffectPool.Instance;

            // 缓存对象池
            m_effectPools[effectName] = pool;
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        /// <param name="effectName">特效名称</param>
        /// <param name="position">播放位置</param>
        /// <param name="autoRecycle">是否自动回收</param>
        /// <returns>特效实例</returns>
        public ParticleSystem PlayEffect(string effectName, Vector3 position, bool autoRecycle = true)
        {
            // 获取对象池
            if (!m_effectPools.TryGetValue(effectName, out var pool))
            {
                PreloadEffect(effectName, Constants.PoolConfig.INITIAL_EFFECT_POOL_SIZE);
                pool = m_effectPools[effectName];
            }

            // 获取特效实例
            var effect = pool.Get();
            if (effect == null) return null;

            // 设置位置
            effect.transform.position = position;

            // 自动回收
            if (autoRecycle)
            {
                float duration = effect.main.duration + effect.main.startLifetime.constantMax;
                StartCoroutine(Utils.DelayAction(duration, () =>
                {
                    if (effect != null)
                    {
                        pool.ReturnToPool(effect);
                    }
                }));
            }

            return effect;
        }

        /// <summary>
        /// 回收特效
        /// </summary>
        /// <param name="effectName">特效名称</param>
        /// <param name="effect">特效实例</param>
        public void RecycleEffect(string effectName, ParticleSystem effect)
        {
            if (effect == null) return;

            if (m_effectPools.TryGetValue(effectName, out var pool))
            {
                pool.ReturnToPool(effect);
            }
            else
            {
                Destroy(effect.gameObject);
            }
        }

        /// <summary>
        /// 清理所有特效
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (var pool in m_effectPools.Values)
            {
                pool.Clear();
            }
        }
    }
} 