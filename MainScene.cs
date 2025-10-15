using kysSharp;
using kysSharp.Types;
using SDL;
using System.Drawing;

namespace kysSharp
{
    class MainScene : Scene
    {
        private static MainScene main_scene_ = new MainScene();

        public MapSquare earth_layer_, surface_layer_, building_layer_, build_x_layer_, build_y_layer_, entrance_layer_;

        public bool data_readed_ = false;

        public int rest_time_ = 0;                     //停止操作的时间

        public int MAN_PIC_0 = 2501;                   //初始主角图偏移量
        public int MAN_PIC_COUNT = 7;                  //单向主角图张数
        public int REST_PIC_0 = 2529;                  //主角休息图偏移量
        public int REST_PIC_COUNT = 6;                 //单向休息图张数
        public int SHIP_PIC_0 = 3715;                  //初始主角图偏移量
        public int SHIP_PIC_COUNT = 4;                 //单向主角图张数
        public int BEGIN_REST_TIME = 200;              //开始休息的时间
        public int REST_INTERVAL = 15;                 //休息图切换间隔

        public int force_submap_ = -1;
        public int force_submap_x_ = -1;
        public int force_submap_y_ = -1;

        Cloud.CloudTowards cloud_towards = Cloud.CloudTowards.Left;
        List<Cloud> cloud_vector_ = new List<Cloud>();

        private struct DrawInfo { public int i; public Point p; }

        public static MainScene getInstance()
        {
            if(main_scene_==null)
            {
                main_scene_ = new MainScene();
            }
            return main_scene_;
        }

        public MainScene()
        {

            full_window_ = true;
            COORD_COUNT = Constant.MAINMAP_COORD_COUNT;

            if (!data_readed_)
            {
                earth_layer_ = new MapSquare(COORD_COUNT);
                surface_layer_ = new MapSquare(COORD_COUNT);
                building_layer_ = new MapSquare(COORD_COUNT);
                build_x_layer_ = new MapSquare(COORD_COUNT);
                build_y_layer_ = new MapSquare(COORD_COUNT);

                int length = COORD_COUNT * COORD_COUNT * sizeof(MAP_INT);

                GameFile.readFile(Path.Combine("game","resource","earth.002"), out earth_layer_.data_, length / 2);
                GameFile.readFile(Path.Combine("game", "resource", "surface.002"), out surface_layer_.data_, length / 2);
                GameFile.readFile(Path.Combine("game", "resource", "building.002"), out building_layer_.data_, length / 2);
                GameFile.readFile(Path.Combine("game", "resource", "buildx.002"), out build_x_layer_.data_, length / 2);
                GameFile.readFile(Path.Combine("game", "resource", "buildy.002"), out build_y_layer_.data_, length / 2);

                Divide2(ref earth_layer_);
                Divide2(ref surface_layer_);
                Divide2(ref building_layer_);
            }
            data_readed_ = true;


            //100个云
            for (int i = 0; i < 100; i++)
            {
                var c = new Cloud();
                cloud_vector_.Add(c);
                c.initRand();
            }
        }

