using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal static class SdlTools
    {
        public static void RectToFrect(SDL_Rect rect, out SDL_FRect Frect)
        {             
            Frect = new SDL_FRect
            {
                x = rect.x,
                y = rect.y,
                w = rect.w,
                h = rect.h
            };
        }
    }
}
