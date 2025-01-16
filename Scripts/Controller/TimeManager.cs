using UnityEngine;
using System;

namespace MahjongProject
{
    /// <summary>
    /// 时间管理器：负责游戏时间的管理和倒计时
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        private static TimeManager m_instance;
        public static TimeManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("TimeManager");
                    m_instance = go.AddComponent<TimeManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        // 时间状态
        private bool m_isRunning;           // 是否正在运行
        private float m_remainingTime;      // 剩余时间
        private bool m_hasExtendedTime;     // 是否已延长时间

        // 事件
        public event Action<float> OnTimeUpdate;        // 时间更新事件
        public event Action OnTimeUp;                   // 时间耗尽事件

        private void Awake()
        {
            if (m_instance != null && m_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (m_isRunning && m_remainingTime > 0)
            {
                m_remainingTime -= Time.deltaTime;
                if (m_remainingTime <= 0)
                {
                    m_remainingTime = 0;
                    OnTimeUp?.Invoke();
                    StopTimer();
                }
                OnTimeUpdate?.Invoke(m_remainingTime);
            }
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        public void StartTimer(float duration)
        {
            m_remainingTime = duration;
            m_isRunning = true;
            m_hasExtendedTime = false;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public void StopTimer()
        {
            m_isRunning = false;
        }

        /// <summary>
        /// 暂停计时
        /// </summary>
        public void PauseTimer()
        {
            m_isRunning = false;
        }

        /// <summary>
        /// 继续计时
        /// </summary>
        public void ResumeTimer()
        {
            if (m_remainingTime > 0)
            {
                m_isRunning = true;
            }
        }

        /// <summary>
        /// 延长时间
        /// </summary>
        /// <param name="extraTime">额外时间（秒）</param>
        /// <returns>是否成功延长时间</returns>
        public bool ExtendTime(float extraTime)
        {
            if (m_hasExtendedTime) return false;

            m_remainingTime += extraTime;
            m_hasExtendedTime = true;
            if (!m_isRunning)
            {
                ResumeTimer();
            }
            return true;
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        public float RemainingTime => m_remainingTime;

        /// <summary>
        /// 是否已延长时间
        /// </summary>
        public bool HasExtendedTime => m_hasExtendedTime;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => m_isRunning;

        /// <summary>
        /// 格式化时间显示
        /// </summary>
        /// <param name="timeInSeconds">时间（秒）</param>
        /// <returns>格式化的时间字符串（mm:ss）</returns>
        public static string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60);
            return $"{minutes:00}:{seconds:00}";
        }
    }
} 