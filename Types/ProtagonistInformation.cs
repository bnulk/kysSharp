namespace kysSharp.Types
{
    class ProtagonistInformation
    {
        //此处为全局数据，载入和保存使用，必须放在类开头，按照顺序，否则自己看着办
        public int InShip, InSubMap, MainMapX, MainMapY, SubMapX, SubMapY, FaceTowards, ShipX, ShipY, ShipX1, ShipY1, Encode;
        public int[] Team = new int[Constant.TEAMMATE_COUNT];
        public ItemList[] Items = new ItemList[Constant.ITEM_IN_BAG_COUNT];
    }
}
