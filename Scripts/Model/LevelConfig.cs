using UnityEngine;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 关卡配置：定义关卡的参数和规则
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 关卡参数的计算和管理
    /// 2. 方块类型的生成和配置
    /// 3. 游戏规则的定义和控制
    /// 
    /// 设计考虑：
    /// 1. 使用公式计算替代硬编码配置
    /// 2. 支持动态难度调整
    /// 3. 确保游戏平衡性
    /// 
    /// 难度曲线：
    /// 1. 1-10关：方块数量递增，时间固定
    /// 2. 11-15关：方块数量固定，时间递减
    /// 3. 16关之后：方块数量和时间都固定
    /// 
    /// 扩展性：
    /// 1. 支持新方块类型的添加
    /// 2. 支持难度规则的调整
    /// 3. 预留配置接口
    /// </remarks>
    public class LevelConfig
    {
        // 关卡基础信息
        /// <summary>
        /// 关卡ID，用于配置管理和数据存储
        /// </summary>
        public int LevelId { get; private set; }

        /// <summary>
        /// 关卡编号，用于游戏逻辑和显示
        /// </summary>
        public int LevelNumber { get; private set; }

        /// <summary>
        /// 关卡时间限制（秒）
        /// </summary>
        public float TimeLimit { get; private set; }

        /// <summary>
        /// 关卡方块总数，必须是偶数以确保可配对
        /// </summary>
        public int BlockCount { get; private set; }

        /// <summary>
        /// 当前关卡可用的方块类型列表
        /// </summary>
        public List<int> AvailableBlockTypes { get; private set; }

        // 关卡难度参数
        /// <summary>
        /// 基础方块数量：每关的起始数量
        /// </summary>
        private const int BASE_BLOCK_COUNT = 20;

        /// <summary>
        /// 每关增加的方块数量
        /// </summary>
        private const int BLOCK_INCREMENT = 10;

        /// <summary>
        /// 基础时间限制：10分钟（秒）
        /// </summary>
        private const float BASE_TIME_LIMIT = 600f;

        /// <summary>
        /// 每关减少的时间：1分钟（秒）
        /// </summary>
        private const float TIME_DECREMENT = 60f;

        /// <summary>
        /// 最大难度关卡：方块数量增加的上限关卡
        /// </summary>
        private const int MAX_DIFFICULTY_LEVEL = 10;

        /// <summary>
        /// 最小时间限制关卡：时间不再减少的关卡
        /// </summary>
        private const int MIN_TIME_LIMIT_LEVEL = 16;

        /// <summary>
        /// 创建关卡配置
        /// </summary>
        /// <param name="levelNumber">关卡编号</param>
        /// <remarks>
        /// 初始化流程：
        /// 1. 设置关卡编号和ID
        /// 2. 计算关卡参数
        /// 3. 生成可用方块类型
        /// 
        /// 注意事项：
        /// - 确保levelNumber大于0
        /// - 参数计算考虑难度曲线
        /// - 方块类型生成确保可配对
        /// </remarks>
        public LevelConfig(int levelNumber)
        {
            LevelNumber = levelNumber;
            LevelId = levelNumber;
            CalculateParameters();
            GenerateAvailableBlockTypes();
        }

        /// <summary>
        /// 计算关卡参数
        /// </summary>
        /// <remarks>
        /// 计算规则：
        /// 1. 方块数量计算：
        ///    - 1-10关：每关增加10个
        ///    - 10关之后：固定数量
        /// 
        /// 2. 时间限制计算：
        ///    - 1-10关：固定10分钟
        ///    - 11-15关：每关减少1分钟
        ///    - 16关之后：固定5分钟
        /// 
        /// 设计原理：
        /// - 前期保证玩家熟悉游戏
        /// - 中期提供适度挑战
        /// - 后期维持稳定难度
        /// </remarks>
        private void CalculateParameters()
        {
            // 计算方块数量
            if (LevelNumber <= MAX_DIFFICULTY_LEVEL)
            {
                BlockCount = BASE_BLOCK_COUNT + (LevelNumber - 1) * BLOCK_INCREMENT;
            }
            else
            {
                BlockCount = BASE_BLOCK_COUNT + (MAX_DIFFICULTY_LEVEL - 1) * BLOCK_INCREMENT;
            }

            // 计算时间限制
            if (LevelNumber <= MAX_DIFFICULTY_LEVEL)
            {
                TimeLimit = BASE_TIME_LIMIT;
            }
            else if (LevelNumber < MIN_TIME_LIMIT_LEVEL)
            {
                TimeLimit = BASE_TIME_LIMIT - (LevelNumber - MAX_DIFFICULTY_LEVEL) * TIME_DECREMENT;
            }
            else
            {
                TimeLimit = BASE_TIME_LIMIT - (MIN_TIME_LIMIT_LEVEL - MAX_DIFFICULTY_LEVEL - 1) * TIME_DECREMENT;
            }
        }

        /// <summary>
        /// 生成可用方块类型列表
        /// </summary>
        /// <remarks>
        /// 生成规则：
        /// 1. 单个数字（1-9）：基础类型
        /// 2. 相同数字组合（11-99）：每个数字与自身组合
        /// 3. 连续数字组合（12-89）：相邻数字组合
        /// 
        /// 匹配规则：
        /// 1. 单个数字可以与对应的双数字匹配（如1和11）
        /// 2. 单个数字可以与包含它的连续数字匹配（如2和23）
        /// 
        /// 设计考虑：
        /// - 确保每种类型都有匹配对象
        /// - 保持游戏的趣味性和挑战性
        /// - 预留扩展空间
        /// </remarks>
        private void GenerateAvailableBlockTypes()
        {
            AvailableBlockTypes = new List<int>();

            // 添加单个数字（1-9）
            for (int i = 1; i <= 9; i++)
            {
                AvailableBlockTypes.Add(i);
            }

            // 添加相同数字组合（11,22,33,44,55,66,77,88,99）
            for (int i = 1; i <= 9; i++)
            {
                AvailableBlockTypes.Add(i * 11);
            }

            // 添加连续数字组合（12,23,34,45,56,67,78,89）
            for (int i = 1; i <= 8; i++)
            {
                AvailableBlockTypes.Add(i * 10 + (i + 1));
            }
        }

        /// <summary>
        /// 检查是否可以延长时间
        /// </summary>
        /// <returns>是否可以延长时间</returns>
        /// <remarks>
        /// 使用规则：
        /// - 每关仅能使用一次
        /// - 需要观看广告
        /// - 所有关卡都可用
        /// </remarks>
        public bool CanExtendTime()
        {
            return true;
        }

        /// <summary>
        /// 获取延长的时间
        /// </summary>
        /// <returns>延长的时间（秒）</returns>
        /// <remarks>
        /// 延长规则：
        /// - 固定延长5分钟
        /// - 与关卡难度无关
        /// - 可与原有时间叠加
        /// </remarks>
        public float GetExtendedTime()
        {
            return 300f; // 5分钟
        }

        /// <summary>
        /// 获取初始背包容量
        /// </summary>
        /// <returns>初始背包容量</returns>
        /// <remarks>
        /// 设计考虑：
        /// - 固定6个格子
        /// - 适合大多数玩家操作
        /// - 保持游戏节奏
        /// </remarks>
        public int GetInitialBackpackCapacity()
        {
            return 6;
        }

        /// <summary>
        /// 获取扩展背包容量
        /// </summary>
        /// <returns>扩展后的背包容量</returns>
        /// <remarks>
        /// 扩展规则：
        /// - 观看广告后扩展到8个格子
        /// - 每关仅能使用一次
        /// - 不可与其他扩展叠加
        /// </remarks>
        public int GetExtendedBackpackCapacity()
        {
            return 8;
        }
    }
} 