        public override void draw()
        {
            int k = 0;

            ///////////////////////////////////////////////////////////////////////
            /// 设置辅助渲染纹理并清空画面
            /// --------------------------------------------------------------
            /// 本步骤将渲染目标切换到辅助纹理（RenderAssistTexture），
            /// 所有后续绘制操作都将在离屏纹理上进行，而非直接显示到窗口。
            /// fillColor 用于清空离屏缓冲区，作为新一帧的绘制起点。
            ///////////////////////////////////////////////////////////////////////
            Engine.getInstance().setRenderAssistTexture();
            Engine.getInstance().fillColor(new SDL_Color() { r = 0, g = 0, b = 0, a = 255 }, 0, 0, render_center_x_ * 2, render_center_y_ * 2);

            Dictionary<int, DrawInfo> map = new Dictionary<int, DrawInfo>();
            DrawInfo tmpDrawInfo = new DrawInfo();

            for (int sum = -view_sum_region_; sum <= view_sum_region_ + 15; sum++)
            {
                for (int i = -view_width_region_; i <= view_width_region_; i++)
                {
                    int ix = man_x_ + i + (sum / 2);
                    int iy = man_y_ - i + (sum - sum / 2);
                    var p = getPositionOnRender(ix, iy, man_x_, man_y_);
                    p.x += x_;
                    p.y += y_;
                    //auto p = getMapPoint(ix, iy, *_Mx, *_My);
                    if (!isOutLine(ix, iy))
                    {
                        //共分3层，地面，表面，建筑，主角包括在建筑中
                        //调试模式下不画出地面，图的数量太多占用CPU很大

                        if (earth_layer_.GetData(ix, iy) > 0)
                        {
                            TextureManager.getInstance().renderTexture("mmap", earth_layer_.GetData(ix, iy), p.x, p.y);
                        }

                        if (surface_layer_.GetData(ix, iy) > 0)
                        {
                            TextureManager.getInstance().renderTexture("mmap", surface_layer_.GetData(ix, iy), p.x, p.y);
                        }


                        if (building_layer_.GetData(ix, iy) > 0)
                        {
                            var t = building_layer_.GetData(ix, iy);

                            //根据图片的宽度计算图的中点, 为避免出现小数, 实际是中点坐标的2倍
                            //次要排序依据是y坐标
                            //直接设置z轴                            
                            var tex = TextureManager.getInstance().loadTexture("mmap", t);

                            int w = 0, h = 0, dy = 0;
                            if (tex != null)
                            {
                                w = tex.w;
                                h = tex.h;
                                dy = tex.dy;
                            }

                            int c = ((ix + iy) - (w + 35) / 36 - (dy - h + 1) / 9) * 1024 + ix;                //生成一个绘制建筑的顺序，从上到下，从左到右。

                            tmpDrawInfo.i = t;
                            tmpDrawInfo.p = p;


                            if (map.ContainsKey(2 * c + 1) == false)
                            {
                                map.Add(2 * c + 1, tmpDrawInfo);
                            }
                            else
                            {
                                map[2 * c + 1] = tmpDrawInfo;
                            }
                        }


                        if (ix == man_x_ && iy == man_y_)
                        {
                            if (IsWater(man_x_, man_y_))
                            {
                                man_pic_ = SHIP_PIC_0 + (int)towards_ * SHIP_PIC_COUNT + step_;
                            }
                            else
                            {
                                man_pic_ = MAN_PIC_0 + (int)towards_ * MAN_PIC_COUNT + step_;  //每个方向的第一张是静止图
                                if (rest_time_ >= BEGIN_REST_TIME)
                                {
                                    man_pic_ = REST_PIC_0 + (int)towards_ * REST_PIC_COUNT + (rest_time_ - BEGIN_REST_TIME) / REST_INTERVAL % REST_PIC_COUNT;
                                }
                            }
                            int c = 1024 * (ix + iy) + ix;                                 //绘制主角的顺序，和遮挡相关。

                            tmpDrawInfo.i = man_pic_;
                            tmpDrawInfo.p = p;
                            if (map.ContainsKey(2 * c) == false)
                            {
                                map.Add(2 * c, tmpDrawInfo);
                            }
                            else
                            {
                                map[2 * c] = tmpDrawInfo;
                            }
                        }
                    }
                    k++;
                }
            }

            //按键值排序，由小到大。
            map = map.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);


            foreach (var item in map)
            {
                TextureManager.getInstance().renderTexture("mmap", item.Value.i, item.Value.p.x, item.Value.p.y);
            }

            //鼠标的位置，此处直接画到最上面了
            var pMouse = getMousePosition(man_x_, man_y_);
            pMouse = getPositionOnRender(pMouse.x, pMouse.y, man_x_, man_y_);
            TextureManager.getInstance().renderTexture("mmap", 1, pMouse.x, pMouse.y, 
                new SDL_Color() { r=255,g=255,b=255,a=255 }, 255);

            foreach (var c in cloud_vector_)
            {
                c.draw();
            }

            ///////////////////////////////////////////////////////////////////////
            /// 恢复默认渲染目标并显示画面
            ///////////////////////////////////////////////////////////////////////
            Engine.getInstance().renderAssistTextureToWindow();
        }

        public override void onEntrance()
        {
            calViewRegion();

            //云的贴图
            foreach (var c in cloud_vector_)
            {
                c.flow();
                c.SetPositionOnScreen(man_x_, man_y_, render_center_x_, render_center_y_);
            }
            
        }

        public void Divide2(ref MapSquare m)
        {
            for (int i = 0; i < m.SquareSize(); i++)
            {
                m.Data_[i] = (MAP_INT)(m.GetData(i) / 2);
            }
        }

        public override void backRun()
        {
            //云的贴图
            foreach (var c in cloud_vector_)
            {
                c.flow();
                c.SetPositionOnScreen(man_x_, man_y_, render_center_x_, render_center_y_);
            }
        }

