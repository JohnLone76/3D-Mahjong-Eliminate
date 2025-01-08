using UnityEngine;
using System.Collections;

namespace MahjongProject
{
    /// <summary>
    /// 特效对象池
    /// </summary>
    public class EffectPool : BaseObjectPool<ParticleSystem>
    {
        private static EffectPool m_instance;
        public static EffectPool Instance
        {
            get
            {
                if (m_instance == null)
                {
                    // 从Resources加载预制体
                    GameObject prefab = Resources.Load<GameObject>(Constants.ResourcePaths.Prefabs.EFFECT_PATH + "Effect");
                    m_instance = new EffectPool(prefab);
                }
                return m_instance;
            }
        }

        private EffectPool(GameObject prefab) : base(prefab, 
            Constants.PoolConfig.INITIAL_EFFECT_POOL_SIZE, 
            Constants.PoolConfig.MAX_EFFECT_POOL_SIZE)
        {
        }

        protected override void OnGet(ParticleSystem effect)
        {
            base.OnGet(effect);
            effect.Clear();
            effect.Play();
        }

        protected override void OnReturn(ParticleSystem effect)
        {
            base.OnReturn(effect);
            effect.Stop();
            effect.Clear();
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        public ParticleSystem PlayEffect(Vector3 position, float duration = 1f)
        {
            ParticleSystem effect = Get();
            if (effect != null)
            {
                effect.transform.position = position;
                GameManager.Instance.StartCoroutine(AutoRecycle(effect, duration));
            }
            return effect;
        }

        /// <summary>
        /// 自动回收特效
        /// </summary>
        private IEnumerator AutoRecycle(ParticleSystem effect, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (effect != null)
            {
                ReturnToPool(effect);
            }
        }
    }
} 