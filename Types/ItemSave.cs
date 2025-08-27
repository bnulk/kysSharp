using System;

namespace kysSharp.Types
{
    /// <summary>
    /// 存档中的物品数据
    /// </summary>
    [Serializable]
    public class ItemSave
    {
        public int ID;
        public sbyte[] Name = new sbyte[40];
        public int[] Name1 = new int[10];
        public sbyte[] Introduction = new sbyte[60];
        public int MagicID, HiddenWeaponEffectID, User, EquipType, ShowIntroduction;
        public int ItemType;   //0剧情，1装备，2秘笈，3药品，4暗器
        public int UnKnown5, UnKnown6, UnKnown7;
        public int AddHP, AddMaxHP, AddPoison, AddPhysicalPower, ChangeMPType, AddMP, AddMaxMP;
        public int AddAttack, AddSpeed, AddDefence, AddMedcine, AddUsePoison, AddDetoxification, AddAntiPoison;
        public int AddFist, AddSword, AddKnife, AddUnusual, AddHiddenWeapon, AddKnowledge, AddMorality, AddAttackTwice, AddAttackWithPoison;
        public int OnlySuitableRole, NeedMPType, NeedMP, NeedAttack, NeedSpeed, NeedUsePoison, NeedMedcine, NeedDetoxification;
        public int NeedFist, NeedSword, NeedKnife, NeedUnusual, NeedHiddenWeapon, NeedIQ;
        public int NeedExp, NeedExpForMakeItem, NeedMaterial;
        public int[] MakeItem = new int[5];
        public int[] MakeItemCount = new int[5];

        public string strName;
        public string strIntroduction;
    }
}
