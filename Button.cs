using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal class Button:TextBox
    {
        public Button()
        {
            resize_with_text_ = true;
        }
        
        public Button(string path, int normal_id, int pass_id = -1, int press_id = -1)
        {
            setTexture(path, normal_id, pass_id, press_id);
        }

        public override void dealEvent(SDL_Event e)
        {
            result_ = -1;
            if (e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
            {
                if (inSide((int)e.motion.x, (int)e.motion.y))
                {
                    result_ = 0;
                }
            }
        }

        public override void draw()
        {
            //视情况重新计算尺寸
            if (w_ * h_ == 0)
            {
                var tex = TextureManager.getInstance().loadTexture(texture_path_, texture_normal_id_);
                if (tex!=null)
                {
                    w_ = tex.w;
                    h_ = tex.h;
                }
            }
            int x = x_;
            int y = y_;
            var id = texture_normal_id_;
            SDL_Color color = new SDL_Color { r=255, b=255, g=255, a=255 };
            byte alpha = 225;
            if (state_ == State.Normal)
            {
                if (texture_normal_id_ == texture_pass_id_)
                {
                    color = new SDL_Color { r = 128, b = 128, g = 128, a = 255 };
                }
            }
            if (state_ == State.Pass)
            {
                id = texture_pass_id_;
                alpha = 240;
            }
            else if (state_ == State.Press)
            {
                id = texture_press_id_;
                alpha = 255;
            }
            TextureManager.getInstance().renderTexture(texture_path_, id, x, y, color, alpha);


            if (text_!=null)
            {
                SDL_Color color_text = color_normal_;
                if (state_ == State.Pass)
                {
                    color_text = color_pass_;
                }
                else if (state_ == State.Press)
                {
                    color_text = color_press_;
                }
                GameFont.getInstance().drawWithBox(text_, font_size_, x_ + text_x_, y_ + text_y_, color_text, 255, alpha);
            }
        }
















    }
}
