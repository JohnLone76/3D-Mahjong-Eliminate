using UnityEngine;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        private static ConfigManager m_instance;
        public static ConfigManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("ConfigManager");
                    m_instance = go.AddComponent<ConfigManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        private GameConfig m_gameConfig;                           // 游戏配置
        private Dictionary<int, BlockConfig> m_blockConfigs;       // 方块配置
        private Dictionary<int, LevelConfig> m_levelConfigs;       // 关卡配置

        private void Awake()
        {
            if (m_instance != null && m_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_instance = this;
            DontDestroyOnLoad(gameObject);
            InitConfigs();
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        private void InitConfigs()
        {
            // 加载游戏配置
            m_gameConfig = GameConfig.Load();

            // 加载方块配置
            LoadBlockConfigs();

            // 加载关卡配置
            LoadLevelConfigs();

            // 开启自动保存
            if (Constants.GameSettings.AUTO_SAVE_INTERVAL > 0)
            {
                InvokeRepeating("AutoSave", Constants.GameSettings.AUTO_SAVE_INTERVAL, 
                    Constants.GameSettings.AUTO_SAVE_INTERVAL);
            }
        }

        /// <summary>
        /// 加载方块配置
        /// </summary>
        private void LoadBlockConfigs()
        {
            m_blockConfigs = new Dictionary<int, BlockConfig>();
            TextAsset configAsset = Resources.Load<TextAsset>(Constants.ResourcePaths.Configs.BLOCK_CONFIG);
            if (configAsset != null)
            {
                BlockConfig[] configs = JsonUtility.FromJson<BlockConfig[]>(configAsset.text);
                foreach (var config in configs)
                {
                    m_blockConfigs[config.BlockType] = config;
                }
            }
            else
            {
                Debug.LogError("找不到方块配置文件");
            }
        }

        /// <summary>
        /// 加载关卡配置
        /// </summary>
        private void LoadLevelConfigs()
        {
            m_levelConfigs = new Dictionary<int, LevelConfig>();
            TextAsset configAsset = Resources.Load<TextAsset>(Constants.ResourcePaths.Configs.LEVEL_CONFIG);
            if (configAsset != null)
            {
                LevelConfig[] configs = JsonUtility.FromJson<LevelConfig[]>(configAsset.text);
                foreach (var config in configs)
                {
                    m_levelConfigs[config.LevelId] = config;
                }
            }
            else
            {
                Debug.LogError("找不到关卡配置文件");
            }
        }

        /// <summary>
        /// 自动保存配置
        /// </summary>
        private void AutoSave()
        {
            if (m_gameConfig != null)
            {
                m_gameConfig.Save();
            }
        }

        /// <summary>
        /// 获取游戏配置
        /// </summary>
        public GameConfig GameConfig => m_gameConfig;

        /// <summary>
        /// 获取方块配置
        /// </summary>
        public BlockConfig GetBlockConfig(int blockType)
        {
            if (m_blockConfigs.TryGetValue(blockType, out BlockConfig config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// 获取关卡配置
        /// </summary>
        public LevelConfig GetLevelConfig(int levelId)
        {
            if (m_levelConfigs.TryGetValue(levelId, out LevelConfig config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// 重置所有配置
        /// </summary>
        public void ResetAllConfigs()
        {
            m_gameConfig.Reset();
            m_gameConfig.Save();
            LoadBlockConfigs();
            LoadLevelConfigs();
        }

        private void OnDestroy()
        {
            if (m_gameConfig != null)
            {
                m_gameConfig.Save();
            }
        }
    }
} 