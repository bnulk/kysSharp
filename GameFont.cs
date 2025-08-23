using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal unsafe class GameFont
    {
        private static GameFont? font_;
        ///////////////////////////////////////////////////////////////////////
        // buffer_ : 用于缓存已经绘制过的字体纹理，避免重复创建，提高效率
        // key   = 字体索引 (由 calIndex 计算得到, 与字符码点/大小有关)
        // value = 字符对应的纹理对象 (BP_Texture 在这里假设对应 SDL_Texture)
        ///////////////////////////////////////////////////////////////////////
        private Dictionary<int, IntPtr> buffer_ = new();

        private readonly string fontnamec_ = Path.Combine("..", "game", "font", "chinese.ttf");
        private readonly string fontnamee_ = Path.Combine("..", "game", "font", "english.ttf");

        public static GameFont getInstance()
        {
            if (font_ == null)
            {
                font_ = new GameFont();
            }
            return font_;
        }

        private int calIndex(int size, ushort c) { return size * 0x1000000 + c; }

        public void draw(string text, int size, int x, int y, SDL_Color color, byte alpha)
        {
            int p = 0;
            while (p < text.Length)
            {
                int w = size, h = size;

                ///////////////////////////////////////////////////////////////////////
                // Step1: 解析当前字符
                // 在原始 C++ 代码中，通过 uint16_t + cp936toutf8 处理中文编码
                // 在 C# 里字符串本身就是 UTF-16，不需要复杂的 cp936 转换
                ///////////////////////////////////////////////////////////////////////
                char c = text[p];
                p++;

                // 计算唯一索引 (与字符和字体大小相关)
                int index = CalIndex(size, c);

                ///////////////////////////////////////////////////////////////////////
                // Step2: 检查缓存，如果没有则创建纹理
                ///////////////////////////////////////////////////////////////////////
                if (!buffer_.ContainsKey(index))
                {
                    string s = c.ToString();

                    // 创建文字纹理 (调用引擎接口, 假设返回 IntPtr 表示 SDL_Texture)
                    IntPtr tex = (IntPtr)Engine.getInstance().createTextTexture(
                        fontnamec_, s, size,
                        new SDL_Color { r = 255, g = 255, b = 255, a = 255 }
                    );

                    buffer_[index] = tex;
                }

                IntPtr texture = buffer_[index];

                ///////////////////////////////////////////////////////////////////////
                // Step3: 宽度调整 (ASCII 字符宽度缩小一半)
                ///////////////////////////////////////////////////////////////////////
                if (c <= 128)
                {
                    w = size / 2;
                }

                ///////////////////////////////////////////////////////////////////////
                // Step4: 绘制字符 (跳过空格)
                // 先绘制阴影 (偏移1像素, 颜色减半)
                // 再绘制正文字体
                ///////////////////////////////////////////////////////////////////////
                if (c != ' ')
                {
                    Engine.getInstance().setColor(
                        (SDL_Texture*)texture,
                        new SDL_Color
                        {
                            r = (byte)(color.r / 2),
                            g = (byte)(color.g / 2),
                            b = (byte)(color.b / 2),
                            a = color.a
                        },
                        alpha
                    );
                    Engine.getInstance().renderCopy((SDL_Texture*)texture, x + 1, y, w, h);

                    Engine.getInstance().setColor((SDL_Texture*)texture, color, alpha);
                    Engine.getInstance().renderCopy((SDL_Texture*)texture, x, y, w, h);
                }

                ///////////////////////////////////////////////////////////////////////
                // Step5: 移动绘制位置 (逐字绘制, x 累加宽度)
                ///////////////////////////////////////////////////////////////////////
                x += w;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // calIndex : 根据 字体大小 + 字符码点 计算唯一索引
        // 用于保证不同大小/不同字符缓存区分
        ///////////////////////////////////////////////////////////////////////
        private int CalIndex(int size, char c)
        {
            return size * 65536 + c; // 相当于 (size << 16) | c
        }

        public void drawWithBox(string text, int size, int x, int y, SDL_Color color, byte alpha, byte alpha_box)
        {
            SDL_Rect r;
            r.x = x - 10;
            r.y = y - 3;
            r.w = size * text.Length / 2 + 20;
            r.h = size + 6;
            TextureManager.getInstance().renderTexture("title", 
                126, 
                r,
                new SDL_Color
                {
                    r = (byte)(color.r / 2),
                    g = (byte)(color.g / 2),
                    b = (byte)(color.b / 2),
                    a = color.a
                },
                alpha_box);
            draw(text, size, x, y, color, alpha);
        }





















    }
}