        public override void dealEvent(SDL_Event e)
        {
            //强制进入，通常用于开始
            if (force_submap_ >= 0)
            {
                var sub_map = new SubScene(force_submap_);
                sub_map.setManViewPosition(force_submap_x_, force_submap_y_);
                sub_map.setTowards(towards_);
                sub_map.run();
                towards_ = sub_map.towards_;
                force_submap_ = -1;
                setVisible(true);
            }

            int x = man_x_, y = man_y_;

            //键盘走路部分，检测4个方向键           
            int pressed = 0;
            for (var i = (int)(SDL_Keycode.SDLK_RIGHT); i <= (int)(SDL_Keycode.SDLK_UP); i++)
            {
                if (i != (int)pre_pressed_ && Engine.getInstance().checkKeyPress((SDL_Keycode)i))
                {
                    pressed = i;
                }
            }
            if (pressed == 0 && Engine.getInstance().checkKeyPress(pre_pressed_))
            {
                pressed = (int)pre_pressed_;
            }
            pre_pressed_ = (SDL_Keycode)pressed;

            if (pressed!=0)
            {
                //注意，中间空出几个步数是为了可以单步行动，子场景同
                if (total_step_ < 1 || total_step_ >= 5)
                {
                    changeTowardsByKey((SDL_Keycode)pressed);
                    getTowardsPosition(man_x_, man_y_, towards_, ref x, ref y);
                    TryWalk(x, y);
                }
                total_step_++;
            }
            else
            {
                total_step_ = 0;
            }

            if (pressed!=0 && checkEntrance(x, y))
            {
                way_que_.Clear();
                clearEvent(e);
                total_step_ = 0;
            }

            rest_time_++;
        }

        /// <summary>
        /// 尝试走向x，y位置
        /// </summary>
        /// <param name="x">目的地x</param>
        /// <param name="y">目的地y</param>
        public void TryWalk(int x, int y)
        {
            if (canWalk(x, y))
            {
                man_x_ = x;
                man_y_ = y;
            }
            step_++;
            if (IsWater(man_x_, man_y_))
            {
                step_ = step_ % SHIP_PIC_COUNT;
            }
            else
            {
                if (step_ >= MAN_PIC_COUNT)
                {
                    step_ = 1;
                }
            }
            rest_time_ = 0;
        }

        public override void onPressedCancel()
        {
            UI.getInstance().run();
        }
        public bool IsBuilding(int x, int y)
        {
            return (building_layer_.GetData(build_x_layer_.GetData(x, y), build_y_layer_.GetData(x, y)) > 0);
        }

        public bool IsWater(int x, int y)
        {
            var pic = earth_layer_.GetData(x, y);
            if (pic == 419 || pic >= 306 && pic <= 335)
            {
                return true;
            }
            else if (pic >= 179 && pic <= 181
                || pic >= 253 && pic <= 335
                || pic >= 508 && pic <= 511)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool canWalk(int x, int y)
        {
            //if (checkEntrance(x, y, true))
            //{
            //    return true;
            //}  这里不需要加，实际上入口都是无法走到的

            if(isOutLine(x, y))
            {
                return false;
            }
            if (IsBuilding(x, y)/*|| checkIsWater(x, y)*/)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool checkEntrance(int x, int y, bool only_check = false)
        {
            /*
            for (int i = 0; i < Save.GetInstance().GetSubMapInfos().Count; i++)
            {
                var s = Save.GetInstance().GetSubMapInfo(i);
                if (x == s.MainEntranceX1 && y == s.MainEntranceY1 || x == s.MainEntranceX2 && y == s.MainEntranceY2)
                {
                    bool can_enter = false;
                    if (s.EntranceCondition == 0)
                    {
                        can_enter = true;
                    }
                    else if (s.EntranceCondition == 2)
                    {
                        //注意进入条件2的设定
                        foreach (var r in Save.GetInstance().protagonistInformation.Team)
                        {
                            if (Save.GetInstance().GetRole(r).Speed >= 70)
                            {
                                can_enter = true;
                                break;
                            }
                        }
                    }
                    if (only_check)
                    {
                        return true;
                    }
                    if (can_enter)
                    {
                        //UISave.AutoSave();
                        //这里看起来要主动多画一帧，待修
                        //DrawAndPresent();
                        var sub_map = new SubScene(i);
                        sub_map.SetManViewPosition(s.EntranceX, s.EntranceY);
                        sub_map.Run();
                        towards_ = sub_map.towards_;
                        return true;
                    }
                }
            }
            */
            return false;
        }

        /// <summary>
        /// 强制进入子场景
        /// </summary>
        /// <param name="submap_id">子场景id</param>
        /// <param name="x">位置x</param>
        /// <param name="y">位置y</param>
        public void ForceEnterSubScene(int submap_id, int x, int y)
        {
            force_submap_ = submap_id;
            if (x >= 0) { force_submap_x_ = x; }
            if (y >= 0) { force_submap_y_ = y; }
            setVisible(false);
        }

        public void SetEntrance()
        {
        }

        /// <summary>
        /// 判断是否在屏幕以外
        /// </summary>
        /// <param name="x">点x</param>
        /// <param name="y">点y</param>
        /// <returns>是否在屏幕以外</returns>
        public override bool isOutScreen(int x, int y)
        {
            return (Math.Abs(man_x_ - x) >= 2 * view_width_region_ || Math.Abs(man_y_ - y) >= view_sum_region_);
        }


        












       







    }
}
