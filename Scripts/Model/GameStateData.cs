using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 游戏状态数据类：管理游戏全局状态和进度
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理游戏状态（主菜单、游戏中、暂停等）
    /// 2. 追踪关卡进度和解锁状态
    /// 3. 处理游戏存档和读档
    /// 
    /// 设计考虑：
    /// 1. 使用单例模式确保全局唯一
    /// 2. 通过事件系统通知状态变化
    /// 3. 支持数据持久化和读取
    /// 
    /// 状态转换：
    /// 1. MainMenu -> Gaming：开始游戏
    /// 2. Gaming -> Paused：暂停游戏
    /// 3. Gaming -> GameOver/LevelComplete：结束判定
    /// </remarks>
    public class GameStateData : BaseModel
    {
        private static GameStateData m_instance;
        public static GameStateData Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new GameStateData();
                }
                return m_instance;
            }
        }

        /// <summary>
        /// 游戏状态枚举：定义所有可能的游戏状态
        /// </summary>
        /// <remarks>
        /// 状态说明：
        /// - MainMenu: 主菜单界面，可进入游戏或查看设置
        /// - Gaming: 游戏进行中，接受玩家输入
        /// - Paused: 游戏暂停，可继续或返回主菜单
        /// - GameOver: 游戏失败，可重试或返回主菜单
        /// - LevelComplete: 关卡完成，可进入下一关
        /// 
        /// 状态转换规则：
        /// 1. MainMenu只能转换到Gaming
        /// 2. Gaming可以转换到Paused/GameOver/LevelComplete
        /// 3. Paused只能返回到Gaming或MainMenu
        /// </remarks>
        public enum GameState
        {
            MainMenu,       // 主菜单
            Gaming,         // 游戏中
            Paused,        // 暂停
            GameOver,      // 游戏结束
            LevelComplete  // 关卡完成
        }

        private GameState m_currentState;              // 当前游戏状态
        private int m_currentLevel;                    // 当前关卡
        private int m_maxUnlockedLevel;               // 最大解锁关卡
        private LevelRuntimeData m_levelRuntimeData;   // 关卡运行时数据

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        /// <remarks>
        /// 初始化流程：
        /// 1. 调用基类构造函数
        /// 2. 加载存档数据
        /// 3. 初始化游戏状态
        /// </remarks>
        private GameStateData() : base()
        {
            LoadGameData();
        }

        /// <summary>
        /// 加载游戏存档数据
        /// </summary>
        /// <remarks>
        /// 加载流程：
        /// 1. 尝试读取存档文件
        /// 2. 如果存档不存在，使用默认值
        /// 3. 初始化运行时数据
        /// 
        /// 存档内容：
        /// 1. 最大解锁关卡
        /// 2. 当前关卡进度
        /// 3. 游戏设置数据
        /// </remarks>
        private void LoadGameData()
        {
            // 从DataStorage加载存档数据
            var saveData = DataStorage.LoadData<GameSaveData>(Constants.GameSettings.SAVE_KEY);
            if (saveData != null)
            {
                // 检查存档版本
                if (saveData.Version != Constants.GameSettings.SAVE_VERSION)
                {
                    Debug.LogWarning($"存档版本不匹配：当前{Constants.GameSettings.SAVE_VERSION}，存档{saveData.Version}");
                    // TODO: 处理存档升级逻辑
                }

                m_maxUnlockedLevel = saveData.MaxUnlockedLevel;
                m_currentLevel = saveData.CurrentLevel;
            }
            else
            {
                // 使用默认值
                m_maxUnlockedLevel = 1;
                m_currentLevel = 1;
                
                // 创建并保存新的存档
                SaveGameData();
            }

            // 初始化状态
            m_currentState = GameState.MainMenu;
            m_levelRuntimeData = null;
        }

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <remarks>
        /// 保存流程：
        /// 1. 创建存档数据对象
        /// 2. 填充当前游戏状态
        /// 3. 调用DataStorage保存
        /// 
        /// 保存时机：
        /// 1. 关卡完成时
        /// 2. 退出游戏时
        /// 3. 解锁新关卡时
        /// </remarks>
        public void SaveGameData()
        {
            var saveData = new GameSaveData
            {
                MaxUnlockedLevel = m_maxUnlockedLevel,
                CurrentLevel = m_currentLevel,
                Version = Constants.GameSettings.SAVE_VERSION
            };
            DataStorage.SaveData(Constants.GameSettings.SAVE_KEY, saveData);
        }

        /// <summary>
        /// 开始新关卡
        /// </summary>
        /// <param name="level">要开始的关卡编号</param>
        /// <remarks>
        /// 开始流程：
        /// 1. 验证关卡是否已解锁
        /// 2. 创建关卡运行时数据
        /// 3. 切换到Gaming状态
        /// 4. 触发关卡开始事件
        /// </remarks>
        public void StartLevel(int level)
        {
            if (level > m_maxUnlockedLevel) return;

            m_currentLevel = level;
            m_levelRuntimeData = new LevelRuntimeData(level);
            m_currentState = GameState.Gaming;
            SendEvent(Constants.EventNames.LEVEL_STARTED, level);
        }

        /// <summary>
        /// 完成当前关卡
        /// </summary>
        /// <remarks>
        /// 完成流程：
        /// 1. 更新最大解锁关卡
        /// 2. 切换到LevelComplete状态
        /// 3. 保存游戏进度
        /// 4. 触发关卡完成事件
        /// </remarks>
        public void CompleteLevelLevel()
        {
            if (m_currentLevel >= m_maxUnlockedLevel)
            {
                m_maxUnlockedLevel = m_currentLevel + 1;
            }
            m_currentState = GameState.LevelComplete;
            SaveGameData();
            SendEvent(Constants.EventNames.LEVEL_COMPLETE, m_currentLevel);
        }

        /// <summary>
        /// 游戏失败处理
        /// </summary>
        /// <remarks>
        /// 失败流程：
        /// 1. 切换到GameOver状态
        /// 2. 触发游戏结束事件
        /// 3. 保存当前进度
        /// 
        /// 失败原因：
        /// 1. 时间耗尽
        /// 2. 背包满且无法消除
        /// 3. 其他游戏规则判定
        /// </remarks>
        public void GameOver()
        {
            m_currentState = GameState.GameOver;
            SendEvent(Constants.EventNames.GAME_OVER);
            SaveGameData();
        }

        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (m_currentState == GameState.Gaming)
            {
                m_currentState = GameState.Paused;
                SendEvent(Constants.EventNames.GAME_PAUSE);
            }
        }

        /// <summary>
        /// 继续游戏
        /// </summary>
        public void ResumeGame()
        {
            if (m_currentState == GameState.Paused)
            {
                m_currentState = GameState.Gaming;
                SendEvent(Constants.EventNames.GAME_RESUME);
            }
        }

        /// <summary>
        /// 返回主菜单
        /// </summary>
        public void ReturnToMainMenu()
        {
            m_currentState = GameState.MainMenu;
            m_levelRuntimeData = null;
        }

        // 获取器
        public GameState CurrentState => m_currentState;
        public int CurrentLevel => m_currentLevel;
        public int MaxUnlockedLevel => m_maxUnlockedLevel;
        public LevelRuntimeData LevelRuntimeData => m_levelRuntimeData;
    }
} 