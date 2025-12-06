using kysSharp.Types;
using SDL;

namespace kysSharp
{
    class UI : Element
    {
        public static UI ui_ = new UI();
        private int current_head_ = 0;
        private int current_button_ = 0;

        public List<Button> buttons_ = new List<Button>();
        public List<Head> heads_ = new List<Head>();

        public Button button_status_, button_item_, button_system_;
        public UIStatus? ui_status_ = null;
        public UIItem? ui_item_ = null;
        public UISystem? ui_system_ = null;
        int item_id_ = -1;


        public static UI getInstance()
        {
            if(ui_== null)
                ui_= new UI();
            return ui_; 
        }

        public UI()
        {
            full_window_ = true;

            //注意，此处约定childs_[0]为子UI，创建好对应的指针，需要显示哪个赋值到childs_[0]即可
            ui_status_ = new UIStatus();
            ui_item_ = new UIItem();
            ui_system_ = new UISystem();
            ui_status_.setPosition(350, 0);
            ui_item_.setPosition(350, 0);
            ui_system_.setPosition(300, 0);
            addChild(ui_status_);

            //貌似这里不能直接调用其他单例，静态量的创建顺序不确定
            button_status_ = new Button("title", 122);
            button_item_ = new Button("title", 124);
            button_system_ = new Button("title", 125);
            addChild(button_status_, 10, 10);
            addChild(button_item_, 90, 10);
            addChild(button_system_, 170, 10);
            buttons_.Add(button_status_);
            buttons_.Add(button_item_);
            buttons_.Add(button_system_);

            for (int i = 0; i < Constant.TEAMMATE_COUNT; i++)
            {
                var h = new Head();
                addChild(h, 20, 60 + i * 90);
                heads_.Add(h);
            }
            heads_[0].setState(State.Pass);
            result_ = -1; //非负：物品id，负数：其他情况，再定
        }


        public Item? GetUsedItem() 
        {
            if(ui_item_==null)
                return null;
            return ui_item_.getCurrentItem(); 
        }

        public override void draw()
        {
            Engine.getInstance().fillColor(new SDL_Color() { r=0,g=0,b=0,a=192}, 0, 0, -1, -1);
        }

        public override void onEntrance()
        {
            
        }

        public void reSetHeads()
        {
            
        }

        /// <summary>
        /// 显示头像
        /// </summary>
        public override void dealEvent(SDL_Event e)
        {
            for (int i = 0; i < Constant.TEAMMATE_COUNT; i++)
            {
                var head = heads_[i];
                var role = Save.getInstance().GetTeamMate(i);
                head.SetRole(ref role);
                
                if (head.getState() == State.Pass)
                {
                    if(ui_status_==null) continue;
                    ui_status_.SetRole(role);
                    current_head_ = i;
                }

                //如在物品栏则判断是否在使用，或者可以使用
                if (childs_[0] == ui_item_)
                {
                    Item? item = null;
                    if (ui_item_==null)
                    {
                        continue;
                    }
                    else
                        item = ui_item_.getCurrentItem();
                    if (item!=null)
                    {
                        if (role.Equip0 == item.ID || role.Equip1 == item.ID || role.PracticeItem == item.ID)
                        {
                            head.setText("使用中");
                            //Font::getInstance()->draw("使用中", 25, x + 5, y + 60, { 255,255,255,255 });
                        }
                        else
                        {
                            head.setText("");
                        }
                        if (GameUtil.CanUseItem(role, item))
                        {
                            head.setState(State.Pass);
                        }
                    }
                }
            }
            //这里设定当前头像为Pass，令其不变暗，因为检测事件是先检测子节点，所以这里可以生效
            if (childs_[0] == ui_status_)
            {
                heads_[current_head_].setState(State.Pass);
            }
            buttons_[current_button_].setState(State.Pass);
        }

        public override void onPressedOK()
        {
            //这里检测是否使用了物品，返回物品的id
            if (childs_[0] == ui_item_)
            {
                var item = ui_item_.getCurrentItem();
                if (item != null && item.ItemType == 0)
                {
                    setExit(true);
                }
            }

            
            if (childs_[0] == ui_system_)
            {
                if (ui_system_.getResult() == 0)
                {
                    setExit(true);
                }
            }
            

            //四个按钮的响应
            if (button_status_.getState() == State.Press)
            {
                if(ui_status_!=null)
                {
                    childs_[0] = ui_status_;                    
                }
                current_button_ = 0;

            }
            if (button_item_.getState() == State.Press)
            {
                if(ui_item_!=null)
                    childs_[0] = ui_item_;
                current_button_ = 1;
            }
            if (button_system_.getState() == State.Press)
            {
                if(ui_system_!=null)
                    childs_[0] = ui_system_;
                current_button_ = 2;
            }
        }

        public override void onPressedCancel()
        {
            exitWithResult(-1);
        }




















    }
}
