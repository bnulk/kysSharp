

namespace kysSharp
{
    class UISystem : Element
    {
        public MenuText? title_ = null;
        private static UISystem? _instance;
        public static UISystem Instance => _instance ??= new UISystem();

        public UISystem()
        {
            title_ = new MenuText();
            string[] tmpTitle = new string[3] { "讀檔", "存檔", "離開" };
            title_.setStrings(tmpTitle.ToList());
            title_.setFontSize(24);
            title_.arrange(100, 50, 64, 0);
            addChild(title_);
        }

        public override void onPressedOK()
        {
            result_ = -1;
            if (title_ == null)
                return;
            if (title_.getResult() == 0)
            {
                //读档
                var ui_save = new UISave();
                ui_save.setMode(0);
                ui_save.setFontSize(22);
                result_ = ui_save.runAtPosition(400, 100);
            }
            else if (title_.getResult() == 1)
            {
                var ui_save = new UISave();
                ui_save.setMode(1);
                ui_save.setFontSize(22);
                result_ = ui_save.runAtPosition(464, 100);
            }
            else if (title_.getResult() == 2)
            {
                result_ = AskExit();
            }
            title_.setResult(-1);
        }

        public int AskExit()
        {
            bool asking = false;
            int ret = -1;
            if (!asking)
            {
                asking = true;
                var menu = new MenuText();
                string[] tmpStr = new string[3] { "離開遊戲", "返回開頭", "我點錯了" };
                menu.setStrings(tmpStr.ToList());
                menu.setFontSize(50);
                menu.arrange(0, 0, 0, 100);
                int r = menu.runAtPosition(528, 100);
                if (r == 0)
                {
                    exitAll(0);
                    ret = 0;
                }
                else if (r == 1)
                {
                    exitAll(1);;
                    ret = 0;
                }
                asking = false;
            }
            return ret;
        }

        public override void onPressedCancel()
        {
            exitWithResult(-1);
        }







    }
}
