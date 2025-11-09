using kysSharp.Types;
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

        // 获得物品，有提示
        public void addItem(int itemId, int count)
        {
            AddItemWithoutHint(itemId, count);

            // 假设 Save 是单例，getItem 返回物品，Name 是物品的名称
            var itemName = GameUtil.EraseModredundantChar(Save.getInstance().GetItem(itemId).strName);

            if(text_box_!=null)
            {
                // 格式化字符串
                text_box_.setText(string.Format("获得{0}{1}", itemName, count));

                // 设置纹理（假设 setTexture 用法类似）
                text_box_.setTexture("item", itemId);
                text_box_.run();

                // 重置纹理
                text_box_.setTexture("item", -1);
            }
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

        //是否使用了某物品
        public bool isUsingItem(int item_id)
        {
            return item_id_ == item_id;
        }

        //询问战斗
        public bool askBattle()
        {
            if(menu2_==null)
                return false;
            menu2_.setText("是否與之過招？");
            return menu2_.run() == 0;
        }

        public bool tryBattle(int battle_id, int get_exp)
        {
            /*
            var battle = new BattleScene(battle_id);
            battle.setHaveFailExp(get_exp);
            int result = battle.run();
            //int result = 0;    //测试用

            if (talk_box_up_ != null)
            {
                talk_box_up_.setContent("");
            }

            if (talk_box_down_ != null)
            {
                talk_box_down_.setContent("");
            }           
            
            return result == 0;
            */

            return false;
        }

        public void changeMainMapMusic(int music_id)
        {
            if (subscene_!=null)
            {
                subscene_.changeExitMusic(music_id);
            }
        }

        public bool askJoin()
        {
            if (menu2_ == null)
                return false;
            menu2_.setText("是否要求加入？");
            return menu2_.run() == 0;
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数功能：角色加入队伍，同时获得对方身上的物品
        // 对应 C++ 函数：void Event::join(int role_id)
        /////////////////////////////////////////////////////////////////////////

        public void join(int roleId)
        {
            /////////////////////////////////////////////////////////////////////////
            // 遍历队伍，寻找空位（Team 数组中值为 -1 表示空槽）
            /////////////////////////////////////////////////////////////////////////
            for (int i = 0; i < Save.getInstance().protagonistInformation.Team.Length; i++)
            {
                if (Save.getInstance().protagonistInformation.Team[i] < 0)
                {
                    // 将角色 ID 填入该空位
                    Save.getInstance().protagonistInformation.Team[i] = roleId;

                    /////////////////////////////////////////////////////////////////////////
                    // 获取该角色对象
                    /////////////////////////////////////////////////////////////////////////
                    var role = Save.getInstance().GetRole(roleId);
                    if (role == null)
                        return;

                    /////////////////////////////////////////////////////////////////////////
                    // 遍历角色携带物品列表，将物品转移到全局物品栏
                    /////////////////////////////////////////////////////////////////////////
                    for (int j = 0; j < Constant.ROLE_TAKING_ITEM_COUNT; j++)
                    {
                        if (role.TakingItem[j] >= 0)
                        {
                            // 若物品数量为 0，补为 1
                            if (role.TakingItemCount[j] == 0)
                                role.TakingItemCount[j] = 1;

                            // 将物品添加到全局库存
                            addItem(role.TakingItem[j], role.TakingItemCount[j]);

                            // 清空角色物品槽
                            role.TakingItem[j] = -1;
                            role.TakingItemCount[j] = 0;
                        }
                    }

                    // 找到空位并成功加入后，直接返回
                    return;
                }
            }
        }

        ///询问是否休息
        public bool askRest()
        {
            if(menu2_==null)
                return false;
            menu2_.setText("請選擇是或否？");
            return menu2_.run() == 0;
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数功能：全队休息，回复所有角色的生命值与内力，并清除伤害与中毒状态
        // 对应 C++ 函数：void Event::rest()
        /////////////////////////////////////////////////////////////////////////
        public void rest()
        {
            /////////////////////////////////////////////////////////////////////////
            // 遍历队伍成员数组（Team 中存储的是角色 ID，-1 表示空位）
            /////////////////////////////////////////////////////////////////////////
            foreach (int roleId in Save.getInstance().protagonistInformation.Team)
            {
                if (roleId < 0)
                    continue; // 跳过空槽

                /////////////////////////////////////////////////////////////////////////
                // 获取角色对象
                /////////////////////////////////////////////////////////////////////////
                var role = Save.getInstance().GetRole(roleId);
                if (role == null)
                    continue;

                /////////////////////////////////////////////////////////////////////////
                // 恢复角色状态：生命、内力、伤害、中毒等
                /////////////////////////////////////////////////////////////////////////
                role.HP = role.MaxHP;
                role.MP = role.MaxMP;
                role.Hurt = 0;
                role.Poison = 0;
            }
        }

        public void lightScence()
        {
            if (subscene_!=null)
            {
                subscene_.lightScene();
            }
        }

        public void darkScence()
        {
            if (subscene_!=null)
            {
                subscene_.darkScene();
            }
        }

        //死亡
        public void dead()
        {
            Element.exitAll(1);
            forceExit();
        }

        //某人是否在队伍
        public bool inTeam(int role_id)
        {
            foreach (var r in Save.getInstance().protagonistInformation.Team)
            {
                if (r == role_id)
                {
                    return true;
                }
            }
            return false;
        }

        //设置子地图层数
        public void setSubMapLayerData(int submap_id, int layer, int x, int y, int v)
        {
            getSubMapRecordFromID(submap_id).SetLayerData(layer, x, y, (short)v);
        }

        //检查是否有某物品
        public bool haveItemBool(int item_id)
        {
            return Save.getInstance().GetItemCountInBag(item_id) > 0;
        }


        //32 使用物品后增加或减少物品（无提示）
        public void addItemWithoutHint(int item_id, int count)
        {
            if (item_id < 0 || count == 0) { return; }
            int pos = -1;
            var save = Save.getInstance();
            for (int i = 0; i < Constant.ITEM_IN_BAG_COUNT; i++)
            {
                if (save.protagonistInformation.Items[i].item_id == item_id)
                {
                    pos = i;
                    break;
                }
            }
            if (pos >= 0)
            {
                save.protagonistInformation.Items[pos].count += count;
            }
            else
            {
                for (int i = 0; i < Constant.ITEM_IN_BAG_COUNT; i++)
                {
                    if (save.protagonistInformation.Items[i].item_id < 0)
                    {
                        pos = i;
                        break;
                    }
                }
                if (pos >= 0)
                {
                    save.protagonistInformation.Items[pos].item_id = item_id;
                    save.protagonistInformation.Items[pos].count = count;
                }
            }
            //当物品数量为负，需要整理背包
            if (count < 0)
            {
                ArrangeBag();
            }
        }



        //软星随机语句
        public void askSoftStar()
        {
            Random random = new Random();
            oldTalk(2547 + random.Next(18), 114, 0);
        }













































    }
}
