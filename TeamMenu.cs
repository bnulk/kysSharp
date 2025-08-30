using kysSharp.Types;
using SDL;

namespace kysSharp
{
    class TeamMenu : Menu
    {
        private List<Head> heads_ = new List<Head>();
        //std::set<int> selected_;
        private Role? role_ = new Role();
        private Item item_ = new Item();
        private int mode_ = 0;   //为0是单选，为1是多选

        private Button button_all_;
        private Button button_ok_;


        public TeamMenu()
        {
            for (int i = 0; i < Constant.TEAMMATE_COUNT; i++)
            {
                var h = new Head();
                h.setHaveBox(false);
                heads_.Add(h);

                addChild(h, i % 2 * 250, i / 2 * 100);

                //h->setOnlyHead(true);

                //selected_.push_back(0);
            }
            button_all_ = new Button();
            button_all_.setText("全選");
            button_ok_ = new Button();
            button_ok_.setText("確定");
            addChild(button_all_, 50, 300);
            addChild(button_ok_, 150, 300);
            setPosition(200, 150);
            setTextPosition(20, -30);
        }

        public void SetItem(Item item) { item_ = item; }
        public void SetMode(int m) { mode_ = m; }

        public override void onEntrance()
        {
            for (int i = 0; i < 1; i++)
            {
                var r = Save.getInstance().GetTeamMate(i);

                if (r != null)
                {
                    heads_[i].SetRole(ref r);
                    if (mode_ == 0 && item_.strName != null)
                    {
                        if (!GameUtil.CanUseItem(r, item_))
                        {
                            heads_[i].setText("不適合");

                        }
                        if (r.PracticeItem == item_.ID || r.Equip0 == item_.ID || r.Equip1 == item_.ID)
                        {
                            heads_[i].setText("使用中");
                        }
                    }
                }
            }
            if (mode_ == 0)
            {
                button_all_.setVisible(false);
                button_ok_.setVisible(false);
            }
        }

        public Role? GetRole()
        {
            return role_;
        }

        public List<Role> GetRoles()
        {
            List<Role> roles = new List<Role>();
            foreach (var h in heads_)
            {
                if (h.getResult() == 0)
                {
                    var role = h.GetRole();
                    if (role != null)
                    {
                        roles.Add(role);
                    }
                }
            }
            return roles;
        }

        public override void draw()
        {
            //Engine.GetInstance().FillColor(Color.FromArgb(128,0, 0, 0), 0, 0, -1, -1);
            //GameTextBox gameTextBox = this as TeamMenu;
            //gameTextBox.Draw();

            base.draw();
        }

        public override void onPressedOK()
        {
            if (mode_ == 0)
            {
                role_ = null;
                foreach (var h in heads_)
                {
                    if (h.getState() == State.Press)
                    {
                        role_ = h.GetRole();
                    }
                }
                if (role_ != null)
                {
                    result_ = 0;
                }
            }
            if (mode_ == 1)
            {
                foreach (var h in heads_)
                {
                    if (h.getState() == State.Press)
                    {
                        if (h.getResult() == -1)
                        {
                            h.setResult(0);
                        }
                        else
                        {
                            h.setResult(-1);
                        }
                    }
                }
                if (button_all_.getState() == State.Press)
                {
                    //如果已经全选，则是清除
                    int all = -1;
                    foreach (var h in heads_)
                    {
                        if (h.getResult() != 0)
                        {
                            all = 0;
                            break;
                        }
                    }
                    foreach (var h in heads_)
                    {
                        h.setResult(all);
                    }
                }
                if (button_ok_.getState() == State.Press)
                {
                    //没有人被选中，不能确定
                    foreach (var h in heads_)
                    {
                        if (h.getResult() == 0)
                        {
                            setExit(true);
                        }
                    }
                }
            }
        }

        public override void onPressedCancel()
        {
            if (mode_ == 0)
            {
                role_ = null;
                result_ = -1;
                setExit(true);
            }
        }

        public override void dealEvent(SDL_Event e)
        {
            if (mode_ == 0)
            {
                if (item_ != null)
                {
                    foreach (var h in heads_)
                    {
                        if (h.getState() != State.Normal && !GameUtil.CanUseItem(h.GetRole(), item_))
                        {
                            h.setState(State.Normal);
                        }
                    }
                }
            }
            if (mode_ == 1)
            {
                foreach (var h in heads_)
                {
                    if (h.getResult() == 0)
                    {
                        h.setText("已選中");
                    }
                    else
                    {
                        h.setText("");
                    }
                }
            }
        }

       

    }
}
