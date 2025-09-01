using SDL;
using kysSharp.Types;

namespace kysSharp
{
    class UIItem : Element
    {
        //这里注意，用来显示物品图片的按钮的纹理编号实际就是物品编号
        public Button[] item_buttons_;
        public TextBox? cursor_ = null;

        public int leftup_index_ = 0;  //左上角第一个物品在当前种类列表中的索引

        public const int item_each_line_ = 7;
        public const int line_count_ = 3;

        public MenuText? title_ = null;
        public int force_item_type_ = -1;
        public bool select_user_ = true;

        public Item? current_item_ = null;
        public List<Item> available_items_ = new List<Item>();
        public Button? current_button_ = null;

        private SDL_Color white;

        private TeamMenu team_menu_ = new TeamMenu();
        private Role role_ = new Role();
        private bool isDisposeTeamMenu = false;
        private ShowRoleDifference? showRoleDifference = null;
        private bool isShowRoleDifference = false;
        private bool isForceItem = false;

        public UIItem()
        {
            white = new SDL_Color { r=255, b=255, g=255, a=255 };

            item_buttons_ = new Button[line_count_ * item_each_line_];

            for (int i = 0; i < item_buttons_.Length; i++)
            {
                var b = new Button();
                item_buttons_[i] = b;
                b.setPosition(i % item_each_line_ * 85 + 40, i / item_each_line_ * 85 + 100);
                //b.setTexture("item", Save::getInstance().getItemByBagIndex(i).ID);
                addChild(b);
            }
            title_ = new MenuText();
            string[] tmpTitle = new string[9] { "劇情", "兵甲", "丹藥", "暗器", "拳經", "劍譜", "刀錄", "奇門", "心法" };
            title_.setStrings(tmpTitle.ToList());
            title_.setFontSize(24);
            title_.arrange(0, 50, 64, 0);
            addChild(title_);

            cursor_ = new TextBox();
            cursor_.setTexture("title", 127);
            cursor_.setVisible(false);
            addChild(cursor_);
        }




        public MenuText? getTitle() { return title_; }
        public void setSelectUser(bool s) { select_user_ = s; }
        public Item? getCurrentItem() { return current_item_; }

        public void setForceItemType(int f)
        {
            force_item_type_ = f;
            if (title_ == null)
                return;
            if (f >= 0)
            {
                title_.setAllChildVisible(false);
                title_.getChild(f).setVisible(true);
            }
            else
            {
                title_.setAllChildVisible(true);
            }
        }

        /// <summary>
        /// //原分类：0剧情，1装备，2秘笈，3药品，4暗器
        /// //详细分类："0劇情", "1兵甲", "2丹藥", "3暗器", "4拳經", "5劍譜", "6刀錄", "7奇門", "8心法"
        /// </summary>
        /// <param name="item">物品</param>
        /// <returns>物品类型</returns>
        public int getItemDetailType(Item item)
        {
            if (item == null) { return -1; }
            if (item.ItemType == 0)
            {
                return 0;
            }
            else if (item.ItemType == 1)
            {
                return 1;
            }
            else if (item.ItemType == 3)
            {
                return 2;
            }
            else if (item.ItemType == 4)
            {
                return 3;
            }
            else if (item.ItemType == 2)
            {
                var m = Save.getInstance().GetMagic(item.MagicID);
                if (m != null)
                {
                    //吸取内力类归为8
                    if (m.HurtType == 0)
                    {
                        return m.MagicType + 3;
                    }
                }
                return 8;
            }
            //未知的种类当成剧情
            return 0;
        }

        /// <summary>
        /// 根据类型获取物品
        /// </summary>
        /// <param name="item_type">物品类型</param>
        public void getItemsByType(int item_type)
        {
            if (item_type == 3)
            {

            }
            available_items_.Clear();
            for (int i = 0; i < Constant.ITEM_IN_BAG_COUNT; i++)
            {
                var item = Save.getInstance().GetItemByBagIndex(i);
                if (item == null)
                    return;
                if (getItemDetailType(item) == item_type)
                {
                    available_items_.Add(item);
                }
            }
        }

        /// <summary>
        /// 根据编号，获取可用的物品
        /// </summary>
        /// <param name="i">物品编号</param>
        /// <returns>可用的物品</returns>
        public Item? getAvailableItem(int i)
        {
            if (i >= 0 && i < available_items_.Count)
            {
                return available_items_[i];
            }
            return null;
        }

        public override void draw()
        {
            if (current_item_ == null)
                return;
            showItemProperty(current_item_);
        }

