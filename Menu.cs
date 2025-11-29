using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    public class Menu:TextBox
    {
        public Menu()
        {
            pass_child_ = 0;
        }

        public override void dealEvent(SDL_Event e)
        {
            //此处处理键盘响应
            if ((e.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN || e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP))
            {
                int direct = 0;
                if (e.key.key == SDL_Keycode.SDLK_LEFT || e.key.key == SDL_Keycode.SDLK_UP)
                {
                    direct = -1;
                }
                if (e.key.key == SDL_Keycode.SDLK_RIGHT || e.key.key == SDL_Keycode.SDLK_DOWN)
                {
                    direct = 1;
                }

                if (direct != 0)
                {
                    setAllChildState(State.Normal);
                    //仅有两项的菜单两头封住
                    if (getChildCount() <= 2)
                    {
                        pass_child_ = direct > 0 ? getChildCount() - 1 : 0;
                    }
                    else
                    {
                        pass_child_ = findNextVisibleChild(pass_child_, direct);
                    }
                }
            }
            //事务处理中可以强制改变子项的Pass，用于菜单中固定某项
            forcePassChild();
        }

        public void arrange(int x, int y, int inc_x, int inc_y)
        {
            foreach (var c in childs_)
            {
                if (c.getVisible())
                {
                    c.setPosition(x_ + x, y_ + y);
                    x += inc_x;
                    y += inc_y;
                }
            }
        }

        public override void onPressedOK()
        {
            pressToResult();
            if (result_ >= 0)
            {
                setExit(true);
            }
        }

        public override void onEntrance()
        {
            pass_child_ = findFirstVisibleChild();
            forcePassChild();
        }

        public override void onPressedCancel()
        {
            exitWithResult(-1);
        }















    }
}
