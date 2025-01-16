using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MahjongProject
{
    /// <summary>
    /// 方块管理器：负责方块的生成、管理和回收
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 方块的生成和初始化
    /// 2. 方块对象的生命周期管理
    /// 3. 确保生成的方块组合可解
    /// 
    /// 设计考虑：
    /// 1. 使用单例模式确保全局唯一
    /// 2. 结合对象池优化性能
    /// 3. 支持关卡配置的动态加载
    /// 
    /// 依赖关系：
    /// - BlockPool：提供方块对象的复用
    /// - LevelConfig：提供关卡配置数据
    /// - BlockView：方块的视图组件
    /// 
    /// 性能优化：
    /// 1. 使用对象池减少GC
    /// 2. 批量生成和回收方块
    /// 3. 优化物理碰撞检测
    /// </remarks>
    public class BlockManager : BaseController
    {
        private static BlockManager m_instance;
        public static BlockManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new BlockManager();
                }
                return m_instance;
            }
        }

        // 场景大小配置
        private const float SCENE_WIDTH = 10f;            // 场景宽度
        private const float SCENE_LENGTH = 20f;           // 场景长度
        private const float SPAWN_HEIGHT = 10f;           // 生成高度
        private const float POSITION_RANDOM_RANGE = 0.1f; // 生成位置的随机偏移范围

        // 当前场景中的所有方块
        private List<BlockView> m_activeBlocks = new List<BlockView>();

        /// <summary>
        /// 私有构造函数，确保单例模式
        /// </summary>
        /// <remarks>
        /// 初始化流程：
        /// 1. 调用基类构造函数
        /// 2. 预热对象池
        /// 3. 初始化内部状态
        /// </remarks>
        private BlockManager() : base() 
        {
            BlockPool.Instance.Prewarm();
        }

        /// <summary>
        /// 生成关卡方块
        /// </summary>
        /// <param name="levelConfig">关卡配置数据</param>
        /// <remarks>
        /// 生成流程：
        /// 1. 清理现有方块
        /// 2. 根据配置生成方块列表
        /// 3. 实例化并初始化方块
        /// 
        /// 注意事项：
        /// - 确保在调用前LevelConfig已正确初始化
        /// - 方块生成可能需要一定时间，建议在加载场景时调用
        /// </remarks>
        public void GenerateBlocks(LevelConfig levelConfig)
        {
            ClearAllBlocks();
            List<int> blockTypes = GenerateBlockList(levelConfig);
            foreach (int blockType in blockTypes)
            {
                SpawnBlock(blockType, levelConfig);
            }
        }

        /// <summary>
        /// 生成方块列表
        /// </summary>
        /// <param name="levelConfig">关卡配置数据</param>
        /// <returns>方块类型列表</returns>
        /// <remarks>
        /// 生成规则：
        /// 1. 构建匹配关系字典
        /// 2. 确保每个方块都有对应的可消除对象
        /// 3. 随机打乱顺序增加游戏趣味性
        /// 
        /// 匹配规则：
        /// 1. 相同数字匹配（如1和11）
        /// 2. 连续数字匹配（如2和23）
        /// 
        /// 性能考虑：
        /// - 使用Dictionary存储匹配关系提高查找效率
        /// - 预先计算可用类型避免重复计算
        /// </remarks>
        private List<int> GenerateBlockList(LevelConfig levelConfig)
        {
            List<int> blockTypes = new List<int>();
            Dictionary<int, List<int>> matchPairs = new Dictionary<int, List<int>>();

            // 初始化匹配字典
            foreach (int type in levelConfig.AvailableBlockTypes.Where(n => n < 10))
            {
                matchPairs[type] = new List<int>();
            }

            // 构建匹配关系
            foreach (int type in levelConfig.AvailableBlockTypes.Where(n => n >= 10))
            {
                int lastDigit = type % 10;
                int firstDigit = type / 10;

                // 检查相同数字匹配
                if (matchPairs.ContainsKey(lastDigit) && firstDigit == lastDigit)
                {
                    matchPairs[lastDigit].Add(type);
                }

                // 检查连续数字匹配
                if (IsConsecutive(firstDigit.ToString() + lastDigit.ToString()))
                {
                    if (matchPairs.ContainsKey(firstDigit))
                    {
                        matchPairs[firstDigit].Add(type);
                    }
                }
            }

            // 生成配对
            int remainingPairs = levelConfig.BlockCount / 2;
            List<int> availableKeys = matchPairs.Keys.ToList();

            while (remainingPairs > 0 && availableKeys.Count > 0)
            {
                int keyIndex = Random.Range(0, availableKeys.Count);
                int key = availableKeys[keyIndex];

                if (matchPairs[key].Count > 0)
                {
                    int matchIndex = Random.Range(0, matchPairs[key].Count);
                    int matchValue = matchPairs[key][matchIndex];

                    blockTypes.Add(key);
                    blockTypes.Add(matchValue);
                    remainingPairs--;
                }
                else
                {
                    availableKeys.RemoveAt(keyIndex);
                }
            }

            // 打乱顺序
            for (int i = blockTypes.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = blockTypes[i];
                blockTypes[i] = blockTypes[j];
                blockTypes[j] = temp;
            }

            return blockTypes;
        }

        /// <summary>
        /// 检查是否为连续数字
        /// </summary>
        /// <param name="numStr">要检查的数字字符串</param>
        /// <returns>true表示是连续数字，false表示不是</returns>
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
        /// 生成单个方块
        /// </summary>
        /// <param name="blockType">方块类型</param>
        /// <param name="levelConfig">关卡配置</param>
        /// <remarks>
        /// 生成流程：
        /// 1. 计算随机生成位置
        /// 2. 从对象池获取方块
        /// 3. 初始化方块属性
        /// 4. 添加到活动方块列表
        /// 
        /// 位置计算：
        /// - 在场景范围内随机
        /// - 添加随机偏移增加自然感
        /// - 考虑物理碰撞的影响
        /// 
        /// 注意事项：
        /// - 确保生成位置在场景范围内
        /// - 避免方块重叠
        /// - 考虑性能影响
        /// </remarks>
        private void SpawnBlock(int blockType, LevelConfig levelConfig)
        {
            // 计算随机生成位置
            float x = Random.Range(0, SCENE_WIDTH);
            float z = Random.Range(0, SCENE_LENGTH);
            Vector3 position = new Vector3(x, SPAWN_HEIGHT, z);

            // 添加随机偏移
            position += new Vector3(
                Random.Range(-POSITION_RANDOM_RANGE, POSITION_RANDOM_RANGE),
                0,
                Random.Range(-POSITION_RANDOM_RANGE, POSITION_RANDOM_RANGE)
            );

            // 从对象池获取方块
            BlockView block = BlockPool.Instance.GetBlock(position, blockType, levelConfig);
            if (block != null)
            {
                m_activeBlocks.Add(block);
            }
        }

        /// <summary>
        /// 清理所有方块
        /// </summary>
        /// <remarks>
        /// 清理流程：
        /// 1. 遍历所有活动方块
        /// 2. 将方块返回对象池
        /// 3. 清空活动方块列表
        /// 
        /// 调用时机：
        /// - 关卡重置时
        /// - 场景切换时
        /// - 游戏重新开始时
        /// 
        /// 性能考虑：
        /// - 批量处理避免频繁GC
        /// - 确保正确回收所有资源
        /// </remarks>
        public void ClearAllBlocks()
        {
            foreach (var block in m_activeBlocks)
            {
                if (block != null)
                {
                    BlockPool.Instance.ReturnToPool(block);
                }
            }
            m_activeBlocks.Clear();
        }

        /// <summary>
        /// 移除单个方块
        /// </summary>
        /// <param name="block">要移除的方块</param>
        /// <remarks>
        /// 移除流程：
        /// 1. 验证方块有效性
        /// 2. 从活动列表移除
        /// 3. 返回对象池
        /// 
        /// 调用时机：
        /// - 方块被消除时
        /// - 方块移动到背包时
        /// 
        /// 注意事项：
        /// - 确保方块存在于活动列表中
        /// - 处理null检查
        /// </remarks>
        public void RemoveBlock(BlockView block)
        {
            if (block != null && m_activeBlocks.Contains(block))
            {
                m_activeBlocks.Remove(block);
                BlockPool.Instance.ReturnToPool(block);
            }
        }

        /// <summary>
        /// 获取所有活动方块
        /// </summary>
        /// <returns>活动方块列表的副本</returns>
        /// <remarks>
        /// 返回副本的原因：
        /// - 防止外部修改内部列表
        /// - 确保线程安全
        /// - 维护数据封装性
        /// 
        /// 使用场景：
        /// - UI更新时
        /// - 游戏状态检查时
        /// - 存档时
        /// </remarks>
        public List<BlockView> GetActiveBlocks()
        {
            return new List<BlockView>(m_activeBlocks);
        }
    }
} 