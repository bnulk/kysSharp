using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal class SdlInitial
    {
        public void Run()
        {
            // 初始化 TTF
            if (SDL3_ttf.TTF_Init() == false)
            {
                Console.WriteLine($"TTF_Init failed: {SDL3.SDL_GetError()}");
                SDL3.SDL_Quit();
            }
        }
    }
}
