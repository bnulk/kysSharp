using SDL;
using kysSharp.Types;

namespace kysSharp
{
    public class UIItem : Element
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
                string strTmpMagic = "習得武學" + GameUtil.EraseModredundantChar(magic.strName);
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
                string strTmpRoll = "僅適合" + GameUtil.EraseModredundantChar(role.strName);
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
                //int draw_length = size * str.Length / 2 + size;
                int draw_length = size * str.Length + size;
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

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // UIItem.dealEvent(BP_Event e)
        // 说明：处理物品栏事件，包括鼠标滚轮滚动、物品刷新、光标显示等。
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public override void dealEvent(SDL_Event e)
        {
            /////////////////////////////////////////////////////////////////////////////////
            // 1. 如果强制显示某一类物品（如仅显示药品），则设置类型选择结果
            /////////////////////////////////////////////////////////////////////////////////
            if (force_item_type_ >= 0)
            {
                title_?.setResult(force_item_type_);
            }

            /////////////////////////////////////////////////////////////////////////////////
            // 2. 按类别筛选物品
            /////////////////////////////////////////////////////////////////////////////////
            int type_item_count = 0;
            if (title_!=null)
            {
                getItemsByType(title_.getPassChild());
                type_item_count = available_items_.Count;
            }

            /////////////////////////////////////////////////////////////////////////////////
            // 3. 计算左上角可显示的最大索引位置
            //    算法： (总行数 - 可见行数) * 每行物品数
            /////////////////////////////////////////////////////////////////////////////////
            int total_rows = (type_item_count + item_each_line_ - 1) / item_each_line_;
            int max_leftup = (total_rows - line_count_) * item_each_line_;
            if (max_leftup < 0) max_leftup = 0;

            /////////////////////////////////////////////////////////////////////////////////
            // 4. 鼠标滚轮事件
            /////////////////////////////////////////////////////////////////////////////////
            if (e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
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

            // 限制索引范围
            leftup_index_ = GameUtil.limit(leftup_index_, 0, max_leftup);

            /////////////////////////////////////////////////////////////////////////////////
            // 5. 更新每个物品按钮的显示状态
            /////////////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < item_buttons_.Length; i++)
            {
                var button = item_buttons_[i];
                int index = i + leftup_index_;
                var item = getAvailableItem(index);

                if (item != null)
                {
                    button.setTexture("item", item.ID);

                    if (button.getState() == State.Pass || button.getState() == State.Press)
                    {
                        current_item_ = item;
                        current_button_ = button;
                    }
                }
                else
                {
                    button.setTexture("item", -1);
                }
            }

            /////////////////////////////////////////////////////////////////////////////////
            // 6. 更新光标显示位置
            /////////////////////////////////////////////////////////////////////////////////
            if (current_button_ != null)
            {
                int x = 0, y = 0;
                current_button_.getPosition(ref x, ref y);
                cursor_.setPosition(x, y);
                cursor_.setVisible(true);
            }
            else
            {
                cursor_.setVisible(false);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数名称：OnPressedOK
        // 函数功能：处理物品菜单中按下“确定”键后的逻辑
        // 翻译来源：C++ void UIItem::onPressedOK()
        // 主要功能：
        //   1. 检查当前按下的物品按钮
        //   2. 判断所选物品类型，执行不同逻辑（使用、装备、修炼等）
        //   3. 可能调用 TeamMenu、ShowRoleDifference 等界面
        //   4. 设置退出标志
        /////////////////////////////////////////////////////////////////////////
        public override void onPressedOK()
        {
            /////////////////////////////////////////////////////////////////////////
            // 【步骤1】初始化当前选中物品为空
            /////////////////////////////////////////////////////////////////////////
            current_item_ = null;

            /////////////////////////////////////////////////////////////////////////
            // 【步骤2】遍历所有物品按钮，检查哪个按钮处于 Press 状态
            // 如果发现被按下的按钮，则通过索引计算出对应的物品对象
            /////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < item_buttons_.Length; i++)
            {
                var button = item_buttons_[i];
                if (button.getState() == State.Press)
                {
                    var item = getAvailableItem(i + leftup_index_);
                    current_item_ = item;
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 【步骤3】若没有选中任何物品，则直接返回，不进行后续处理
            /////////////////////////////////////////////////////////////////////////
            if (current_item_ == null)
                return;

            /////////////////////////////////////////////////////////////////////////
            // 【步骤4】若物品类型为0（剧情物品），则将物品ID作为结果值返回
            // 此结果可能被主UI用于判断剧情触发
            /////////////////////////////////////////////////////////////////////////
            if (current_item_.ItemType == 0)
            {
                result_ = current_item_.ID;
            }

            /////////////////////////////////////////////////////////////////////////
            // 【步骤5】若当前操作要求选择使用者（例如药品、装备、修炼等）
            /////////////////////////////////////////////////////////////////////////
            if (select_user_)
            {
                /////////////////////////////////////////////////////////////////////
                // 【分支1】ItemType == 3 ：药品或可使用类物品
                // 逻辑：弹出队伍选择菜单 -> 选择角色 -> 使用物品 -> 显示效果
                /////////////////////////////////////////////////////////////////////
                if (current_item_.ItemType == 3)
                {
                    var teamMenu = new TeamMenu();
                    teamMenu.SetItem(current_item_);
                    teamMenu.setText($"谁要使用 {GameUtil.EraseModredundantChar(current_item_.strName)}");
                    teamMenu.run();

                    var role = teamMenu.GetRole();
                    if (role != null)
                    {
                        // 复制角色状态以对比使用前后差异
                        var r = role.Clone();

                        // 执行物品使用逻辑
                        GameUtil.UseItem(ref role, ref current_item_);

                        // 显示角色能力变化界面
                        var df = new ShowRoleDifference(r, role);
                        df.setText($"{GameUtil.EraseModredundantChar(role.strName)} 服用 {GameUtil.EraseModredundantChar(current_item_.strName)}");
                        df.run();

                        // 减少该物品数量（无提示）
                        Event.getInstance().AddItemWithoutHint(current_item_.ID, -1);
                    }
                }

                /////////////////////////////////////////////////////////////////////
                // 【分支2】ItemType == 1 或 2 ：装备类或修炼类物品
                // 逻辑：弹出队伍选择菜单 -> 选择角色 -> 装备或修炼
                /////////////////////////////////////////////////////////////////////
                else if (current_item_.ItemType == 1 || current_item_.ItemType == 2)
                {
                    var teamMenu = new TeamMenu();
                    teamMenu.SetItem(current_item_);

                    string formatStr = current_item_.ItemType == 1 ? "谁要装备 {0}" : "谁要修炼 {0}";
                    teamMenu.setText(string.Format(formatStr, GameUtil.EraseModredundantChar(current_item_.strName)));
                    teamMenu.run();

                    var role = teamMenu.GetRole();
                    if (role != null)
                    {
                        GameUtil.Equip(ref role, ref current_item_);
                    }
                }

                /////////////////////////////////////////////////////////////////////
                // 【分支3】ItemType == 4 ：可能为特殊物品，此处暂不处理
                /////////////////////////////////////////////////////////////////////
                else if (current_item_.ItemType == 4)
                {
                    // 似乎不需要特殊处理
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 【步骤6】设置退出标志，用于战斗时的物品使用逻辑
            // 注意：平时物品栏并非根节点运行，因此该标志可能无效
            /////////////////////////////////////////////////////////////////////////
            setExit(true);

            exitWithResult(0);
        }

        public override void onPressedCancel()
        {
            current_item_ = null;
            exitWithResult(-1);
        }











       


    }
}
