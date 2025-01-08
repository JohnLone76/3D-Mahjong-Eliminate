using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 方块视图：负责方块的显示、物理和交互
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public class BlockView : BaseView
    {
        // 组件引用
        private Rigidbody m_rigidbody;
        private BoxCollider m_collider;
        private MeshRenderer m_renderer;

        // 方块数据
        private int m_blockType;
        private bool m_isInteractable = true;

        // 物理参数
        private const float MASS = 1f;
        private const float DRAG = 0.5f;
        private const float ANGULAR_DRAG = 0.5f;

        private void Awake()
        {
            // 获取组件引用
            m_rigidbody = GetComponent<Rigidbody>();
            m_collider = GetComponent<BoxCollider>();
            m_renderer = GetComponent<MeshRenderer>();

            // 初始化物理参数
            InitializePhysics();
        }

        /// <summary>
        /// 初始化物理组件
        /// </summary>
        private void InitializePhysics()
        {
            // 设置刚体属性
            m_rigidbody.mass = MASS;
            m_rigidbody.drag = DRAG;
            m_rigidbody.angularDrag = ANGULAR_DRAG;
            m_rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            // 设置碰撞器属性
            m_collider.isTrigger = false;
        }

        /// <summary>
        /// 设置方块类型
        /// </summary>
        public void SetBlockType(int blockType)
        {
            m_blockType = blockType;
            UpdateVisual();
        }

        /// <summary>
        /// 更新方块视觉效果
        /// </summary>
        private void UpdateVisual()
        {
            // TODO: 根据方块类型更新材质和模型
            // 这里需要从配置或资源管理器获取对应的材质
        }

        /// <summary>
        /// 重置方块状态
        /// </summary>
        public void ResetBlock()
        {
            m_isInteractable = true;
            gameObject.SetActive(true);
            
            // 重置物理状态
            if (m_rigidbody != null)
            {
                m_rigidbody.velocity = Vector3.zero;
                m_rigidbody.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 处理点击事件
        /// </summary>
        private void OnMouseDown()
        {
            if (!m_isInteractable) return;

            // 发送点击事件
            SendEvent(Constants.EventNames.BLOCK_CLICKED, new BlockClickData
            {
                BlockType = m_blockType,
                BlockView = this
            });
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public void PlayDestroyAnimation()
        {
            m_isInteractable = false;

            // TODO: 播放消除动画和特效
            // 这里需要从特效对象池获取特效

            // 动画播放完成后回收到对象池
            BlockPool.Instance.ReturnToPool(this);
        }

        /// <summary>
        /// 移动到背包位置
        /// </summary>
        public void MoveToBackpack(Vector3 targetPosition)
        {
            m_isInteractable = false;
            
            // 禁用物理
            m_rigidbody.isKinematic = true;

            // TODO: 实现曲线移动动画
            // 这里需要使用DOTween或其他动画系统
        }

        // Getter
        public int BlockType => m_blockType;
        public bool IsInteractable => m_isInteractable;
    }

    /// <summary>
    /// 方块点击事件数据
    /// </summary>
    public class BlockClickData
    {
        public int BlockType;
        public BlockView BlockView;
    }
} 