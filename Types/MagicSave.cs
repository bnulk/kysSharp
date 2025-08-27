using System;

namespace kysSharp.Types
{
    /// <summary>
    /// 存档中的武学数据（无适合对应翻译，而且武侠小说中的武学近于魔法，暂且如此）
    /// </summary>
    [Serializable]
    public class MagicSave
    {
        public int ID;
        public sbyte[] Name = new sbyte[20];
        public int[] Unknown = new int[5];
        public int SoundID;
        public int MagicType;  //1-拳，2-剑，3-刀，4-特殊
        public int EffectID;
        public int HurtType;  //0-普通，1-吸取MP
        public int AttackAreaType;  //0-点，1-线，2-十字，3-面
        public int NeedMP, WithPoison;
        public int[] Attack = new int[10];
        public int[] SelectDistance = new int[10];
        public int[] AttackDistance = new int[10];
        public int[] AddMP = new int[10];
        public int[] HurtMP = new int[10];

        public string strName;
    }
}
