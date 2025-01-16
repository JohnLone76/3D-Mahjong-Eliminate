using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 方块数据类：表示游戏中单个方块的所有相关数据
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 存储方块的基本属性（类型、位置、状态等）
    /// 2. 提供方块状态的判断和更新方法
    /// 3. 支持方块数据的克隆和重置
    /// 
    /// 设计考虑：
    /// 1. 不可变的核心属性（BlockType）确保数据一致性
    /// 2. 可变状态使用属性封装，便于状态管理
    /// 3. 提供完整的状态追踪（在背包中、已消除、正在移动等）
    /// 
    /// 性能优化：
    /// 1. 使用属性而不是字段，便于后期添加验证逻辑
    /// 2. 创建时间用于排序和调试，避免额外的排序字段
    /// 3. 状态变化通过布尔值快速判断
    /// </remarks>
    public class BlockData
    {
        /// <summary>
        /// 方块类型：决定方块的外观和消除规则
        /// </summary>
        /// <remarks>
        /// 类型规则：
        /// 1. 相同类型的方块可以消除
        /// 2. 类型值可以是普通数字(1,2,3)或特殊组合(11,22,33)
        /// 3. 一旦设置不可更改，确保游戏逻辑的一致性
        /// </remarks>
        public int BlockType { get; private set; }         // 方块类型

        /// <summary>
        /// 预制体路径：用于加载方块的3D模型
        /// </summary>
        /// <remarks>
        /// 路径规则：
        /// 1. 相对于Resources文件夹的路径
        /// 2. 包含完整的预制体名称
        /// 3. 由方块配置表统一管理
        /// </remarks>
        public string PrefabPath { get; private set; }     // 预制体路径

        /// <summary>
        /// 方块的世界坐标位置
        /// </summary>
        /// <remarks>
        /// 位置更新：
        /// 1. 初始生成时设置
        /// 2. 物理模拟时自动更新
        /// 3. 移动动画时手动更新
        /// </remarks>
        public Vector3 Position { get; set; }              // 位置

        /// <summary>
        /// 标记方块是否在背包中
        /// </summary>
        /// <remarks>
        /// 状态转换：
        /// 1. 添加到背包时设置为true
        /// 2. 从背包移除时设置为false
        /// 3. 用于UI显示和逻辑判断
        /// </remarks>
        public bool IsInBackpack { get; set; }            // 是否在背包中

        /// <summary>
        /// 标记方块是否已被消除
        /// </summary>
        /// <remarks>
        /// 使用场景：
        /// 1. 消除判定时的状态检查
        /// 2. 防止重复消除
        /// 3. 动画播放的触发条件
        /// </remarks>
        public bool IsEliminated { get; set; }            // 是否已被消除

        /// <summary>
        /// 标记方块是否正在移动
        /// </summary>
        /// <remarks>
        /// 应用场景：
        /// 1. 移动动画播放时
        /// 2. 物理模拟状态判断
        /// 3. 防止重复触发移动
        /// </remarks>
        public bool IsMoving { get; set; }                // 是否正在移动

        /// <summary>
        /// 方块的创建时间
        /// </summary>
        /// <remarks>
        /// 用途：
        /// 1. 用于背包中的方块排序
        /// 2. 调试和性能分析
        /// 3. 动画时间计算
        /// </remarks>
        public float CreationTime { get; private set; }    // 创建时间

        /// <summary>
        /// 创建新的方块数据实例
        /// </summary>
        /// <param name="blockType">方块类型</param>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="position">初始位置</param>
        /// <remarks>
        /// 初始化流程：
        /// 1. 设置不可变的核心属性
        /// 2. 初始化位置信息
        /// 3. 重置所有状态标志
        /// 4. 记录创建时间
        /// </remarks>
        public BlockData(int blockType, string prefabPath, Vector3 position)
        {
            BlockType = blockType;
            PrefabPath = prefabPath;
            Position = position;
            IsInBackpack = false;
            IsEliminated = false;
            IsMoving = false;
            CreationTime = Time.time;
        }

        /// <summary>
        /// 创建当前方块数据的深度复制
        /// </summary>
        /// <returns>新的方块数据实例</returns>
        /// <remarks>
        /// 克隆原理：
        /// 1. 创建新实例并复制所有属性
        /// 2. 保持核心属性不变
        /// 3. 复制当前的状态标志
        /// 
        /// 使用场景：
        /// 1. 预览效果时创建临时副本
        /// 2. 回退操作时保存状态
        /// 3. 测试不同配置时
        /// </remarks>
        public BlockData Clone()
        {
            return new BlockData(BlockType, PrefabPath, Position)
            {
                IsInBackpack = this.IsInBackpack,
                IsEliminated = this.IsEliminated,
                IsMoving = this.IsMoving,
                CreationTime = this.CreationTime
            };
        }

        /// <summary>
        /// 检查是否可以与另一个方块消除
        /// </summary>
        /// <param name="other">要检查的另一个方块</param>
        /// <returns>是否可以消除</returns>
        /// <remarks>
        /// 消除条件：
        /// 1. 两个方块都必须有效（非空）
        /// 2. 两个方块都未被消除
        /// 3. 两个方块的类型相同
        /// 
        /// 注意事项：
        /// 1. 不考虑方块是否在背包中
        /// 2. 不考虑方块是否正在移动
        /// 3. 仅判断是否满足消除条件
        /// </remarks>
        public bool CanEliminateWith(BlockData other)
        {
            if (other == null) return false;
            if (this.IsEliminated || other.IsEliminated) return false;
            return this.BlockType == other.BlockType;
        }

        /// <summary>
        /// 重置方块的所有状态
        /// </summary>
        /// <remarks>
        /// 重置内容：
        /// 1. 清除所有状态标志
        /// 2. 更新创建时间
        /// 3. 保持核心属性不变
        /// 
        /// 使用场景：
        /// 1. 从对象池获取时
        /// 2. 重新开始游戏时
        /// 3. 测试和调试时
        /// </remarks>
        public void Reset()
        {
            IsInBackpack = false;
            IsEliminated = false;
            IsMoving = false;
            CreationTime = Time.time;
        }

        /// <summary>
        /// 更新方块的位置
        /// </summary>
        /// <param name="newPosition">新的位置</param>
        /// <remarks>
        /// 更新场景：
        /// 1. 移动动画播放时
        /// 2. 物理模拟更新时
        /// 3. 手动调整位置时
        /// </remarks>
        public void UpdatePosition(Vector3 newPosition)
        {
            Position = newPosition;
        }
    }

    /// <summary>
    /// 方块配置数据：定义方块的静态属性和创建参数
    /// </summary>
    /// <remarks>
    /// 配置用途：
    /// 1. 统一管理方块的外观和物理属性
    /// 2. 支持在Unity编辑器中配置
    /// 3. 便于批量修改和测试
    /// 
    /// 设计考虑：
    /// 1. 使用[Serializable]特性支持序列化
    /// 2. 提供默认值避免空值错误
    /// 3. 包含完整的方块创建参数
    /// </remarks>
    [System.Serializable]
    public class BlockConfig
    {
        /// <summary>
        /// 方块类型：对应BlockData中的类型
        /// </summary>
        public int BlockType;                  // 方块类型

        /// <summary>
        /// 预制体路径：用于加载3D模型
        /// </summary>
        public string PrefabPath;              // 预制体路径

        /// <summary>
        /// 方块显示名称：用于UI显示
        /// </summary>
        public string BlockName;               // 方块名称

        /// <summary>
        /// 方块描述：用于提示和说明
        /// </summary>
        public string Description;             // 描述

        /// <summary>
        /// 碰撞体尺寸：用于物理碰撞
        /// </summary>
        public Vector3 ColliderSize;           // 碰撞体尺寸

        /// <summary>
        /// 质量：用于物理模拟
        /// </summary>
        public float Mass;                     // 质量

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <remarks>
        /// 默认值设置：
        /// 1. 使用安全的默认值
        /// 2. 确保物理参数合理
        /// 3. 提供基础的显示效果
        /// </remarks>
        public BlockConfig()
        {
            BlockType = 0;
            PrefabPath = string.Empty;
            BlockName = "Default Block";
            Description = "Default Description";
            ColliderSize = Vector3.one;
            Mass = 1.0f;
        }

        /// <summary>
        /// 根据配置创建方块数据实例
        /// </summary>
        /// <param name="position">初始位置</param>
        /// <returns>新的方块数据实例</returns>
        /// <remarks>
        /// 创建流程：
        /// 1. 使用配置中的参数
        /// 2. 设置指定的初始位置
        /// 3. 返回完整的方块数据
        /// </remarks>
        public BlockData CreateBlockData(Vector3 position)
        {
            return new BlockData(BlockType, PrefabPath, position);
        }
    }
} 