using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MahjongProject
{
    /// <summary>
    /// 背包UI：负责背包界面的显示和交互
    /// </summary>
    /// <remarks>
    /// 核心功能：
    /// 1. 显示背包格子和方块
    /// 2. 处理背包容量变化
    /// 3. 显示扩容状态
    /// 
    /// 设计考虑：
    /// 1. 固定在屏幕底部
    /// 2. 支持6-8个格子
    /// 3. 显示锁定状态
    /// </remarks>
    public class BackpackUI : BaseView
    {
        [Header("UI References")]
        [SerializeField] private RectTransform m_gridContainer;    // 格子容器
        [SerializeField] private List<Transform> m_cellList;       // 格子中的cell节点列表
        [SerializeField] private List<GameObject> m_lockList;      // 格子中的lock节点列表

        // 运行时数据
        private Dictionary<int, BlockDisplayModel> m_displayModels;       // 格子显示的模型
        private BlockDisplayModelPool m_modelPool;                        // 模型对象池

        protected override void OnInit()
        {
            base.OnInit();
            m_displayModels = new Dictionary<int, BlockDisplayModel>();
            InitializeModelPool();
            InitializeGrids();
        }

        protected override void RegisterEvents()
        {
            base.RegisterEvents();
            m_eventCenter.AddListener(Constants.EventNames.BLOCK_ADDED_TO_BACKPACK, OnBlockAdded);
            m_eventCenter.AddListener(Constants.EventNames.BLOCKS_ELIMINATED, OnBlocksEliminated);
            m_eventCenter.AddListener(Constants.EventNames.BACKPACK_EXTENDED, OnBackpackExtended);
        }

        protected override void UnregisterEvents()
        {
            base.UnregisterEvents();
            m_eventCenter.RemoveListener(Constants.EventNames.BLOCK_ADDED_TO_BACKPACK, OnBlockAdded);
            m_eventCenter.RemoveListener(Constants.EventNames.BLOCKS_ELIMINATED, OnBlocksEliminated);
            m_eventCenter.RemoveListener(Constants.EventNames.BACKPACK_EXTENDED, OnBackpackExtended);
        }

        /// <summary>
        /// 初始化模型对象池
        /// </summary>
        private void InitializeModelPool()
        {
            // 加载UI显示用的轻量级预制体
            GameObject displayPrefab = ResourceLoader.LoadUIPrefab("BlockDisplayModel_1");
            if (displayPrefab == null)
            {
                Debug.LogError("Failed to load BlockDisplayModel prefab");
                return;
            }

            var displayModel = displayPrefab.GetComponent<BlockDisplayModel>();
            if (displayModel == null)
            {
                Debug.LogError("BlockDisplayModel component not found on prefab");
                return;
            }

            // 创建对象池
            m_modelPool = new BlockDisplayModelPool(
                displayModel,
                Constants.PoolConfig.INITIAL_BLOCK_POOL_SIZE,
                Constants.PoolConfig.MAX_BLOCK_POOL_SIZE
            );
        }

        /// <summary>
        /// 初始化背包格子
        /// </summary>
        private void InitializeGrids()
        {
            // 设置初始锁定状态
            for (int i = 0; i < m_lockList.Count; i++)
            {
                bool isLocked = i >= Constants.MAX_BACKPACK_SIZE;
                m_lockList[i].SetActive(isLocked);
            }
        }

        /// <summary>
        /// 处理方块添加事件
        /// </summary>
        private void OnBlockAdded(object param)
        {
            if (param is BlockAddData data)
            {
                // 获取一个显示模型实例
                var displayModel = m_modelPool.Get();
                if (displayModel != null)
                {
                    // 设置模型属性
                    displayModel.transform.SetParent(m_cellList[data.GridIndex], false);
                    displayModel.transform.localPosition = Vector3.zero;
                    displayModel.transform.localRotation = Quaternion.identity;
                    displayModel.transform.localScale = Vector3.one;

                    // 更新显示
                    displayModel.SetBlockType(data.BlockType);

                    // 记录显示模型
                    if (m_displayModels.ContainsKey(data.GridIndex))
                    {
                        RecycleDisplayModel(data.GridIndex);
                    }
                    m_displayModels[data.GridIndex] = displayModel;
                }
            }
        }

        /// <summary>
        /// 处理方块消除事件
        /// </summary>
        private void OnBlocksEliminated(object param)
        {
            if (param is List<int> eliminatedIndices)
            {
                foreach (int index in eliminatedIndices)
                {
                    RecycleDisplayModel(index);
                }
            }
        }

        /// <summary>
        /// 回收显示模型
        /// </summary>
        private void RecycleDisplayModel(int gridIndex)
        {
            if (m_displayModels.TryGetValue(gridIndex, out BlockDisplayModel model))
            {
                m_modelPool.ReturnToPool(model);
                m_displayModels.Remove(gridIndex);
            }
        }

        /// <summary>
        /// 处理背包扩容事件
        /// </summary>
        private void OnBackpackExtended(object param)
        {
            // 解锁额外格子
            for (int i = Constants.MAX_BACKPACK_SIZE; i < Constants.EXTENDED_BACKPACK_SIZE; i++)
            {
                if (i < m_lockList.Count)
                {
                    m_lockList[i].SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 方块添加数据
    /// </summary>
    public class BlockAddData
    {
        public int GridIndex;    // 格子索引
        public int BlockType;    // 方块类型
    }
} 