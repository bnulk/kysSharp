using kysSharp;
using kysSharp.Types;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static kysSharp.GameRandom;

namespace kysSharp
{
    /// <summary>
    /// 此类中是一些游戏中的公式，例如使用物品的效果，伤害公式等
    /// 通常来说应该全部是静态函数
    /// </summary>
    public static class GameUtil
    {

        private static List<int> level_up_list_= new List<int>();

        public static void Initialize()
        {
            var filePath = Path.Combine("..", "game", "list", "levelup.txt");
            string content = File.ReadAllText(filePath);
            string[] strings = content.Split(",");

            if (level_up_list_.Count < Types.Constant.MAX_LEVEL)
            {
                level_up_list_ = new List<int>();
                for (int i = 0; i < Types.Constant.MAX_LEVEL; i++)
                {
                    if (i>=strings.Length)
                    {
                        level_up_list_.Add(60000);
                    }
                    else
                    {
                        level_up_list_.Add(Convert.ToInt32(strings[i]));
                    }                    
                }
            }

        }

        /// <summary>
        /// 释放控制台
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        /// <summary>
        /// 开一个控制台窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        /// <summary>
        /// 开一个控制台窗口，并输出字符串
        /// </summary>
        /// <param name="str">字符串</param>
        public static void ConsoleWriteLine(string str)
        {
            AllocConsole();
            Console.WriteLine(str);
        }

        /// <summary>
        /// 去掉“\0”字符
        /// </summary>
        /// <param name="modredundantString">带“\0”字符的字符串</param>
        /// <returns>去掉“\0”字符的字符串</returns>
        public static string EraseModredundantChar(string modredundantString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char ch in modredundantString)
            {
                if (ch != '\0')
                {
                    stringBuilder.Append(ch);
                }
            }

            return stringBuilder.ToString();
        }

        public static int sign(int v)
        {
            if (v > 0)
            { return 1; }
            if (v < 0)
            { return -1; }
            return 0;
        }

        /// <summary>
        /// 返回限制值
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="min_value">最小值</param>
        /// <param name="max_value">最大值</param>
        /// <returns></returns>
        public static int limit(int current, int min_value, int max_value)
        {
            if (current < min_value) { current = min_value; }
            if (current > max_value) { current = max_value; }
            return current;
        }

        /// <summary>
        /// limit2是直接修改引用值，有两个重载
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="min_value">最小值</param>
        /// <param name="max_value">最大值</param>
        public static void limit2(ref int current, int min_value, int max_value)
        {
            current = limit(current, min_value, max_value);
        }

        /// <summary>
        /// limit2是直接修改引用值，有两个重载
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="min_value">最小值</param>
        /// <param name="max_value">最大值</param>
        public static void limit2(ref Int16 current, int min_value, int max_value)
        {
            current = (Int16)limit((int)current, min_value, max_value);
        }

        /// <summary>
        /// limit2是直接修改引用值，有两个重载
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="min_value">最小值</param>
        /// <param name="max_value">最大值</param>
        public static void limit2(ref UInt16 current, int min_value, int max_value)
        {
            current = (UInt16)limit((int)current, min_value, max_value);
        }

        /// <summary>
        /// 计算某个数值的位数
        /// </summary>
        /// <param name="x">数值</param>
        /// <returns>该数值的位数</returns>
        public static int digit(int x)
        {
            int n = (int)Math.Floor(Math.Log10(0.5 + Math.Abs(x)));
            if (x >= 0)
            {
                return n;
            }
            else
            {
                return n + 1;
            }
        }

        /// <summary>
        /// 某人是否可以使用某物品
        /// </summary>
        /// <param name="r">角色</param>
        /// <param name="i">物品</param>
        /// <returns></returns>
        public static bool CanUseItem(Role r, Item i)
        {
            if (r == null) { return false; }
            if (i == null) { return false; }
            if (i.ItemType == 0)
            {
                //剧情类无人可以使用
                return false;
            }
            else if (i.ItemType == 1 || i.ItemType == 2)
            {
                if (i.ItemType == 2)
                {
                    //内力属性判断
                    if ((r.MPType == 0 || r.MPType == 1) && (i.NeedMPType == 0 || i.NeedMPType == 1))
                    {
                        if (r.MPType != i.NeedMPType)
                        {
                            return false;
                        }
                    }
                    //有仅适合人物，直接判断
                    if (i.OnlySuitableRole >= 0)
                    {
                        return i.OnlySuitableRole == r.ID;
                    }
                }

                //若有相关武学，满级则为假，未满级为真
                //若已经学满武学，则为假
                //此处注意，如果有可制成物品的秘籍，则武学满级之后不会再制药了，请尽量避免这样的设置
                if (i.MagicID > 0)
                {
                    int level = r.GetMagicLevelIndex(i.MagicID);
                    if (level >= 0 && level < Constant.MAX_MAGIC_LEVEL_INDEX) { return true; }
                    if (level < 0 && r.GetLearnedMagicCount() == Constant.ROLE_MAGIC_COUNT) { return false; }
                    if (level == Constant.MAX_MAGIC_LEVEL_INDEX) { return false; }
                }

                //上面的判断未确定则进入下面的判断链
                return test(r.Attack, i.NeedAttack) && test(r.Speed, i.NeedSpeed)
                    && test(r.Medcine, i.NeedMedcine)
                    && test(r.UsePoison, i.NeedUsePoison) && test(r.Detoxification, i.NeedDetoxification)
                    && test(r.Fist, i.NeedFist) && test(r.Sword, i.NeedSword)
                    && test(r.Knife, i.NeedKnife) && test(r.Unusual, i.NeedUnusual)
                    && test(r.HiddenWeapon, i.NeedHiddenWeapon)
                    && test(r.MP, i.NeedMP)
                    && test(r.IQ, i.NeedIQ);
            }
            else if (i.ItemType == 3)
            {
                //药品类所有人可以使用
                return true;
            }
            else if (i.ItemType == 4)
            {
                //暗器类不可以使用
                return false;
            }
            return false;
        }


