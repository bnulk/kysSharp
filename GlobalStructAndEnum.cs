global using MAP_INT = System.Int16;
using System.Runtime.InteropServices;

namespace kysSharp
{
    //状态
    public enum State
    {
        Normal,
        Pass,
        Press,
    };

    public enum Align
    {
        Left,
        Center,
        Right,
    };

    public enum Towards
    {
        RightUp = 0,
        RightDown = 1,
        LeftUp = 2,
        LeftDown = 3,
        None
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ItemList
    {
        public int item_id;
        public int count;
    }

    public enum Type
    {
        MainMap = 0,
        Scene,
        Battle,
        Cloud,
        MaxType
    }


    

    


}
