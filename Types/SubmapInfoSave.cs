namespace kysSharp.Types
{
    /// <summary>
    /// 存档中的子场景数据
    /// 约定：Scene表示游戏中运行的某个Element实例，而Map表示存储的数据
    /// </summary>
    public class SubmapInfoSave
    {
        public int ID;
        public sbyte[] Name = new sbyte[20];
        public int ExitMusic, EntranceMusic;
        public int JumpSubMap, EntranceCondition;
        public int MainEntranceX1, MainEntranceY1, MainEntranceX2, MainEntranceY2;
        public int EntranceX, EntranceY;
        public int[] ExitX = new int[3];
        public int[] ExitY = new int[3];
        public int JumpX, JumpY, JumpReturnX, JumpReturnY;

        public string strName;
    }
}