        public void showItemProperty(Item item)
        {
            if (item == null)
            {
                return;
            }
            if (current_item_ == null)
                return;
            //物品名和数量
            var str = (GameUtil.EraseModredundantChar(item.strName) + " " + Save.getInstance().GetItemCountInBag(current_item_.ID)).ToString();
            GameFont.getInstance().draw(str, 24, x_ + 10, y_ + 370, white);
            GameFont.getInstance().draw(GameUtil.EraseModredundantChar(item.strIntroduction), 20, x_ + 10, y_ + 400, white);

            int x = 10, y = 430;
            int size = 20;

            //以下显示物品的属性
            SDL_Color c = new SDL_Color() { r = 215, g = 0, b = 255, a = 255 };

            //特别判断罗盘
            if (item.isCompass() == true)
            {
                int man_x = 0, man_y = 0;
                //MainScene.getInstance().GetManPosition(ref man_x, ref man_y);
                string strCompass = "當前坐標 " + man_x.ToString() + " " + man_y.ToString();
                ShowOneProperty(1, strCompass, size, c, ref x, ref y);
            }

            //剧情物品不继续显示了
            if (item.ItemType == 0)
            {
                return;
            }

            //GameFont.GetInstance().Draw("效果：", size, x_ + x, y_ + y, c);
            //y += size + 10;

            ShowOneProperty(item.AddHP, "生命", size, c, ref x, ref y);
            ShowOneProperty(item.AddMaxHP, "生命上限", size, c, ref x, ref y);
            ShowOneProperty(item.AddMP, "內力", size, c, ref x, ref y);
            ShowOneProperty(item.AddMaxMP, "內力上限", size, c, ref x, ref y);
            ShowOneProperty(item.AddPhysicalPower, "體力", size, c, ref x, ref y);
            ShowOneProperty(item.AddPoison, "中毒", size, c, ref x, ref y);

            ShowOneProperty(item.AddAttack, "攻擊", size, c, ref x, ref y);
            ShowOneProperty(item.AddSpeed, "輕功", size, c, ref x, ref y);
            ShowOneProperty(item.AddDefence, "防禦", size, c, ref x, ref y);

            ShowOneProperty(item.AddMedcine, "醫療", size, c, ref x, ref y);
            ShowOneProperty(item.AddUsePoison, "用毒", size, c, ref x, ref y);
            ShowOneProperty(item.AddDetoxification, "解毒", size, c, ref x, ref y);
            ShowOneProperty(item.AddAntiPoison, "抗毒", size, c, ref x, ref y);

            ShowOneProperty(item.AddFist, "拳掌", size, c, ref x, ref y);
            ShowOneProperty(item.AddSword, "御劍", size, c, ref x, ref y);
            ShowOneProperty(item.AddKnife, "耍刀", size, c, ref x, ref y);
            ShowOneProperty(item.AddUnusual, "特殊兵器", size, c, ref x, ref y);
            ShowOneProperty(item.AddHiddenWeapon, "暗器", size, c, ref x, ref y);

            ShowOneProperty(item.AddKnowledge, "作弊", size, c, ref x, ref y);
            ShowOneProperty(item.AddMorality, "道德", size, c, ref x, ref y);
            ShowOneProperty(item.AddAttackWithPoison, "攻擊帶毒", size, c, ref x, ref y);

            if (item.ChangeMPType == 2)
            {
                ShowOneProperty(2, "內力調和", size, c, ref x, ref y);
            }

            if (item.AddAttackTwice == 1)
            {
                ShowOneProperty(1, "雙擊", size, c, ref x, ref y);
            }

            var magic = Save.getInstance().GetMagic(item.MagicID);
            if (magic != null)
            {
                string strTmpMagic = "習得武學" + magic.strName;
                ShowOneProperty(1, str, size, c, ref x, ref y);
            }

            //以下显示物品需求

            //药品和暗器类不继续显示了
            if (item.ItemType == 3 || item.ItemType == 4)
            {
                return;
            }

            x = 10;
            y += size + 10;  //换行
            c=new SDL_Color() { r = 170, g = 255, b = 255, a = 254 };
            //Font::getInstance().draw("需求：", size, x_ + ref x, ref y_ + y, c);
            //y += size + 10;
            var role = Save.getInstance().GetRole(item.OnlySuitableRole);
            if (role != null)
            {
                string strTmpRoll = "僅適合" + role.strName;
                ShowOneProperty(1, str, size, c, ref x, ref y);
                return;
            }

            ShowOneProperty(item.NeedMP, "內力", size, c, ref x, ref y);
            ShowOneProperty(item.NeedAttack, "攻擊", size, c, ref x, ref y);
            ShowOneProperty(item.NeedSpeed, "輕功", size, c, ref x, ref y);

            ShowOneProperty(item.NeedMedcine, "醫療", size, c, ref x, ref y);
            ShowOneProperty(item.NeedUsePoison, "用毒", size, c, ref x, ref y);
            ShowOneProperty(item.NeedDetoxification, "解毒", size, c, ref x, ref y);

            ShowOneProperty(item.NeedFist, "拳掌", size, c, ref x, ref y);
            ShowOneProperty(item.NeedSword, "御劍", size, c, ref x, ref y);
            ShowOneProperty(item.NeedKnife, "耍刀", size, c, ref x, ref y);
            ShowOneProperty(item.NeedUnusual, "特殊兵器", size, c, ref x, ref y);
            ShowOneProperty(item.NeedHiddenWeapon, "暗器", size, c, ref x, ref y);

            ShowOneProperty(item.NeedIQ, "資質", size, c, ref x, ref y);

            ShowOneProperty(item.NeedExp, "基礎經驗", size, c, ref x, ref y);
        }