        /// <summary>
        /// 判断某个属性是否适合
        /// </summary>
        /// <param name="v">角色属性</param>
        /// <param name="v_need">Item属性</param>
        /// <returns></returns>
        private static bool test(int v, int v_need)
        {
            if (v_need > 0 && v < v_need) { return false; }
            if (v_need < 0 && v > -v_need) { return false; }
            return true;
        }

        /// <summary>
        /// 使用物品时属性变化
        /// </summary>
        /// <param name="r">角色</param>
        /// <param name="i">物品</param>
        public static void UseItem(ref Role r, ref Item i)
        {
            if (r == null) { return; }
            if (i == null) { return; }
            r.PhysicalPower += i.AddPhysicalPower;
            r.HP += i.AddHP;
            r.MaxHP += i.AddMaxHP;
            r.MP += i.AddMP;
            r.MaxMP += i.AddMaxMP;

            r.Poison += i.AddPoison;

            r.Medcine += i.AddMedcine;
            r.Detoxification += i.AddDetoxification;
            r.UsePoison += i.AddUsePoison;

            r.Attack += i.AddAttack;
            r.Defence += i.AddDefence;
            r.Speed += i.AddSpeed;

            r.Fist += i.AddFist;
            r.Sword += i.AddSword;
            r.Knife += i.AddKnife;
            r.Unusual += i.AddUnusual;
            r.HiddenWeapon += i.AddHiddenWeapon;

            r.Knowledge += i.AddKnowledge;
            r.Morality += i.AddMorality;
            r.AntiPoison += i.AddAntiPoison;
            r.AttackWithPoison += i.AddAttackWithPoison;

            if (i.ChangeMPType == 2) { r.MPType = 2; }
            if (i.AddAttackTwice != 0) { r.AttackTwice = 1; }

            int need_item_exp = GetFinishedExpForItem(ref r, ref i);
            if (r.ExpForItem >= need_item_exp)
            {
                r.LearnMagic(i.MagicID);
                r.ExpForItem -= need_item_exp;
            }

            r.Limit();
        }

        /// <summary>
        /// 升级的属性变化
        /// </summary>
        /// <param name="r">角色</param>
        public static void LevelUp(ref Role r)
        {
            if (r == null) { return; }

            r.Exp -= GameUtil.level_up_list_[r.Level - 1];
            r.Level++;

            r.PhysicalPower = Constant.MAX_PHYSICAL_POWER;
            r.MaxHP += r.IncLife * 3 + RandomClassical.rand(6);
            r.HP = r.MaxHP;
            r.MaxMP += 20 + RandomClassical.rand(6);
            r.MP = r.MaxMP;

            r.Hurt = 0;
            r.Poison = 0;

            r.Attack += RandomClassical.rand(7);
            r.Speed += RandomClassical.rand(7);
            r.Defence += RandomClassical.rand(7);

            Check_Up(ref r.Medcine, 0, 3);
            Check_Up(ref r.Detoxification, 0, 3);
            Check_Up(ref r.UsePoison, 0, 3);

            Check_Up(ref r.Fist, 10, 3);
            Check_Up(ref r.Sword, 10, 3);
            Check_Up(ref r.Knife, 10, 3);
            Check_Up(ref r.Unusual, 10, 3);
            Check_Up(ref r.HiddenWeapon, 10, 3);

            r.Limit();
        }

        private static void Check_Up(ref int value, int limit, int max_inc)
        {
            if (value > limit)
            {
                value += 1 + RandomClassical.rand(max_inc);
            }
        }

