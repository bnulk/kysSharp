using SDL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace kysSharp
{
    internal unsafe class TextureManager
    {
        private static TextureManager? texture_manager_;
        private string path_ = Path.Combine("game", "resource");
        private Random random = new Random();

        public Dictionary<string, List<Texture>> map_ = new Dictionary<string, List<Texture>>();

        public static TextureManager getInstance()
        {
            if (texture_manager_ == null)
            {
                texture_manager_ = new TextureManager();
            }
            return texture_manager_;
        }


        public void renderTexture(Texture? tex, SDL_Rect r, SDL_Color c, byte alpha)
        {
            if (tex != null && tex.tex[0] != null)
            {
                int randomNumber = random.Next(0, tex.count);
                var engine = Engine.getInstance();
                engine.setColor(tex.tex[randomNumber], c, alpha);
                engine.renderCopy(tex.tex[randomNumber], r.x - tex.dx, r.y - tex.dy, r.w, r.h);
            }
        }

        public void renderTexture(string path, int num, SDL_Rect r, SDL_Color c, byte alpha)
        {
            var tex = loadTexture(path, num);
            renderTexture(tex, r, c, alpha);
        }

        public void renderTexture(Texture? tex, int x, int y, SDL_Color c, byte alpha, double zoom_x, double zoom_y)
        {
            var engine = Engine.getInstance();
            if (tex != null && tex.tex[0] != null)
            {
                int randomNumber = random.Next(0, tex.count);
                engine.setColor(tex.tex[randomNumber], c, alpha);
                engine.renderCopy(tex.tex[randomNumber], x - tex.dx, y - tex.dy, Convert.ToInt32(tex.w * zoom_x), Convert.ToInt32(tex.h * zoom_y));
            }
        }

        public void renderTexture(string path, int num, int x, int y, SDL_Color c=default, byte alpha=255, double zoom_x=1, double zoom_y = 1)
        {
            // 如果调用时没传 c，它就是 {0,0,0,0}。改为默认白色
            if (c.r == 0 && c.g == 0 && c.b == 0 && c.a == 0)
            {
                c = new SDL_Color { r = 255, g = 255, b = 255, a = alpha };
            }

            var tex = loadTexture(path, num);
            renderTexture(tex, x, y, c, alpha, zoom_x, zoom_y);
        }

        public Texture? loadTexture(string path, int num)
        {
            var p = Path.Combine(path_, path);
            List<Texture>? v = new List<Texture>();
            Texture tmpTex = new Texture();

            if (texture_manager_!=null && texture_manager_.map_ != null && texture_manager_.map_.ContainsKey(path))
            {
                v = texture_manager_.map_[path];
            }
            else
            {
                if (texture_manager_ != null && texture_manager_.map_ != null)
                {
                    texture_manager_.map_.Add(path, v);
                }
            }

            if (texture_manager_ != null)
            {
                //纹理组信息
                if (getTextureGroupCount(path) == 0)
                {
                    return null;
                }
                //纹理信息
                if (num < 0 || num >= v.Count)
                {
                    return null;
                }
                var t = v[num];
                if (t.loaded == false)
                {
                    loadTexture2(path, num, ref t);
                }
                return t;
            }
            return null;
        }

        public int getTextureGroupCount(string path)
        {
            if (texture_manager_ != null)
            {
                var v = texture_manager_.map_[path];

                if (v.Count == 0)
                {
                    initialTextureGroup(path);
                }

                if (v.Count == 1 && v[0] == null)
                {
                    return 0;
                }
                else
                {
                    return v.Count;
                }
            }
            return 0;
        }

        public void initialTextureGroup(string path, bool load_all = false)
        {
            var p = Path.Combine(path_, path);

            if (texture_manager_ == null)
            {
                return;
            }

            var v = texture_manager_.map_[path];
            //纹理组信息
            //不存在的纹理组也会有一个vector存在，但是里面只有一个空指针
            if (v.Count == 0)
            {
                short[] s;                 //两个字节，不同于C++中的char一字节。后期对比再看。
                int l = 0;
                GameFile.readFile(Path.Combine(p , "index.ka"), out s, out l);                       //读坐标dx和dy的文件index.ka
                l /= 4;
                if (l == 0)
                {
                    Texture texture = new Texture();
                    texture.tex = new SDL_Texture*[1];
                    texture.tex[0] = null; // 空指针
                    v.Add(texture);
                    return;
                }
                for (int i = 0; i < l; i++)
                {
                    Texture texture = new Texture();

                    //texture.dx = Convert.ToInt32(s.Substring(4 * i, 2));
                    //texture.dy = Convert.ToInt32(s.Substring(4 * i + 2, 2));
                    texture.dx = (int)s[2 * i];                                        //和源程序不同，原因是这里用了short，源程序是char* 
                    texture.dy = (int)s[2 * i + 1];
                    v.Add(texture);
                }
                Console.WriteLine("Load texture group from path: " + p.ToString() + " find " + l.ToString() + " textures\n");
            }
            if (load_all)
            {
                var engine = Engine.getInstance();
                for (int i = 0; i < v.Count; i++)
                {
                    Texture texture= new Texture();
                    loadTexture2(path, i, ref texture);
                    v[i] = texture;
                }
            }
        }

        private void loadTexture2(string path, int num, ref Texture t)
        {
            var p = Path.Combine(path_, path);
            if (t.loaded== false)
            {
                //printf("Load texture %s, %d\n", p.c_str(), num);
                t.tex[0] = Engine.getInstance().loadImage(Path.Combine(p, num.ToString()+".png"));
                if (t.tex[0]!=null)
                {

                }
                else
                {
                    for (int i = 0; i < 10; i++)
                    {
                        t.tex[i] = Engine.getInstance().loadImage(Path.Combine(p ,num.ToString()+"_" +i.ToString() + ".png"));
                        if (t.tex[i]==null)
                        {
                            t.count = i;
                            break;
                        }
                    }
                }
                Engine.getInstance().queryTexture(t.tex[0], ref t.w, ref t.h);
                t.loaded = true;
            }
        }







    }
}
