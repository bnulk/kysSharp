using SDL;
using kysSharp.Types;

namespace kysSharp
{
    //绘制带人物头像的简明状态
    //注意，部分类型继承此类，是为了使用role
    unsafe class Head : TextBox
    {
        protected Role? role_ = null;
        protected bool only_head_ = false;

        public Head(Role? r = null)
        {
            role_ = r;
            setTextPosition(20, 65);
            setFontSize(20);
            setTextColor(new SDL_Color() { r = 255, b = 255, g = 255, a = 255 });
            setHaveBox(false);
        }

        public void SetRole(ref Role r)
        {
            role_ = r;
        }

        public Role? GetRole()
        {
            return role_;
        }

        public void SetOnlyHead(bool b)
        {
            only_head_ = b;
        }

        public override void draw()
        {
            w_ = 250;
            h_ = 90;
            if (role_ == null) { return; }
            if(role_.strName==null) { return; }
            SDL_Color color = new SDL_Color() { r = 255, b = 255, g = 255, a = 255 }, 
                white = new SDL_Color() { r = 255, b = 255, g = 255, a = 255 }, 
                black = new SDL_Color() { r = 0, b = 0, g = 0, a = 255 };
            var font = GameFont.getInstance();

            if (!only_head_)
            {
                TextureManager.getInstance().renderTexture("title", 102, x_, y_);
            }

            if (state_ == State.Normal)
            {
                color = new SDL_Color() { r = 128, b = 125, g = 128, a = 255 };
            }

            if(state_==State.Pass)
            {
                color = new SDL_Color() { r = 255, b = 255, g = 255, a = 255 };
            }
            //中毒时突出绿色
            color.r -= (byte)(2 * role_.Poison);
            color.b -= (byte)(2 * role_.Poison);
            TextureManager.getInstance().renderTexture("head", role_.HeadID, x_ + 23, y_ + 8, color, 255, 0.5, 0.5);

            base.draw();

            if (only_head_) { return; }

            //下面都是画血条等

            font.draw(GameUtil.EraseModredundantChar(role_.strName).PadRight(8), 16, x_ + 160, y_ + 13, white, 255);
            SDL_Rect r1 = new SDL_Rect() { w = 0, h=0, x=0, y=0 };
            font.draw(role_.Level.ToString(), 16, x_ + 132 - 4 * GameUtil.Digit(role_.Level), y_ + 9, new SDL_Color() { r = 255, b = 255, g = 255, a = 255 }, 255);

            SDL_Color c, c_text;
            if (role_.MaxHP > 0)
            {
                r1 = new SDL_Rect()
                {
                    x = x_ + 128,
                    y = y_ + 42,
                    w = 185 * role_.HP / role_.MaxHP,
                    h = 12
                };
            }
            else
            {
                r1 = new SDL_Rect() { w = 0, h = 0, x = 0, y = 0 };
            }
            c = new SDL_Color() { r = 196, g = 25, b = 16, a = 255 };

            Engine.getInstance().renderSquareTexture(&r1, c, 192);

            font.draw((role_.HP.ToString().PadRight(3) + "/" + role_.MaxHP.ToString().PadRight(3)), 16, x_ + 195, y_ + 39, 
                new SDL_Color() {r=250, g=200, b=50, a=255 }, 255);

            if (role_.MaxMP > 0)
            {
                r1= new SDL_Rect() { x = x_ + 128, y = y_ + 63, w = 185 * role_.MP / role_.MaxMP, h = 12 };
            }
            else
            {
                r1= new SDL_Rect() { y = 0, w = 0, h = 0, x = 0 };
            }
            c= new SDL_Color() { r=200, g=200, b=200, a=255 };
            c_text = white;
            if (role_.MPType == 0)
            {
                c=new SDL_Color() {r=112,g=12,b=112, a=255 };
                c_text = new SDL_Color() { r = 240, g = 150, b = 240, a = 255 };
            }
            else if (role_.MPType == 1)
            {
                c = new SDL_Color() { r = 224, g = 180, b = 32, a = 255 };
                c_text = new SDL_Color() { r = 250, g = 200, b = 50, a = 255 };
            }

            Engine.getInstance().renderSquareTexture(&r1, c, 192);

            font.draw((role_.MP.ToString().PadRight(3) + "/" + role_.MaxMP.ToString().PadRight(3)), 16, x_ + 195, y_ + 60, c_text, 255);

            r1 = new SDL_Rect() {x= x_ + 153,y= y_ + 86,w= 111 * role_.PhysicalPower / 100,h=12 };
            c=new SDL_Color() { r=128, g=128, b=255, a=255 };
            Engine.getInstance().renderSquareTexture(&r1, c, 192);
            font.draw(role_.PhysicalPower.ToString().PadRight(3), 16, x_ + 203 - 4 * GameUtil.Digit(role_.PhysicalPower), y_ + 84, 
                new SDL_Color() { r=250,g=200,b=50,a=255}, 255);
        }








    }
}
