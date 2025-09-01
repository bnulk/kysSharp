using SDL;
using kysSharp.Types;

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

        public override void draw()
        {
            if (role1_ == null || role2_ == null) { return; }

            if (black_screen_)
            {
                Engine.getInstance().fillColor(new SDL_Color() { r=0,g=0,b=0,a=192}, 0, 0, -1, -1);
            }
            head1_.SetRole(ref role1_);
            head2_.SetRole(ref role2_);
            head1_.setState(State.Press);
            head2_.setState(State.Press);
            
            if (role1_!=null && role2_!=null && role1_.ID == role2_.ID)
            {
                head1_.SetRole(ref role2_);
                head1_.setPosition(200, 20);
                Role tmpRole = new Role();
                head2_.SetRole(ref tmpRole);
            }

            head1_.setVisible(show_head_);
            head2_.setVisible(show_head_);

            var font = GameFont.getInstance();
            SDL_Color color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
            const int font_size = 25;
            int x = x_, y = y_;

            string str;

            ShowDifference(20, color, x, y);
            
            //showOneDifference(role1_->Name, "姓名 %-7s  -> %-7s", 20, color, x, y);
            //ShowOneDifference("Level", "等級 {0}   -> {1}", 20, color, x, y);
                /*
            showOneDifference(role1_->Exp, "經驗 %7d   -> %7d", 20, color, x, y);

            showOneDifference(role1_->PhysicalPower, "體力 %7d   -> %7d", 20, color, x, y);

            if (role1_->HP != role2_->HP || role1_->MaxHP != role2_->MaxHP)
            {
                str = convert::formatString("生命 %3d/%3d   -> %3d/%3d", role1_->HP, role1_->MaxHP, role2_->HP, role2_->MaxHP);
                showOneDifference(role1_->HP, str, 20, color, x, y, 1);
            }
            if (role1_->MP != role2_->MP || role1_->MaxMP != role2_->MaxMP)
            {
                str = convert::formatString("內力 %3d/%3d   -> %3d/%3d", role1_->MP, role1_->MaxMP, role2_->MP, role2_->MaxMP);
                showOneDifference(role1_->MP, str, 20, color, x, y, 1);
            }

            showOneDifference(role1_->Attack, "攻擊 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Defence, "防禦 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Speed, "輕功 %7d   -> %7d", 20, color, x, y);

            showOneDifference(role1_->Medcine, "醫療 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->UsePoison, "用毒 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Detoxification, "解毒 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->AntiPoison, "抗毒 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->AttackWithPoison, "帶毒 %7d   -> %7d", 20, color, x, y);

            showOneDifference(role1_->Fist, "拳掌 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Sword, "御劍 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Knife, "耍刀 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Unusual, "特殊 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->HiddenWeapon, "暗器 %7d   -> %7d", 20, color, x, y);

            showOneDifference(role1_->Poison, "中毒 %7d   -> %7d", 20, color, x, y);

            showOneDifference(role1_->Morality, "道德 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->Fame, "聲望 %7d   -> %7d", 20, color, x, y);
            showOneDifference(role1_->IQ, "資質 %7d   -> %7d", 20, color, x, y);

            str = "內力陰陽調和";
            if (role2_->MPType == 0) { str = "內力陰"; }
            if (role2_->MPType == 1) { str = "內力陽"; }
            showOneDifference(role1_->MPType, str, 20, color, x, y);
            showOneDifference(role1_->AttackTwice, "雙擊", 20, color, x, y);

            for (int i = 0; i < ROLE_MAGIC_COUNT; i++)
            {
                if (role2_->MagicID[i] > 0 && role1_->getRoleShowLearnedMagicLevel(i) != role2_->getRoleShowLearnedMagicLevel(i))
                {
                    str = convert::formatString("武學%s目前修為%d",
                        Save::getInstance()->getMagic(role2_->MagicID[i])->Name, role2_->getRoleShowLearnedMagicLevel(i));
                    showOneDifference(role1_->MagicLevel[i], str, 20, color, x, y);
                }
            }

            if (y == y_)
            {
                Font::getInstance()->draw("無明显效果", 20, x, y, color);
            }
            //showOneDifference(role1_->Level, "御劍 %7d   -> %7d", 20, color, x, y);
            */
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













    }
}