        public void ShowOneProperty(int v, string format_str, int size, SDL_Color c, ref int x, ref int y)
        {
            if (v != 0)
            {
                string str = format_str + " " + v.ToString();
                //测试是不是出界了
                int draw_length = size * str.Length / 2 + size;
                int x1 = x + draw_length;
                if (x1 > 700)
                {
                    x = 10;
                    y += size + 5;
                }
                GameFont.getInstance().draw(str, size, x_ + x, y_ + y, c);
                x += draw_length;
            }
        }

        /*
        public void DealMouseMoveEvent(object sender, MouseEventArgs e)
        {
            //强制停留在某类物品
            if (force_item_type_ >= 0) { title_.SetResult(force_item_type_); }

            GetItemsByType(title_.GetPassChild());
            int type_item_count = available_items_.Count;

            //从这里计算出左上角可以取的最大值
            //计算方法：先计算出总行数，减去可见行数，乘以每行成员数
            int max_leftup = ((type_item_count + item_each_line_ - 1) / item_each_line_ - line_count_) * item_each_line_;
            if (max_leftup < 0) { max_leftup = 0; }



            if (e.type == BP_MOUSEWHEEL)
            {
                if (e.wheel.y > 0)
                {
                    leftup_index_ -= item_each_line_;
                }
                else if (e.wheel.y < 0)
                {
                    leftup_index_ += item_each_line_;
                }
            }
            leftup_index_ = GameUtil::limit(leftup_index_, 0, max_leftup);


            if(isForceItem==false)
            {
                //计算当前指向的物品
                for (int i = 0; i < item_buttons_.Length; i++)
                {
                    var button = item_buttons_[i];
                    int index = i + leftup_index_;
                    var item = GetAvailableItem(index);
                    if (item != null)
                    {
                        button.SetTexture("item", item.ID);

                        button.DealMouseMove(sender, e);
                        if (e.LeftButton == MouseButtonState.Pressed)
                        {
                            button.DealMouseLeftButtonDown(sender, e);
                        }

                        if (button.GetState() == State.Pass || button.GetState() == State.Press)
                        {
                            current_item_ = item;
                            current_button_ = button;
                            //result_ = current_item_.ID;       原始程序注释
                        }

                        if (button.GetState() == State.Press)
                        {
                            current_item_ = item;
                            current_button_ = button;
                            OnPressedOK();
                        }
                    }
                    else
                    {
                        button.SetTexture("item", -1);
                    }
                }

                //让光标显示出来
                if (current_button_ != null)
                {
                    int x = 0, y = 0;
                    current_button_.GetPosition(ref x, ref y);
                    cursor_.SetPosition(x, y);
                    cursor_.SetVisible(true);
                }
                else
                {
                    cursor_.SetVisible(false);
                }
            }
            
        }*/

        public override void onPressedOK()
        {
            if (title_ == null)
                return;

            //强制停留在某类物品
            if (force_item_type_ >= 0) { title_.setResult(force_item_type_); }

            if (current_item_ == null) { return; }

            //在使用剧情物品的时候，返回一个结果，主UI判断此时可以退出
            if (current_item_.ItemType == 0)
            {
                result_ = current_item_.ID;
            }

            if (select_user_)
            {
                if (current_item_.ItemType == 3)
                {
                    team_menu_ = new TeamMenu();
                    team_menu_.SetItem(current_item_);
                    team_menu_.setText("誰要使用" + GameUtil.EraseModredundantChar(current_item_.strName));
                    team_menu_.run();

                    /*
                    role_ = null;
                    role_ = team_menu_.GetRole();
                    
                    if(role_!=null)
                    {
                        isDisposeTeamMenu = true;
                    }
                    */
                    
                }
                
                else if (current_item_.ItemType == 1 || current_item_.ItemType == 2)
                {
                    team_menu_ = new TeamMenu();
                    team_menu_.SetItem(current_item_);
                    var format_str = "誰要修煉";
                    if (current_item_.ItemType == 1) { format_str = "誰要裝備"; }
                    team_menu_.setText(format_str + GameUtil.EraseModredundantChar(current_item_.strName));
                    team_menu_.run();
                    var role = team_menu_.GetRole();
                    if (role == null)
                        return;
                    if (role.strName != null)
                    {
                        GameUtil.Equip(ref role, ref current_item_);
                    }

                    isDisposeTeamMenu = true;
                }
                else if (current_item_.ItemType == 4)
                {
                    //似乎不需要特殊处理
                }
                
            }
            setExit(true);   //用于战斗时。平时物品栏不是以根节点运行，设置这个没有作用
        }

        public override void onPressedCancel()
        {
            current_item_ = null;
            exitWithResult(-1);
        }











       


    }
}
