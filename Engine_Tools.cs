using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal unsafe partial class Engine : IDisposable
    {
        // FRect 转 Rect（取整）
        SDL_Rect ToRect(SDL_FRect f)
        {
            return new SDL_Rect
            {
                x = (int)f.x,
                y = (int)f.y,
                w = (int)f.w,
                h = (int)f.h
            };
        }

        // Rect 转 FRect
        SDL_FRect ToFRect(SDL_Rect r)
        {
            return new SDL_FRect
            {
                x = (float)r.x,
                y = (float)r.y,
                w = (float)r.w,
                h = (float)r.h
            };
        }

        /// <summary>
        /// ppy.SDL3 没暴露 SDL_Surface 内部结构，你需要手动声明它：
        /// </summary>
        unsafe struct SDL_SurfaceData
        {
            public SDL_PixelFormatDetails* format;
            public void* pixels;
            public int w, h;
            public int pitch;
            public uint flags;
            // 其余字段用不到可省略
        }



    }
}
