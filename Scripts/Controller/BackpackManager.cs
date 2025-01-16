using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 背包管理器：负责协调背包数据和UI的交互
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 管理背包容量和扩容
    /// 2. 处理方块的添加和移除
    /// 3. 协调数据和UI的同步
    /// 
    /// 设计考虑：
    /// 1. 使用单例模式确保全局唯一
    /// 2. 通过事件系统通知UI更新
    /// 3. 支持背包扩容功能
    /// </remarks>
    public class BackpackManager : MonoBehaviour
    {
        private static BackpackManager m_instance;
        public static BackpackManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject go = new GameObject("BackpackManager");
                    m_instance = go.AddComponent<BackpackManager>();
                    DontDestroyOnLoad(go);
                }
                return m_instance;
            }
        }

        private BackpackData m_backpackData;
        private BackpackUI m_backpackUI;

        private void Awake()
        {
            if (m_instance != null && m_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            m_instance = this;
            DontDestroyOnLoad(gameObject);
            
            m_backpackData = BackpackData.Instance;
        }

        /// <summary>
        /// 初始化背包系统
        /// </summary>
        /// <param name="initialCapacity">初始容量</param>
        public void Initialize(int initialCapacity)
        {
            // 查找场景中的BackpackUI组件
            m_backpackUI = FindObjectOfType<BackpackUI>();
            if (m_backpackUI == null)
            {
                Debug.LogError("BackpackUI not found in scene");
                return;
            }

            // 初始化数据
            m_backpackData.Clear();
        }

        /// <summary>
        /// 扩展背包容量
        /// </summary>
        /// <param name="newCapacity">新容量</param>
        public void ExtendCapacity(int newCapacity)
        {
            if (m_backpackData.ExtendCapacity())
            {
                Debug.Log($"Backpack capacity extended to {newCapacity}");
            }
        }

        /// <summary>
        /// 添加方块到背包
        /// </summary>
        /// <param name="block">要添加的方块数据</param>
        /// <returns>是否添加成功</returns>
        public bool AddBlock(BlockData block)
        {
            return m_backpackData.AddBlock(block);
        }

        /// <summary>
        /// 从背包移除方块
        /// </summary>
        /// <param name="block">要移除的方块数据</param>
        public void RemoveBlock(BlockData block)
        {
            m_backpackData.RemoveBlock(block);
        }

        /// <summary>
        /// 检查背包是否已满
        /// </summary>
        public bool IsFull()
        {
            return m_backpackData.IsFull();
        }

        /// <summary>
        /// 获取背包剩余空间
        /// </summary>
        public int GetRemainingSpace()
        {
            return m_backpackData.RemainingSpace;
        }

        private void OnDestroy()
        {
            if (m_instance == this)
            {
                m_instance = null;
            }
        }
    }
} 