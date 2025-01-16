using UnityEngine;
using System.Collections;

namespace MahjongProject
{
    /// <summary>
    /// 游戏控制器：负责协调游戏主场景的所有功能
    /// </summary>
    public class GameController : MonoBehaviour
    {
        private static GameController m_instance;
        public static GameController Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("GameController");
                    m_instance = go.AddComponent<GameController>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        // 游戏状态
        private bool m_isGameRunning;
        private bool m_hasExtendedTime;
        private bool m_hasExtendedBackpack;

        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        private void InitializeGame()
        {
            // 1. 获取当前关卡配置
            int currentLevel = GameStateData.Instance.CurrentLevel;
            LevelConfig levelConfig = ConfigManager.Instance.GetLevelConfig(currentLevel);

            if (levelConfig == null)
            {
                Debug.LogError($"找不到关卡配置：{currentLevel}");
                return;
            }

            // 2. 初始化时间系统
            TimeManager.Instance.StartTimer(levelConfig.TimeLimit);
            m_hasExtendedTime = false;

            // 3. 初始化背包系统
            BackpackManager.Instance.Initialize(levelConfig.GetInitialBackpackCapacity());
            m_hasExtendedBackpack = false;

            // 4. 生成方块
            BlockManager.Instance.GenerateBlocks(levelConfig);

            // 5. 注册事件监听
            RegisterEvents();

            // 6. 开始游戏
            m_isGameRunning = true;
            EventCenter.Instance.TriggerEvent(Constants.EventNames.GAME_START);
        }

        /// <summary>
        /// 注册事件监听
        /// </summary>
        private void RegisterEvents()
        {
            // 监听时间耗尽事件
            TimeManager.Instance.OnTimeUp += OnTimeUp;

            // 监听背包满事件
            EventCenter.Instance.AddListener(Constants.EventNames.BACKPACK_FULL, OnBackpackFull);

            // 监听方块消除事件
            EventCenter.Instance.AddListener(Constants.EventNames.BLOCKS_ELIMINATED, OnBlocksEliminated);
        }

        /// <summary>
        /// 时间耗尽回调
        /// </summary>
        private void OnTimeUp()
        {
            if (!m_isGameRunning) return;

            if (!m_hasExtendedTime)
            {
                // 显示广告延长时间的选项
                ShowTimeExtendOption();
            }
            else
            {
                // 游戏失败
                GameOver(false);
            }
        }

        /// <summary>
        /// 背包满回调
        /// </summary>
        private void OnBackpackFull(object param)
        {
            if (!m_isGameRunning) return;

            if (!m_hasExtendedBackpack)
            {
                // 显示广告扩展背包的选项
                ShowBackpackExtendOption();
            }
            else
            {
                // 游戏失败
                GameOver(false);
            }
        }

        /// <summary>
        /// 方块消除回调
        /// </summary>
        private void OnBlocksEliminated(object param)
        {
            if (!m_isGameRunning) return;

            // 检查是否所有方块都已消除
            if (BlockManager.Instance.GetActiveBlocks().Count == 0)
            {
                // 游戏胜利
                GameOver(true);
            }
        }

        /// <summary>
        /// 显示延长时间选项
        /// </summary>
        private void ShowTimeExtendOption()
        {
            // TODO: 显示广告UI
            // 广告完成后调用 ExtendTime()
        }

        /// <summary>
        /// 显示扩展背包选项
        /// </summary>
        private void ShowBackpackExtendOption()
        {
            // TODO: 显示广告UI
            // 广告完成后调用 ExtendBackpack()
        }

        /// <summary>
        /// 延长时间
        /// </summary>
        public void ExtendTime()
        {
            if (m_hasExtendedTime) return;

            LevelConfig levelConfig = ConfigManager.Instance.GetLevelConfig(GameStateData.Instance.CurrentLevel);
            if (levelConfig != null && levelConfig.CanExtendTime())
            {
                TimeManager.Instance.ExtendTime(levelConfig.GetExtendedTime());
                m_hasExtendedTime = true;
            }
        }

        /// <summary>
        /// 扩展背包
        /// </summary>
        public void ExtendBackpack()
        {
            if (m_hasExtendedBackpack) return;

            LevelConfig levelConfig = ConfigManager.Instance.GetLevelConfig(GameStateData.Instance.CurrentLevel);
            if (levelConfig != null)
            {
                BackpackManager.Instance.ExtendCapacity(levelConfig.GetExtendedBackpackCapacity());
                m_hasExtendedBackpack = true;
                EventCenter.Instance.TriggerEvent(Constants.EventNames.BACKPACK_EXTENDED);
            }
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="isVictory">是否胜利</param>
        private void GameOver(bool isVictory)
        {
            if (!m_isGameRunning) return;

            m_isGameRunning = false;
            TimeManager.Instance.StopTimer();

            if (isVictory)
            {
                // 更新关卡进度
                GameStateData.Instance.CurrentLevel++;
                EventCenter.Instance.TriggerEvent(Constants.EventNames.LEVEL_COMPLETE);
            }
            else
            {
                EventCenter.Instance.TriggerEvent(Constants.EventNames.GAME_OVER);
            }
        }

        private void OnDestroy()
        {
            // 移除事件监听
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeUp -= OnTimeUp;
            }

            EventCenter.Instance.RemoveListener(Constants.EventNames.BACKPACK_FULL, OnBackpackFull);
            EventCenter.Instance.RemoveListener(Constants.EventNames.BLOCKS_ELIMINATED, OnBlocksEliminated);

            if (m_instance == this)
            {
                m_instance = null;
            }
        }
    }
} 