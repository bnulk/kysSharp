
using SDL;
using System.Drawing;

namespace kysSharp
{
    internal class Cloud: Element
    {
        public enum CloudTowards
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3,
        };


        public Point position = new Point();
        public float speed;

        public const int maxX = 17280;
        public const int maxY = 8640;
        public int numberTexture = 10;
        public int num_;
        
        byte alpha_;
        SDL_Color color_;

        public void InitCloud()
        {
            Random ra = new Random();
            position.x = ra.Next() % maxX;
            position.y = ra.Next() % maxY;
            speed = 1 + ra.Next() % 3;
            num_= ra.Next() % numberTexture;
            alpha_ = (byte)(128 + ra.Next(128));
            color_ = new SDL_Color { r = (byte)(ra.Next(256)), g = (byte)(ra.Next(256)), b = (byte)(ra.Next(256)), a = 255 };
        }

        public void SetPositionOnScreen(int x, int y, int Center_X, int Center_Y)
        {
            x_ = position.x - (-x * 18 + y * 18 + maxX / 2 - Center_X);
            y_ = position.y - (x * 9 + y * 9 + 9 - Center_Y);
        }

        public void ChangePosition()
        {
            position.x += (int)speed;
            if (position.x > maxX)
            {
                position.x = 0;
            }
        }

        public override void draw()
        {
            TextureManager.getInstance().renderTexture("cloud", num_, x_, y_, color_, alpha_);
        }

        public void flow()
        {
            position.x += (int)speed;
            if (position.x > maxX)
            {
                position.x = 0;
            }
        }
    }
}
