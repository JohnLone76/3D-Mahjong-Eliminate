using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MahjongProject
{
    /// <summary>
    /// 时间UI组件：负责显示游戏时间
    /// </summary>
    public class TimeUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_timeText;           // 时间文本
        [SerializeField] private Image m_timeProgressBar;              // 时间进度条
        [SerializeField] private float m_warningThreshold = 10f;       // 警告阈值（秒）
        [SerializeField] private Color m_normalColor = Color.white;    // 正常颜色
        [SerializeField] private Color m_warningColor = Color.red;     // 警告颜色

        private float m_initialTime;                                   // 初始时间

        private void Start()
        {
            // 注册时间更新事件
            TimeManager.Instance.OnTimeUpdate += UpdateTimeDisplay;
            TimeManager.Instance.OnTimeUp += OnTimeUp;
        }

        private void OnDestroy()
        {
            // 取消注册事件
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeUpdate -= UpdateTimeDisplay;
                TimeManager.Instance.OnTimeUp -= OnTimeUp;
            }
        }

        /// <summary>
        /// 初始化时间显示
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        public void Initialize(float duration)
        {
            m_initialTime = duration;
            UpdateTimeDisplay(duration);
        }

        /// <summary>
        /// 更新时间显示
        /// </summary>
        /// <param name="remainingTime">剩余时间</param>
        private void UpdateTimeDisplay(float remainingTime)
        {
            // 更新时间文本
            m_timeText.text = TimeManager.FormatTime(remainingTime);

            // 更新进度条
            if (m_timeProgressBar != null)
            {
                m_timeProgressBar.fillAmount = remainingTime / m_initialTime;
            }

            // 更新颜色
            if (remainingTime <= m_warningThreshold)
            {
                m_timeText.color = m_warningColor;
            }
            else
            {
                m_timeText.color = m_normalColor;
            }
        }

        /// <summary>
        /// 时间耗尽回调
        /// </summary>
        private void OnTimeUp()
        {
            // 可以在这里添加时间耗尽时的特效或动画
            Debug.Log("Time's up!");
        }
    }
} 