using kysSharp.Types;
using SDL;

namespace kysSharp
{
    class UIStatus : Element
    {
        protected Button button_medcine_;
        protected Button button_detoxification_;
        protected Button button_leave_;
        protected bool show_button_ = true;

        protected Role? role_ = null;
        protected TeamMenu? team_menu_ = null;

        private ShowRoleDifference? showRoleDifference = null;
        private bool isShowRoleDifference = false;

        private enum MenuType
        {
            uiButton,
            medcine,
            detoxification,
            leave
        }
        private MenuType menuType = MenuType.uiButton;

        public void SetShowButton(bool b) { show_button_ = b; }
        public void SetRole(Role r) { role_ = r; }

        public Role? GetRole() { return role_; }

        public UIStatus()
        {
            button_medcine_ = new Button();
            button_medcine_.setText("醫療");
            addChild(button_medcine_, 350, 55);

            button_detoxification_ = new Button();
            button_detoxification_.setText("解毒");
            addChild(button_detoxification_, 400, 55);

            button_leave_ = new Button();
            button_leave_.setText("離隊");
            addChild(button_leave_, 450, 55);
        }

        public override void draw()
        {
            string str;
            int tmpLength;
            int exp_up;

            if (role_ != null || !show_button_)
            {
                button_medcine_.setVisible(false);
                button_detoxification_.setVisible(false);
                button_leave_.setVisible(false);
            }

            if (role_ != null)
            {
                if (show_button_)
                {
                    button_medcine_.setVisible(role_.Medcine > 0);
                    button_detoxification_.setVisible(role_.Detoxification > 0);
                    button_leave_.setVisible(role_.ID != 0);
                }
            }
            else
            {
                return;
            }

            TextureManager.getInstance().renderTexture("head", role_.HeadID, x_ + 10, y_ + 20);

            var font = GameFont.getInstance();
            SDL_Color color =new SDL_Color() { r=255, g=255, b=255, a=255 };
            int font_size = 22;

            int x, y;

            x = x_ + 200;
            y = y_ + 50;
            font.draw(GameUtil.EraseModredundantChar(role_.strName).PadRight(6), 30, x - 10, y, color,color.a);
            font.draw("等級" + role_.Level.ToString().PadLeft(7), font_size, x, y + 50, color,color.a);
            font.draw("經驗" + role_.Exp.ToString().PadLeft(7), font_size, x, y + 75, color, color.a);

            str = "升級 ------";
            exp_up = GameUtil.GetLevelUpExp(role_.Level);
            if (exp_up != int.MaxValue)
            {
                str = "升級" + exp_up.ToString().PadLeft(7);
            }

            font.draw(str, font_size, x, y + 100, color, color.a);
            font.draw("生命" + (role_.HP.ToString() + "/" + role_.MaxHP.ToString()).PadLeft(11), font_size, x + 175, y + 50, color, color.a);
            font.draw("邏輯" + (role_.MP.ToString() + "/" + role_.MaxMP.ToString()).PadLeft(11), font_size, x + 175, y + 75, color, color.a);
            font.draw("體力" + (role_.PhysicalPower.ToString() + "/" + 100.ToString()).PadLeft(11), font_size, x + 175, y + 100, color, color.a);

            x = x_ + 20;
            y = y_ + 200;

            font.draw("攻擊" + role_.Attack.ToString().PadLeft(5), font_size, x, y, color,color.a);
            font.draw("防禦" + role_.Defence.ToString().PadLeft(5), font_size, x + 200, y, color,color.a);
            font.draw("輕功" + role_.Speed.ToString().PadLeft(5), font_size, x + 400, y, color, color.a);

            font.draw("醫療" + role_.Medcine.ToString().PadLeft(5), font_size, x, y + 25, color,color.a);
            font.draw("解毒" + role_.Detoxification.ToString().PadLeft(5), font_size, x + 200, y + 25, color, color.a);
            font.draw("用毒" + role_.UsePoison.ToString().PadLeft(5), font_size, x + 400, y + 25, color, color.a);

            x = x_ + 20;
            y = y_ + 270;
            font.draw("技能".PadRight(4), 25, x - 10, y, color,color.a);
            font.draw("數學" + role_.Fist.ToString().PadLeft(5), font_size, x, y + 30, color, color.a);
            font.draw("物理" + role_.Sword.ToString().PadLeft(5), font_size, x, y + 55, color, color.a);
            font.draw("化學" + role_.Knife.ToString().PadLeft(5), font_size, x, y + 80, color, color.a);
            font.draw("算法" + role_.Unusual.ToString().PadLeft(5), font_size, x, y + 105, color, color.a);
            font.draw("生物" + role_.HiddenWeapon.ToString().PadLeft(5), font_size, x, y + 130, color, color.a);

            x = x_ + 220;
            y = y_ + 270;
            font.draw("知識".PadRight(4), 25, x - 10, y, color, color.a);
            for (int i = 0; i < 10; i++)
            {
                var magic = Save.getInstance().GetRoleLearnedMagic(ref role_, i);
                str = "__________";
                if (magic != null)
                {
                    tmpLength = GameUtil.EraseModredundantChar(magic.strName).Length;
                    str = GameUtil.EraseModredundantChar(magic.strName).PadRight(tmpLength * 2) + role_.GetRoleShowLearnedMagicLevel(i).ToString().PadLeft(13 - tmpLength * 2);
                    //tmpLength = "小學數學".Length;
                    //str = "小學數學".PadRight(tmpLength * 2) + role_.GetRoleShowLearnedMagicLevel(i).ToString().PadLeft(13 - tmpLength * 2);
                }
                int x1 = x + i % 2 * 200;
                int y1 = y + 30 + i / 2 * 25;
                font.draw(str, font_size, x1, y1, color,color.a);
            }

            x = x_ + 420;
            y = y_ + 445;
            font.draw("修煉".PadRight(4), 25, x - 10, y, color,color.a);
            var book = Save.getInstance().GetItem(role_.PracticeItem);
            if (book != null)
            {
                TextureManager.getInstance().renderTexture("item", book.ID, x, y + 30);
                font.draw(GameUtil.EraseModredundantChar(book.strName), font_size, x + 90, y + 30, color,color.a);
                font.draw("經驗" + role_.ExpForItem.ToString().PadLeft(5), 18, x + 90, y + 55, color, color.a);
                str = "升級 ----";
                exp_up = GameUtil.GetFinishedExpForItem(ref role_, ref book);
                if (exp_up != int.MaxValue)
                {
                    str = "升級" + exp_up.ToString().PadLeft(5);
                }
                font.draw(str, 18, x + 90, y + 75, color, color.a);
            }

            x = x_ + 20;
            y = y_ + 445;
            font.draw("儀器".PadRight(4), 25, x - 10, y, color, color.a);
            var equip0 = Save.getInstance().GetItem(role_.Equip0);
            if (equip0 != null)
            {
                TextureManager.getInstance().renderTexture("item", equip0.ID, x, y + 30);
                font.draw(GameUtil.EraseModredundantChar(equip0.strName), font_size, x + 90, y + 30, color,color.a);
                font.draw("攻擊+" + equip0.AddAttack.ToString(), 18, x + 90, y + 55, color,color.a);
                font.draw("防禦+" + equip0.AddDefence.ToString(), 18, x + 90, y + 75, color, color.a);
                font.draw("輕功+" + equip0.AddSpeed.ToString(), 18, x + 90, y + 95, color, color.a);
            }

            x = x_ + 220;
            y = y_ + 445;
            font.draw("防具".PadRight(4), 25, x - 10, y, color,color.a);
            var equip1 = Save.getInstance().GetItem(role_.Equip1);
            if (equip1 != null)
            {
                TextureManager.getInstance().renderTexture("item", equip1.ID, x, y + 30);
                font.draw(GameUtil.EraseModredundantChar(equip1.strName), font_size, x + 90, y + 30, color, color.a);
                font.draw("攻擊+" + equip1.AddAttack.ToString(), 18, x + 90, y + 55, color, color.a);
                font.draw("防禦+" + equip1.AddDefence.ToString(), 18, x + 90, y + 75, color, color.a);
                font.draw("輕功+" + equip1.AddSpeed.ToString(), 18, x + 90, y + 95, color, color.a);
            }
        }

        public override void onPressedOK()
        {
            if (role_ == null) { return; }

            if (button_leave_.getState() == State.Press)
            {
                Event.GetInstance().CallLeaveEvent(role_);
                role_ = null;
                menuType = MenuType.leave;
            }
            else if (button_medcine_.getState() == State.Press)
            {
                team_menu_ = new TeamMenu();
                team_menu_.setText(GameUtil.EraseModredundantChar(role_.strName).PadLeft(7) + "要為誰醫療");
                team_menu_.run();
                button_medcine_.setState(State.Normal);
                menuType = MenuType.medcine;
            }
            else if (button_detoxification_.getState() == State.Press)
            {
                team_menu_ = new TeamMenu();
                team_menu_.setText(GameUtil.EraseModredundantChar(role_.strName).PadLeft(7) + "要為誰解毒");
                team_menu_.run();
                button_detoxification_.setState(State.Normal);
                menuType = MenuType.detoxification;
            }
        }


      






    }
}
