using System;

namespace MahjongProject
{
    /// <summary>
    /// 游戏存档数据类：用于序列化和持久化游戏进度
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 存储游戏进度和解锁状态
    /// 2. 支持JSON序列化
    /// 3. 便于存档和读档
    /// 
    /// 设计考虑：
    /// 1. 只存储必要的数据
    /// 2. 结构简单便于维护
    /// 3. 支持版本升级
    /// </remarks>
    [Serializable]
    public class GameSaveData
    {
        /// <summary>
        /// 最大解锁关卡
        /// </summary>
        /// <remarks>
        /// 用途：
        /// 1. 控制关卡解锁进度
        /// 2. 显示可选关卡范围
        /// 3. 计算游戏完成度
        /// </remarks>
        public int MaxUnlockedLevel;

        /// <summary>
        /// 当前关卡
        /// </summary>
        /// <remarks>
        /// 用途：
        /// 1. 记录玩家进度
        /// 2. 重新进入游戏时恢复
        /// 3. 计算关卡难度
        /// </remarks>
        public int CurrentLevel;

        /// <summary>
        /// 存档版本号
        /// </summary>
        /// <remarks>
        /// 用途：
        /// 1. 处理存档格式升级
        /// 2. 确保向后兼容
        /// 3. 验证存档有效性
        /// </remarks>
        public int Version;

        /// <summary>
        /// 创建新的存档数据
        /// </summary>
        public GameSaveData()
        {
            MaxUnlockedLevel = 1;
            CurrentLevel = 1;
            Version = Constants.GameSettings.SAVE_VERSION;
        }
    }
} 