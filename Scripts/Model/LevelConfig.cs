using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MahjongProject
{
    /// <summary>
    /// 关卡配置：定义关卡的参数和规则
    /// </summary>
    [System.Serializable]
    public class LevelConfig
    {
        // 关卡基础信息
        [SerializeField] private int m_levelId;                   // 关卡ID
        [SerializeField] private int m_levelNumber;               // 关卡编号
        [SerializeField] private float m_timeLimit;               // 时间限制
        [SerializeField] private int m_blockCount;                // 方块数量
        
        // 预制体路径和方块类型的序列化支持
        [System.Serializable]
        private class BlockPrefabPathEntry
        {
            [SerializeField] public int blockType;
            [SerializeField] public string prefabPath;

            public BlockPrefabPathEntry(int type, string path)
            {
                blockType = type;
                prefabPath = path;
            }

            public BlockPrefabPathEntry() { }
        }
        [SerializeField] private List<BlockPrefabPathEntry> m_serializedPrefabPaths = new List<BlockPrefabPathEntry>();
        private Dictionary<int, string> m_blockPrefabPaths = new Dictionary<int, string>();

        // 关卡难度参数（推荐值计算用）
        private const int BASE_BLOCK_COUNT = 20;     // 基础方块数量
        private const int BLOCK_INCREMENT = 10;      // 每关增加的方块数量
        private const float BASE_TIME_LIMIT = 600f;  // 基础时间限制（10分钟）
        private const float TIME_DECREMENT = 60f;    // 每关减少的时间（1分钟）
        private const int MAX_DIFFICULTY_LEVEL = 10; // 最大难度关卡
        private const int MIN_TIME_LIMIT_LEVEL = 16; // 最小时间限制关卡

        // 属性访问器
        public int LevelId => m_levelId;
        public int LevelNumber => m_levelNumber;
        public float TimeLimit => m_timeLimit;
        public int BlockCount => m_blockCount;
        public List<int> AvailableBlockTypes => m_serializedPrefabPaths.Select(entry => entry.blockType).ToList();

        /// <summary>
        /// 创建关卡配置
        /// </summary>
        public LevelConfig(int levelNumber)
        {
            m_levelNumber = levelNumber;
            m_levelId = levelNumber;
            m_blockPrefabPaths = new Dictionary<int, string>();
            m_serializedPrefabPaths = new List<BlockPrefabPathEntry>();
            
            // 初始化默认值
            m_timeLimit = BASE_TIME_LIMIT;
            m_blockCount = BASE_BLOCK_COUNT;
        }

        /// <summary>
        /// 获取推荐的方块数量
        /// </summary>
        public int GetRecommendedBlockCount()
        {
            if (m_levelNumber <= MAX_DIFFICULTY_LEVEL)
            {
                return BASE_BLOCK_COUNT + (m_levelNumber - 1) * BLOCK_INCREMENT;
            }
            return BASE_BLOCK_COUNT + (MAX_DIFFICULTY_LEVEL - 1) * BLOCK_INCREMENT;
        }

        /// <summary>
        /// 获取推荐的时间限制
        /// </summary>
        public float GetRecommendedTimeLimit()
        {
            if (m_levelNumber <= MAX_DIFFICULTY_LEVEL)
            {
                return BASE_TIME_LIMIT;
            }
            else if (m_levelNumber < MIN_TIME_LIMIT_LEVEL)
            {
                return BASE_TIME_LIMIT - (m_levelNumber - MAX_DIFFICULTY_LEVEL) * TIME_DECREMENT;
            }
            return BASE_TIME_LIMIT - (MIN_TIME_LIMIT_LEVEL - MAX_DIFFICULTY_LEVEL - 1) * TIME_DECREMENT;
        }

        /// <summary>
        /// 应用推荐的参数值
        /// </summary>
        public void ApplyRecommendedValues()
        {
            m_blockCount = GetRecommendedBlockCount();
            m_timeLimit = GetRecommendedTimeLimit();
        }

        /// <summary>
        /// 生成推荐的方块类型列表
        /// </summary>
        public List<int> GetRecommendedBlockTypes()
        {
            var recommendedTypes = new List<int>();

            // 添加单个数字（1-9）
            for (int i = 1; i <= 9; i++)
            {
                recommendedTypes.Add(i);
            }

            // 添加相同数字组合（11,22,33,44,55,66,77,88,99）
            for (int i = 1; i <= 9; i++)
            {
                recommendedTypes.Add(i * 11);
            }

            // 添加连续数字组合（12,23,34,45,56,67,78,89）
            for (int i = 1; i <= 8; i++)
            {
                recommendedTypes.Add(i * 10 + (i + 1));
            }

            return recommendedTypes;
        }

        /// <summary>
        /// 设置可用方块类型列表
        /// </summary>
        public void SetAvailableBlockTypes(List<int> blockTypes)
        {
            m_serializedPrefabPaths.Clear();
            foreach (var blockType in blockTypes)
            {
                string path = GetBlockPrefabPath(blockType);
                m_serializedPrefabPaths.Add(new BlockPrefabPathEntry(blockType, path));
            }
        }

        /// <summary>
        /// 应用推荐的方块类型列表
        /// </summary>
        public void ApplyRecommendedBlockTypes()
        {
            SetAvailableBlockTypes(GetRecommendedBlockTypes());
        }

        /// <summary>
        /// 更新序列化的预制体路径列表
        /// </summary>
        public void UpdateSerializedPrefabPaths()
        {
            var currentTypes = m_serializedPrefabPaths.Select(entry => entry.blockType).ToList();
            SetAvailableBlockTypes(currentTypes);
        }

        /// <summary>
        /// 从序列化数据恢复字典
        /// </summary>
        private void RestorePrefabPathsFromSerialized()
        {
            if (m_blockPrefabPaths == null)
            {
                m_blockPrefabPaths = new Dictionary<int, string>();
            }
            else
            {
                m_blockPrefabPaths.Clear();
            }

            if (m_serializedPrefabPaths == null)
            {
                m_serializedPrefabPaths = new List<BlockPrefabPathEntry>();
            }

            foreach (var entry in m_serializedPrefabPaths)
            {
                m_blockPrefabPaths[entry.blockType] = entry.prefabPath;
            }
        }

        /// <summary>
        /// 获取方块预制体路径
        /// </summary>
        public string GetBlockPrefabPath(int blockType)
        {
            if (m_blockPrefabPaths != null && m_blockPrefabPaths.TryGetValue(blockType, out string path))
            {
                return path;
            }
            return $"Prefabs/Blocks/Block_{blockType}"; // 默认路径
        }

        /// <summary>
        /// 设置方块预制体路径
        /// </summary>
        public void SetBlockPrefabPath(int blockType, string path)
        {
            if (m_blockPrefabPaths == null)
            {
                m_blockPrefabPaths = new Dictionary<int, string>();
            }
            m_blockPrefabPaths[blockType] = path;
        }

        /// <summary>
        /// 检查是否可以延长时间
        /// </summary>
        public bool CanExtendTime() => true;

        /// <summary>
        /// 获取延长的时间（5分钟）
        /// </summary>
        public float GetExtendedTime() => 300f;

        /// <summary>
        /// 获取初始背包容量（6个格子）
        /// </summary>
        public int GetInitialBackpackCapacity() => 6;

        /// <summary>
        /// 获取扩展背包容量（8个格子）
        /// </summary>
        public int GetExtendedBackpackCapacity() => 8;

        // 在反序列化后调用
        public void OnAfterDeserialize()
        {
            RestorePrefabPathsFromSerialized();
        }

        // 编辑器专用的Set方法
        public void SetLevelId(int value) => m_levelId = value;
        public void SetLevelNumber(int value) => m_levelNumber = value;
        public void SetTimeLimit(float value) => m_timeLimit = value;
        public void SetBlockCount(int value) => m_blockCount = value;
    }
} 