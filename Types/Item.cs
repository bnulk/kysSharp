using System;

namespace kysSharp.Types
{
    /// <summary>
    /// //实际的物品数据
    /// </summary>
    [Serializable]
    public class Item : ItemSave
    {
        public bool isCompass()
        {
            return ID == Constant.COMPASS_ITEM_ID;
        }
    }
}
