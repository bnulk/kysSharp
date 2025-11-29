using SDL;

namespace kysSharp
{
    public class Scene : Element
    {
        public int render_center_x_ = 0;
        public int render_center_y_ = 0;

        public const int TILE_W = 18;  //小图块大小X
        public const int TILE_H = 9;   //小图块大小Y

        //确定视野使用
        public int view_width_region_ = 0;
        public int view_sum_region_ = 0;

        public int total_step_ = 0;         //键盘走路的计数
        public SDL_Keycode pre_pressed_;    //键盘走路的上次按键

        public int man_x_, man_y_;
        public int mouse_event_x_ = -1, mouse_event_y_ = -1;    //鼠标行路时的最终目标，可能为事件或者入口
        public Towards towards_ = Towards.RightUp;              //朝向，共用一个即可
        public int step_ = 0;
        public int man_pic_ = 0;

        public int COORD_COUNT = 0;

        public List<PointEx> way_que_ = new List<PointEx>();      //栈(路径栈)
        public int min_step_;                                     //起点(Mx,My),终点(Fx,Fy),最少移动次数minStep

        public virtual bool canWalk(int x, int y) { return false; }
        public virtual bool isOutScreen(int x, int y) { return false; }



        public void calViewRegion()
        {
            Engine.getInstance().getMainTextureSize(ref render_center_x_, ref render_center_y_);
            render_center_x_ /= 2;
            render_center_y_ /= 2;
            view_width_region_ = render_center_x_ / TILE_W / 2 + 3;
            view_sum_region_ = render_center_y_ / TILE_H + 2;
        }

        public void setManPosition(int x, int y) { man_x_ = x; man_y_ = y; }
        public void getManPosition(ref int x, ref int y) { x = man_x_; y = man_y_; }
        public void setManPic(int pic) { man_pic_ = pic; }
        public void checkWalk(int x, int y, SDL_Event e)   //一些公共部分，未完成
        {
        }

        /// <summary>
        /// 后面两个参数是当前屏幕中心位置的游戏坐标，通常是人的坐标
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="view_x">屏幕中心位置的游戏坐标，通常是人的坐标</param>
        /// <param name="view_y">屏幕中心位置的游戏坐标，通常是人的坐标</param>
        /// <returns></returns>
        public Point getPositionOnRender(int x, int y, int view_x, int view_y)
        {
            Point p= new Point();
            x = x - view_x;
            y = y - view_y;
            p.x = -y * TILE_W + x * TILE_W + render_center_x_;
            p.y = y * TILE_H + x * TILE_H + render_center_y_;
            return p;
        }

        /// <summary>
        /// 后面两个参数同上，一些情况下窗口尺寸和渲染尺寸不同
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="view_x"></param>
        /// <param name="view_y"></param>
        /// <returns></returns>
        public Point getPositionOnWindow(int x, int y, int view_x, int view_y)
        {
            var p = getPositionOnRender(x, y, view_x, view_y);
            int w=0, h=0;
            Engine.getInstance().getPresentSize(ref w, ref h);
            p.x = p.x * w / render_center_x_ / 2;
            p.y = p.y * h / render_center_y_ / 2;
            return p;
        }

        /// <summary>
        /// 角色处于x1，y1，朝向x2，y2时，脸的方向
        /// </summary>
        /// <param name="x1">角色x</param>
        /// <param name="y1">角色y</param>
        /// <param name="x2">朝向x</param>
        /// <param name="y2">朝向y</param>
        /// <returns></returns>
        public Towards calTowards(int x1, int y1, int x2, int y2)
        {
            int d1, d2, dm;
            d1 = y2 - y1;
            d2 = x2 - x1;
            dm = Math.Abs(d1) - Math.Abs(d2);
            if ((d1 != 0) || (d2 != 0))
            {
                if (dm >= 0)
                {
                    if (d1 < 0)
                    {
                        return Towards.RightUp;
                    }
                    else
                    {
                        return Towards.LeftDown;
                    }
                }
                else
                {
                    if (d2 < 0)
                    {
                        return Towards.LeftUp;
                    }
                    else
                    {
                        return Towards.RightDown;
                    }
                }
            }
            return Towards.None;
        }

        public void setTowards(Towards t) { towards_ = t; }

        public int calBlockTurn(int x, int y, int layer) { return 4 * (128 * (x + y) + x) + layer; }

        public void changeTowardsByKey(SDL_Keycode key)
        {
            Towards tw = getTowardsByKey(key);
            if (tw != Towards.None) { towards_ = tw; }
        }

        public Towards getTowardsByKey(SDL_Keycode key)
        {
            Towards tw = Towards.None;
            switch (key)
            {
                case (SDL_Keycode)SDL3.SDLK_LEFT: tw = Towards.LeftUp; break;
                case (SDL_Keycode)SDL3.SDLK_RIGHT: tw = Towards.RightDown; break;
                case (SDL_Keycode)SDL3.SDLK_UP: tw = Towards.RightUp; break;
                case (SDL_Keycode)SDL3.SDLK_DOWN: tw = Towards.LeftDown; break;
            }
            return tw;
        }

