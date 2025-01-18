using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 特效对象：负责管理特效预制体的所有粒子系统
    /// </summary>
    public class EffectObject : MonoBehaviour
    {
        private ParticleSystem[] m_particleSystems;

        private void Awake()
        {
            m_particleSystems = GetComponentsInChildren<ParticleSystem>();
        }

        /// <summary>
        /// 播放所有粒子系统
        /// </summary>
        public void Play()
        {
            foreach (var ps in m_particleSystems)
            {
                ps.Clear();
                ps.Play();
            }
        }

        /// <summary>
        /// 停止所有粒子系统
        /// </summary>
        public void Stop()
        {
            foreach (var ps in m_particleSystems)
            {
                ps.Stop();
            }
        }

        /// <summary>
        /// 清除所有粒子系统的粒子
        /// </summary>
        public void Clear()
        {
            foreach (var ps in m_particleSystems)
            {
                ps.Clear();
            }
        }
    }
} 