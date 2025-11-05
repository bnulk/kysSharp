using System;

namespace kysSharp.Types
{
    /// <summary>
    /// 存档中的角色数据
    /// </summary>
    [Serializable]
    public class RoleSave
    {
        public int ID;
        public int HeadID, IncLife, UnUse;
        public sbyte[] Name = new sbyte[20];
        public sbyte[] Nick = new sbyte[20];
        public int Sexual;  //性别 0-男 1 女 2 其他
        public int Level;
        public int Exp;
        public int HP, MaxHP, Hurt, Poison, PhysicalPower;
        public int ExpForMakeItem;
        public int Equip0, Equip1;
        public int[] Frame = new int[15];    //动作帧数，改为不在此处保存，故实际无用，另外延迟帧数对效果几乎无影响，废弃
        public int MPType, MP, MaxMP;
        public int Attack, Speed, Defence, Medcine, UsePoison, Detoxification, AntiPoison, Fist, Sword, Knife, Unusual, HiddenWeapon;
        public int Knowledge, Morality, AttackWithPoison, AttackTwice, Fame, IQ;
        public int PracticeItem;
        public int ExpForItem;
        public int[] MagicID = new int[Constant.ROLE_MAGIC_COUNT];
        public int[] MagicLevel = new int[Constant.ROLE_MAGIC_COUNT];
        public int[] TakingItem = new int[Constant.ROLE_TAKING_ITEM_COUNT];
        public int[] TakingItemCount = new int[Constant.ROLE_TAKING_ITEM_COUNT];
    }
}