        public Towards getTowardsByMouse(int mouse_x, int mouse_y)
        {
            int w=0, h=0;
            Engine.getInstance().getPresentSize(ref w, ref h);
            mouse_x = mouse_x * render_center_x_ * 2 / w;
            mouse_y = mouse_y * render_center_y_ * 2 / h;
            if (mouse_x < render_center_x_ && mouse_y < render_center_y_)
            {
                return Towards.LeftUp;
            }
            if (mouse_x < render_center_x_ && mouse_y > render_center_y_)
            {
                return Towards.LeftDown;
            }
            if (mouse_x > render_center_x_ && mouse_y < render_center_y_)
            {
                return Towards.RightUp;
            }
            if (mouse_x > render_center_x_ && mouse_y > render_center_y_)
            {
                return Towards.RightDown;
            }
            return Towards.None;
        }

        /// <summary>
        /// 获取面向一格的坐标
        /// </summary>
        /// <param name="x0">现在位置x</param>
        /// <param name="y0">现在位置y</param>
        /// <param name="tw">人的朝向</param>
        /// <param name="x1">面向一格x</param>
        /// <param name="y1">面向一格y</param>
        public static void getTowardsPosition(int x0, int y0, Towards tw, ref int x1, ref int y1)
        {
            if (tw == Towards.None) { return; }
            x1 = x0;
            y1 = y0;
            switch (tw)
            {
                case Towards.LeftUp: x1--; break;
                case Towards.RightDown: x1++; break;
                case Towards.RightUp: y1--; break;
                case Towards.LeftDown: y1++; break;
            }
        }

        public Point getMousePosition(int view_x, int view_y)
        {
            int mouse_x=0, mouse_y=0;
            Engine.getInstance().getMouseState(ref mouse_x, ref mouse_y);
            return getMousePosition(mouse_x, mouse_y, view_x, view_y);
        }

        public Point getMousePosition(int mouse_x, int mouse_y, int view_x, int view_y)
        {
            int w=0, h=0;
            Engine.getInstance().getPresentSize(ref w, ref h);
            double mouse_x1 = mouse_x * render_center_x_ * 2.0 / w;
            double mouse_y1 = mouse_y * render_center_y_ * 2.0 / h;

            //mouse_x1 += TILE_W;
            mouse_y1 += TILE_H * 2;

            Point p = new Point();
            double x = ((mouse_x1 - render_center_x_) / TILE_W + (mouse_y1 - render_center_y_) / TILE_H) / 2 + view_x;
            double y = ((-mouse_x1 + render_center_x_) / TILE_W + (mouse_y1 - render_center_y_) / TILE_H) / 2 + view_y;
            p.x=Convert.ToInt32(x);
            p.y=Convert.ToInt32(y);
            return p;
        }

        public void stopFindWay() { way_que_.Clear();/*while (!way_que_.empty()) { way_que_.pop(); }*/ }

