using kysSharp.Types;
using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace kysSharp
{
    internal class SubScene : Scene
    {
        public int view_x_ = 0, view_y_ = 0;

        public int MAN_PIC_0 = 2501;            //初始场景主角图偏移量
        public int MAN_PIC_COUNT = 7;                  //单向主角图张数
        public int submap_id_;   //场景号

        SubMapInfo submap_info_;

        int exit_music_;

        int force_man_pic_ = -1;


        private SubScene()
        {
            full_window_ = true;
            COORD_COUNT = Constant.SUBMAP_COORD_COUNT;
        }
        public SubScene(int id)
        {
            full_window_ = true;
            COORD_COUNT = Constant.SUBMAP_COORD_COUNT;
            setID(id);
        }

        public SubMapInfo getMapInfo() { return submap_info_; }

        public void changeExitMusic(int m) { exit_music_ = m; }

        //第一类事件，主动触发
        bool checkEvent1(int x, int y, Towards tw) { return checkEvent(x, y, tw, -1); }
        //第二类事件，物品触发
        bool checkEvent2(int x, int y, Towards tw, int item_id) { return checkEvent(x, y, tw, item_id); }
        //第三类事件，经过触发
        bool checkEvent3(int x, int y) { return checkEvent(x, y, Towards.None, -1); }

        private void setID(int id)
        {
            submap_id_ = id;
            submap_info_ = Save.getInstance().GetSubMapInfo(submap_id_);
            if (submap_info_ == null) { setExit(true); }
            //submap_info_->ID = submap_id_;   //这句是修正存档中可能存在的错误
            if (submap_info_ != null)
            {
                exit_music_ = submap_info_.ExitMusic;
                if (submap_info_.EntranceMusic > 0)
                {
                    Audio.getInstance().playMusic(submap_info_.EntranceMusic);
                }
                Console.WriteLine("Sub Scene %d, %s\n", submap_id_, submap_info_.Name);
            }
        }

        //注意视角和主角的位置可能不一样
        public void setViewPosition(int x, int y) { view_x_ = x; view_y_ = y; }
        public void setManViewPosition(int x, int y) { setManPosition(x, y); setViewPosition(x, y); }
        public void forceManPic(int pic) { force_man_pic_ = pic; }

        public override void draw()
        {
            ///////////////////////////////////////////////////////////////////////
            /// 设置辅助渲染纹理并清空画面
            /// --------------------------------------------------------------
            /// 本步骤将渲染目标切换到辅助纹理（RenderAssistTexture），
            /// 所有后续绘制操作都将在离屏纹理上进行，而非直接显示到窗口。
            /// fillColor 用于清空离屏缓冲区，作为新一帧的绘制起点。
            ///////////////////////////////////////////////////////////////////////
            Engine.getInstance().setRenderAssistTexture();
            Engine.getInstance().fillColor(new SDL_Color() { r = 0, g = 0, b = 0, a = 255 }, 0, 0, render_center_x_ * 2, render_center_y_ * 2);

            //鼠标的位置
            var position_mouse = getMousePosition(view_x_, view_y_);
            

            for (int sum = -view_sum_region_; sum <= view_sum_region_ + 15; sum++)
            {
                for (int i = -view_width_region_; i <= view_width_region_; i++)
                {
                    int ix = view_x_ + i + (sum / 2);
                    int iy = view_y_ - i + (sum - sum / 2);
                    var p = getPositionOnRender(ix, iy, view_x_, view_y_);
                    p.x += x_;
                    p.y += y_;
                    if (!isOutLine(ix, iy))
                    {
                        int h = submap_info_.GetBuildingHeight(ix, iy);
                        int num = submap_info_.GetEarth(ix, iy) / 2;
                        //无高度地面
                        if (num > 0 && h == 0)
                        {
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y);
                        }
                    }
                }
            }

            for (int sum = -view_sum_region_; sum <= view_sum_region_ + 15; sum++)
            {
                for (int i = -view_width_region_; i <= view_width_region_; i++)
                {
                    int ix = view_x_ + i + (sum / 2);
                    int iy = view_y_ - i + (sum - sum / 2);
                    var p = getPositionOnRender(ix, iy, view_x_, view_y_);
                    p.x += x_;
                    p.y += y_;
                    if (!isOutLine(ix, iy))
                    {
                        //有高度地面
                        int h = submap_info_.GetBuildingHeight(ix, iy);
                        int num = submap_info_.GetEarth(ix, iy) / 2;
                        if (num > 0 && h > 0)
                        {
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y);
                        }
                        //鼠标位置
                        if (ix == position_mouse.x && iy == position_mouse.y)
                        {
                            TextureManager.getInstance().renderTexture("mmap", 1, p.x, p.y - h, new SDL.SDL_Color() { r=255,g=255,b=255,a=255}, 128);
                        }
                        //建筑和主角
                        num = submap_info_.GetBuilding(ix, iy) / 2;
                        if (num > 0)
                        {
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y - h);
                        }
                        if (ix == man_x_ && iy == man_y_)
                        {
                            //此处当主角的贴图为负值时，表示强制设置贴图号
                            if (force_man_pic_ < 0)
                            {
                                man_pic_ = calManPic();
                            }
                            else
                            {
                                man_pic_ = force_man_pic_;
                            }
                            TextureManager.getInstance().renderTexture("smap", man_pic_, p.x, p.y - h);
                        }
                        //事件
                        var eventData = submap_info_.Event(ix, iy);
                        if(eventData != null)
                        {
                            num = eventData.CurrentPic / 2;
                            if (num > 0)
                            {
                                TextureManager.getInstance().renderTexture("smap", num, p.x, p.y - h);
                            }
                        }
                        //装饰
                        num = submap_info_.GetDecoration(ix, iy) / 2;
                        if (num > 0)
                        {
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y - submap_info_.GetDecorationHeight(ix, iy));
                        }
                    }
                }
            }

            ///////////////////////////////////////////////////////////////////////
            /// 恢复默认渲染目标并显示画面
            ///////////////////////////////////////////////////////////////////////
            Engine.getInstance().renderAssistTextureToWindow(); 
        }

        //每个方向的第一张是静止图
        public int calManPic() { return MAN_PIC_0 + (int)towards_ * MAN_PIC_COUNT + step_; } 

        public override void dealEvent(SDL_Event e)
        {
            int x = man_x_, y = man_y_;

            checkEvent3(x, y);
            if (isExit(x, y) || isJumpSubScene())
            {
                way_que_.Clear();
                clearEvent(e);
                total_step_ = 0;
            }

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

            if (pressed != 0)
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

            if(e.type == (uint)SDL_EventType.SDL_EVENT_KEY_UP && (e.key.key == SDL_Keycode.SDLK_RETURN || e.key.key==SDL_Keycode.SDLK_SPACE))
            {

                if (checkEvent1(x, y, towards_))
                {
                    clearEvent(e);
                    step_ = 0;
                }
            }

        }

        public override void onEntrance()
        {
            calViewRegion();
            towards_ = MainScene.getInstance().towards_;
        }

        public override void onExit()
        {
            if (exit_music_ > 0)
            {
                Audio.getInstance().playMusic(exit_music_);
            }
        }

        //冗余过多待清理
        public void TryWalk(int x, int y)
        {
            if (canWalk(x, y))
            {
                man_x_ = x;
                man_y_ = y;
                view_x_ = x;
                view_y_ = y;
            }
            step_++;
            if (step_ >= MAN_PIC_COUNT)
            {
                step_ = 1;
            }
        }

        public bool checkEvent(int x, int y, Towards tw /*= None*/, int item_id /*= -1*/)
        {
            getTowardsPosition(man_x_, man_y_, tw, ref x, ref y);
            int event_index_submap = submap_info_.GetEventIndex(x, y);
            if (event_index_submap >= 0)
            {
                int id = 0;
                var eventObj = submap_info_?.Event(x, y);
                if (tw != Towards.None)
                {
                    if (item_id < 0)
                    {
                        if (eventObj != null)
                        {
                            id = eventObj.Event1;
                        }
                    }
                    else
                    {
                        if (eventObj != null)
                        {
                            id = eventObj.Event2;
                        }
                    }
                    if (id > 0) { step_ = 0; }
                }
                else
                {
                    if (eventObj != null)
                    {
                        id = eventObj.Event3;
                    }
                }
                if (id > 0)
                {
                    var info = submap_info_ ;
                    if (info?.ID != null)
                    {
                        return Event.getInstance().CallEvent(id, this, info.ID, item_id, event_index_submap, x, y);
                    }
                }
            }
            return false;
        }

        public override bool canWalk(int x, int y)
        {
            //if (checkEntrance(x, y, true))
            //{
            //    return true;
            //}  这里不需要加，实际上入口都是无法走到的

            if (isOutLine(x, y))
            {
                return false;
            }
            if (isBuilding(x, y)/*|| checkIsWater(x, y)*/)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool isBuilding(int x, int y)
        {
            return submap_info_.GetBuilding(x, y) > 0;
        }

        public bool isWater(int x, int y)
        {
            int num = submap_info_.GetEarth(x, y) / 2;
            if (num >= 179 && num <= 181
                || num == 261 || num == 511
                || num >= 662 && num <= 665
                || num == 674)
            {
                return true;
            }
            return false;
        }

        public bool isCanPassEvent(int x, int y)
        {
            /*
            var e = submap_info_.GetEventIndex(x, y);
            if (e && !e.CannotWalk)
            {
                return true;
            }
            */
            return false;
        }

        public bool isCannotPassEvent(int x, int y)
        {
            /*
            var e = submap_info_.GetEventIndex(x, y);
            if (e && e.CannotWalk)
            {
                return true;
            }
            */
            return false;
        }

        public bool isExit(int x, int y)
        {
            if (submap_info_.ExitX[0] == x && submap_info_.ExitY[0] == y
                || submap_info_.ExitX[1] == x && submap_info_.ExitY[1] == y
                || submap_info_.ExitX[2] == x && submap_info_.ExitY[2] == y)
            {
                setExit(true);
                return true;
            }
            return false;
        }

        public bool isJumpSubScene()
        {
            if (submap_info_.JumpSubMap >= 0 && man_x_ == submap_info_.JumpX && man_y_ == submap_info_.JumpY)
            {
                int x, y;
                var new_submap = Save.getInstance().GetSubMapInfo(submap_info_.JumpSubMap);
                if (submap_info_.MainEntranceX1 != 0)
                {
                    //若原场景在大地图上有正常入口，则设置人物位置为新场景入口位置
                    x = new_submap.EntranceX;
                    y = new_submap.EntranceY;
                }
                else
                {
                    //若原场景无法从大地图上进入，则设置人物在跳转返回位置
                    x = new_submap.JumpReturnX;
                    y = new_submap.JumpReturnY;
                }
                forceJumpSubScene(submap_info_.JumpSubMap, x, y);
                return true;
            }
            return false;
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

        public Point getPositionOnWholeEarth(int x, int y)
        {
            var p = getPositionOnRender(x, y, 0, 0);
            p.x += COORD_COUNT * TILE_W - render_center_x_;
            p.y += 2 * TILE_H - render_center_y_;
            return p;
        }

        public void forceExit()
        {
            setVisible(false);
            setExit(true);
        }

        public void forceJumpSubScene(int submap_id, int x, int y)
        {
            setID(submap_id);
            setManViewPosition(x, y);
        }












    }
}
