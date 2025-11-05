
namespace kysSharp.Types
{
    /// <summary>
    /// 实际的角色数据，基类之外的通常是战斗属性
    /// </summary>
    [Serializable]
    public class Role : RoleSave
    {
        public string strName = "";
        public string strNick = "";

        public int Team;
        public int FaceTowards, Dead, Step;
        public int Pic, BattleSpeed;
        public int ExpGot, Auto;
        public int[] FightFrame = new int[5];
        public int FightingFrame;
        public int Moved, Acted;
        public int ActTeam;  //选择行动阵营 0-我方，1-非我方，画效果层时有效

        public string ShowString = "";
        public int[] ShowColor = new int[4];

        public MapSquare position_layer_ = new MapSquare();

        private int X_, Y_;
        private int prevX_, prevY_;

        int AI_Action = 0;
        int AI_MoveX, AI_MoveY;
        int AI_ActionX, AI_ActionY;
        Magic AI_Magic = new Magic();
        Item AI_Item = new Item();

        public Role()
        {
            int i;
            for (i = 0; i < 5; i++)
            {
                FightFrame[i] = -1;
            }
        }

        public void SetPoitionLayer(MapSquare l) { position_layer_ = l; }

        //设置人物坐标，若输入值为负，相当于从人物层清除
        public void SetPosition(int x, int y)
        {
            if (position_layer_ == null)
            {
                return;
            }
            if (X_ >= 0 && Y_ >= 0)
            {
                position_layer_.SetData(X_, Y_, -1);
            }
            if (x >= 0 && y >= 0)
            {
                position_layer_.SetData(x, y, (MAP_INT)ID);
            }
            X_ = x;
            Y_ = y;
        }

        public void SetPrevPosition(int x, int y)
        {
            prevX_ = x;
            prevY_ = y;
        }


        public void ResetPosition()
        {
            SetPosition(prevX_, prevY_);
        }
        public int X()
        {
            return X_;
        }
        public int Y()
        {
            return Y_;
        }

        //带role的，表示后面的参数是人物武功栏

        /// <summary>
        /// 显示用的，比内部数组用的多1
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetRoleShowLearnedMagicLevel(int i)
        {
            return GetRoleMagicLevelIndex(i) + 1;
        }

        /// <summary>
        /// 获取武学等级，返回值是0~9，可以直接用于索引武功的威力等数据
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetRoleMagicLevelIndex(int i)
        {
            int l = MagicLevel[i] / 100;
            if (l < 0) { l = 0; }
            if (l > 9) { l = 9; }
            return l;
        }

        /// <summary>
        /// 获取已学习武学的数量
        /// </summary>
        /// <returns>已学习武学的数量</returns>
        public int GetLearnedMagicCount()
        {
            int n = 0;
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                if (MagicID[i] > 0) { n++; }
            }
            return n;
        }

        /// <summary>
        /// 依据武学获取等级，-1表示未学得
        /// </summary>
        /// <param name="magic">武学</param>
        /// <returns></returns>
        public int GetMagicLevelIndex(Magic magic)
        {
            return GetMagicLevelIndex(magic.ID);
        }

        /// <summary>
        /// 依据武学ID获取等级，-1表示未学得
        /// </summary>
        /// <param name="magic_id">武学ID</param>
        /// <returns></returns>
        public int GetMagicLevelIndex(int magic_id)
        {
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                if (MagicID[i] == magic_id)
                {
                    return GetRoleMagicLevelIndex(i);
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取武学在角色的栏位编号
        /// </summary>
        /// <param name="magic">武学</param>
        /// <returns></returns>
        public int GetMagicOfRoleIndex(Magic magic)
        {
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                if (MagicID[i] == magic.ID)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 限制人物的属性
        /// </summary>
        public void Limit()
        {

            GameUtil.limit2(ref Level, 0, Constant.MAX_LEVEL);

            GameUtil.limit2(ref Exp, 0, Constant.MAX_EXP);
            GameUtil.limit2(ref ExpForItem, 0, Constant.MAX_EXP);
            GameUtil.limit2(ref ExpForMakeItem, 0, Constant.MAX_EXP);

            GameUtil.limit2(ref Poison, 0, Constant.MAX_POISON);

            GameUtil.limit2(ref MaxHP, 0, Constant.MAX_HP);
            GameUtil.limit2(ref MaxMP, 0, Constant.MAX_MP);
            GameUtil.limit2(ref HP, 0, MaxHP);
            GameUtil.limit2(ref MP, 0, MaxMP);
            GameUtil.limit2(ref PhysicalPower, 0, Constant.MAX_PHYSICAL_POWER);

            GameUtil.limit2(ref Attack, 0, Constant.MAX_ATTACK);
            GameUtil.limit2(ref Defence, 0, Constant.MAX_DEFENCE);
            GameUtil.limit2(ref Speed, 0, Constant.MAX_SPEED);

            GameUtil.limit2(ref Medcine, 0, Constant.MAX_MEDCINE);
            GameUtil.limit2(ref UsePoison, 0, Constant.MAX_USE_POISON);
            GameUtil.limit2(ref Detoxification, 0, Constant.MAX_DETOXIFICATION);
            GameUtil.limit2(ref AntiPoison, 0, Constant.MAX_ANTI_POISON);

            GameUtil.limit2(ref Fist, 0, Constant.MAX_FIST);
            GameUtil.limit2(ref Sword, 0, Constant.MAX_SWORD);
            GameUtil.limit2(ref Knife, 0, Constant.MAX_KNIFE);
            GameUtil.limit2(ref Unusual, 0, Constant.MAX_UNUSUAL);
            GameUtil.limit2(ref HiddenWeapon, 0, Constant.MAX_HIDDEN_WEAPON);

            GameUtil.limit2(ref Knowledge, 0, Constant.MAX_KNOWLEDGE);
            GameUtil.limit2(ref Morality, 0, Constant.MAX_MORALITY);
            GameUtil.limit2(ref AttackWithPoison, 0, Constant.MAX_ATTACK_WITH_POISON);
            GameUtil.limit2(ref Fame, 0, Constant.MAX_FAME);
            GameUtil.limit2(ref IQ, 0, Constant.MAX_IQ);

            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                GameUtil.limit2(ref MagicLevel[i], 0, Constant.MAX_MAGIC_LEVEL);
            }

        }

        public int LearnMagic(Magic magic)
        {
            if (magic == null || magic.ID <= 0) { return -1; }  //武学id错误
            return LearnMagic(magic.ID);
        }

        public int LearnMagic(int magic_id)
        {
            if (magic_id <= 0) { return -1; }
            //检查是否已经学得
            int index = -1;
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                if (MagicID[i] == magic_id)
                {
                    if (MagicLevel[i] / 100 < Constant.MAX_MAGIC_LEVEL_INDEX)
                    {
                        MagicLevel[i] += 100;
                        return 0;
                    }
                    else
                    {
                        return -2;   //满级
                    }
                }
                if (MagicID[i] <= 0)
                {
                    index = i;
                }
            }

            if (index < 0)
            {
                return -3;   //若进行到此index为负，表示武学栏已满
            }
            else
            {
                //增加武学
                MagicID[index] = magic_id;
                MagicLevel[index] = 0;
                return 0;
            }
        }

        public bool isAuto()
        {
            return Auto != 0 || Team != 0;
        }

        public Role Clone()
        {
            return (Role)this.MemberwiseClone();
        }

    }
}
