using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MahjongProject
{
    /// <summary>
    /// 菜单UI管理器：负责管理菜单场景的UI交互
    /// </summary>
    public class MenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button m_startButton;    // 开始按钮
        [SerializeField] private Text m_levelText;        // 关卡文本

        private void Start()
        {
            InitializeUI();
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 设置按钮监听
            if (m_startButton != null)
            {
                m_startButton.onClick.AddListener(OnStartButtonClick);
            }

            // 更新关卡文本
            UpdateLevelText();

            // 注册事件监听
            EventCenter.Instance.AddListener(Constants.EventNames.LEVEL_CHANGED, UpdateLevelText);
        }

        /// <summary>
        /// 更新关卡文本
        /// </summary>
        private void UpdateLevelText(object param = null)
        {
            if (m_levelText != null)
            {
                int currentLevel = GameStateData.Instance.CurrentLevel;
                m_levelText.text = $"第{currentLevel}关";
            }
        }

        /// <summary>
        /// 开始按钮点击事件
        /// </summary>
        private void OnStartButtonClick()
        {
            // 切换到游戏场景
            SceneManager.LoadScene("Game");
        }

        private void OnDestroy()
        {
            // 移除事件监听
            EventCenter.Instance.RemoveListener(Constants.EventNames.LEVEL_CHANGED, UpdateLevelText);

            // 移除按钮监听
            if (m_startButton != null)
            {
                m_startButton.onClick.RemoveListener(OnStartButtonClick);
            }
        }
    }
} 