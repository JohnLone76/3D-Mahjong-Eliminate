using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 方块对象池
    /// </summary>
    public class BlockPool : BaseObjectPool<BlockView>
    {
        private static BlockPool m_instance;
        public static BlockPool Instance
        {
            get
            {
                if (m_instance == null)
                {
                    // 从Resources加载预制体
                    GameObject prefab = Resources.Load<GameObject>(Constants.ResourcePaths.Prefabs.BLOCK_PATH + "Block");
                    m_instance = new BlockPool(prefab);
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
        /// 获取方块并设置位置
        /// </summary>
        public BlockView GetBlock(Vector3 position)
        {
            BlockView block = Get();
            if (block != null)
            {
                block.transform.position = position;
            }
            return block;
        }
    }
} 