        /// <summary>
        /// 是否可以升级
        /// </summary>
        /// <param name="r">角色</param>
        /// <returns>是否可以升级</returns>
        public static bool CanLevelUp(ref Role r)
        {
            if (r.Level >= 1 && r.Level <= Constant.MAX_LEVEL)
            {
                if (r.Exp >= GetLevelUpExp(r.Level))
                {
                    return true;
                }
            }
            return false;
        }

        public static int GetLevelUpExp(int level)
        {
            if (level <= 0 || level >= Constant.MAX_LEVEL) { return int.MaxValue; }
            return GameUtil.level_up_list_[level - 1];
        }

        /// <summary>
        /// 物品经验值是否足够
        /// </summary>
        /// <param name="r">角色</param>
        /// <returns>物品经验值是否足够</returns>
        public static bool CanFinishedItem(ref Role r)
        {
            var item = Save.getInstance().GetItem(r.PracticeItem);
            if (r.ExpForItem >= GetFinishedExpForItem(ref r, ref item))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修炼物品所需经验
        /// </summary>
        /// <param name="r">角色</param>
        /// <param name="i">物品</param>
        /// <returns>修炼物品所需经验</returns>
        public static int GetFinishedExpForItem(ref Role r, ref Item i)
        {
            //无经验设定物品不可修炼
            if (i == null || i.NeedExp <= 0)
            {
                return int.MaxValue;
            }

            int multiple = 7 - r.IQ / 15;
            if (multiple <= 0) { multiple = 1; }

            //有关联武学的，如已满级则不可修炼
            if (i.MagicID > 0)
            {
                int magic_level_index = r.GetMagicLevelIndex(i.MagicID);
                if (magic_level_index == Constant.MAX_MAGIC_LEVEL_INDEX)
                {
                    return int.MaxValue;
                }
                //初次修炼和从1级升到2级的是一样的
                if (Constant.MAX_MAGIC_LEVEL_INDEX > 0)
                {
                    multiple *= magic_level_index;
                }
            }
            return i.NeedExp * multiple;
        }

        public static void Equip(ref Role r, ref Item i)
        {
            if (r == null) { return; }
            if (i == null) { return; }

            var r0 = Save.getInstance().GetRole(i.User);
            var book = Save.getInstance().GetItem(r.PracticeItem);
            var equip0 = Save.getInstance().GetItem(r.Equip0);
            var equip1 = Save.getInstance().GetItem(r.Equip1);

            if (r0.ID != 0) { r0.PracticeItem = -1; }
            i.User = r.ID;

            if (i.ItemType == 2)
            {
                //秘籍        
                if (book.ID != 0) { book.User = -1; }
                r.PracticeItem = i.ID;
            }
            if (i.ItemType == 1)
            {
                if (i.EquipType == 0)
                {
                    if (equip0.ID != 0) { equip0.User = -1; }
                    r.Equip0 = i.ID;
                }
                if (i.EquipType == 1)
                {
                    if (equip1.ID != 0) { equip1.User = -1; }
                    r.Equip1 = i.ID;
                }
            }
        }

        /// <summary>
        /// 医疗的效果
        /// </summary>
        /// <param name="r1">角色1</param>
        /// <param name="r2">角色2</param>
        /// <returns>医疗值</returns>
        public static int Medcine(ref Role r1, ref Role r2)
        {
            if (r1.ID == 0 || r2.ID == 0) { return 0; }
            var temp = r2.HP;
            r2.HP += r1.Medcine;
            GameUtil.limit2(ref r2.HP, 0, r2.MaxHP);
            return r2.HP - temp;
        }

        /// <summary>
        /// 解毒
        /// </summary>
        /// <param name="r1">角色1</param>
        /// <param name="r2">角色2</param>
        /// <returns>解毒值，这个返回值通常应为负</returns>
        public static int Detoxification(ref Role r1, ref Role r2)
        {
            if (r1.ID == 0 || r2.ID == 0) { return 0; }
            var temp = r2.Poison;
            r2.Poison -= r1.Detoxification / 3;
            limit2(ref r2.Poison, 0, Constant.MAX_POISON);
            return r2.Poison - temp;
        }

        /// <summary>
        /// 用毒
        /// </summary>
        /// <param name="r1">角色1</param>
        /// <param name="r2">角色2</param>
        /// <returns>用毒值</returns>
        public static int UsePoison(ref Role r1, ref Role r2)
        {
            if (r1.ID == 0 || r2.ID == 0) { return 0; }
            var temp = r2.Poison;
            r2.Poison += r1.UsePoison / 3;
            limit2(ref r2.Poison, 0, Constant.MAX_POISON);
            return r2.Poison - temp;
        }

        /// <summary>
        /// 计算某个数值的位数
        /// </summary>
        /// <param name="x">数</param>
        /// <returns>该数的位数</returns>
        public static int Digit(int x)
        {
            int n = (int)Math.Floor(Math.Log10(0.5 + Math.Abs(x)));
            if (x >= 0)
            {
                return n;
            }
            else
            {
                return n + 1;
            }
        }














    }
}
