using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// 方块UI显示模型：负责在UI中显示方块的3D模型
    /// </summary>
    public class BlockDisplayModel : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private Vector3 m_rotationOffset = new Vector3(30f, 45f, 0f);  // 旋转偏移
        [SerializeField] private Vector3 m_scaleOffset = Vector3.one;                    // 缩放偏移

        private int m_blockType;  // 当前方块类型

        /// <summary>
        /// 设置方块类型
        /// </summary>
        public void SetBlockType(int blockType)
        {
            m_blockType = blockType;
            UpdateVisual();
        }

        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisual()
        {
            // 获取当前关卡配置
            var levelConfig = ConfigManager.Instance.GetLevelConfig(GameStateData.Instance.CurrentLevel);
            if (levelConfig != null)
            {
                // 获取预制体路径
                string prefabPath = levelConfig.GetBlockPrefabPath(m_blockType);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    // 加载并实例化预制体
                    GameObject prefab = ResourceLoader.LoadPrefab(prefabPath);
                    if (prefab != null)
                    {
                        // 创建预制体实例作为子物体
                        GameObject instance = Instantiate(prefab, transform);
                        instance.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        Debug.LogWarning($"找不到方块预制体：{prefabPath}");
                    }
                }
                else
                {
                    Debug.LogWarning($"找不到方块类型 {m_blockType} 的预制体路径");
                }
            }
            else
            {
                Debug.LogWarning($"找不到当前关卡配置");
            }

            // 应用显示设置
            transform.localRotation = Quaternion.Euler(m_rotationOffset);
            transform.localScale = m_scaleOffset;
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void ResetState()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(m_rotationOffset);
            transform.localScale = m_scaleOffset;

            // 清理所有子物体
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 当对象被禁用时重置状态
        /// </summary>
        private void OnDisable()
        {
            ResetState();
        }
    }
} 