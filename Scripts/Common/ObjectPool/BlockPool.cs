using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 方块对象池
    /// </summary>
    public class BlockPool : BaseObjectPool<BlockView>
    {
        private static BlockPool m_instance;
        private static GameObject m_prefab;

        public static BlockPool Instance
        {
            get
            {
                if (m_instance == null)
                {
                    // 使用Block_1作为默认预制体
                    m_prefab = Resources.Load<GameObject>(Constants.ResourcePaths.Prefabs.BLOCK_PATH + "Block_1");
                    if (m_prefab == null)
                    {
                        Debug.LogError("找不到默认方块预制体：Block_1");
                        return null;
                    }
                    m_instance = new BlockPool(m_prefab);
                }
                return m_instance;
            }
        }

        private BlockPool(GameObject prefab) : base(prefab, 
            Constants.PoolConfig.INITIAL_BLOCK_POOL_SIZE, 
            Constants.PoolConfig.MAX_BLOCK_POOL_SIZE)
        {
        }

        protected override void OnGet(BlockView block)
        {
            base.OnGet(block);
            block.ResetBlock();
        }

        protected override void OnReturn(BlockView block)
        {
            base.OnReturn(block);
            block.transform.localPosition = Vector3.zero;
            block.transform.localRotation = Quaternion.identity;
            block.GetComponent<Rigidbody>().velocity = Vector3.zero;
            block.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// 获取方块并设置位置和类型
        /// </summary>
        public BlockView GetBlock(Vector3 position, int blockType, LevelConfig levelConfig)
        {
            // 从对象池获取方块
            BlockView block = Get();
            if (block != null)
            {
                // 根据 blockType 和 levelConfig 获取预制体路径
                string prefabPath = levelConfig.GetBlockPrefabPath(blockType);
                
                // 设置位置和类型
                block.transform.position = position;
                block.SetBlockType(blockType, prefabPath);
            }
            return block;
        }
    }
} 