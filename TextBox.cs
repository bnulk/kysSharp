using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal unsafe class TextBox:Element
    {
        protected string text_ = "";
        protected int font_size_ = 20;
        protected int text_x_ = 0, text_y_ = 0;
        protected Texture tex_ = new Texture();
        protected SDL_Color color_normal_ = new SDL_Color() { r=32, b=32, g=32, a=255 };
        protected SDL_Color color_pass_ = new SDL_Color() { r=255, b=255, g=255, a=255 };
        protected SDL_Color color_press_ = new SDL_Color() { r=255, b=0, g=0, a=255 };
        protected bool have_box_ = true;

        protected string texture_path_ = "";
        protected int texture_normal_id_ = -1, texture_pass_id_ = -1, texture_press_id_ = -1; //三种状态的按钮图片

        protected bool resize_with_text_ = false;


        public override void draw()         //如何画本节点
        {
            if (texture_path_!=null && texture_path_!="")
            {
                SDL_Color sDL_Color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
                TextureManager.getInstance().renderTexture(texture_path_, texture_normal_id_, x_, y_, sDL_Color, 255);
            }
            //实际上仅用了一个颜色，需要有颜色变化请用button
            if (text_!=null)
            {
                if (have_box_)
                {
                    GameFont.getInstance().drawWithBox(text_, font_size_, x_ + text_x_, y_ + text_y_, color_normal_,255);
                }
                else
                {
                    GameFont.getInstance().draw(text_, font_size_, x_ + text_x_, y_ + text_y_, color_normal_);
                }
            }
        }


        public void setHaveBox(bool h) { have_box_ = h; }
        public void setTexture(string path, int normal_id, int pass_id = -1, int press_id = -1)
        {
            if (pass_id < 0) { pass_id = normal_id; }
            if (press_id < 0) { press_id = normal_id; }
            texture_path_ = path;
            texture_normal_id_ = normal_id;
            texture_pass_id_ = pass_id;
            texture_press_id_ = press_id;
        }
        
        public void setText(string text)
        {
            text_ = text;
            if (resize_with_text_)
            {
                w_ = font_size_ * text_.Length / 2;
                h_ = font_size_;
            }
        }

        public int getNormalTextureID() { return texture_normal_id_; }

        public void setFontSize(int size)
        {
            foreach (var c in childs_)
            {
                // 尝试把 c 转换成 TextBox 类型
                if (c is TextBox t)
                {
                    // 如果转换成功，就调用其 SetFontSize 方法
                    t.setFontSize(size);
                }
            }
            font_size_ = size;
            if (resize_with_text_)
            {
                w_ = font_size_ * text_.Length / 2;
                h_ = font_size_;
            }
        }

        public string getText() { return text_; }

        public void setTexture(Texture t) { tex_ = t; }
        public void setTextPosition(int x, int y) { text_x_ = x; text_y_ = y; }  //注意：这个会导致焦点出现问题，通常是为了实现一些其他效果，请勿任意使用

        public void setTextColor(SDL_Color c1) { color_normal_ = c1; }

        public void setTextColor(SDL_Color c1, SDL_Color c2, SDL_Color c3)
        {
            color_normal_ = c1;
            color_pass_ = c2;
            color_press_ = c3;
        }







    }
}
