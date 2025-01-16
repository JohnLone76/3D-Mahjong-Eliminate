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
        /// 加载关卡配置
        /// </summary>
        private void LoadLevelConfigs()
        {
            m_levelConfigs = new Dictionary<int, LevelConfig>();
            TextAsset configAsset = Resources.Load<TextAsset>(Constants.ResourcePaths.Configs.LEVEL_CONFIG);
            if (configAsset != null)
            {
                var wrapper = JsonUtility.FromJson<LevelConfigWrapper>(configAsset.text);
                if (wrapper != null && wrapper.levels != null)
                {
                    foreach (var config in wrapper.levels)
                    {
                        m_levelConfigs[config.LevelId] = config;
                    }
                }
                else
                {
                    Debug.LogError("关卡配置文件格式错误");
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
            LoadLevelConfigs();
        }

        private void OnDestroy()
        {
            if (m_gameConfig != null)
            {
                m_gameConfig.Save();
            }
        }

        [System.Serializable]
        private class LevelConfigWrapper
        {
            public LevelConfig[] levels;
        }
    }
} 