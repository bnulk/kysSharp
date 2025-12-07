using kysSharp.Types;
using SDL;

namespace kysSharp
{
    public class UIShop : Element
    {
        private int x_ = 200;
        private int y_ = 200;

        private List<int> plan_buy_ = new List<int>();
        private List<Button> buttons_ = new List<Button>();
        private Button button_ok_;
        private Button button_cancel_;
        private Button button_clear_;

        private Shop shop_;

        ///////////////////////////////////////////////////////////////////////////
        // 构造函数：创建左/右按钮、确认/取消/清除按钮
        ///////////////////////////////////////////////////////////////////////////
        public UIShop()
        {
            // 预设 plan_buy_ 大小
            for (int i = 0; i < Constant.SHOP_ITEM_COUNT; i++)
            {
                plan_buy_.Add(0);

                // 左按钮
                var button_left_ = new Button();
                button_left_.setTexture("title", 104);
                addChild(button_left_, 36 * 12 + 36, 30 + 25 * i);
                buttons_.Add(button_left_);

                // 右按钮
                var button_right_ = new Button();
                button_right_.setTexture("title", 105);
                addChild(button_right_, 36 * 12 + 108, 30 + 25 * i);
                buttons_.Add(button_right_);
            }

            button_ok_ = new Button();
            button_ok_.setText("確認");
            addChild(button_ok_, 300, 190);

            button_cancel_ = new Button();
            button_cancel_.setText("取消");
            addChild(button_cancel_, 400, 190);

            button_clear_ = new Button();
            button_clear_.setText("清除");
            addChild(button_clear_, 500, 190);
        }

        ///////////////////////////////////////////////////////////////////////////
        // C++ ~UIShop 析构函数——C# 不需要手动写
        ///////////////////////////////////////////////////////////////////////////
        ~UIShop() { }


        ///////////////////////////////////////////////////////////////////////////
        // 设置商店 ID
        ///////////////////////////////////////////////////////////////////////////
        public void setShopID(int id)
        {
            shop_ = Save.getInstance().GetShop(id);
        }


        ///////////////////////////////////////////////////////////////////////////
        // 绘制 UI：背景、物品列表、价格、金钱
        ///////////////////////////////////////////////////////////////////////////
        public override void draw()
        {
            Engine.getInstance().fillColor(new SDL_Color() { r = 0, g = 0, b = 0, a = 192 }, 0, 0, -1, -1);

            int x = x_;
            int y = y_;

            var font = GameFont.getInstance();
            string str = "";
            /////////////////////////////////////////////////////////////////////////
            // 标题行
            /////////////////////////////////////////////////////////////////////////
            str = "品名".PadRight(12) +
                "價格".PadLeft(8) +
                "存貨".PadLeft(8) +
                "持有".PadLeft(8) +
                "計劃".PadLeft(8);

            font.draw(str, 24, x, y, new SDL_Color() { r=200,g=150,b=50,a=255 });

            /////////////////////////////////////////////////////////////////////////
            // 列出 5 个物品
            /////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < 5; i++)
            {
                var item = Save.getInstance().GetItem(shop_.ItemID[i]);
                int count = Save.getInstance().GetItemCountInBag(item.ID);

                str = GameUtil.EraseModredundantChar(item.strName).PadRight(12) +
                    shop_.Price[i].ToString().PadLeft(8) +
                    shop_.Total[i].ToString().PadLeft(8) +
                    count.ToString().PadLeft(8) +
                    plan_buy_[i].ToString().PadLeft(8);

                font.draw(str, 24, x, y + 25 + i * 25, new SDL_Color() { r=255, g=255, b=255, a=255 });
            }

            /////////////////////////////////////////////////////////////////////////
            // 总价
            /////////////////////////////////////////////////////////////////////////
            int need_money = calNeedMoney();
            str = "總計銀兩" + need_money.ToString().PadLeft(8);
            font.draw(str, 24, x, y + 25 + 6 * 25, new SDL_Color() { r = 255, g = 255, b = 255, a = 255 });

            /////////////////////////////////////////////////////////////////////////
            // 持有银两
            /////////////////////////////////////////////////////////////////////////
            int money = Save.getInstance().GetMoneyCountInBag();
            SDL_Color c = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
            if (money < need_money) c = new SDL_Color() { r = 250, g = 50, b = 50, a = 255 };

            str = $"持有銀兩{money,8}";

            font.draw(str, 24, x, y + 25 + 7 * 25, c);
        }


        ///////////////////////////////////////////////////////////////////////////
        // 按钮行为：修改计划购买数量 / 清除 / 确认购买
        ///////////////////////////////////////////////////////////////////////////
        public override void onPressedOK()
        {
            /////////////////////////////////////////////////////////////////////////
            // 左右+-按钮逻辑
            /////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < Constant.SHOP_ITEM_COUNT * 2; i++)
            {
                if (buttons_[i].getState() == State.Press)
                {
                    int index = i / 2;
                    int lr = i % 2;

                    if (lr == 0)
                    {
                        // left
                        if (plan_buy_[index] > 0)
                            plan_buy_[index]--;
                    }
                    else
                    {
                        // right
                        if (plan_buy_[index] < shop_.Total[index])
                            plan_buy_[index]++;
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 确认购买
            /////////////////////////////////////////////////////////////////////////
            if (button_ok_.getState() == State.Press)
            {
                if (calNeedMoney() <= Save.getInstance().GetMoneyCountInBag())
                {
                    // 购买物品
                    for (int i = 0; i < Constant.SHOP_ITEM_COUNT; i++)
                    {
                        Event.getInstance().addItemWithoutHint(shop_.ItemID[i], plan_buy_[i]);
                        shop_.Total[i] -= plan_buy_[i];
                    }

                    // 扣钱
                    Event.getInstance().addItemWithoutHint(Constant.MONEY_ITEM_ID, -calNeedMoney());
                    exitWithResult(0);
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 取消
            /////////////////////////////////////////////////////////////////////////
            if (button_cancel_.getState() == State.Press)
            {
                exitWithResult(-1);
            }

            /////////////////////////////////////////////////////////////////////////
            // 清除计划购买
            /////////////////////////////////////////////////////////////////////////
            if (button_clear_.getState() == State.Press)
            {
                for (int i = 0; i < Constant.SHOP_ITEM_COUNT; i++)
                {
                    plan_buy_[i] = 0;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        // 计算需要花的钱
        ///////////////////////////////////////////////////////////////////////////
        public int calNeedMoney()
        {
            int need_money = 0;

            for (int i = 0; i < Constant.SHOP_ITEM_COUNT; i++)
            {
                need_money += plan_buy_[i] * shop_.Price[i];
            }

            return need_money;
        }

        public override void onPressedCancel() { exitWithResult(-1); }


























    }
}
