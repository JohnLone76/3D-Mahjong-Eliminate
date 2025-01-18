using UnityEngine;
using System.Collections;

namespace MahjongProject
{
    /// <summary>
    /// 特效对象池
    /// </summary>
    public class EffectPool : BaseObjectPool<EffectObject>
    {
        private static EffectPool m_instance;
        public static EffectPool Instance
        {
            get
            {
                if (m_instance == null)
                {
                    // 从Resources加载预制体
                    GameObject prefab = Resources.Load<GameObject>(Constants.ResourcePaths.Prefabs.EFFECT_PATH + "ClickEffect");
                    if (prefab == null)
                    {
                        Debug.LogError("找不到特效预制体：ClickEffect");
                        return null;
                    }

                    // 确保预制体上有 EffectObject 组件
                    EffectObject effectObj = prefab.GetComponent<EffectObject>();
                    if (effectObj == null)
                    {
                        effectObj = prefab.AddComponent<EffectObject>();
                    }

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

        protected override void OnGet(EffectObject effect)
        {
            base.OnGet(effect);
            effect.Clear();
            effect.Play();
        }

        protected override void OnReturn(EffectObject effect)
        {
            base.OnReturn(effect);
            effect.Stop();
            effect.Clear();
        }

        /// <summary>
        /// 播放特效
        /// </summary>
        public EffectObject PlayEffect(Vector3 position, float duration = 1f)
        {
            EffectObject effect = Get();
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
        private IEnumerator AutoRecycle(EffectObject effect, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (effect != null)
            {
                ReturnToPool(effect);
            }
        }
    }
} 