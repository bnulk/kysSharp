using kysSharp.Types;
using SDL;
using static System.Net.Mime.MediaTypeNames;

namespace kysSharp
{
    //这个类专用于显示两个Role的不同，供升级，吃药等显示前后比较
    //可以在属性变化前，以一临时对象记录，再比较前后的变化
    class ShowRoleDifference : TextBox
    {
        private Head head1_ = new Head();
        private Head head2_ = new Head();

        private bool show_head_ = true;
        private bool black_screen_ = true;
        private bool isShowDifference = false;

        public Role role1_ = new Role();
        public Role role2_ = new Role();

        public ShowRoleDifference()
        {
            head1_ = new Head();
            addChild(head1_);
            head2_ = new Head();
            addChild(head2_, 400, 0);
            //setText("修習成功");
            setPosition(250, 180);
            setTextPosition(0, -30);
        }

        public ShowRoleDifference(Role r1, Role r2)
        {
            head1_ = new Head(r1);

            addChild(head1_);
            head2_ = new Head(r2);
            addChild(head2_, 400, 0);
            //setText("修習成功");
            setPosition(250, 180);
            setTextPosition(0, -30);

            SetTwinRole(r1, r2);
        }

        /////////////////////////////////////////////////////////////////////////
        // 绘制方法：比较两个角色的属性并绘制差异
        /////////////////////////////////////////////////////////////////////////
        public override void draw()
        {
            if (role1_ == null || role2_ == null)
                return;

            if (black_screen_)
            {
                Engine.getInstance().fillColor(new SDL_Color() { r = 0, g = 0, b = 0, a = 192 }, 0, 0, -1, -1);
            }

            head1_.SetRole(ref role1_);
            head2_.SetRole(ref role2_);
            head1_.setState(State.Press);
            head2_.setState(State.Press);

            // 如果是同一角色，仅显示一个头像
            if (role1_ != null && role2_ != null && role1_.ID == role2_.ID)
            {
                head1_.SetRole(ref role2_);
                head1_.setPosition(200, 50);
                Role tmpRole = new Role();
                head2_.SetRole(ref tmpRole);
            }

            head1_.setVisible(show_head_);
            head2_.setVisible(show_head_);

            var font = GameFont.getInstance();
            SDL_Color color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
            const int fontSize = 25;
            int x = x_;
            int y = y_;

            string str;

            /////////////////////////////////////////////////////////////////////////
            // 基础属性差异显示
            /////////////////////////////////////////////////////////////////////////
            ShowOneDifference(role1_ => role1_.Level, "等級 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Exp, "經驗 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.PhysicalPower, "體力 {0,7}   -> {1,7}", 20, color, ref x, ref y);

            if (role1_.HP != role2_.HP || role1_.MaxHP != role2_.MaxHP)
            {
                str = string.Format("生命 {0,3}/{1,3}   -> {2,3}/{3,3}",
                    role1_.HP, role1_.MaxHP, role2_.HP, role2_.MaxHP);
                ShowOneDifference(role1_ => role1_.HP, str, 20, color, ref x, ref y, 1);
            }

            if (role1_.MP != role2_.MP || role1_.MaxMP != role2_.MaxMP)
            {
                str = string.Format("內力 {0,3}/{1,3}   -> {2,3}/{3,3}",
                    role1_.MP, role1_.MaxMP, role2_.MP, role2_.MaxMP);
                ShowOneDifference(role1_ => role1_.MP, str, 20, color, ref x, ref y, 1);
            }

            /////////////////////////////////////////////////////////////////////////
            // 战斗与技能属性
            /////////////////////////////////////////////////////////////////////////
            ShowOneDifference(role1_ => role1_.Attack, "攻擊 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Defence, "防禦 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Speed, "輕功 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Medcine, "醫療 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.UsePoison, "用毒 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Detoxification, "解毒 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.AntiPoison, "抗毒 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.AttackWithPoison, "帶毒 {0,7}   -> {1,7}", 20, color, ref x, ref y);

            ShowOneDifference(role1_ => role1_.Fist, "拳掌 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Sword, "御劍 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Knife, "耍刀 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Unusual, "特殊 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.HiddenWeapon, "暗器 {0,7}   -> {1,7}", 20, color, ref x, ref y);

            ShowOneDifference(role1_ => role1_.Poison, "中毒 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Morality, "道德 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.Fame, "聲望 {0,7}   -> {1,7}", 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.IQ, "資質 {0,7}   -> {1,7}", 20, color, ref x, ref y);

            /////////////////////////////////////////////////////////////////////////
            // 內力陰陽
            /////////////////////////////////////////////////////////////////////////
            str = "內力陰陽調和";
            if (role2_.MPType == 0) str = "內力陰";
            if (role2_.MPType == 1) str = "內力陽";
            ShowOneDifference(role1_ => role1_.MPType, str, 20, color, ref x, ref y);
            ShowOneDifference(role1_ => role1_.AttackTwice, "雙擊", 20, color, ref x, ref y);

            /////////////////////////////////////////////////////////////////////////
            // 武學修為變化
            /////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                if (role2_.MagicID[i] > 0 &&
                    role1_.GetRoleShowLearnedMagicLevel(i) != role2_.GetRoleShowLearnedMagicLevel(i))
                {
                    str = string.Format("武學 {0} 目前修為 {1}",
                        Save.getInstance().GetMagic(role2_.MagicID[i]).Name,
                        role2_.GetRoleShowLearnedMagicLevel(i));
                    ShowOneDifference(role1_ => role1_.MagicLevel[i], str, 20, color, ref x, ref y);
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 若沒有任何變化，顯示提示文字
            /////////////////////////////////////////////////////////////////////////
            if (y == y_)
            {
                GameFont.getInstance().draw("無明显效果", 20, x, y, color);
            }

            base.draw();
        }





        public void SetTwinRole(Role r1, Role r2) { role1_ = r1; role2_ = r2; }

        public void SetShowHead(bool s) { show_head_ = s; }

        public void SetBlackScreen(bool b) { black_screen_ = b; }

        public override void onPressedOK()
        {
            exitWithResult(0);
        }

        public override void onPressedCancel()
        {
            exitWithResult(-1);
        }

        private void ShowDifference(int size, SDL_Color c, int x, int y, int force = 0)
        {
            bool isEqual = IsEqual();

            if (isEqual == true)
            {
                GameFont.getInstance().draw("無明显效果", 20, x, y, c);
            }

            if (force != 0)
            {
                GameFont.getInstance().draw("hello", size, x, y, c);
            }

            isShowDifference = true;
        }

        private bool IsEqual()
        {
            bool isEqual = true;

            if (role1_.Level != role2_.Level) isEqual = false;                                               //等级
            if (role1_.Exp != role2_.Exp) isEqual = false;                                                   //经验
            if (role1_.PhysicalPower != role2_.PhysicalPower) isEqual = false;                               //体力
            if (role1_.HP != role2_.HP || role1_.MaxHP != role2_.MaxHP) isEqual = false;                     //生命
            if (role1_.MP != role2_.MP || role1_.MaxMP != role2_.MaxMP) isEqual = false;                     //内力

            if (role1_.Attack != role2_.Attack) isEqual = false;                                             //攻击
            if (role1_.Defence != role2_.Defence) isEqual = false;                                           //防御
            if (role1_.Speed != role2_.Speed) isEqual = false;                                               //轻功

            if (role1_.Medcine != role2_.Medcine) isEqual = false;                                           //医疗
            if (role1_.UsePoison != role2_.UsePoison) isEqual = false;                                       //用毒
            if (role1_.Detoxification != role2_.Detoxification) isEqual = false;                             //解毒
            if (role1_.AntiPoison != role2_.AntiPoison) isEqual = false;                                     //抗毒
            if (role1_.AttackWithPoison != role2_.AttackWithPoison) isEqual = false;                         //解毒

            if (role1_.Fist != role2_.Fist) isEqual = false;                                                 //拳
            if (role1_.Sword != role2_.Sword) isEqual = false;                                               //剑
            if (role1_.Knife != role2_.Knife) isEqual = false;                                               //刀
            if (role1_.Unusual != role2_.Unusual) isEqual = false;                                           //特殊
            if (role1_.HiddenWeapon != role2_.HiddenWeapon) isEqual = false;                                 //暗器

            if (role1_.Poison != role2_.Poison) isEqual = false;                                             //中毒

            if (role1_.Morality != role2_.Morality) isEqual = false;                                         //道德
            if (role1_.Fame != role2_.Fame) isEqual = false;                                                 //声望
            if (role1_.IQ != role2_.IQ) isEqual = false;                                                     //资质

            if (role1_.MPType != role2_.MPType) isEqual = false;                                             //内力类型
            if (role1_.AttackTwice != role2_.AttackTwice) isEqual = false;                                   //双击

            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)                                              //武学修为
            {
                if (role2_.MagicID[i] > 0 && role1_.GetRoleShowLearnedMagicLevel(i) != role2_.GetRoleShowLearnedMagicLevel(i))
                {
                    if (role1_.MagicLevel[i] != role2_.MagicLevel[i]) isEqual = false;
                }
            }
            return isEqual;
        }

        public void ShowOneDifference<T>(
            Func<Role, T> selector,
            string formatStr,
            int size,
            SDL_Color color,
            ref int x,
            ref int y,
            int force = 0)
            where T : IComparable
        {
            if (role1_ == null || role2_ == null)
                return;

            T value1 = selector(role1_);
            T value2 = selector(role2_);

            // 仅在不同或强制显示时绘制
            if (value1.CompareTo(value2) != 0 || force != 0)
            {
                string str = string.Format(formatStr, value1, value2);
                GameFont.getInstance().draw(str, size, x, y, color);
                y += size + 5;
            }
        }












    }
}
