using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 游戏管理器，负责管理游戏全局状态和MVC实例
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager m_instance;
        public static GameManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    m_instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        // 初始化状态
        private bool m_isInitialized;
        public bool IsInitialized => m_isInitialized;

        // 事件
        public event System.Action OnGameInitialized;

        private void Awake()
        {
            if (m_instance != null && m_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_instance = this;
            DontDestroyOnLoad(gameObject);
            InitGame();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitGame()
        {
            if (m_isInitialized) return;

            try
            {
                // 确保EventCenter已经初始化
                EventCenter.Instance.ToString();

                // 确保ConfigManager已经初始化
                ConfigManager.Instance.ToString();

                // 初始化各个系统
                InitSystems();

                // 应用配置
                ApplyConfig();

                // 标记初始化完成
                m_isInitialized = true;
                OnGameInitialized?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"游戏初始化失败: {e.Message}");
                m_isInitialized = false;
            }
        }

        /// <summary>
        /// 初始化各个系统
        /// </summary>
        private void InitSystems()
        {
            // 初始化对象池
            InitObjectPools();
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        private void InitObjectPools()
        {
            // 预热方块对象池
            BlockPool.Instance.Prewarm();
            
            // 预热特效对象池
            EffectPool.Instance.Prewarm();
        }

        /// <summary>
        /// 应用配置
        /// </summary>
        private void ApplyConfig()
        {
            var config = ConfigManager.Instance.GameConfig;
            
            // 应用音频设置
            AudioListener.volume = config.MusicVolume;

            // 应用相机设置
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(0, config.CameraHeight, -config.CameraHeight);
                Camera.main.transform.rotation = Quaternion.Euler(config.CameraAngle, 0, 0);
            }

            // 应用物理设置
            Physics.gravity = new Vector3(0, -9.81f * config.GravityScale, 0);

            // 应用UI设置
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                canvas.scaleFactor = config.UiScale;
            }
        }

        /// <summary>
        /// 游戏暂停
        /// </summary>
        public void PauseGame()
        {
            Time.timeScale = 0;
            EventCenter.Instance.TriggerEvent(Constants.EventNames.GAME_PAUSE);
        }

        /// <summary>
        /// 游戏继续
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1;
            EventCenter.Instance.TriggerEvent(Constants.EventNames.GAME_RESUME);
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        public void GameOver()
        {
            EventCenter.Instance.TriggerEvent(Constants.EventNames.GAME_OVER);
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }
    }
} 