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

            //头像
            TextureManager.getInstance().renderTexture("head", role_.HeadID, x_ + 10, y_ + 20);



            //颜色设定
            SDL_Color color_white = new() { r=255, g=255, b=255, a=255 };
            SDL_Color color_name = new() { r = 255, g = 215, b = 0, a = 255 };
            SDL_Color color_ability1 = new() { r = 255, g = 250, b = 205, a = 255 };
            SDL_Color color_ability2 = new() { r = 236, g = 200, b = 40, a = 255 };
            SDL_Color color_red = new() { r = 255, g = 90, b = 60, a = 255 };
            SDL_Color color_magic = new() { r = 236, g = 200, b = 40, a = 255 };
            SDL_Color color_magic_level1 = new() { r = 253, g = 101, b = 101, a = 255 };
            SDL_Color color_purple = new() { r = 208, g = 152, b = 208, a = 255 };
            SDL_Color color_magic_empty = new() { r = 236, g = 200, b = 40, a = 255 };
            SDL_Color color_equip = new() { r = 162, g = 28, b = 218, a = 255 };

            /////////////////////////////////////////////////////////////////////////
            // 函数对象：select_color1
            // 功能描述：根据数值 v 相对于最大值 max_v 的比例，选择不同颜色。
            // 参数说明：
            //   int v      —— 当前数值。
            //   int max_v  —— 最大参考值。
            // 返回值：
            //   SDL_Color —— SDL3 的颜色结构体，包含 RGBA 分量。
            // 判断逻辑：
            //   1. v >= max_v * 0.9 → 红色 (color_red)
            //   2. v >= max_v * 0.8 → 橙色 (255,165,79,255)
            //   3. v >= max_v * 0.7 → 黄色 (255,255,50,255)
            //   4. v < 0            → 紫色 (color_purple)
            //   5. 其他情况         → 白色 (color_white)
            // 应用场景：
            //   - 数据可视化（热力图、进度条、数值提示）
            //   - UI 警告（接近最大值时标红）
            //   - 统计显示（分段颜色标识）
            /////////////////////////////////////////////////////////////////////////
            Func<int, int, SDL_Color> select_color1 = (v, max_v) =>
            {
                if (v >= max_v * 9 / 10)
                {
                    return color_red;
                }
                else if (v >= max_v * 8 / 10)
                {
                    return new SDL_Color { r = 255, g = 165, b = 79, a = 255 };
                }
                else if (v >= max_v * 7 / 10)
                {
                    return new SDL_Color { r = 255, g = 255, b = 50, a = 255 };
                }
                else if (v < 0)
                {
                    return color_purple;
                }
                return color_white;
            };

            /////////////////////////////////////////////////////////////////////////
            // 函数对象：select_color2
            // 功能描述：根据数值 v 的正负性，选择不同颜色。
            // 参数说明：
            //   int v —— 当前数值。
            // 返回值：
            //   SDL_Color —— SDL3 的颜色结构体。
            // 判断逻辑：
            //   v > 0 → 红色 (color_red)
            //   v < 0 → 紫色 (color_purple)
            //   v = 0 → 白色 (color_white)
            // 应用场景：
            //   - 三值逻辑（正/负/零）颜色显示
            //   - UI 状态提示（增益/损失/中性）
            //   - 统计结果符号显示
            /////////////////////////////////////////////////////////////////////////
            Func<int, SDL_Color> select_color2 = (v) =>
            {
                if (v > 0) return color_red;
                if (v < 0) return color_purple;
                return color_white;
            };

            var font = GameFont.getInstance();
            int font_size = 22;
            int x, y;


            x = x_ + 200;
            y = y_ + 50;
            font.draw(GameUtil.EraseModredundantChar(role_.strName).PadRight(6), 30, x - 10, y, color_name);
            font.draw("等級".PadRight(7), font_size, x, y + 50, color_ability1);
            font.draw(GameUtil.EraseModredundantChar(role_.Level.ToString().PadLeft(5)), font_size, x + 66, y + 50, color_white);
            font.draw("經驗".PadRight(7), font_size, x, y + 75, color_ability1);
            font.draw(GameUtil.EraseModredundantChar(role_.Exp.ToString().PadLeft(5)), font_size, x + 66, y + 75, color_white);

            str = "升級 ------";
            exp_up = GameUtil.GetLevelUpExp(role_.Level);
            if (exp_up != int.MaxValue)
            {
                str = exp_up.ToString();
            }
            font.draw("升級".PadRight(7), font_size, x, y + 100, color_ability1);
            font.draw(str.PadLeft(5), font_size, x + 66, y + 100, color_white);




            font.draw("生命".PadRight(7), font_size, x+175, y + 50, color_ability1);
            font.draw((role_.HP.ToString() + "/" + role_.MaxHP.ToString()).PadLeft(9), font_size, x + 241, y + 50, color_white);

            SDL_Color c = color_white;
            if (role_.MPType == 0)
            {
                c = color_purple;
            }
            else if (role_.MPType == 1)
            {
                c = color_magic;
            }
            font.draw("邏輯".PadRight(7), font_size, x + 175, y + 75, color_ability1);
            font.draw((role_.MP.ToString() + "/" + role_.MaxMP.ToString()).PadLeft(9), font_size, x + 241, y + 75, c);
            font.draw("體力".PadRight(7), font_size, x + 175, y + 100, color_ability1);
            font.draw((role_.PhysicalPower.ToString() + "/" + 100.ToString()).PadLeft(9), font_size, x + 241, y + 100, color_white);



            x = x_ + 20;
            y = y_ + 200;

            font.draw("攻擊", font_size, x, y, color_ability1);
            font.draw(role_.Attack.ToString().PadLeft(5), font_size, x+50, y, 
                select_color1(role_.Attack, Constant.MAX_ATTACK));
            font.draw("防禦", font_size, x + 200, y, color_ability1);
            font.draw(role_.Defence.ToString().PadLeft(5), font_size, x + 250, y,
                select_color1(role_.Defence, Constant.MAX_DEFENCE));
            font.draw("輕功", font_size, x + 400, y, color_ability1);
            font.draw(role_.Speed.ToString().PadLeft(5), font_size, x + 450, y, select_color1(role_.Speed, Constant.MAX_SPEED));

            font.draw("醫療", font_size, x, y + 25, color_ability1);
            font.draw(role_.Medcine.ToString().PadLeft(5), font_size, x+50, y + 25, select_color1(role_.Medcine, Constant.MAX_MEDCINE));
            font.draw("解毒", font_size, x + 200, y + 25, color_ability1);
            font.draw(role_.Detoxification.ToString().PadLeft(5), font_size, x + 250, y + 25, select_color1(role_.Detoxification, Constant.MAX_DETOXIFICATION));
            font.draw("用毒", font_size, x + 400, y + 25, color_ability1);
            font.draw(role_.UsePoison.ToString().PadLeft(5), font_size, x + 450, y + 25, select_color1(role_.UsePoison, Constant.MAX_USE_POISON));

            x = x_ + 20;
            y = y_ + 270;
            font.draw("技能".PadRight(4), 25, x - 10, y, color_name);

            font.draw("數學", font_size, x, y + 30, color_ability1);
            font.draw(role_.Fist.ToString().PadLeft(8), font_size, x+20, y + 30, select_color1(role_.Fist, Constant.MAX_FIST));
            font.draw("物理", font_size, x, y + 55, color_ability1);
            font.draw(role_.Sword.ToString().PadLeft(8), font_size, x+20, y + 55, select_color1(role_.Sword, Constant.MAX_SWORD));
            font.draw("化學", font_size, x, y + 80, color_ability1);
            font.draw(role_.Knife.ToString().PadLeft(8), font_size, x+20, y + 80, select_color1(role_.Knife, Constant.MAX_KNIFE));
            font.draw("算法", font_size, x, y + 105, color_ability1);
            font.draw(role_.Unusual.ToString().PadLeft(8), font_size, x+20, y + 105, select_color1(role_.Unusual, Constant.MAX_UNUSUAL));
            font.draw("生物", font_size, x, y + 130, color_ability1);
            font.draw(role_.HiddenWeapon.ToString().PadLeft(8), font_size, x+20, y + 130, select_color1(role_.HiddenWeapon, Constant.MAX_HIDDEN_WEAPON));

            x = x_ + 220;
            y = y_ + 270;
            font.draw("知識".PadRight(4), 25, x - 10, y, color_name);
            for (int i = 0; i < 10; i++)
            {
                var magic = Save.getInstance().GetRoleLearnedMagic(ref role_, i);
                str = "__________";
                if (magic != null)
                {
                    int x1 = x + i % 2 * 200;
                    int y1 = y + 30 + i / 2 * 25;

                    str = GameUtil.EraseModredundantChar(magic.strName).PadRight(50);
                    font.draw(str, font_size, x1, y1, color_ability1);
                    str = GameUtil.EraseModredundantChar(role_.GetRoleShowLearnedMagicLevel(i).ToString().PadLeft(3));
                    font.draw(str, font_size, x1+100, y1, role_.GetRoleShowLearnedMagicLevel(i) > 9 ? color_red : color_purple);
                }
                else
                {

                    int x1 = x + i % 2 * 200;
                    int y1 = y + 30 + i / 2 * 25;
                    font.draw(str, font_size, x1, y1, color_ability1);
                }
            }

            x = x_ + 420;
            y = y_ + 445;
            font.draw("修煉".PadRight(4), 25, x - 10, y, color_name);
            var book = Save.getInstance().GetItem(role_.PracticeItem);
            if (book != null)
            {
                TextureManager.getInstance().renderTexture("item", book.ID, x, y + 30);
                font.draw(GameUtil.EraseModredundantChar(book.strName), font_size, x + 90, y + 30, color_name);
                font.draw("經驗" + role_.ExpForItem.ToString().PadLeft(5), 18, x + 90, y + 55, color_ability1);
                str = "升級 ----";
                exp_up = GameUtil.GetFinishedExpForItem(ref role_, ref book);
                if (exp_up != int.MaxValue)
                {
                    str = "升級" + exp_up.ToString().PadLeft(5);
                }
                font.draw(str, 18, x + 90, y + 75, color_ability1);
            }

            x = x_ + 20;
            y = y_ + 445;
            font.draw("儀器".PadRight(4), 25, x - 10, y, color_name);
            var equip0 = Save.getInstance().GetItem(role_.Equip0);
            if (equip0 != null)
            {
                TextureManager.getInstance().renderTexture("item", equip0.ID, x, y + 30);
                font.draw(GameUtil.EraseModredundantChar(equip0.strName), font_size, x + 90, y + 30, color_name);
                font.draw("攻擊+" + equip0.AddAttack.ToString(), 18, x + 90, y + 55, select_color2(equip0.AddAttack));
                font.draw("防禦+" + equip0.AddDefence.ToString(), 18, x + 90, y + 75, select_color2(equip0.AddDefence));
                font.draw("輕功+" + equip0.AddSpeed.ToString(), 18, x + 90, y + 95, select_color2(equip0.AddSpeed));
            }

            x = x_ + 220;
            y = y_ + 445;
            font.draw("防具".PadRight(4), 25, x - 10, y, color_name);
            var equip1 = Save.getInstance().GetItem(role_.Equip1);
            if (equip1 != null)
            {
                TextureManager.getInstance().renderTexture("item", equip1.ID, x, y + 30);
                font.draw(GameUtil.EraseModredundantChar(equip1.strName), font_size, x + 90, y + 30, color_name);
                font.draw("攻擊+" + equip1.AddAttack.ToString(), 18, x + 90, y + 55, select_color2(equip1.AddAttack));
                font.draw("防禦+" + equip1.AddDefence.ToString(), 18, x + 90, y + 75, select_color2(equip1.AddDefence));
                font.draw("輕功+" + equip1.AddSpeed.ToString(), 18, x + 90, y + 95, select_color2(equip1.AddSpeed));
            }
        }
        
        public override void onPressedOK()
        {
            if (role_ == null) { return; }

            if (button_leave_.getState() == State.Press)
            {
                Event.getInstance().CallLeaveEvent(role_);
                role_ = null;
            }
            else if (button_medcine_.getState() == State.Press)
            {
                var teamMenu = new TeamMenu();
                teamMenu.setText(string.Format("{0}要為誰醫療", GameUtil.EraseModredundantChar(role_.strName)));
                teamMenu.run();
                var role = teamMenu.GetRole();
                if (role != null)
                {
                    var r = (Role)role;
                    GameUtil.Medicine(ref role_, ref role);
                    var df = new ShowRoleDifference(r, role);
                    df.setText(string.Format("{0}接受{1}醫療", GameUtil.EraseModredundantChar(role.strName), GameUtil.EraseModredundantChar(role_.strName)));
                    df.run();
                }
            }
            else if (button_detoxification_.getState() == State.Press)
            {
                var teamMenu = new TeamMenu();
                teamMenu.setText(string.Format("{0}要為誰解毒", GameUtil.EraseModredundantChar(role_.strName)));
                teamMenu.run();
                var role = teamMenu.GetRole();
                if (role != null)
                {
                    var r = (Role)role;
                    GameUtil.Detoxification(ref role_, ref role);
                    var df = new ShowRoleDifference(r, role);
                    df.setText(string.Format("{0}接受{1}解毒", GameUtil.EraseModredundantChar(role.strName), GameUtil.EraseModredundantChar(role_.strName)));
                    df.run();
                }
            }
        }
      






    }
}
