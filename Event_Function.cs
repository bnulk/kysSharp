using System;
using System.Collections.Generic;
using System.Text;

namespace kysSharp
{
    partial class Event
    {
        //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 原对话指令（C# 版本）
        ///
        /// 对应 C++ 的：
        /// void Event::oldTalk(int talk_id, int head_id, int style)
        /// 实现角色头像、台词、对话框样式的切换。
        /// </summary>
        //////////////////////////////////////////////////////////////////////
        private void oldTalk(int talkId, int headId, int style)
        {
            ///////////////////////////////////////////////////////////////////////
            // 选择上方或下方对话框
            ///////////////////////////////////////////////////////////////////////
            Talk? talk;
            if (style % 2 == 0)
            {
                talk = talk_box_up_;
            }
            else
            {
                talk = talk_box_down_;
            }

            if (talk is null)
            {
                Console.WriteLine("[警告] 对话框对象未初始化。");
                return; // 安全退出
            }

            ///////////////////////////////////////////////////////////////////////
            // 设置对话内容
            ///////////////////////////////////////////////////////////////////////
            talk.setContent(talk_[talkId]);
            Console.WriteLine(talk_[talkId]);  // 控制台输出（调试用）

            ///////////////////////////////////////////////////////////////////////
            // 设置头像 ID（head_id）
            ///////////////////////////////////////////////////////////////////////
            talk.setHeadID(headId);

            // 若 style 为 2 或 3，则隐藏头像
            if (style == 2 || style == 3)
            {
                talk.setHeadID(-1);
            }

            ///////////////////////////////////////////////////////////////////////
            // 设置头像样式（左右朝向）
            ///////////////////////////////////////////////////////////////////////
            // 0/5 表示左侧头像
            if (style == 0 || style == 5)
            {
                talk.setHeadStyle(0);
            }

            // 4/1 表示右侧头像
            if (style == 4 || style == 1)
            {
                talk.setHeadStyle(1);
            }

            ///////////////////////////////////////////////////////////////////////
            // 执行对话
            ///////////////////////////////////////////////////////////////////////
            talk.run(false);
        }

        //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 播放动画（C# 版本）
        ///
        /// 对应 C++：
        /// void Event::playAnimation(int event_index, int begin_pic, int end_pic)
        ///
        /// 功能说明：
        /// - 若 event_index == -1，则播放主角动画（通过 forceManPic 切换帧）
        /// - 否则播放指定事件的动画（通过 setPic 切换帧）
        /// - 每帧绘制并刷新场景
        /// </summary>
        //////////////////////////////////////////////////////////////////////
        private void playAnimation(int eventIndex, int beginPic, int endPic)
        {
            ///////////////////////////////////////////////////////////////////////
            // 空值检查（确保子场景存在）
            ///////////////////////////////////////////////////////////////////////
            if (subscene_ is null)
                return;

            ///////////////////////////////////////////////////////////////////////
            // 计算帧变化方向 (+1 或 -1)
            ///////////////////////////////////////////////////////////////////////
            int inc = Math.Sign(endPic - beginPic);
            if (inc == 0)
                return; // 起止帧相同，无需播放

            ///////////////////////////////////////////////////////////////////////
            // 分支一：eventIndex == -1 表示主角动画
            ///////////////////////////////////////////////////////////////////////
            if (eventIndex == -1)
            {
                for (int i = beginPic / 2; i != endPic / 2; i += inc)
                {
                    subscene_.forceManPic(i);
                    subscene_.drawAndPresent();
                }

                subscene_.forceManPic(endPic / 2);
                subscene_.drawAndPresent();

                // 动画播放结束，重置主角贴图
                subscene_.forceManPic(-1);
            }
            ///////////////////////////////////////////////////////////////////////
            // 分支二：播放地图事件动画
            ///////////////////////////////////////////////////////////////////////
            else
            {
                var mapInfo = subscene_.getMapInfo();
                var e = mapInfo?.Event(eventIndex);

                if (e is null)
                    return; // 没有对应事件，直接退出

                for (int i = beginPic; i != endPic; i += inc)
                {
                    e.SetPic(i);
                    subscene_.drawAndPresent();
                }

                // 最后一帧
                e.SetPic(endPic);
            }
        }

        //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 修改事件定义（C# 版本）
        ///
        /// 对应 C++：
        /// void Event::modifyEvent(int submap_id, int event_index, ... )
        ///
        /// 功能说明：
        /// - 修改子地图中特定事件的参数（位置、贴图、事件索引等）
        /// - 参数为 -2 表示“保持原值不修改”
        /// - 参数为 -1 时，根据逻辑情况取默认值
        /// </summary>
        //////////////////////////////////////////////////////////////////////
        private void modifyEvent(
            int submapId,
            int eventIndex,
            int cannotWalk,
            int index,
            int event1,
            int event2,
            int event3,
            int currentPic,
            int endPic,
            int beginPic,
            int picDelay,
            int x,
            int y)
        {
            ///////////////////////////////////////////////////////////////////////
            // 处理默认子地图 ID
            ///////////////////////////////////////////////////////////////////////
            if (submapId < 0)
                submapId = submap_id_;

            if (submapId < 0)
                return; // 子地图 ID 无效

            ///////////////////////////////////////////////////////////////////////
            // 处理默认事件索引
            ///////////////////////////////////////////////////////////////////////
            if (eventIndex < 0)
                eventIndex = event_index_;

            ///////////////////////////////////////////////////////////////////////
            // 获取事件对象
            ///////////////////////////////////////////////////////////////////////
            var subMapInfo = Save.getInstance().GetSubMapInfo(submapId);
            var e = subMapInfo?.Event(eventIndex);

            if (e is null)
                return; // 无对应事件则直接退出

            ///////////////////////////////////////////////////////////////////////
            // 修改属性（值为 -2 表示不修改）
            ///////////////////////////////////////////////////////////////////////
            if (cannotWalk >= -1) e.CannotWalk = (short)cannotWalk;
            if (index >= -1) e.Index = (short)index;
            if (event1 >= -1) e.Event1 = (short)event1;
            if (event2 >= -1) e.Event2 = (short)event2;
            if (event3 >= -1) e.Event3 = (short)event3;
            if (currentPic >= -1) e.CurrentPic = (short)currentPic;
            if (endPic >= -1) e.EndPic = (short)endPic;
            if (beginPic >= -1) e.BeginPic = (short)beginPic;
            if (picDelay >= -1) e.PicDelay = (short)picDelay;

            ///////////////////////////////////////////////////////////////////////
            // 修正坐标参数（若为 -1 以下，则保持原值）
            ///////////////////////////////////////////////////////////////////////
            if (x < -1) x = e.X();
            if (y < -1) y = e.Y();

            ///////////////////////////////////////////////////////////////////////
            // 更新事件位置
            ///////////////////////////////////////////////////////////////////////
            if (subMapInfo == null) return;
            else
            {
                e.SetPosition(x, y, subMapInfo);
            }  
        }

        public void askSoftStar()
        {
            Random random = new Random();
            oldTalk(2547 + random.Next(18), 114, 0);
        }













































    }
}