        /////////////////////////////////////////////////////////////////////////
        // 路径搜索函数：A* 算法
        /////////////////////////////////////////////////////////////////////////
        public void FindWay(int Mx, int My, int Fx, int Fy)
        {
            /////////////////////////////////////////////////////////////////////////
            // visited: 访问标记，相当于关闭列表
            /////////////////////////////////////////////////////////////////////////
            bool[,] visited = new bool[479, 479];

            /////////////////////////////////////////////////////////////////////////
            // 四个方向：上下左右
            /////////////////////////////////////////////////////////////////////////
            int[,] dirs = new int[4, 2] { { 1, 0 }, { 0, -1 }, { 0, 1 }, { -1, 0 } };

            /////////////////////////////////////////////////////////////////////////
            // 起点节点
            /////////////////////////////////////////////////////////////////////////
            var myPoint = new PointEx
            {
                x = Mx,
                y = My,
                towards = calTowards(Mx, My, Fx, Fy),
            };
            myPoint.parent = myPoint; // 起点的父亲是自己
            myPoint.heuristic(Fx, Fy);

            /////////////////////////////////////////////////////////////////////////
            // 清空已有路径
            /////////////////////////////////////////////////////////////////////////
            way_que_.Clear();

            /////////////////////////////////////////////////////////////////////////
            // 优先队列（开启列表）
            /////////////////////////////////////////////////////////////////////////
            PriorityQueue<PointEx, int> que = new PriorityQueue<PointEx, int>();
            que.Enqueue(myPoint, myPoint.f);

            int sNum = 0;

            /////////////////////////////////////////////////////////////////////////
            // A* 主循环
            /////////////////////////////////////////////////////////////////////////
            while (que.Count > 0 && sNum <= 4096)
            {
                PointEx t = que.Dequeue();
                visited[t.x, t.y] = true;
                sNum++;

                // 如果到达终点
                if (t.x == Fx && t.y == Fy)
                {
                    min_step_ = t.step;
                    way_que_.Add(t);

                    int k = 0;
                    while (t != myPoint && k <= min_step_ && t.parent!=null)
                    {
                        t.towards = t.parent.towards;
                        way_que_.Add(t);
                        t = t.parent;
                        k++;
                    }
                    break;
                }
                else
                {
                    /////////////////////////////////////////////////////////////////////////
                    // 四个方向的扩展
                    /////////////////////////////////////////////////////////////////////////
                    for (int i = 0; i < 4; i++)
                    {
                        var s = new PointEx
                        {
                            x = t.x + dirs[i, 0],
                            y = t.y + dirs[i, 1]
                        };

                        if (canWalk(s.x, s.y) && !visited[s.x, s.y])
                        {
                            s.g = t.g + 10;
                            s.towards = (Towards)i;

                            if (s.towards == t.towards)
                            {
                                s.heuristic(Fx, Fy);
                            }
                            else
                            {
                                s.h = s.heuristic(Fx, Fy) + 1;
                            }

                            s.step = t.step + 1;
                            s.f = s.g + s.h;
                            s.parent = t;

                            que.Enqueue(s, s.f);
                        }
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 清理树（递归删除子节点）
            /////////////////////////////////////////////////////////////////////////
            myPoint.delTree(myPoint);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数名称：lightScene
        // 功能描述：实现一个“场景淡出效果”，从透明到逐渐变黑。
        // 实现原理：通过循环逐步增加黑色遮罩层的透明度（alpha 值），
        //          每次调用 drawAndPresent() 绘制并显示结果，直到全黑。
        /////////////////////////////////////////////////////////////////////////
        public void lightScene()
        {
            /////////////////////////////////////////////////////////////////////////
            // 循环次数：从 i = 10 递减到 0
            // 每一次循环，都绘制一个覆盖全屏的半透明黑色矩形
            // alpha 值由 i 控制，i 越小，黑色越透明。
            /////////////////////////////////////////////////////////////////////////
            for (int i = 10; i >= 0; i--)
            {
                /////////////////////////////////////////////////////////////////////////
                // 局部函数 fill —— 传入 drawAndPresent 作为回调
                // 作用：根据当前循环次数 i，计算 alpha 值，并绘制黑色遮罩层
                // 参数 object? _ ：
                //   - 这是为了匹配 drawAndPresent 中的 Action<object?> 委托签名
                //   - 实际上这里不需要使用传入参数，所以写成 `_`
                /////////////////////////////////////////////////////////////////////////
                void fill(object? _)
                {
                    /////////////////////////////////////////////////////////////////////////
                    // 计算透明度 alpha
                    // GameUtil.limit(x, min, max) 用来限制数值范围，避免越界
                    // i * 25：随着循环次数不同，alpha 在 0 ~ 250 之间变化
                    // (byte) 强制转换成字节，符合 SDL_Color 的 a 通道类型
                    /////////////////////////////////////////////////////////////////////////
                    byte alpha = (byte)GameUtil.limit(i * 25, 0, 255);

                    /////////////////////////////////////////////////////////////////////////
                    // 调用引擎的 fillColor 函数，绘制一个全屏矩形作为遮罩层
                    // SDL_Color 结构体：
                    //   r=0, g=0, b=0 → 黑色
                    //   a=alpha        → 透明度（由上面计算得出）
                    // 参数 (0,0,-1,-1) 表示覆盖整个屏幕
                    /////////////////////////////////////////////////////////////////////////
                    Engine.getInstance().fillColor(
                        new SDL_Color { r = 0, g = 0, b = 0, a = alpha },
                        0, 0, -1, -1
                    );
                }

                /////////////////////////////////////////////////////////////////////////
                // 调用 drawAndPresent
                // 参数：
                //   times = 1        → 只绘制并显示一次
                //   func  = fill     → 回调函数，负责绘制黑色遮罩层
                //   data  = null     → 这里用不到 data，所以传 null
                //
                // drawAndPresent 内部会做：
                //   1. 调用 drawAll() 绘制场景
                //   2. 调用 fill() 绘制黑色遮罩层
                //   3. 调用 checkEventAndPresent() 显示到屏幕
                /////////////////////////////////////////////////////////////////////////
                drawAndPresent(1, fill, null);
            }
        }

       
        public void darkScene()
        {
            for (int i = 0; i <= 10; i++)
            {
                void fill(object? _)
                {
                    byte alpha = (byte)GameUtil.limit(i * 25, 0, 255);
                    Engine.getInstance().fillColor(
                        new SDL_Color { r = 0, g = 0, b = 0, a = alpha },
                        0, 0, -1, -1
                    );
                }
                drawAndPresent(1, fill, null);
            }
        }

        public bool isOutLine(int x, int y)
        {
            return (x < 0 || x >= COORD_COUNT || y < 0 || y >= COORD_COUNT);
        }

        public void setMouseEventPoint(int x, int y) { mouse_event_x_ = x; mouse_event_y_ = y; }












    }
}
