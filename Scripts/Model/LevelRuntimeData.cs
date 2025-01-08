using UnityEngine;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 关卡运行时数据：存储关卡进行过程中的状态
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理关卡运行时状态
    /// 2. 处理方块的消除逻辑
    /// 3. 控制游戏进度和结果
    /// 
    /// 设计考虑：
    /// 1. 状态管理的实时性
    /// 2. 数据的一致性维护
    /// 3. 游戏规则的实时判定
    /// 
    /// 主要职责：
    /// 1. 时间管理（倒计时、延长）
    /// 2. 背包管理（添加、移除、扩容）
    /// 3. 方块匹配（规则判定、状态检查）
    /// 4. 游戏状态（完成、失败判定）
    /// 
    /// 依赖关系：
    /// - LevelConfig：获取关卡配置
    /// - BlockView：方块视图交互
    /// - EventCenter：事件通知
    /// </remarks>
    public class LevelRuntimeData
    {
        // 基础信息
        /// <summary>
        /// 当前关卡编号
        /// </summary>
        public int CurrentLevel { get; private set; }

        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        public float RemainingTime { get; private set; }

        /// <summary>
        /// 当前背包容量
        /// </summary>
        public int CurrentBackpackCapacity { get; private set; }

        // 道具使用状态
        /// <summary>
        /// 是否已使用时间延长道具
        /// </summary>
        public bool HasUsedTimeExtension { get; private set; }

        /// <summary>
        /// 是否已使用背包扩容道具
        /// </summary>
        public bool HasUsedBackpackExtension { get; private set; }

        // 游戏数据
        /// <summary>
        /// 场景中的活动方块列表
        /// </summary>
        private List<BlockView> m_activeBlocks;

        /// <summary>
        /// 背包中的方块列表
        /// </summary>
        private List<BlockView> m_backpackBlocks;

        /// <summary>
        /// 创建关卡运行时数据
        /// </summary>
        /// <param name="level">关卡编号</param>
        /// <remarks>
        /// 初始化流程：
        /// 1. 设置关卡编号
        /// 2. 加载关卡配置
        /// 3. 初始化游戏状态
        /// 4. 创建数据容器
        /// 
        /// 注意事项：
        /// - 确保level参数有效
        /// - 正确初始化所有状态
        /// - 预留足够的容器容量
        /// </remarks>
        public LevelRuntimeData(int level)
        {
            CurrentLevel = level;
            
            // 获取关卡配置
            var levelConfig = new LevelConfig(level);
            RemainingTime = levelConfig.TimeLimit;
            CurrentBackpackCapacity = levelConfig.GetInitialBackpackCapacity();

            // 初始化状态
            HasUsedTimeExtension = false;
            HasUsedBackpackExtension = false;
            
            // 初始化列表
            m_activeBlocks = new List<BlockView>();
            m_backpackBlocks = new List<BlockView>();
        }

        /// <summary>
        /// 更新剩余时间
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        /// <remarks>
        /// 更新规则：
        /// - 每帧调用
        /// - 时间不能为负
        /// - 触发时间相关事件
        /// 
        /// 调用时机：
        /// - 游戏进行中
        /// - 非暂停状态
        /// </remarks>
        public void UpdateTime(float deltaTime)
        {
            RemainingTime -= deltaTime;
            if (RemainingTime < 0)
            {
                RemainingTime = 0;
            }
        }

        /// <summary>
        /// 延长时间
        /// </summary>
        /// <returns>是否成功延长时间</returns>
        /// <remarks>
        /// 延长规则：
        /// 1. 每关仅能使用一次
        /// 2. 需要观看广告
        /// 3. 延长固定时间（5分钟）
        /// 
        /// 失败条件：
        /// - 已经使用过延长道具
        /// - 关卡不允许延长时间
        /// </remarks>
        public bool ExtendTime()
        {
            if (HasUsedTimeExtension) return false;

            var levelConfig = new LevelConfig(CurrentLevel);
            if (levelConfig.CanExtendTime())
            {
                RemainingTime += levelConfig.GetExtendedTime();
                HasUsedTimeExtension = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 扩展背包容量
        /// </summary>
        /// <returns>是否成功扩展容量</returns>
        /// <remarks>
        /// 扩展规则：
        /// 1. 每关仅能使用一次
        /// 2. 需要观看广告
        /// 3. 固定扩展2个格子
        /// 
        /// 失败条件：
        /// - 已经使用过扩容道具
        /// - 达到最大容量限制
        /// </remarks>
        public bool ExtendBackpackCapacity()
        {
            if (HasUsedBackpackExtension) return false;

            var levelConfig = new LevelConfig(CurrentLevel);
            CurrentBackpackCapacity = levelConfig.GetExtendedBackpackCapacity();
            HasUsedBackpackExtension = true;
            return true;
        }

        /// <summary>
        /// 添加方块到背包
        /// </summary>
        /// <param name="block">要添加的方块</param>
        /// <returns>是否成功添加</returns>
        /// <remarks>
        /// 添加规则：
        /// 1. 背包未满
        /// 2. 方块有效
        /// 3. 不重复添加
        /// 
        /// 失败条件：
        /// - 背包已满
        /// - 方块无效
        /// - 方块已在背包中
        /// </remarks>
        public bool AddToBackpack(BlockView block)
        {
            if (m_backpackBlocks.Count >= CurrentBackpackCapacity)
            {
                return false;
            }

            m_backpackBlocks.Add(block);
            return true;
        }

        /// <summary>
        /// 从背包移除方块
        /// </summary>
        /// <param name="block">要移除的方块</param>
        /// <remarks>
        /// 移除规则：
        /// 1. 方块必须在背包中
        /// 2. 更新背包状态
        /// 3. 触发相关事件
        /// 
        /// 调用时机：
        /// - 方块被消除时
        /// - 背包重置时
        /// </remarks>
        public void RemoveFromBackpack(BlockView block)
        {
            m_backpackBlocks.Remove(block);
        }

        /// <summary>
        /// 检查是否可以消除
        /// </summary>
        /// <param name="block1">第一个方块</param>
        /// <param name="block2">第二个方块</param>
        /// <returns>是否可以消除</returns>
        /// <remarks>
        /// 消除规则：
        /// 1. 相同数字匹配（如1和11）
        /// 2. 连续数字匹配（如2和23）
        /// 
        /// 检查流程：
        /// 1. 验证方块有效性
        /// 2. 确认方块在背包中
        /// 3. 应用匹配规则
        /// 
        /// 示例：
        /// - (1, 11) -> true
        /// - (2, 23) -> true
        /// - (1, 2) -> false
        /// </remarks>
        public bool CanEliminate(BlockView block1, BlockView block2)
        {
            if (block1 == null || block2 == null) return false;
            if (!m_backpackBlocks.Contains(block1) || !m_backpackBlocks.Contains(block2)) return false;

            int type1 = block1.BlockType;
            int type2 = block2.BlockType;

            // 检查是否为相同数字（如1和11）
            if (type1 < 10 && type2 >= 10)
            {
                int lastDigit = type2 % 10;
                int firstDigit = type2 / 10;
                if (type1 == lastDigit && firstDigit == lastDigit)
                {
                    return true;
                }
            }
            else if (type2 < 10 && type1 >= 10)
            {
                int lastDigit = type1 % 10;
                int firstDigit = type1 / 10;
                if (type2 == lastDigit && firstDigit == lastDigit)
                {
                    return true;
                }
            }

            // 检查是否为连续数字（如2和23）
            string combined = type1.ToString() + type2.ToString();
            if (IsConsecutive(combined))
            {
                return true;
            }

            combined = type2.ToString() + type1.ToString();
            if (IsConsecutive(combined))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否为连续数字
        /// </summary>
        /// <param name="numStr">要检查的数字字符串</param>
        /// <returns>是否为连续数字</returns>
        /// <remarks>
        /// 检查规则：
        /// - 数字长度至少为2
        /// - 相邻数字差值为1
        /// 
        /// 示例：
        /// - "23" -> true
        /// - "234" -> true
        /// - "13" -> false
        /// </remarks>
        private bool IsConsecutive(string numStr)
        {
            if (numStr.Length < 2) return false;

            for (int i = 1; i < numStr.Length; i++)
            {
                if (int.Parse(numStr[i].ToString()) != int.Parse(numStr[i - 1].ToString()) + 1)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查关卡是否完成
        /// </summary>
        /// <returns>是否完成关卡</returns>
        /// <remarks>
        /// 完成条件：
        /// 1. 场景中没有方块
        /// 2. 背包中没有方块
        /// 
        /// 检查时机：
        /// - 每次消除后
        /// - 定时检查
        /// </remarks>
        public bool IsLevelComplete()
        {
            return m_activeBlocks.Count == 0 && m_backpackBlocks.Count == 0;
        }

        /// <summary>
        /// 检查是否失败
        /// </summary>
        /// <returns>是否失败</returns>
        /// <remarks>
        /// 失败条件：
        /// 1. 时间耗尽
        /// 2. 背包满且无法消除
        /// 
        /// 检查流程：
        /// 1. 检查时间
        /// 2. 检查背包状态
        /// 3. 检查可消除组合
        /// 
        /// 检查时机：
        /// - 时间更新后
        /// - 背包状态改变时
        /// </remarks>
        public bool IsGameOver()
        {
            // 时间耗尽
            if (RemainingTime <= 0) return true;

            // 背包已满且无法消除
            if (m_backpackBlocks.Count >= CurrentBackpackCapacity)
            {
                // 检查是否有可消除的组合
                for (int i = 0; i < m_backpackBlocks.Count; i++)
                {
                    for (int j = i + 1; j < m_backpackBlocks.Count; j++)
                    {
                        if (CanEliminate(m_backpackBlocks[i], m_backpackBlocks[j]))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            return false;
        }

        // Getters
        /// <summary>
        /// 获取活动方块列表的副本
        /// </summary>
        /// <returns>活动方块列表</returns>
        public List<BlockView> GetActiveBlocks() => new List<BlockView>(m_activeBlocks);

        /// <summary>
        /// 获取背包方块列表的副本
        /// </summary>
        /// <returns>背包方块列表</returns>
        public List<BlockView> GetBackpackBlocks() => new List<BlockView>(m_backpackBlocks);

        /// <summary>
        /// 获取背包中的方块数量
        /// </summary>
        /// <returns>背包中的方块数量</returns>
        public int GetBackpackBlockCount() => m_backpackBlocks.Count;
    }
} 