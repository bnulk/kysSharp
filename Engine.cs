using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal unsafe partial class Engine : IDisposable
    {
        private static Engine? engine_;
        public static Engine getInstance()
        {
            if (engine_ == null)
            {
                engine_ = new Engine();
            }
            return engine_;
        }
        public void ShowMessage(string message)
        {
            // 显示消息对话框
            SDL3.SDL_ShowSimpleMessageBox(SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION, "提示", message, window_);
        }
        public bool isDispose = false;

        //图形相关
        private SDL_Window* window_;
        private SDL_Renderer* renderer_;
        private SDL_Texture* tex_, tex2_, logo_;
        private SDL_AudioSpec want_, spec_;
        private SDL_Rect rect_;

        private bool full_screen_ = false;
        private bool keep_ratio_ = true;

        private int start_w_ = 1024, start_h_ = 640;
        private int win_w_, win_h_, min_x_, min_y_, max_x_, max_y_;
        private double rotation_ = 0;
        private int ratio_x_ = 1, ratio_y_ = 1;

        public void getWindowSize(out int w, out int h)
        {
            fixed (int* pw = &w)
            fixed (int* ph = &h)
            {
                SDL3.SDL_GetWindowSize(window_, pw, ph);
            }
        }

        public void getWindowMaxSize(out int w, out int h)
        {
            fixed (int* pw = &w)
            fixed (int* ph = &h)
            {
                SDL3.SDL_GetWindowMaximumSize(window_, pw, ph);
            }
        }
        public int getWindowsWidth()
        {
            int w;
            SDL3.SDL_GetWindowSize(window_, &w, null);
            return w;
        }
        public int getWindowsHeight()
        {
            int h;
            SDL3.SDL_GetWindowSize(window_, null, &h);
            return h;
        }
        public int getMaxWindowWidth() { return max_x_ - min_x_; }
        public int getMaxWindowHeight() { return max_y_ - min_y_; }
        public void setWindowSize(int w, int h)
        {
            if (w <= 0 || h <= 0) return;
            win_w_ = Math.Min(max_x_ - min_x_, w);
            win_h_ = Math.Min(min_y_ - min_y_, h);
            SDL3.SDL_SetWindowSize(window_, win_w_, win_h_);
            setPresentPosition();

            SDL3.SDL_ShowWindow(window_);
            SDL3.SDL_RaiseWindow(window_);
            getWindowSize(out win_w_, out win_h_);
        }
        public void setStartWindowSize(int w, int h) { start_w_ = w; start_h_ = h; }
        public void setWindowPosition(int x, int y)
        {
            int w, h;
            SDL3.SDL_GetWindowSize(window_, &w, &h);
            if (x == SDL3.SDL_WINDOWPOS_CENTERED)
            {
                x = min_x_ + (max_x_ - min_x_ - w) / 2;
            }
            if (y == SDL3.SDL_WINDOWPOS_CENTERED)
            {
                y = min_y_ + (max_y_ - min_y_ - h) / 2;
            }
            SDL3.SDL_SetWindowPosition(window_, x, y);
        }
        public void setWindowTitle(string str)
        {
            SDL3.SDL_SetWindowTitle(window_, str);
        }
        public SDL_Renderer* getRenderer() { return renderer_; }

        /// <summary>
        /// 创建一个专用于画场景的，后期放大
        /// </summary>
        /// <param name="w">宽</param>
        /// <param name="h">高</param>
        public void createAssistTexture(int w, int h)
        {
            tex2_ = createARGBRenderedTexture(w, h);
            setPresentPosition();
        }

        /// <summary>
        /// 设置贴图的位置
        /// </summary>
        public void setPresentPosition()
        {
            if (tex_ == null)
                return;
            int w_dst = 0, h_dst = 0;
            float w_src = 0, h_src = 0;
            getWindowSize(out w_dst, out h_dst);
            SDL3.SDL_GetTextureSize(tex_, &w_src, &h_src);
            w_src *= ratio_x_;
            h_src *= ratio_y_;
            if (keep_ratio_)
            {
                if (w_src == 0 || h_src == 0) return;
                double w_ratio = 1.0 * w_dst / w_src;
                double h_ratio = 1.0 * h_dst / h_src;
                double ratio = Math.Min(w_ratio, h_ratio);
                if (w_ratio > h_ratio)
                {
                    //宽度大，左右留空
                    rect_.x = Convert.ToInt32((Convert.ToDouble(w_dst) - Convert.ToDouble(w_src) * ratio) / 2);
                    rect_.y = 0;
                    rect_.w = Convert.ToInt32(Convert.ToDouble(w_src) * ratio);
                    rect_.h = h_dst;
                }
                else
                {
                    //高度大，上下留空
                    rect_.x = 0;
                    rect_.y = Convert.ToInt32((Convert.ToDouble(h_dst) - Convert.ToDouble(h_src) * ratio) / 2);
                    rect_.w = w_dst;
                    rect_.h = Convert.ToInt32(Convert.ToDouble(h_src) * ratio);
                }
            }
            else
            {
                rect_.x = 0;
                rect_.y = 0;
                rect_.w = w_dst;
                rect_.h = h_dst;
            }
        }
        public void getPresentSize(ref int w, ref int h) { w = rect_.w; h = rect_.h; }
        public int getPresentWidth() { return rect_.w; }
        public int getPresentHeight() { return rect_.h; }
        public void getMainTextureSize(ref int w, ref int h)
        {
            float floatW, floatH;
            {
                SDL3.SDL_GetTextureSize(tex2_, &floatW, &floatH);
            }
            w = (int)floatW;
            h = (int)floatH;
        }
        public void destroyAssistTexture() { if (tex2_ != null) { destroyTexture(tex2_); } }
        public static void destroyTexture(SDL_Texture* t) { SDL3.SDL_DestroyTexture(t); }

        /*
         * YV12 是 YUV420p 格式的一种排列方式，内存布局如下：所有 Y（亮度）数据在前，占 w * h 字节；接着是 V 分量，占 w * h / 4；
         * 最后是 U 分量，占 w * h / 4。每 4 个像素共享一个 UV，适合压缩视频数据。
         * 
         * SDL_TEXTUREACCESS_STREAMING	含义：内容会频繁改变	适用场景：视频播放、动态图像
         */
        public SDL_Texture* createYUVTexture(int w, int h)
        {
            return SDL3.SDL_CreateTexture(renderer_, SDL_PixelFormat.SDL_PIXELFORMAT_YV12, SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, w, h);
        }

        public void updateYUVTexture(SDL_Texture* t, SDL_Rect* rect, byte* Yplane, int Ypitch, byte* Uplane, int Upitch, byte* Vplane, int Vpitch)
        {
            SDL3.SDL_UpdateYUVTexture(t, rect, Yplane, Ypitch, Uplane, Ypitch, Vplane, Vpitch);
        }

        public SDL_Texture* createRGBATexture(int w, int h)
        {
            return SDL3.SDL_CreateTexture(renderer_, SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, w, h);
        }
        public SDL_Texture* createARGBRenderedTexture(int w, int h)
        {
            return SDL3.SDL_CreateTexture(renderer_, SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, w, h);
        }

        public void updateRGBATexture(SDL_Texture* t, SDL_Rect* rect, nint pixels, int pitch)
        {
            SDL3.SDL_UpdateTexture(t, rect, pixels, pitch);
        }
        public void renderCopy(SDL_Texture* t)
        {
            SDL_FRect fRect = ToFRect(rect_);
            SDL_FRect* fRectPtr = &fRect;
            SDL3.SDL_RenderTextureRotated(renderer_, t, null, fRectPtr, rotation_, null, SDL_FlipMode.SDL_FLIP_NONE);
        }

        public void renderCopy(SDL_Texture* t, int x = 0, int y = 0, int w = 0, int h = 0, int inPresent = 0)
        {
            if (inPresent == 1)
            {
                x += rect_.x;
                y += rect_.y;
            }
            SDL_FRect r = new SDL_FRect { x = x, y = y, w = w, h = h };
            SDL3.SDL_RenderTexture(renderer_, t, null, &r);
        }

        public void renderCopy(SDL_Texture* t, SDL_Rect* rect0, SDL_Rect* rect1, int inPresent /*= 0*/)
        {
            SDL_FRect r0;
            SDL_FRect r1;
            SDL_FRect* r0Ptr;
            SDL_FRect* r1Ptr;

            if (rect0!=null)
            {
                r0 = new SDL_FRect { x = (*rect0).x, y = (*rect0).y, w = (*rect0).w, h = (*rect0).h };
                r0Ptr = &r0;
            }
            else
            {
                r0Ptr = null;
            }
            if(rect1!=null)
            {
                r1 = new SDL_FRect { x = (*rect1).x, y = (*rect1).y, w = (*rect1).w, h = (*rect1).h };
                r1Ptr = &r1;
            }
            else
            {
                r1Ptr = null;
            }
            SDL3.SDL_RenderTexture(renderer_, t, r0Ptr, r1Ptr);
        }

        public void showLogo() { SDL3.SDL_RenderTexture(renderer_, logo_, null, null); }
        public void renderPresent() { SDL3.SDL_RenderPresent(renderer_); }
        public void renderClear() { SDL3.SDL_RenderClear(renderer_); }
        public void setTextureAlphaMod(SDL_Texture* t, byte alpha) { SDL3.SDL_SetTextureAlphaMod(t, alpha); }
        public void queryTexture(SDL_Texture* t, ref int w, ref int h)
        {
            float wFloat = 0, hFloat = 0;
            queryTextureFloat(t, ref wFloat, ref hFloat);
            w = (int)wFloat;
            h = (int)hFloat;
        }
        private void queryTextureFloat(SDL_Texture* t, ref float w, ref float h)
        {
            fixed (float* pw = &w)
            fixed (float* ph = &h)
            {
                SDL3.SDL_GetTextureSize(t, pw, ph);
            }
        }
        public void setRenderTarget(SDL_Texture* t) { SDL3.SDL_SetRenderTarget(renderer_, t); }
        public void resetRenderTarget() { SDL3.SDL_SetRenderTarget(renderer_, null); }
        public void createWindow() { }
        public void createRenderer() { }
        public void Destroy() { Dispose(); }
        public bool isFullScreen()
        {
            SDL_WindowFlags windowFlags = SDL3.SDL_GetWindowFlags(window_);
            uint flag = (uint)windowFlags;
            full_screen_ = (flag & SDL3.SDL_WINDOW_FULLSCREEN) != 0;

            return full_screen_;
        }
        public void toggleFullscreen()
        {
            full_screen_ = !full_screen_;
            uint flags = (uint)SDL3.SDL_GetWindowFlags(window_);

            if ((flags & SDL3.SDL_WINDOW_FULLSCREEN) == 0)
            {
                // 不是全屏，切换为全屏
                SDL3.SDL_SetWindowFullscreen(window_, true);
            }
            else
            {
                // 是全屏，取消
                SDL3.SDL_SetWindowFullscreen(window_, false);
            }
            SDL3.SDL_RenderClear(renderer_);
        }
        public SDL_Texture* loadImage(string filename)
        {
            // 加载图像为 surface
            SDL_Surface* surface = SDL3_image.IMG_Load(filename);
            if (surface == null)
            {
                Console.WriteLine("加载图像失败: " + SDL3.SDL_GetError());
                return null;
            }

            // 从 surface 创建纹理
            SDL_Texture* texture = SDL3.SDL_CreateTextureFromSurface(renderer_, surface);

            // 销毁 surface（SDL3 用 SDL_DestroySurface）
            SDL3.SDL_DestroySurface(surface);

            if (texture == null)
            {
                Console.WriteLine("创建纹理失败: " + SDL3.SDL_GetError());
            }

            return texture;
        }
        public bool setKeepRatio(bool b) { return keep_ratio_ = b; }


        ///////////////////////////////////////////////////////////////////////
        /// 将灰度图转换为带颜色 + 透明度纹理
        ///
        /// 参数说明：
        ///   - src    : 指向灰度图像的字节数组（每像素一个字节）
        ///   - color  : 颜色（0xRRGGBB 格式）
        ///   - w, h   : 图像宽度和高度（像素单位）
        ///   - stride : 每行数据的跨度（通常等于宽度）
        ///
        /// 返回值：
        ///   SDL_Texture（带颜色和透明度的纹理对象）
        ///////////////////////////////////////////////////////////////////////
        public unsafe SDL_Texture* transBitmapToTexture(
            byte[] src, uint color, int w, int h, int stride)
        {
            // 解码颜色（0xRRGGBB 格式）
            byte r = (byte)((color >> 16) & 0xFF);
            byte g = (byte)((color >> 8) & 0xFF);
            byte b = (byte)(color & 0xFF);

            // 创建一个 RGBA8888 格式的 surface
            SDL_Surface* surface = SDL3.SDL_CreateSurface(w, h, SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888);
            SDL3.SDL_LockSurface(surface);
            byte* pixels = (byte*)surface->pixels;
            int pitch = surface->pitch;

            // 填充像素
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int offset = y * pitch + x * 4;
                    byte alpha = src[y * w + x];

                    pixels[offset + 0] = b;
                    pixels[offset + 1] = g;
                    pixels[offset + 2] = r;
                    pixels[offset + 3] = alpha;
                }
            }

            SDL3.SDL_UnlockSurface(surface);

            // 生成纹理
            SDL_Texture* tex = SDL3.SDL_CreateTextureFromSurface(renderer_, surface);
            SDL3.SDL_SetTextureBlendMode(tex, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL3.SDL_SetTextureAlphaMod(tex, 192);

            SDL3.SDL_DestroySurface(surface);


            return tex;

            /*
               SDL_Renderer* renderer = ...;
               byte[] grayPixels = LoadGrayBitmap(); // 你已有的灰度源数据
               SDL_Texture* tex = TransBitmapToTexture(grayPixels, 0x66CCFF, 128, 128, 128, renderer);
            */
        }
        public double setRotation(double r) { return rotation_ = r; }
        public void resetWindowsPosition()
        {
            int x, y, w, h, x0, y0;
            SDL3.SDL_GetWindowSize(window_, &w, &h);
            SDL3.SDL_GetWindowPosition(window_, &x0, &y0);
            x = Math.Max(min_x_, x0);
            y = Math.Max(min_y_, y0);
            if (x + w > max_x_) x = Math.Min(x, max_x_ - w);
            if (y + h > max_y_) y = Math.Min(y, max_y_ - h);
            if (x != x0 || y != y0)
                SDL3.SDL_SetWindowPosition(window_, x, y);
        }
        public void setRatio(int x, int y) { ratio_x_ = x; ratio_y_ = y; }
        public void setColor(SDL_Texture* tex, SDL_Color c, byte alpha)
        {
            SDL3.SDL_SetTextureColorMod(tex, c.r, c.g, c.b);
            SDL3.SDL_SetTextureAlphaMod(tex, alpha);
            SDL3.SDL_SetTextureBlendMode(tex, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        }
        public void fillColor(SDL_Color color, int x, int y, int w, int h)
        {
            if (w < 0 || h < 0) { getPresentSize(ref w, ref h); }
            SDL_FRect r = new SDL_FRect
            {
                x = (float)x,
                y = (float)y,
                w = (float)w,
                h = (float)h
            };
            SDL3.SDL_SetRenderDrawColor(renderer_, color.r, color.g, color.b, color.a);
            SDL3.SDL_SetRenderDrawBlendMode(renderer_, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL3.SDL_RenderFillRect(renderer_, &r);
        }
        public void setRenderAssistTexture() { SDL3.SDL_SetRenderTarget(renderer_, tex2_); }
        public void renderAssistTextureToWindow()
        {
            SDL3.SDL_SetRenderTarget(renderer_, null);
            SDL3.SDL_RenderTexture(renderer_, tex2_, null, null);
        }

        /// <summary>
        /// 声音相关
        /// </summary>
        public SDL_AudioDeviceID _audioDevice;
        public void pauseAudio(int pause) { SDL3.SDL_PauseAudioDevice(_audioDevice); }
        public void closeAudio() { SDL3.SDL_CloseAudioDevice(_audioDevice); }

        public void playMusic(int num)
        {
            //Audio.getInstance().checkAndReplayMusic(num);
        }

        public void stopMusic()
        {
            //Audio.getInstance().stopMusic();
        }


        ///
        /// 事件相关
        /// 
        public SDL_Event e_;
        public int time_;

        public void delay(int t) { SDL3.SDL_Delay((uint)t); }
        public uint getTicks() { return (uint)SDL3.SDL_GetTicks(); }
        public uint tic() { time_ = (int)SDL3.SDL_GetTicks(); return (uint)time_; }
        public void toc()
        {
            // 获取当前时间（以毫秒为单位）
            int now = (int)SDL3.SDL_GetTicks();

            // 如果时间发生了变化，打印与 _time 的差值
            if (now != time_)
            {
                Console.Write("%u ms elapsed\n", now - time_);
            }
        }
        public void getMouseState(ref int x, ref int y)
        {
            float xFloat = 0, yFloat = 0;
            SDL3.SDL_GetMouseState(&xFloat, &yFloat);
            x = (int)xFloat;
            y = (int)yFloat;
        }

        /// <summary>
        /// SDL_PollEvent(&e) 是非阻塞地从 SDL 的事件队列里取出下一个事件，写进你传入的 SDL_Event e 里
        /// 如果当前队列为空，立刻返回“没事件”。
        /// 内部会先收集操作系统的原始输入（相当于先做 SDL_PumpEvents），再从队列里弹出一条事件给你处理。
        /// 这是游戏/多媒体程序每帧最常见的用法。
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool pollEvent(ref SDL_Event e)
        {
            bool hasEvent;
            fixed (SDL_Event* pe = &e)
            {
                hasEvent = SDL3.SDL_PollEvent(pe);
            }
            return hasEvent;
        }

        public bool pushEvent(ref SDL_Event e)
        {
            bool hasEvent;
            fixed (SDL_Event* pe = &e)
            {
                hasEvent = SDL3.SDL_PushEvent(pe);
            }
            return hasEvent;
        }

        public void flushEvent() { SDL3.SDL_FlushEvent((SDL_EventType)0); }

        public void free(void* mem) { SDL3.SDL_free(mem); }

        // 在 Engine 类中添加 numKeys 变量定义，并修正 checkKeyPress 方法
        private int numKeys; // 新增字段

        /// <summary>
        /// /////////////////////////////////////////////////////////////////////////
        /// 函数功能：
        ///   检查某个 SDL 键盘按键 (SDL_Keycode) 是否被按下，
        ///   并返回当前的修饰键状态 (SDL_Keymod)，例如 Shift / Ctrl / Alt 等。
        ///
        /// 参数说明：
        ///   key       —— 要检测的键盘按键，类型为 SDL_Keycode（逻辑按键）
        ///   modstate  —— 输出参数，返回该按键对应的修饰键状态
        ///
        /// 返回值：
        ///   true  —— 指定按键当前被按下
        ///   false —— 指定按键当前未被按下
        /////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="key"> SDL 键盘按键</param>
        /// <param name="modstate">修饰键状态 (SDL_Keymod),没有的选null</param>
        /// <returns></returns>
        public bool checkKeyPress(SDL_Keycode key, SDL_Keymod* modstate)
        {
            int* numKeys = null;
            /////////////////////////////////////////////////////////////////////////
            // SDL_GetKeyboardState:
            //   获取当前键盘的状态数组，返回一个指针 (IntPtr)，数组长度通过 numKeys 输出。
            //   该数组中每个元素 (byte) 对应一个 SDL_Scancode：
            //     - 值为 1 表示该物理按键被按下
            //     - 值为 0 表示该物理按键未按下
            /////////////////////////////////////////////////////////////////////////
            SDLBool* keyboardState = SDL3.SDL_GetKeyboardState(numKeys);

            /////////////////////////////////////////////////////////////////////////
            // SDL_GetScancodeFromKey:
            //   将逻辑按键 (SDL_Keycode) 转换为物理按键 (SDL_Scancode)，
            //   同时输出该按键对应的修饰键状态 (SDL_Keymod)，例如：
            //     - KMOD_SHIFT 表示 Shift 键被按下
            //     - KMOD_CTRL  表示 Ctrl 键被按下
            //     - KMOD_ALT   表示 Alt 键被按下
            /////////////////////////////////////////////////////////////////////////
            SDL_Scancode scancode = SDL3.SDL_GetScancodeFromKey(key, modstate);

            /////////////////////////////////////////////////////////////////////////
            // C# 的 IntPtr 无法直接索引，需要通过 unsafe 指针转换。
            // 将 keyboardState 转换为 byte* 指针，方便访问数组内容。
            /////////////////////////////////////////////////////////////////////////
            unsafe
            {
                byte* state = (byte*)keyboardState;

                /////////////////////////////////////////////////////////////////////
                // 检查对应的 scancode 是否按下：
                //   state[(int)scancode] == 1 表示该按键被按下
                //   state[(int)scancode] == 0 表示该按键未被按下
                /////////////////////////////////////////////////////////////////////
                bool isPressed = state[(int)scancode] != 0;

                /////////////////////////////////////////////////////////////////////
                // 如果该按键被按下，并且修饰键中包含 KMOD_SHIFT，
                // 则输出提示信息："Shift key is pressed"
                /////////////////////////////////////////////////////////////////////
                if (isPressed)
                {
                    if (((int)modstate & (int)SDL_Keymod.SDL_KMOD_SHIFT) != 0)
                    {
                        Console.WriteLine("Shift key is pressed");
                    }
                }

                /////////////////////////////////////////////////////////////////////
                // 返回该按键是否被按下的布尔值
                /////////////////////////////////////////////////////////////////////
                return isPressed;
            }
        }

        //UI相关
        private SDL_Texture* square_;
        private string title_ = "All Heroes in Kam Yung Stories";

        /// <summary>
        /// 创建一个方形纹理，并在其中绘制基于 cos 曲线的 alpha 渐变。
        /// </summary>
        /// <param name="size">方形的边长</param>
        /// <returns>SDL.Texture —— SDL3 的纹理对象</returns>
        public SDL_Texture* createSquareTexture(int size)
        {
            int d = size;

            /////////////////////////////////////////////////////////////////////////
            // SDL_CreateRGBSurface:
            //   在 SDL2 里是 SDL_CreateRGBSurface，
            //   在 SDL3 中用 SDL.CreateSurface。
            //   这里我们创建一个 32 位 RGBA Surface。
            /////////////////////////////////////////////////////////////////////////
            SDL_Surface* square_s = SDL3.SDL_CreateSurface(
                d, d,
                SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888
            );

            if ((nint)square_s == IntPtr.Zero)
            {
                throw new Exception($"Failed to create surface: {SDL3.SDL_GetError()}");
            }

            /////////////////////////////////////////////////////////////////////////
            // 循环填充像素：
            //   - 使用 SDL.FillRect 在每个像素位置绘制颜色
            //   - alpha 值基于 cos 函数渐变
            /////////////////////////////////////////////////////////////////////////
            SDL_Rect r = new SDL_Rect { w = 1, h = 1 };
            for (int x = 0; x < d; x++)
            {
                for (int y = 0; y < d; y++)
                {
                    r.x = x;
                    r.y = y;

                    // 计算 alpha 值 (100 ~ 250)
                    byte a = (byte)(100 + 150 * Math.Cos(Math.PI * ((1.0 * y / d) - 0.5)));

                    // 颜色：白色 (0xFFFFFF) + alpha 通道
                    uint c = 0x00FFFFFF | ((uint)a << 24);

                    SDL3.SDL_FillSurfaceRect(square_s, &r, c);
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 从 Surface 创建 Texture
            /////////////////////////////////////////////////////////////////////////
            SDL_Texture* texture = SDL3.SDL_CreateTextureFromSurface(renderer_, square_s);
            if ((IntPtr)texture == IntPtr.Zero)
            {
                SDL3.SDL_DestroySurface(square_s);
                throw new Exception($"Failed to create texture: {SDL3.SDL_GetError()}");
            }

            /////////////////////////////////////////////////////////////////////////
            // 设置纹理混合模式和透明度
            /////////////////////////////////////////////////////////////////////////
            SDL3.SDL_SetTextureBlendMode(texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            SDL3.SDL_SetTextureAlphaMod(texture, 128);

            /////////////////////////////////////////////////////////////////////////
            // 释放临时 Surface
            /////////////////////////////////////////////////////////////////////////
            SDL3.SDL_DestroySurface(square_s);

            return texture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fontname"></param>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="alpha"></param>
        /// <param name="align">-1左对齐，1右对齐，0居中</param>
        /// <param name="c"></param>
        public void drawText(string fontname, string text, int size, int x, int y, byte alpha, Align align, SDL_Color c)
        {
            if (alpha == 0)
            {
                return;
            }
            var text_t = createTextTexture(fontname, text, size, c);
            if (text_t == null) { return; }
            SDL3.SDL_SetTextureAlphaMod(text_t, alpha);
            SDL_FRect fRect;
            SDL3.SDL_GetTextureSize(text_t, &fRect.w, &fRect.h);
            fRect.y = (float)y;
            switch (align)
            {
                case Align.Left:
                    fRect.x = x;
                    break;
                case Align.Right:
                    fRect.x = x - fRect.w;
                    break;
                case Align.Center:
                    fRect.x = x - fRect.w / 2;
                    break;
            }
            SDL3.SDL_RenderTexture(renderer_, text_t, null, &fRect);
            SDL3.SDL_DestroyTexture(text_t);
        }

        public void drawSubtitle(string fontname, string text, int size, int x, int y, byte alpha, Align align)
        {
            if (alpha == 0)
            {
                return;
            }
            // 加载字体
            TTF_Font* font = SDL3_ttf.TTF_OpenFont(fontname, size);
            if (font == null)
            {
                Console.WriteLine($"Failed to load font: {SDL3.SDL_GetError()}");
                return;
            }

            SDL_Color color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
            SDL_Color colorb = new SDL_Color() { r = 0, g = 0, b = 0, a = 255 };
            string[] ret = text.Split("\n");
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i] == "")
                {
                    continue;
                }
                SDL3_ttf.TTF_SetFontOutline(font, 2);
                var text_sb = SDL3_ttf.TTF_RenderText_Blended(font, ret[i].ToString(), (nuint)text.Length, colorb);

                SDL3_ttf.TTF_SetFontOutline(font, 0);
                var text_s = SDL3_ttf.TTF_RenderText_Blended(font, ret[i].ToString(), (nuint)text.Length, colorb);
                //SDL_SetTextureAlphaMod(text_t, alpha);
                SDL_Rect rectb = new SDL_Rect { x = 2, y = 2, w = 0, h = 0 };
                SDL3.SDL_BlitSurface(text_s, null, text_sb, &rectb);

                var text_t = SDL3.SDL_CreateTextureFromSurface(renderer_, text_sb);

                SDL3.SDL_DestroySurface(text_s);
                SDL3.SDL_DestroySurface(text_sb);

                SDL_FRect fRect;
                SDL3.SDL_GetTextureSize(text_t, &fRect.w, &fRect.h);
                fRect.y = y + i * (size + 2);

                switch (align)
                {
                    case Align.Left:
                        fRect.x = x;
                        break;
                    case Align.Right:
                        fRect.x = x - fRect.w;
                        break;
                    case Align.Center:
                        fRect.x = x - fRect.w / 2;
                        break;
                }

                SDL3.SDL_RenderTexture(renderer_, text_t, null, &fRect);
                SDL3.SDL_DestroyTexture(text_t);
            }
            SDL3_ttf.TTF_CloseFont(font);
        }

        public SDL_Texture* createTextTexture(string fontname, string text, int size, SDL_Color c)
        {
            SDL3.SDL_SetHint(SDL3.SDL_HINT_RENDER_LINE_METHOD, "1");
            // 加载字体
            TTF_Font* font = SDL3_ttf.TTF_OpenFont(fontname, size);
            if (font == null)
            {
                Console.WriteLine($"Failed to load font: {SDL3.SDL_GetError()}");
                return null;
            }

            //SDL_Surface* surface = SDL3_ttf.TTF_RenderText_Solid(font, text, 0, c);            //快速，但是质量差
            SDL_Surface* surface = SDL3_ttf.TTF_RenderText_Blended(font, text, 0, c);            //质量好，但是慢
            if (surface == null)
            {
                Console.WriteLine($"Failed to render text: {SDL3.SDL_GetError()}");
                SDL3_ttf.TTF_CloseFont(font);
                return null;
            }
            // 创建纹理
            SDL_Texture* texture = SDL3.SDL_CreateTextureFromSurface(renderer_, surface);
            SDL3.SDL_DestroySurface(surface);
            SDL3_ttf.TTF_CloseFont(font);
            if (texture == null)
            {
                Console.WriteLine($"Failed to create texture from surface: {SDL3.SDL_GetError()}");
                return null;
            }
            return texture;

        }



        /// <summary>
        /// 显示消息框函数
        /// </summary>
        /// <param name="content">要显示的内容</param>
        /// <returns>用户点击的按钮 ID</returns>
        public int showMessage(string content)
        {
            
            string str1 = "no", str2 = "yes", str3 = "cancel";
            fixed (byte* noBytePtr = Encoding.UTF8.GetBytes(str1 + "\0"))
            fixed (byte* yesBytePtr = Encoding.UTF8.GetBytes(str2 + "\0"))
            fixed (byte* cancelBytePtr = Encoding.UTF8.GetBytes(str3 + "\0"))
            fixed (byte* title_Ptr = Encoding.UTF8.GetBytes(title_ + "\0"))
            fixed (byte* contentPtr = Encoding.UTF8.GetBytes(content + "\0"))
            {
                // 定义按钮数组
                SDL_MessageBoxButtonData* buttons = stackalloc SDL_MessageBoxButtonData[3];
                buttons[0] = new SDL_MessageBoxButtonData { buttonID = 0, flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT, text = noBytePtr };
                buttons[1] = new SDL_MessageBoxButtonData { buttonID = 1, flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_RETURNKEY_DEFAULT, text = noBytePtr };
                buttons[2] = new SDL_MessageBoxButtonData { buttonID = 2, flags = SDL_MessageBoxButtonFlags.SDL_MESSAGEBOX_BUTTON_ESCAPEKEY_DEFAULT, text = noBytePtr };


                // 定义颜色方案
                SDL_MessageBoxColorScheme colorScheme = new SDL_MessageBoxColorScheme();
                SDL_MessageBoxColor* pColors = (SDL_MessageBoxColor*)Unsafe.AsPointer(ref colorScheme.colors.e0);
                pColors[0] = new SDL_MessageBoxColor { r = 255, g = 0, b = 0 };     // 背景
                pColors[1] = new SDL_MessageBoxColor { r = 0, g = 255, b = 0 };     // 文本
                pColors[2] = new SDL_MessageBoxColor { r = 255, g = 255, b = 0 };   // 按钮边框
                pColors[3] = new SDL_MessageBoxColor { r = 0, g = 0, b = 255 };     // 按钮背景
                pColors[4] = new SDL_MessageBoxColor { r = 255, g = 0, b = 255 };   // 按钮选中

                // 组装消息框数据
                SDL_MessageBoxData messageboxdata = new SDL_MessageBoxData
                {
                    flags = SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION,
                    window = null, // 这里没有指定窗口
                    title = title_Ptr,
                    message = contentPtr,
                    numbuttons = 3,
                    buttons = buttons,
                    colorScheme = &colorScheme
                };

                // 调用 SDL3 API 显示消息框
                int* buttonid = null;
                if (SDL3.SDL_ShowMessageBox(&messageboxdata, buttonid) == false)
                {
                    Console.WriteLine($"显示消息框失败: {SDL3.SDL_GetError()}");
                    return -1;
                }
                return *buttonid;
            }
        }

        public void renderSquareTexture(SDL_Rect* rect, SDL_Color color, byte alpha)
        {
            setColor(square_, color, alpha);
            renderCopy(square_,null,rect, 0);
        }




















        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="handle">句柄</param>
        /// <returns></returns>
        public int init(void* handle = null)
        {
            if (SDL3.SDL_Init((SDL_InitFlags)SDL3.SDL_INIT_VIDEO) == false)
            {
                Console.WriteLine($"Init failed: {SDL3.SDL_GetError()}");
                return -1;
            }

            window_ = SDL3.SDL_CreateWindow("BigPotPlayer",
                    start_w_, start_h_,
                    SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            SDL3.SDL_SetWindowPosition(window_, (int)SDL3.SDL_WINDOWPOS_CENTERED, (int)SDL3.SDL_WINDOWPOS_CENTERED);


            SDL3.SDL_ShowWindow(window_);
            SDL3.SDL_RaiseWindow(window_);

            // 创建硬件加速渲染器，并允许渲染到纹理
            renderer_ = SDL3.SDL_CreateRenderer(window_, (byte*)null);

            // 启用文件拖放事件
            SDL3.SDL_SetEventEnabled(SDL_EventType.SDL_EVENT_DROP_FILE, true);

            // 在初始化SDL之前设置应用程序名称
            SDL3.SDL_SetHint(SDL3.SDL_HINT_APP_NAME, "My SDL3 Game");

            // 设置渲染缩放质量为线性插值
            if (!SDL3.SDL_SetHint(SDL3.SDL_HINT_RENDER_LINE_METHOD, "linear"))
            {
                Console.WriteLine($"Init failed: {SDL3.SDL_GetError()}");
                return -1;
            }

            // 设置渲染区域
            rect_ = new SDL_Rect { x = 0, y = 0, w = start_w_, h = start_h_ };
            // 加载 Logo 贴图（你需要实现 LoadImage 函数）
            string logoPath = Path.Combine("..", "game","resource","title","logo.png");
            logo_ = loadImage(logoPath);
            // 显示 logo 并呈现
            showLogo();
            SDL3.SDL_RenderPresent(renderer_);

            // 初始化 TTF 字体库（保持一致）
            SDL3_ttf.TTF_Init();


            // 获取第 0 个显示器的边界
            SDL_Rect displayBounds;
            int* count = null;
            SDL_DisplayID* ids = SDL3.SDL_GetDisplays(count);
            if (ids != null)
            {
                SDL3.SDL_GetDisplayBounds(ids[0], &displayBounds);
            }
            else
            {
                displayBounds = new SDL_Rect
                {
                    x = 0,
                    y = 0,
                    w = 1920, // 默认宽度
                    h = 1080 // 默认高度
                };
            }
            min_x_ = displayBounds.x;
            min_y_ = displayBounds.y;
            max_x_ = displayBounds.x + displayBounds.w;
            max_y_ = displayBounds.y + displayBounds.h;
            Console.WriteLine($"maximum width and height are: {max_x_}, {max_y_}");


            //UI初始化
            square_ = createSquareTexture(100);
            //音频部分初始化
            //Audio.getInstance().Init();

            return 0;
        }























        public void Dispose()
        {
            SDL3.SDL_Quit();
            SDL3.SDL_DestroyTexture(tex_);
            SDL3.SDL_DestroyTexture(tex2_);
            SDL3.SDL_DestroyTexture(logo_);
            SDL3.SDL_DestroyTexture(square_);
            SDL3.SDL_DestroyRenderer(renderer_);
            SDL3.SDL_DestroyWindow(window_);

            //Texture.GetInstance().Dispose();

            //SDL3.SDL_CloseAudioDevice(_audioDevice);

            isDispose = true;
        }

    }
}
