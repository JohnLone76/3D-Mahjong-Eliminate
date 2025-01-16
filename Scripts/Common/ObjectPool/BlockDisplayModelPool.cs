using UnityEngine;

namespace MahjongProject
{
    /// <summary>
    /// BlockDisplayModel对象池：专门用于管理UI中显示的方块模型
    /// </summary>
    public class BlockDisplayModelPool : BaseObjectPool<BlockDisplayModel>
    {
        public BlockDisplayModelPool(BlockDisplayModel prefab, int initialSize, int maxSize) 
            : base(prefab.gameObject, initialSize, maxSize)
        {
        }

        protected override void OnGet(BlockDisplayModel model)
        {
            base.OnGet(model);
            model.gameObject.SetActive(true);
        }

        protected override void OnReturn(BlockDisplayModel model)
        {
            model.gameObject.SetActive(false);
            base.OnReturn(model);
        }
    }
} 