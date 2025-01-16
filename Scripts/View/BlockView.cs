using UnityEngine;
using DG.Tweening;

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

        // 动画参数
        private const float MOVE_DURATION = 0.5f;           // 移动动画时长
        private const float DESTROY_DURATION = 0.3f;        // 消除动画时长
        private const float SHAKE_STRENGTH = 0.5f;          // 震动强度
        private const float SHAKE_DURATION = 0.2f;          // 震动时长
        private const float CURVE_HEIGHT = 2f;              // 曲线高度

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
        /// <param name="blockType">方块类型</param>
        /// <param name="prefabPath">预制体路径</param>
        public void SetBlockType(int blockType, string prefabPath = null)
        {
            m_blockType = blockType;
            UpdateVisual(prefabPath);
        }

        /// <summary>
        /// 更新方块视觉效果
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        private void UpdateVisual(string prefabPath = null)
        {
            // 加载预制体
            GameObject prefab = null;
            if (!string.IsNullOrEmpty(prefabPath))
            {
                prefab = Resources.Load<GameObject>(prefabPath);
            }

            if (prefab != null)
            {
                // 获取预制体的网格和材质
                var meshFilter = prefab.GetComponent<MeshFilter>();
                var renderer = prefab.GetComponent<MeshRenderer>();

                if (meshFilter != null)
                {
                    var myMeshFilter = GetComponent<MeshFilter>();
                    if (myMeshFilter != null)
                    {
                        myMeshFilter.mesh = meshFilter.sharedMesh;
                    }
                }

                if (renderer != null && m_renderer != null)
                {
                    m_renderer.material = renderer.sharedMaterial;
                }
            }
            else
            {
                // 如果找不到预制体，使用默认材质
                Material material = Resources.Load<Material>($"Materials/Block_{m_blockType}");
                if (material != null)
                {
                    m_renderer.material = material;
                }
                else
                {
                    Debug.LogWarning($"找不到方块材质：Block_{m_blockType}");
                }
            }
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

            // 重置变换
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 处理点击事件
        /// </summary>
        private void OnMouseDown()
        {
            if (!m_isInteractable) return;

            // 播放点击特效
            PlayClickEffect();

            // 发送点击事件
            SendEvent(Constants.EventNames.BLOCK_CLICKED, new BlockClickData
            {
                BlockType = m_blockType,
                BlockView = this
            });
        }

        /// <summary>
        /// 播放点击特效
        /// </summary>
        private void PlayClickEffect()
        {
            // 播放震动动画
            transform.DOShakePosition(SHAKE_DURATION, SHAKE_STRENGTH, 10, 90, false, true);

            // 播放点击音效
            AudioManager.Instance.PlaySFX("BlockClick");

            // 生成点击特效
            EffectManager.Instance.PlayEffect("ClickEffect", transform.position);
        }

        /// <summary>
        /// 播放消除动画
        /// </summary>
        public void PlayDestroyAnimation()
        {
            m_isInteractable = false;

            // 播放震动
            transform.DOShakeScale(SHAKE_DURATION, SHAKE_STRENGTH)
                .OnComplete(() =>
                {
                    // 缩放消失
                    transform.DOScale(Vector3.zero, DESTROY_DURATION)
                        .SetEase(Ease.InBack)
                        .OnComplete(() =>
                        {
                            // 生成消除特效
                            EffectManager.Instance.PlayEffect("DestroyEffect", transform.position);
                            
                            // 播放消除音效
                            AudioManager.Instance.PlaySFX("BlockDestroy");

                            // 回收到对象池
                            BlockPool.Instance.ReturnToPool(this);
                        });
                });
        }

        /// <summary>
        /// 移动到背包位置
        /// </summary>
        public void MoveToBackpack(Vector3 targetPosition)
        {
            m_isInteractable = false;
            
            // 禁用物理
            m_rigidbody.isKinematic = true;

            // 计算贝塞尔曲线控制点
            Vector3 startPos = transform.position;
            Vector3 controlPoint = (startPos + targetPosition) / 2 + Vector3.up * CURVE_HEIGHT;

            // 创建贝塞尔曲线路径
            Vector3[] path = new Vector3[] { startPos, controlPoint, targetPosition };

            // 播放移动动画
            transform.DOPath(path, MOVE_DURATION, PathType.CubicBezier)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    SendEvent(Constants.EventNames.BLOCK_MOVE_END, this);
                });

            // 发送移动开始事件
            SendEvent(Constants.EventNames.BLOCK_MOVE_START, this);
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