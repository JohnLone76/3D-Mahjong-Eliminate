using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 背包数据类：管理背包中方块的数据和消除逻辑
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理背包容量（默认6格，可扩展至8格）
    /// 2. 处理方块的添加、移除和消除
    /// 3. 维护方块的排序和分组
    /// 
    /// 设计考虑：
    /// 1. 使用单例模式确保全局唯一
    /// 2. 通过事件系统通知UI更新
    /// 3. 支持多组同时消除的场景
    /// 
    /// 性能优化：
    /// 1. 使用Dictionary加速类型分组
    /// 2. 批量处理消除，减少事件触发次数
    /// 3. 维护方块引用，避免重复查找
    /// </remarks>
    public class BackpackData : BaseModel
    {
        private static BackpackData m_instance;
        public static BackpackData Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new BackpackData();
                }
                return m_instance;
            }
        }

        private List<BlockData> m_blocks;         // 背包中的方块
        private int m_capacity;                   // 背包容量
        private bool m_isExtended;                // 是否已扩容

        private BackpackData() : base()
        {
        }

        protected override void OnInit()
        {
            m_blocks = new List<BlockData>();
            m_capacity = Constants.MAX_BACKPACK_SIZE;
            m_isExtended = false;
        }

        /// <summary>
        /// 添加方块到背包
        /// </summary>
        /// <param name="block">要添加的方块数据</param>
        /// <returns>是否添加成功</returns>
        /// <remarks>
        /// 添加流程：
        /// 1. 检查背包容量
        /// 2. 更新方块状态
        /// 3. 添加到列表
        /// 4. 触发事件通知
        /// 5. 检查消除
        /// 6. 更新排序
        /// </remarks>
        public bool AddBlock(BlockData block)
        {
            if (IsFull())
            {
                SendEvent(Constants.EventNames.BACKPACK_FULL);
                return false;
            }

            block.IsInBackpack = true;
            m_blocks.Add(block);
            SendEvent(Constants.EventNames.BLOCK_ADDED_TO_BACKPACK, block);

            // 检查是否可以消除
            CheckElimination();

            // 对方块进行排序
            SortBlocks();

            return true;
        }

        /// <summary>
        /// 从背包移除方块
        /// </summary>
        /// <param name="block">要移除的方块数据</param>
        /// <remarks>
        /// 移除流程：
        /// 1. 验证方块有效性
        /// 2. 更新方块状态
        /// 3. 从列表移除
        /// 4. 更新排序
        /// </remarks>
        public void RemoveBlock(BlockData block)
        {
            if (block == null || !m_blocks.Contains(block)) return;

            block.IsInBackpack = false;
            m_blocks.Remove(block);

            // 对方块进行排序
            SortBlocks();
        }

        /// <summary>
        /// 检查并执行方块消除
        /// </summary>
        /// <remarks>
        /// 消除规则：
        /// 1. 相同类型的方块可以消除（如：1-11, 2-22）
        /// 2. 连续数字也可以消除（如：2-34，因为在生成时已将其归为同一类型）
        /// 
        /// 实现步骤：
        /// 1. 按方块类型分组（使用Dictionary提高查找效率）
        /// 2. 检查每组是否满足消除条件（同类型数量>=2）
        /// 3. 执行消除并触发相关事件
        /// 
        /// 优化措施：
        /// 1. 使用Dictionary避免重复遍历
        /// 2. 批量收集待消除方块，一次性处理
        /// 3. 统一触发消除事件，减少事件调用次数
        /// 
        /// 注意事项：
        /// 1. 支持多组同时消除
        /// 2. 消除后自动整理背包
        /// 3. 消除动画由View层处理
        /// </remarks>
        private void CheckElimination()
        {
            // 按类型分组
            Dictionary<int, List<BlockData>> typeGroups = new Dictionary<int, List<BlockData>>();
            
            foreach (var block in m_blocks)
            {
                if (!typeGroups.ContainsKey(block.BlockType))
                {
                    typeGroups[block.BlockType] = new List<BlockData>();
                }
                typeGroups[block.BlockType].Add(block);
            }

            // 检查每个组是否有可消除的
            List<BlockData> eliminatedBlocks = new List<BlockData>();
            foreach (var group in typeGroups.Values)
            {
                if (group.Count >= 2)
                {
                    eliminatedBlocks.AddRange(group);
                }
            }

            // 执行消除
            if (eliminatedBlocks.Count > 0)
            {
                foreach (var block in eliminatedBlocks)
                {
                    block.IsEliminated = true;
                    RemoveBlock(block);
                }
                SendEvent(Constants.EventNames.BLOCKS_ELIMINATED, eliminatedBlocks);
            }
        }

        /// <summary>
        /// 对背包中的方块进行排序
        /// </summary>
        /// <remarks>
        /// 排序规则：
        /// 1. 首先按类型分组
        /// 2. 组内按创建时间排序
        /// 3. 组间按类型ID排序
        /// 
        /// 实现原理：
        /// 1. 使用LINQ的GroupBy进行分组
        /// 2. OrderBy确保组间顺序
        /// 3. 组内使用CreationTime保持添加顺序
        /// 
        /// 性能优化：
        /// 1. 仅在必要时（添加、消除后）进行排序
        /// 2. 使用LINQ优化排序逻辑
        /// 3. 一次性重建列表，避免频繁操作
        /// </remarks>
        private void SortBlocks()
        {
            // 首先按类型分组
            var groups = m_blocks.GroupBy(b => b.BlockType).OrderBy(g => g.Key);
            
            // 清空当前列表
            m_blocks.Clear();
            
            // 按组添加回列表
            foreach (var group in groups)
            {
                m_blocks.AddRange(group.OrderBy(b => b.CreationTime));
            }
        }

        /// <summary>
        /// 扩展背包容量
        /// </summary>
        /// <returns>是否扩展成功</returns>
        /// <remarks>
        /// 扩展规则：
        /// 1. 每关仅能扩展一次
        /// 2. 从6格扩展到8格
        /// 3. 需要观看广告才能扩展
        /// </remarks>
        public bool ExtendCapacity()
        {
            if (m_isExtended) return false;

            m_capacity = Constants.EXTENDED_BACKPACK_SIZE;
            m_isExtended = true;
            SendEvent(Constants.EventNames.BACKPACK_EXTENDED);
            return true;
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        /// <remarks>
        /// 清空操作：
        /// 1. 清空方块列表
        /// 2. 重置容量
        /// 3. 重置扩展状态
        /// </remarks>
        public void Clear()
        {
            m_blocks.Clear();
            m_capacity = Constants.MAX_BACKPACK_SIZE;
            m_isExtended = false;
        }

        /// <summary>
        /// 检查背包是否已满
        /// </summary>
        public bool IsFull()
        {
            return m_blocks.Count >= m_capacity;
        }

        /// <summary>
        /// 获取指定类型的方块数量
        /// </summary>
        public int GetBlockTypeCount(int blockType)
        {
            return m_blocks.Count(b => b.BlockType == blockType);
        }

        /// <summary>
        /// 检查是否有可消除的方块
        /// </summary>
        /// <returns>是否存在可消除的方块组合</returns>
        /// <remarks>
        /// 检查逻辑：
        /// 1. 按类型分组
        /// 2. 查找是否有数量>=2的组
        /// </remarks>
        public bool HasEliminableBlocks()
        {
            var groups = m_blocks.GroupBy(b => b.BlockType);
            return groups.Any(g => g.Count() >= 2);
        }

        // 获取器
        public IReadOnlyList<BlockData> Blocks => m_blocks.AsReadOnly();
        public int Capacity => m_capacity;
        public bool IsExtended => m_isExtended;
        public int RemainingSpace => m_capacity - m_blocks.Count;
    }
} 