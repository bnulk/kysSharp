using kysSharp.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Text;
using static kysSharp.GameRandom;
using static System.Net.Mime.MediaTypeNames;

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
            var battle = new BattleScene(battle_id);

            // 设置是否失败后获得经验
            if (get_exp==0)
            {
                battle.setHaveFailExp(false);
            }
            else
            {
                battle.setHaveFailExp(true);
            }

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
                role.PhysicalPower = Constant.MAX_PHYSICAL_POWER;
                role.Hurt = 0;
                role.Poison = 0;
            }
        }

        public void lightScene()
        {
            if (subscene_!=null)
            {
                subscene_.lightScene();
            }
        }

        public void darkScene()
        {
            if (subscene_!=null)
            {
                subscene_.darkScene();
            }
        }

        //7 强制退出对话
        public void forceExit()
        {
            loop_ = false;
        }

        //15 死亡
        public void dead()
        {
            Element.exitAll(1);
            forceExit();
        }

        //16 某人是否在队伍
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

        //17 设置子地图层数
        public void setSubMapLayerData(int submap_id, int layer, int x, int y, int v)
        {
            getSubMapRecordFromID(submap_id).SetLayerData(layer, x, y, (short)v);
        }

        //18 检查是否有某物品
        public bool haveItemBool(int item_id)
        {
            return Save.getInstance().GetItemCountInBag(item_id) > 0;
        }

        // 19 设置场景位置
        public void oldSetScencePosition(int x, int y)
        {
            if (subscene_!=null)
            {
                subscene_.setManViewPosition(x, y);
            }
        }

        // 20 队伍是否已满
        public bool teamIsFull()
        {
            foreach (var r in Save.getInstance().protagonistInformation.Team)
            {
                if (r < 0) { return false; }
            }
            return true;
        }

        // 21 某人离开队伍
        public void leaveTeam(int role_id)
        {
            var save = Save.getInstance();
            for (int i = 0; i < Constant.TEAMMATE_COUNT; i++)
            {
                if (save.protagonistInformation.Team[i] == role_id)
                {
                    for (int j = i; j < Constant.TEAMMATE_COUNT - 1; j++)
                    {
                        save.protagonistInformation.Team[j] = save.protagonistInformation.Team[j + 1];
                    }
                    save.protagonistInformation.Team[Constant.TEAMMATE_COUNT - 1] = -1;
                    break;
                }
            }
        }

        // 22 全队内力归零
        public void zeroAllMP()
        {
            var save = Save.getInstance();
            foreach (var r in save.protagonistInformation.Team)
            {
                if (r >= 0)
                {
                    save.GetRole(r).MP = 0;
                }
            }
        }

        // 23 设置角色使用毒药
        public void setRoleUsePoison(int role_id, int v)
        {
            Save.getInstance().GetRole(role_id).UsePoison = v;
        }

        // 25 子地图视角从一点移动到另一点
        public void subMapViewFromTo(int x0, int y0, int x1, int y1)
        {
            /////////////////////////////////////////////////////////////////////////
            // 若子场景对象为空，则直接返回
            /////////////////////////////////////////////////////////////////////////
            if (subscene_ == null)
                return;

            /////////////////////////////////////////////////////////////////////////
            // 计算移动方向（incx, incy 分别为 -1、0 或 1）
            /////////////////////////////////////////////////////////////////////////
            int incx = GameUtil.sign(x1 - x0);
            int incy = GameUtil.sign(y1 - y0);

            /////////////////////////////////////////////////////////////////////////
            // 若 x 方向存在移动，则逐步更新视图位置
            /////////////////////////////////////////////////////////////////////////
            if (incx != 0)
            {
                for (int i = x0; i != x1; i += incx)
                {
                    subscene_.setViewPosition(i, y0);
                    subscene_.drawAndPresent();
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 若 y 方向存在移动，则逐步更新视图位置
            /////////////////////////////////////////////////////////////////////////
            if (incy != 0)
            {
                for (int i = y0; i != y1; i += incy)
                {
                    subscene_.setViewPosition(x1, i);
                    subscene_.drawAndPresent();
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 最后确保视图停在目标位置
            /////////////////////////////////////////////////////////////////////////
            subscene_.setViewPosition(x1, y1);
        }

        // 26 添加3个事件编号
        public void add3EventNum(int submap_id, int event_index, int v1, int v2, int v3)
        {
            /////////////////////////////////////////////////////////////////////////
            // 获取指定子地图记录
            /////////////////////////////////////////////////////////////////////////
            var s = getSubMapRecordFromID(submap_id);
            if (s == null)
                return;

            /////////////////////////////////////////////////////////////////////////
            // 获取指定索引的事件对象
            /////////////////////////////////////////////////////////////////////////
            var e = s.Event(event_index);
            if (e == null)
                return;

            /////////////////////////////////////////////////////////////////////////
            // 累加三个事件数值参数
            /////////////////////////////////////////////////////////////////////////
            e.Event1 += (short)v1;
            e.Event2 += (short)v2;
            e.Event3 += (short)v3;
        }

        //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 27 播放动画（C# 版本）
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

        // 28 检查角色道德
        public bool checkRoleMorality(int role_id, int low, int high)
        {
            var role = Save.getInstance().GetRole(role_id);
            return (role.Morality >= low && role.Morality <= high);
        }

        // 29 检查角色攻击力
        public bool checkRoleAttack(int role_id, int low, int high)
        {
            return (Save.getInstance().GetRole(role_id).Attack >= low);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数功能：30 控制角色在子地图上从 (x0, y0) 移动到 (x1, y1)
        // 对应 C++ 函数：void Event::walkFromTo(int x0, int y0, int x1, int y1)
        /////////////////////////////////////////////////////////////////////////
        public void walkFromTo(int x0, int y0, int x1, int y1)
        {
            /////////////////////////////////////////////////////////////////////////
            // 如果当前没有子场景（SubScene）则直接返回
            /////////////////////////////////////////////////////////////////////////
            if (subscene_ == null)
                return;

            /////////////////////////////////////////////////////////////////////////
            // 计算 X 与 Y 方向的移动步长（可能为 -1、0 或 +1）
            /////////////////////////////////////////////////////////////////////////
            int incx = GameUtil.sign(x1 - x0);
            int incy = GameUtil.sign(y1 - y0);

            /////////////////////////////////////////////////////////////////////////
            // 若存在 X 方向移动
            /////////////////////////////////////////////////////////////////////////
            if (incx != 0)
            {
                for (int i = x0; i != x1; i += incx)
                {
                    // 尝试行走至 (i, y0)
                    subscene_.TryWalk(i, y0);

                    // 计算并设置行进方向
                    subscene_.setTowards(subscene_.calTowards(x0, y0, i, y0));

                    // 绘制并刷新画面
                    subscene_.drawAndPresent();
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 若存在 Y 方向移动
            /////////////////////////////////////////////////////////////////////////
            if (incy != 0)
            {
                for (int i = y0; i != y1; i += incy)
                {
                    // 尝试行走至 (x1, i)
                    subscene_.TryWalk(x1, i);

                    // 计算并设置行进方向
                    subscene_.setTowards(subscene_.calTowards(x1, y0, x1, i));

                    // 绘制并刷新画面
                    subscene_.drawAndPresent();
                }
            }

            /////////////////////////////////////////////////////////////////////////
            // 最终设置人物视角位置为目标点
            /////////////////////////////////////////////////////////////////////////
            subscene_.setManViewPosition(x1, y1);
        }

        // 31 检查钱是否足够
        public bool checkEnoughMoney(int money_count)
        {
            return (Save.getInstance().GetMoneyCountInBag() >= money_count);
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
                arrangeBag();
            }
        }

        // 33 角色习得武学，有提示
        public void oldLearnMagic(int role_id, int magic_id, int no_display)
        {
            var r = Save.getInstance().GetRole(role_id);
            var m = Save.getInstance().GetMagic(magic_id);
            r.LearnMagic(m);
            if (no_display != 0) { return; }

            if (r != null && m != null && text_box_ != null)
            {
                string msg = $"{GameUtil.EraseModredundantChar(r.strName)}习得武学 {GameUtil.EraseModredundantChar(m.strName)}";
                text_box_.setText(msg);
                text_box_.run();
            }
        }

        // 34 增加角色智商，有提示
        public void addIQ(int role_id, int value)
        {
            var r = Save.getInstance().GetRole(role_id);
            var v0 = r.IQ;
            r.IQ = GameUtil.limit(v0 + value, 0, Constant.MAX_IQ);

            if (r != null && text_box_ != null)
            {
                string msg= $"{GameUtil.EraseModredundantChar(r.strName)}資質增加 {r.IQ - v0}";
                text_box_.setText(msg);
                text_box_.run();
            }  
        }

        // 35 设置角色某武学
        public void setRoleMagic(int role_id, int magic_index_role, int magic_id, int level)
        {
            var r = Save.getInstance().GetRole(role_id);
            r.MagicID[magic_index_role] = magic_id;
            r.MagicLevel[magic_index_role] = level;
        }

        // 36 检查角色性别
        public bool checkRoleSexual(int sexual)
        {
            if (sexual <= 255)
            {
                return Save.getInstance().GetRole(0).Sexual == sexual;
            }
            else
            {
                return x50[0x7000] == 0;
            }
        }

        // 37 增加主角道德值
        public void addMorality(int value)
        {
            var role = Save.getInstance().GetRole(0);
            role.Morality = GameUtil.limit(role.Morality + value, 0, Constant.MAX_MORALITY);
        }

        // 38 更改子地图图片
        public void changeSubMapPic(int submap_id, int layer, int old_pic, int new_pic)
        {
            var s = getSubMapRecordFromID(submap_id);
            if (s!=null)
            {
                for (int i1 = 0; i1 < Constant.SUBMAP_COORD_COUNT; i1++)
                {
                    for (int i2 = 0; i2 < Constant.SUBMAP_COORD_COUNT; i2++)
                    {
                        if (s.GetLayerData(layer, i1, i2) == old_pic)
                        {
                            s.SetLayerData(layer, i1, i2, (short)new_pic);
                        }
                    }
                }
            }
        }

        // 39 打开子地图 
        public void openSubMap(int submap_id)
        {
            Save.getInstance().GetSubMapInfo(submap_id).EntranceCondition = 0;
        }

        // 40 设置朝向
        public void setTowards(int towards)
        {
            subscene_.towards_ = (Towards)towards;
        }

        ///////////////////////////////////////////////////////////////////////
        // 41 函数名称：RoleAddItem
        // 功能描述：为指定角色添加物品；如果角色已拥有该物品则叠加数量，否则添加新物品。
        // 参数说明：
        //   roleId   —— 角色ID
        //   itemId   —— 物品ID（小于0无效）
        //   count    —— 添加的数量（0时不处理）
        // 注意事项：
        //   本函数在添加后会整理角色物品，使物品按唯一ID合并。
        // 对应C++原型：void Event::roleAddItem(int role_id, int item_id, int count)
        ///////////////////////////////////////////////////////////////////////
        public void roleAddItem(int roleId, int itemId, int count)
        {
            if (itemId < 0 || count == 0)
                return;

            var role = Save.getInstance().GetRole(roleId);
            if (role == null)
                return;

            int pos = -1;

            ///////////////////////////////////////////////////////////////////////
            // 检查角色是否已持有该物品，如有则叠加数量
            ///////////////////////////////////////////////////////////////////////
            for (int i = 0; i < Constant.ROLE_TAKING_ITEM_COUNT; i++)
            {
                if (role.TakingItem[i] == itemId)
                {
                    pos = i;
                    break;
                }
            }

            if (pos >= 0)
            {
                role.TakingItemCount[pos] += count;
            }
            else
            {
                ///////////////////////////////////////////////////////////////////////
                // 如果角色未持有该物品，则在空位中添加
                ///////////////////////////////////////////////////////////////////////
                for (int i = 0; i < Constant.ROLE_TAKING_ITEM_COUNT; i++)
                {
                    if (role.TakingItem[i] < 0)
                    {
                        pos = i;
                        break;
                    }
                }

                if (pos >= 0)
                {
                    role.TakingItem[pos] = itemId;
                    role.TakingItemCount[pos] = count;
                }
            }

            ///////////////////////////////////////////////////////////////////////
            // 整理角色的物品：将相同物品合并，移除数量为0的项
            ///////////////////////////////////////////////////////////////////////
            var itemCount = new Dictionary<int, int>();
            for (int i = 0; i < Constant.ROLE_TAKING_ITEM_COUNT; i++)
            {
                if (role.TakingItem[i] >= 0 && role.TakingItemCount[i] > 0)
                {
                    if (itemCount.ContainsKey(role.TakingItem[i]))
                        itemCount[role.TakingItem[i]] += role.TakingItemCount[i];
                    else
                        itemCount[role.TakingItem[i]] = role.TakingItemCount[i];
                }

                // 清空原物品栏
                role.TakingItem[i] = -1;
                role.TakingItemCount[i] = 0;
            }

            ///////////////////////////////////////////////////////////////////////
            // 将整理后的物品重新写入角色物品栏
            ///////////////////////////////////////////////////////////////////////
            int k = 0;
            foreach (var pair in itemCount)
            {
                if (k >= Constant.ROLE_TAKING_ITEM_COUNT)
                    break; // 防止越界
                role.TakingItem[k] = pair.Key;
                role.TakingItemCount[k] = pair.Value;
                k++;
            }
        }

        // 42 检查队伍中是否有女性角色
        public bool checkFemaleInTeam()
        {
            foreach (var r in Save.getInstance().protagonistInformation.Team)
            {
                if (r >= 0)
                {
                    if (Save.getInstance().GetRole(r).Sexual == 1) { return true; }
                }
            }
            return false;
        }

        ///////////////////////////////////////////////////////////////////////
        // 44 函数名称：Play2Animation
        // 功能描述：让两个事件（event）同时播放动画，从起始帧到结束帧逐步更新。
        // 参数说明：
        //   eventIndex1 —— 第一个事件索引
        //   beginPic1   —— 第一个事件的起始贴图帧编号
        //   endPic1     —— 第一个事件的结束贴图帧编号
        //   eventIndex2 —— 第二个事件索引
        //   beginPic2   —— 第二个事件的起始贴图帧编号
        //   endPic2     —— 第二个事件的结束贴图帧编号
        // 注意事项：
        //   两个事件必须都存在，否则函数不执行。
        //   动画每次更新后调用 drawAndPresent() 绘制画面。
        // 对应C++原型：void Event::play2Amination(...)
        ///////////////////////////////////////////////////////////////////////
        public void play2Amination(int eventIndex1, int beginPic1, int endPic1,
                                   int eventIndex2, int beginPic2, int endPic2)
        {
            var e1 = subscene_.getMapInfo().Event(eventIndex1);
            var e2 = subscene_.getMapInfo().Event(eventIndex2);

            if (e1 != null && e2 != null)
            {
                int inc1 = GameUtil.sign(endPic1 - beginPic1);

                ///////////////////////////////////////////////////////////////////////
                // 从起始帧逐步更新两个事件的贴图，实现同步动画播放
                ///////////////////////////////////////////////////////////////////////
                for (int i = 0; i != endPic1 - beginPic1; i += inc1)
                {
                    e1.SetPic(beginPic1 + i);
                    e2.SetPic(beginPic2 + i);
                    subscene_.drawAndPresent();
                }

                ///////////////////////////////////////////////////////////////////////
                // 最终帧：确保两事件都设置到结束状态
                ///////////////////////////////////////////////////////////////////////
                e1.SetPic(endPic1);
                e2.SetPic(beginPic2 + endPic1 - beginPic1);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 函数名称：Play3Animation
        // 功能描述：让三个事件（Event）同时播放动画，从起始帧到结束帧逐步更新。
        // 参数说明：
        //   eventIndex1 —— 第一个事件索引
        //   beginPic1   —— 第一个事件起始贴图帧编号
        //   endPic1     —— 第一个事件结束贴图帧编号
        //   eventIndex2 —— 第二个事件索引
        //   beginPic2   —— 第二个事件起始贴图帧编号
        //   eventIndex3 —— 第三个事件索引
        //   beginPic3   —— 第三个事件起始贴图帧编号
        // 注意事项：
        //   所有三个事件都必须存在，否则不执行动画。
        //   每一帧更新后调用 DrawAndPresent() 重绘场景。
        // 对应C++原型：void Event::play3Amination(...)
        ///////////////////////////////////////////////////////////////////////
        public void play3Animation(int eventIndex1, int beginPic1, int endPic1,
                                   int eventIndex2, int beginPic2,
                                   int eventIndex3, int beginPic3)
        {
            var e1 = subscene_.getMapInfo().Event(eventIndex1);
            var e2 = subscene_.getMapInfo().Event(eventIndex2);
            var e3 = subscene_.getMapInfo().Event(eventIndex3); // ✅ 修正原C++中的错误（原代码错误地重复获取 event_index2）

            if (e1 != null && e2 != null && e3 != null)
            {
                int inc1 = GameUtil.sign(endPic1 - beginPic1);

                ///////////////////////////////////////////////////////////////////////
                // 动画主循环：逐帧更新三个事件的贴图
                ///////////////////////////////////////////////////////////////////////
                for (int i = 0; i != endPic1 - beginPic1; i += inc1)
                {
                    e1.SetPic(beginPic1 + i);
                    e2.SetPic(beginPic2 + i);
                    e3.SetPic(beginPic3 + i);
                    subscene_.drawAndPresent();
                }

                ///////////////////////////////////////////////////////////////////////
                // 设置最终帧，确保动画结束时贴图帧同步
                ///////////////////////////////////////////////////////////////////////
                e1.SetPic(endPic1);
                e2.SetPic(beginPic2 + endPic1 - beginPic1);
                e3.SetPic(beginPic3 + endPic1 - beginPic1);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 45 函数名称：AddSpeed
        // 功能描述：提升指定角色的“轻功”（Speed）属性。
        // 参数说明：
        //   roleId —— 角色编号
        //   value  —— 增加的轻功值（可正可负）
        // 逻辑说明：
        //   1. 获取指定角色对象。
        //   2. 计算新速度并使用 GameUtil.Limit 限制在 [0, MAX_SPEED] 范围。
        //   3. 显示提示文字：“角色名 輕功增加 X”。
        //   4. 调用文本框执行显示。
        // 对应C++原型：void Event::addSpeed(int role_id, int value)
        ///////////////////////////////////////////////////////////////////////
        public void addSpeed(int roleId, int value)
        {
            var r = Save.getInstance().GetRole(roleId);
            int v0 = r.Speed;

            ///////////////////////////////////////////////////////////////////////
            // 更新角色速度（轻功）并限制在合法范围
            ///////////////////////////////////////////////////////////////////////
            r.Speed = GameUtil.limit(v0 + value, 0, Constant.MAX_SPEED);

            ///////////////////////////////////////////////////////////////////////
            // 计算提升量并显示提示
            ///////////////////////////////////////////////////////////////////////
            string msg = $"{GameUtil.EraseModredundantChar(r.strName)}輕功增加 {r.Speed - v0}";
            text_box_?.setText(msg);
            text_box_?.run();
        }

        ///////////////////////////////////////////////////////////////////////
        // 46 函数名称：AddMaxMP
        // 功能描述：增加指定角色的最大内力（MaxMP）
        // 参数说明：
        //   roleId —— 角色编号
        //   value  —— 增加的内力值（可正可负）
        // 对应C++原型：void Event::addMaxMP(int role_id, int value)
        ///////////////////////////////////////////////////////////////////////
        public void addMaxMP(int roleId, int value)
        {
            var r = Save.getInstance().GetRole(roleId);
            int v0 = r.MaxMP;

            r.MaxMP = GameUtil.limit(v0 + value, 0, Constant.MAX_MP);

            string msg = $"{GameUtil.EraseModredundantChar(r.strName)}內力增加 {r.MaxMP - v0}";
            text_box_?.setText(msg);
            text_box_?.run();
        }

        ///////////////////////////////////////////////////////////////////////
        // 47 函数名称：AddAttack
        // 功能描述：增加指定角色的攻击力（武力）
        // 参数说明：
        //   roleId —— 角色编号
        //   value  —— 增加的攻击力值（可正可负）
        // 对应C++原型：void Event::addAttack(int role_id, int value)
        ///////////////////////////////////////////////////////////////////////
        public void addAttack(int roleId, int value)
        {
            var r = Save.getInstance().GetRole(roleId);
            int v0 = r.Attack;

            r.Attack = GameUtil.limit(v0 + value, 0, Constant.MAX_ATTACK);

            string msg = $"{GameUtil.EraseModredundantChar(r.strName)}武力增加 {r.Attack - v0}";
            text_box_?.setText(msg);
            text_box_?.run();
        }

        ///////////////////////////////////////////////////////////////////////
        // 48 函数名称：AddMaxHP
        // 功能描述：增加指定角色的最大生命值（MaxHP）
        // 参数说明：
        //   roleId —— 角色编号
        //   value  —— 增加的生命值（可正可负）
        // 对应C++原型：void Event::addMaxHP(int role_id, int value)
        ///////////////////////////////////////////////////////////////////////
        public void addMaxHP(int roleId, int value)
        {
            var r = Save.getInstance().GetRole(roleId);
            int v0 = r.MaxHP;

            r.MaxHP = GameUtil.limit(v0 + value, 0, Constant.MAX_HP);

            string msg = $"{GameUtil.EraseModredundantChar(r.strName)}生命增加 {r.MaxHP - v0}";
            text_box_?.setText(msg);
            text_box_?.run();
        }

        ///////////////////////////////////////////////////////////////////////
        // 49 函数名称：SetMPType
        // 功能描述：设置指定角色的内力类型（MPType）
        // 参数说明：
        //   roleId —— 角色编号
        //   value  —— 内力类型值
        // 对应C++原型：void Event::setMPType(int role_id, int value)
        ///////////////////////////////////////////////////////////////////////
        public void setMPType(int roleId, int value)
        {
            var role = Save.getInstance().GetRole(roleId);
            if (role != null)
            {
                role.MPType = value;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 50 函数名称：CheckHave5Item
        // 功能描述：检查玩家是否同时拥有指定的 5 个物品。
        // 参数说明：
        //   itemId1 ~ itemId5 —— 要检查的五个物品编号
        // 返回值：
        //   true  —— 玩家同时拥有全部 5 个物品
        //   false —— 至少缺少其中一个物品
        // 对应C++原型：bool Event::checkHave5Item(int item_id1, int item_id2, int item_id3, int item_id4, int item_id5)
        ///////////////////////////////////////////////////////////////////////
        public bool checkHave5Item(int itemId1, int itemId2, int itemId3, int itemId4, int itemId5)
        {
            return haveItemBool(itemId1) &&
                   haveItemBool(itemId2) &&
                   haveItemBool(itemId3) &&
                   haveItemBool(itemId4) &&
                   haveItemBool(itemId5);
        }


        // 51 软星随机语句
        public void askSoftStar()
        {
            Random random = new Random();
            oldTalk(2547 + random.Next(18), 114, 0);
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 52 显示玩家的道德指数
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public void showMorality()
        {
            var role = Save.getInstance().GetRole(0);
            text_box_?.setText($"你的道德指數為 {role.Morality}");
            text_box_?.run();
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 53 显示玩家的声望指数
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public void showFame()
        {
            var role = Save.getInstance().GetRole(0);
            text_box_?.setText($"你的聲望指數為 {role.Fame}");
            text_box_?.run();
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 54 打开所有子地图入口条件（即解锁所有子地图）
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public void openAllSubMap()
        {
            int i = 0;
            var save = Save.getInstance();

            // 遍历所有子地图，取消进入条件限制
            while (save.GetSubMapInfo(i) != null)
            {
                save.GetSubMapInfo(i).EntranceCondition = 0;
                i++;
            }

            // 特定地图保留部分进入条件
            save.GetSubMapInfo(2).EntranceCondition = 2;
            save.GetSubMapInfo(38).EntranceCondition = 2;
            save.GetSubMapInfo(75).EntranceCondition = 1;
            save.GetSubMapInfo(80).EntranceCondition = 1;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 55 检查指定事件编号对应的 Event1 值是否等于给定值
        /// </summary>
        /// <param name="eventIndex">事件索引</param>
        /// <param name="value">要比较的值</param>
        /// <returns>若相等则返回 true，否则 false</returns>
        ///////////////////////////////////////////////////////////////////////
        public bool checkEventID(int eventIndex, int value)
        {
            return subscene_?.getMapInfo()?.Event(eventIndex)?.Event1 == (short)value;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 56 增加主角声望值，并在声望超过特定阈值（200）时触发事件修改
        /// </summary>
        /// <param name="value">增加的声望值</param>
        ///////////////////////////////////////////////////////////////////////
        public void addFame(int value)
        {
            var save = Save.getInstance();
            var role0 = save.GetRole(0);
            int oldFame = role0.Fame;

            role0.Fame += value;

            // 当声望从 200 以下跨越到 200 以上时触发特定剧情事件修改
            if (role0.Fame > 200 && oldFame <= 200)
            {
                modifyEvent(70, 11, 0, 11, 932, -1, -1, 7968, 7968, 7968, 0, 18, 21);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 播放破石门动画（组合两段动画）
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public void breakStoneGate()
        {
            playAnimation(-1, 3832 * 2, 3844 * 2);
            play3Animation(2, 3845 * 2, 3873 * 2, 3, 3874 * 2, 4, 3903 * 2);
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 58 武林大会事件流程
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public void fightForTop()
        {
            // 预设人物头像数组
            int[] heads = new int[]
            {
        8, 21, 23, 31, 32, 43, 7, 11, 14, 20, 33, 34, 10, 12, 19,
        22, 56, 68, 13, 55, 62, 67, 70, 71, 26, 57, 60, 64, 3, 69
            };

            // 循环15轮比试
            for (int i = 0; i < 15; i++)
            {
                int p = RandomClassical.rand(2);
                oldTalk(2854 + i * 2 + p, heads[i * 2 + p], RandomClassical.rand(2) * 4 + RandomClassical.rand(2));

                // 尝试战斗，若失败则主角死亡并返回
                if (!tryBattle(102 + i * 2 + p, 0))
                {
                    dead();
                    return;
                }

                darkScene();
                lightScene();

                // 每3轮显示特定对话并休息
                if (i % 3 == 2)
                {
                    oldTalk(2891, 70, 4);
                    rest();
                    darkScene();
                    lightScene();
                }
            }

            // 战斗结束后的对话
            oldTalk(2884, 0, 3);
            oldTalk(2885, 0, 3);
            oldTalk(2886, 0, 3);
            oldTalk(2887, 0, 3);
            oldTalk(2888, 0, 3);
            oldTalk(2889, 0, 1);

            // 获得奖励物品
            addItem(0x8F, 1);
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 队伍中除主角外的所有成员离队
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public void allLeave()
        {
            var save = Save.getInstance();
            for (int i = 1; i < Constant.TEAMMATE_COUNT; i++)
            {
                save.protagonistInformation.Team[i] = -1;
            }
        }


























        /////////////////////////////////////////////////////////////////////////
        // 函数名称：Instruct50E
        // 功能说明：/50 扩展指令解释器（事件脚本虚拟机的扩展命令处理）
        // 设计说明：
        //   - 用于执行复杂的脚本逻辑，如变量操作、存档读写、绘图、菜单、延时等。
        //   - 对应原 C++ 函数 Event::instruct_50e。
        //   - 所有与内存指针相关的部分使用安全的 C# 数组与对象访问替代。
        // 参数：
        //   code     —— 子指令编号
        //   e1~e6    —— 参数（用于传入脚本中的操作数）
        //   ref codePtr —— 当前脚本执行位置（通过引用传递）
        // 备注：
        //   * 本函数并不推荐直接使用，仅用于保持兼容旧脚本逻辑。
        /////////////////////////////////////////////////////////////////////////
        public void instruct_50e(int code, int e1, int e2, int e3, int e4, int e5, int e6, ref int codePtr)
        {
            int index = 0, len = 0, offset = 0;
            int i1 = 0, i2 = 0;
            string str = string.Empty;
            List<string> strs = new List<string>();
            MenuText? menu = null;
            var save = Save.getInstance();

            switch (code)
            {
                /////////////////////////////////////////////////////////////////
                // 0：赋值
                /////////////////////////////////////////////////////////////////
                case 0:
                    x50[e1] = e2;
                    break;

                /////////////////////////////////////////////////////////////////
                // 1：数组赋值
                /////////////////////////////////////////////////////////////////
                case 1:
                    index = e3 + E_GetValue(0, e1, e4);
                    x50[index] = E_GetValue(1, e1, e4);
                    if (e2 != 0)
                        x50[index] &= 0xFF;
                    break;

                /////////////////////////////////////////////////////////////////
                // 2：数组取值
                /////////////////////////////////////////////////////////////////
                case 2:
                    index = e3 + E_GetValue(0, e1, e4);
                    x50[e5] = x50[index];
                    if (e2 != 0)
                        x50[index] &= 0xFF;
                    break;

                /////////////////////////////////////////////////////////////////
                // 3：基本运算
                /////////////////////////////////////////////////////////////////
                case 3:
                    index = E_GetValue(0, e1, e5);
                    switch (e2)
                    {
                        case 0: x50[e3] = x50[e4] + index; break;
                        case 1: x50[e3] = x50[e4] - index; break;
                        case 2: x50[e3] = x50[e4] * index; break;
                        case 3: if (index != 0) x50[e3] = x50[e4] / index; break;
                        case 4: if (index != 0) x50[e3] = x50[e4] % index; break;
                        case 5: if (index != 0) x50[e3] = (int)((uint)x50[e4] / index); break;
                    }
                    break;

                /////////////////////////////////////////////////////////////////
                // 4：判断变量并改写跳转标记
                /////////////////////////////////////////////////////////////////
                case 4:
                    x50[0x7000] = 0;
                    index = E_GetValue(0, e1, e4);
                    switch (e2)
                    {
                        case 0: if (!(x50[e3] < index)) x50[0x7000] = 1; break;
                        case 1: if (!(x50[e3] <= index)) x50[0x7000] = 1; break;
                        case 2: if (!(x50[e3] == index)) x50[0x7000] = 1; break;
                        case 3: if (!(x50[e3] != index)) x50[0x7000] = 1; break;
                        case 4: if (!(x50[e3] >= index)) x50[0x7000] = 1; break;
                        case 5: if (!(x50[e3] > index)) x50[0x7000] = 1; break;
                        case 6: x50[0x7000] = 0; break;
                        case 7: x50[0x7000] = 1; break;
                    }
                    break;

                /////////////////////////////////////////////////////////////////
                // 5：全部清零
                /////////////////////////////////////////////////////////////////
                case 5:
                    Array.Clear(x50, 0, x50.Length);
                    break;

                /////////////////////////////////////////////////////////////////
                // 8：读取对话（从 talk_ 数组）
                /////////////////////////////////////////////////////////////////
                case 8:
                    index = E_GetValue(0, e1, e2);
                    var destIndex = e3;
                    if (index >= 0 && index < talk_.Count)
                        WriteStringToX50(destIndex, talk_[index]);
                    break;

                /////////////////////////////////////////////////////////////////
                // 9：格式化字符串
                /////////////////////////////////////////////////////////////////
                case 9:
                    e4 = E_GetValue(0, e1, e4);
                    string format = ReadStringFromX50(e3);
                    WriteStringToX50(e2, string.Format(format, e4));
                    break;

                /////////////////////////////////////////////////////////////////
                // 10：字符串长度
                /////////////////////////////////////////////////////////////////
                case 10:
                    x50[e2] = ReadStringFromX50(e1).Length;
                    break;

                /////////////////////////////////////////////////////////////////
                // 11：合并字符串
                /////////////////////////////////////////////////////////////////
                case 11:
                    string s1 = ReadStringFromX50(e1);
                    string s2 = ReadStringFromX50(e2);
                    WriteStringToX50(e1, s1 + s2);
                    break;

                /////////////////////////////////////////////////////////////////
                // 12：制造空格字符串
                /////////////////////////////////////////////////////////////////
                case 12:
                    e3 = E_GetValue(0, e1, e3);
                    WriteStringToX50(e2, new string(' ', e3 / 2));
                    break;

                /////////////////////////////////////////////////////////////////
                // 16：写存档数据
                /////////////////////////////////////////////////////////////////
                case 16:
                    e3 = E_GetValue(0, e1, e3);
                    e4 = E_GetValue(1, e1, e4);
                    e5 = E_GetValue(2, e1, e5);

                    /*
                    switch (e2)
                    {
                        case 0: save.GetRole(e3)?.SetIntField(e4, e5); break;
                        case 1: save.GetItem(e3)?.SetIntField(e4, e5); break;
                        case 2: save.GetSubMapInfo(e3)?.SetIntField(e4, e5); break;
                        case 3: save.GetMagic(e3)?.SetIntField(e4, e5); break;
                        case 4: save.GetShop(e3)?.SetIntField(e4, e5); break;
                    }
                    */

                    break;

                /////////////////////////////////////////////////////////////////
                // 17：读存档数据
                /////////////////////////////////////////////////////////////////
                case 17:
                    e3 = E_GetValue(0, e1, e3);
                    e4 = E_GetValue(1, e1, e4);
                    /*
                    switch (e2)
                    {
                        case 0: x50[e5] = save.GetRole(e3)?.GetIntField(e4) ?? 0; break;
                        case 1: x50[e5] = save.GetItem(e3)?.GetIntField(e4) ?? 0; break;
                        case 2: x50[e5] = save.GetSubMapInfo(e3)?.GetIntField(e4) ?? 0; break;
                        case 3: x50[e5] = save.GetMagic(e3)?.GetIntField(e4) ?? 0; break;
                        case 4: x50[e5] = save.GetShop(e3)?.GetIntField(e4) ?? 0; break;
                    }
                    */
                    break;

                /////////////////////////////////////////////////////////////////
                // 18：写队伍数据
                /////////////////////////////////////////////////////////////////
                case 18:
                    e2 = E_GetValue(0, e1, e2);
                    e3 = E_GetValue(1, e1, e3);
                    save.protagonistInformation.Team[e2] = e3;
                    break;

                /////////////////////////////////////////////////////////////////
                // 19：读队伍数据
                /////////////////////////////////////////////////////////////////
                case 19:
                    e2 = E_GetValue(0, e1, e2);
                    x50[e3] = save.protagonistInformation.Team[e2];
                    break;

                /////////////////////////////////////////////////////////////////
                // 20：获取物品个数
                /////////////////////////////////////////////////////////////////
                case 20:
                    e2 = E_GetValue(0, e1, e2);
                    x50[e3] = save.GetItemCountInBag(e2);
                    break;

                /////////////////////////////////////////////////////////////////
                // 33：绘制字符串
                /////////////////////////////////////////////////////////////////
                case 33:
                    e3 = E_GetValue(0, e1, e3);
                    e4 = E_GetValue(1, e1, e4);
                    e5 = E_GetValue(2, e1, e5);
                    string text = ReadStringFromX50(e2);
                    GameFont.getInstance().draw(text, 20, e3, e4, new SDL.SDL_Color() { r=255,g=255,b=255,a=255});
                    break;

                /////////////////////////////////////////////////////////////////
                // 35：暂停等待按键
                /////////////////////////////////////////////////////////////////
                case 35:
                    if (text_box_ == null) break;
                    text_box_.setText("");
                    text_box_.setTexture("", 0);
                    x50[e1] = text_box_.run();
                    break;

                /////////////////////////////////////////////////////////////////
                // 37：延时
                /////////////////////////////////////////////////////////////////
                case 37:
                    Engine.getInstance().delay(E_GetValue(0, e1, e2));
                    break;

                /////////////////////////////////////////////////////////////////
                // 38：随机数
                /////////////////////////////////////////////////////////////////
                case 38:
                    e2 = E_GetValue(0, e1, e2);
                    x50[e3] = RandomClassical.rand(e2);
                    break;

                /////////////////////////////////////////////////////////////////
                // 42：修改主地图坐标
                /////////////////////////////////////////////////////////////////
                case 42:
                    e2 = E_GetValue(0, e1, e2);
                    e3 = E_GetValue(0, e1, e3);
                    MainScene.getInstance().setManPosition(e2, e3);
                    break;

                /////////////////////////////////////////////////////////////////
                // 43：调用其他事件
                /////////////////////////////////////////////////////////////////
                case 43:
                    e2 = E_GetValue(0, e1, e2);
                    e3 = E_GetValue(1, e1, e3);
                    e4 = E_GetValue(2, e1, e4);
                    e5 = E_GetValue(3, e1, e5);
                    e6 = E_GetValue(4, e1, e6);
                    x50[0x7100] = e3;
                    x50[0x7101] = e4;
                    x50[0x7102] = e5;
                    x50[0x7103] = e6;
                    CallEvent(e2);
                    break;

                /////////////////////////////////////////////////////////////////
                // 52：判断角色是否掌握某武学等级
                /////////////////////////////////////////////////////////////////
                case 52:
                    e2 = E_GetValue(0, e1, e2);
                    e3 = E_GetValue(1, e1, e3);
                    e4 = E_GetValue(2, e1, e4);
                    x50[0x7000] = 1;
                    if (save.GetRole(e2)?.GetMagicLevelIndex(e3) + 1 >= e4)
                        x50[0x7000] = 0;
                    break;

                /////////////////////////////////////////////////////////////////
                // 默认：忽略未实现指令
                /////////////////////////////////////////////////////////////////
                default:
                    break;
            }
        }

        private int E_GetValue(int type, int a, int b)
        {
            // 模拟 e_GetValue：根据脚本上下文取值
            // 实际逻辑请按你的脚本系统替换
            return b;
        }

        private string ReadStringFromX50(int offset)
        {
            // 从 x50[] 读取 null 结尾字符串
            List<char> chars = new List<char>();
            for (int i = offset; i < x50.Length && x50[i] != 0; i++)
                chars.Add((char)x50[i]);
            return new string(chars.ToArray());
        }

        private void WriteStringToX50(int offset, string value)
        {
            // 将字符串写入 x50[]
            for (int i = 0; i < value.Length && offset + i < x50.Length; i++)
                x50[offset + i] = value[i];
            if (offset + value.Length < x50.Length)
                x50[offset + value.Length] = 0;
        }


































    }
}
