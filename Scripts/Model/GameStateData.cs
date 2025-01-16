using UnityEngine;
using System;

namespace MahjongProject
{
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
        Paused,         // 暂停
        GameOver,       // 游戏结束
        LevelComplete   // 关卡完成
    }

    /// <summary>
    /// 游戏状态数据类：管理游戏全局状态和进度
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理游戏状态（主菜单、游戏中、暂停等）
    /// 2. 追踪关卡进度和分数
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
    public class GameStateData
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

        // 游戏状态相关字段
        private GameState m_currentState;    // 当前游戏状态
        private int m_currentLevel;          // 当前关卡编号
        private int m_score;                 // 当前得分
        private bool m_isNewHighScore;       // 是否创造新高分

        // 事件系统
        public event Action<GameState> OnGameStateChanged;    // 游戏状态改变事件
        public event Action<int> OnScoreChanged;             // 分数改变事件
        public event Action<int> OnLevelChanged;             // 关卡改变事件

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        /// <remarks>
        /// 初始化流程：
        /// 1. 加载存档数据
        /// 2. 初始化游戏状态
        /// </remarks>
        private GameStateData()
        {
            LoadGameData();
        }

        /// <summary>
        /// 当前游戏状态属性
        /// </summary>
        /// <remarks>
        /// 状态变化时：
        /// 1. 触发状态改变事件
        /// 2. 根据新状态执行相应操作（如暂停/继续计时器）
        /// 3. 必要时保存游戏数据
        /// </remarks>
        public GameState CurrentState
        {
            get => m_currentState;
            set
            {
                if (m_currentState != value)
                {
                    m_currentState = value;
                    OnGameStateChanged?.Invoke(m_currentState);

                    // 状态变化时的特殊处理
                    switch (m_currentState)
                    {
                        case GameState.Gaming:
                            TimeManager.Instance.ResumeTimer();
                            break;
                        case GameState.Paused:
                            TimeManager.Instance.PauseTimer();
                            break;
                        case GameState.GameOver:
                        case GameState.LevelComplete:
                            TimeManager.Instance.StopTimer();
                            SaveGameData();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 当前关卡属性
        /// </summary>
        /// <remarks>
        /// 关卡变化时：
        /// 1. 触发关卡改变事件
        /// 2. 保存游戏数据
        /// </remarks>
        public int CurrentLevel
        {
            get => m_currentLevel;
            set
            {
                if (m_currentLevel != value)
                {
                    m_currentLevel = value;
                    OnLevelChanged?.Invoke(m_currentLevel);
                    SaveGameData();
                }
            }
        }

        /// <summary>
        /// 当前分数属性
        /// </summary>
        /// <remarks>
        /// 分数变化时：
        /// 1. 触发分数改变事件
        /// 2. 检查是否创造新高分
        /// </remarks>
        public int Score
        {
            get => m_score;
            set
            {
                if (m_score != value)
                {
                    m_score = value;
                    OnScoreChanged?.Invoke(m_score);
                    CheckHighScore();
                }
            }
        }

        /// <summary>
        /// 是否创造新高分
        /// </summary>
        public bool IsNewHighScore => m_isNewHighScore;

        /// <summary>
        /// 开始新游戏
        /// </summary>
        /// <remarks>
        /// 执行流程：
        /// 1. 重置关卡为第一关
        /// 2. 重置分数为0
        /// 3. 重置新高分标志
        /// 4. 切换到Gaming状态
        /// 5. 保存游戏数据
        /// </remarks>
        public void StartNewGame()
        {
            CurrentLevel = 1;
            Score = 0;
            m_isNewHighScore = false;
            CurrentState = GameState.Gaming;
            SaveGameData();
        }

        /// <summary>
        /// 进入下一关
        /// </summary>
        /// <remarks>
        /// 执行流程：
        /// 1. 增加关卡编号
        /// 2. 切换到Gaming状态
        /// 3. 保存游戏数据
        /// </remarks>
        public void EnterNextLevel()
        {
            CurrentLevel++;
            CurrentState = GameState.Gaming;
            SaveGameData();
        }

        /// <summary>
        /// 检查高分
        /// </summary>
        /// <remarks>
        /// 检查流程：
        /// 1. 获取历史最高分
        /// 2. 比较当前分数
        /// 3. 如果超过最高分，更新记录并设置新高分标志
        /// </remarks>
        private void CheckHighScore()
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (m_score > highScore)
            {
                PlayerPrefs.SetInt("HighScore", m_score);
                m_isNewHighScore = true;
            }
        }

        /// <summary>
        /// 保存游戏数据
        /// </summary>
        /// <remarks>
        /// 保存内容：
        /// 1. 当前关卡
        /// 2. 当前分数
        /// 
        /// 保存时机：
        /// 1. 关卡完成时
        /// 2. 游戏结束时
        /// 3. 分数更新时
        /// </remarks>
        private void SaveGameData()
        {
            PlayerPrefs.SetInt("CurrentLevel", m_currentLevel);
            PlayerPrefs.SetInt("CurrentScore", m_score);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载游戏数据
        /// </summary>
        /// <remarks>
        /// 加载内容：
        /// 1. 当前关卡（默认为1）
        /// 2. 当前分数（默认为0）
        /// 3. 初始状态设为主菜单
        /// </remarks>
        private void LoadGameData()
        {
            m_currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            m_score = PlayerPrefs.GetInt("CurrentScore", 0);
            m_currentState = GameState.MainMenu;
        }

        /// <summary>
        /// 重置游戏数据
        /// </summary>
        /// <remarks>
        /// 重置流程：
        /// 1. 删除所有存档数据
        /// 2. 删除最高分记录
        /// 3. 重新加载初始数据
        /// </remarks>
        public void ResetGameData()
        {
            PlayerPrefs.DeleteKey("CurrentLevel");
            PlayerPrefs.DeleteKey("CurrentScore");
            PlayerPrefs.DeleteKey("HighScore");
            PlayerPrefs.Save();
            LoadGameData();
        }
    }
} 