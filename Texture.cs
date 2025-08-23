using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    public unsafe class Texture : IDisposable
    {
        public SDL_Texture*[] tex;
        public int w = 0, h = 0, dx = 0, dy = 0;
        public bool loaded = false;
        public int count = 1;
        public Texture()
        {
            tex = new SDL_Texture*[10];
        }
        public void setTex(SDL_Texture* t)
        {
            Dispose();
            tex = new SDL_Texture*[10];
            tex[0] = t;
            count = 1;
            loaded = true;
            Engine.getInstance().queryTexture(t, ref w, ref h);
        }
        public SDL_Texture* getTexture(int i = 0) { return tex[i]; }

        public void Dispose()
        {
            for (int i = 0; i < 10; i++)
            {
                if (tex[i] != null)
                {
                    SDL3.SDL_DestroyTexture(tex[i]);
                }
            }
        }
    }
}
