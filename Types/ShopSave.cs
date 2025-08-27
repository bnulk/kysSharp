namespace kysSharp.Types
{
    /// <summary>
    /// 存档中的商店数据
    /// </summary>
    public class ShopSave
    {
        public int[] ItemID = new int[Constant.SHOP_ITEM_COUNT];
        public int[] Total = new int[Constant.SHOP_ITEM_COUNT];
        public int[] Price = new int[Constant.SHOP_ITEM_COUNT];
    }
}
