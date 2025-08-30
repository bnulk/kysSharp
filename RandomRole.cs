using kysSharp.Types;

namespace kysSharp
{
    class RandomRole : UIStatus
    {
        public Button button_ok_;
        public Head head_ = new Head();

        public RandomRole()
        {
            SetShowButton(false);        //隐藏解读医疗等三个按钮

            button_ok_ = new Button();
            button_ok_.setText("確定".PadRight(4));
            addChild(button_ok_, 350, 55);
            head_ = new Head();
            addChild(head_, -350, 50);

        }

        public override void onPressedCancel()
        {
            exitWithResult(-1);
        }

        public override void onPressedOK()
        {
            if (button_ok_.getState() == State.Press)
            {
                result_ = 0;
                setExit(true);
                return;
            }

            Random random = new Random();
            role_= new Role();
            role_.MaxHP = 25 + random.Next(26);
            role_.HP = role_.MaxHP;
            role_.MaxMP = 25 + random.Next(26);
            role_.MP = role_.MaxMP;
            role_.MPType = random.Next(2);
            role_.IncLife = 1 + random.Next(10);
            role_.Attack = 25 + random.Next(6);
            role_.Speed = 25 + random.Next(6);
            role_.Defence = 25 + random.Next(6);
            role_.Medcine = 25 + random.Next(6);
            role_.UsePoison = 25 + random.Next(6);
            role_.Detoxification = 25 + random.Next(6);
            role_.Fist = 25 + random.Next(6);
            role_.Sword = 25 + random.Next(6);
            role_.Knife = 25 + random.Next(6);
            role_.Unusual = 25 + random.Next(6);
            role_.HiddenWeapon = 25 + random.Next(6);
            role_.IQ = 1 + random.Next(100);
        }

        public override void draw()
        {
            Engine.getInstance().fillColor(new SDL.SDL_Color() { r=0,g=0,b=0,a=192}, 0, 0, -1, -1);
            if(role_!=null)
            head_.SetRole(ref role_);
            base.draw();
        }


    }
}

