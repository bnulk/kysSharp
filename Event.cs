using kysSharp.Types;
using System;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace kysSharp
{
    partial class Event
    {
        private static Event event_ = new Event();


        private List<int> offset = new List<int>();
        private List<int> length = new List<int>();
        private List<string> talk_= new List<string>();
        private List<List<int>> kdef_ = new List<List<int>>();

        private int leave_event_0_;
        private List<int> leave_event_id_ = new List<int>();

        //两个对话，用于上面和下面，两个可以同时显示
        //视需要可增加更多
        private Element? talk_box_ = null;
        private Talk? talk_box_up_ = null;
        private Talk? talk_box_down_ = null;

        //专用于显示确认和取消选项
        private MenuText? menu2_ = null;
        //专用于显示一个文本框
        private TextBox? text_box_ = null;
        private int event_id_ = -1;

        private SubScene subscene_;
        private int submap_id_;
        private int x_, y_;
        private int event_index_;
        private int item_id_;
        private Item? item_;
        private Save? save_;
        private bool loop_;

        private int[] x50 = new int[65535];

        public Event()
        {
            loadEventData();
            talk_box_ = new Element();
            talk_box_up_ = new Talk();
            talk_box_down_ = new Talk();
            talk_box_.addChild(talk_box_up_);
            talk_box_.addChild(talk_box_down_, 0, 400);
            menu2_ = new MenuText(new List<string>{ "確認（Y）", "取消（N）" });
            menu2_.setPosition(400, 300);
            menu2_.setFontSize(24);
            menu2_.setHaveBox(true);
            menu2_.arrange(0, 50, 150, 0);
            text_box_ = new TextBox();
            text_box_.setPosition(400, 200);
            text_box_.setTextPosition(-20, 100);
        }

        public static Event getInstance()
        {
            if(event_==null)
            {
                event_ = new Event();
            }
            return event_;
        }

        public void forceExit() { loop_ = false; }

        /////////////////////////////////////////////////////////////////////////
        // 函数：loadEventData
        // 功能：加载对话、事件定义与离队列表数据
        // 返回：bool（暂时固定返回 false，与原 C++ 保持一致）
        /////////////////////////////////////////////////////////////////////////
        public bool loadEventData()
        {
            /////////////////////////////////////////////////////////////////////////
            // 1. 读取 talk 数据
            /////////////////////////////////////////////////////////////////////////
            var talk = GameFile.getIdxContent(Path.Combine("game", "resource", "talk.idx"), Path.Combine("game", "resource", "talk.grp"), ref offset, ref length);
            // XOR 解码
            for (int i = 0; i < offset[^1]; i++)
            {
                talk[i] ^= 0xFF;
            }
            // 读取每个字符串（假设 cp950 → gb2312 转换）
            for (int i = 0; i < length.Count; i++)
            {
                string str = PotConv.Cp950ToCp936(talk, offset[i], length[i]);
                str = str.Replace("*", ""); // 移除 '*'
                str = str.Replace("", ""); // 移除 ''
                talk_.Add(str);
            }
            talk = null; // 对应 delete talk

            /////////////////////////////////////////////////////////////////////////
            // 2. 读取事件定义 kdef（int16_t 列表）
            ///   读取事件，全部转为整型
            /////////////////////////////////////////////////////////////////////////
            var kdef = GameFile.getIdxContent(Path.Combine("game", "resource", "kdef.idx"), Path.Combine("game", "resource", "kdef.grp"), ref offset, ref length);
            for (int i = 0; i < length.Count; i++)
            {
                int count = length[i] / sizeof(short);
                List<int> eventList = new(count);
                for (int k = 0; k < count; k++)
                {
                    short val = BitConverter.ToInt16(kdef, offset[i] + k * 2);
                    eventList.Add(val);
                }
                kdef_.Add(eventList);            
            }

            kdef = null;

            /////////////////////////////////////////////////////////////////////////
            // 3. 读取离队列表
            /////////////////////////////////////////////////////////////////////////
            var filePath = Path.Combine("game", "list", "leave.txt");
            string leaveTxt = File.ReadAllText(filePath);
            ConvertLibs.FindNumbers(leaveTxt, ref leave_event_id_);

            if (leave_event_id_.Count > 0)
            {
                leave_event_0_ = leave_event_id_[0];
                leave_event_id_.RemoveAt(0);
            }

            return false;
        }

        public bool CallEvent(int event_id, Element? subscene=null, int supmap_id=-1, int item_id=-1, int event_index=-1, int x=-1, int y=-1) //调用指令的内容写这里
        {
            if (event_id <= 0 || event_id >= kdef_.Count) { return false; }
            if (subscene is not SubScene s)
            {
                s = null;
                if (subscene != null)
                    return false;
            }
            subscene_ = s;
            submap_id_ = -1;

            if (subscene_ != null)
            {
                submap_id_ = subscene_.getMapInfo().ID;
            }

            item_id_ = item_id;
            event_index_ = event_index;
            x_ = x;
            y_ = y;

            // 将节点加载到绘图栈的最上，这样两个对话可以画出来
            if(talk_box_!=null)
            {
                talk_box_.setExit(false);
                Element.addOnRootTop(talk_box_);
            }

            int p = 0;
            loop_ = true;
            int i = 0;

            var e = kdef_[event_id];

            Console.Write($"Event {event_id}: ");
            foreach (var c in e)
            {
                Console.Write($"{c} ");
            }
            Console.WriteLine();

            // 预留缓冲区，避免越界错误
            for (int pad = 0; pad < 20; pad++)
            {
                e.Add(-1);
            }

            ///////////////////////////////////////////////////////////////////////
            // 宏模拟区域：使用委托和局部函数代替 C++ 宏
            ///////////////////////////////////////////////////////////////////////

            void PRINT_E(int n)
            {
                for (int __i = 1; __i <= n; __i++)
                    Console.Write($"{e[i + __i]}, ");
            }

            void VOID(Action func, int n)
            {
                PRINT_E(n);
                func();
                i += n + 1;
            }

            void VOID0(Action func)
            {
                PRINT_E(0);
                func();
                i += 1;
            }

            void VOID1(Action<int> func)
            {
                PRINT_E(1);
                func(e[i + 1]);
                i += 2;
            }

            void VOID2(Action<int, int> func)
            {
                PRINT_E(2);
                func(e[i + 1], e[i + 2]);
                i += 3;
            }

            void VOID3(Action<int, int, int> func)
            {
                PRINT_E(3);
                func(e[i + 1], e[i + 2], e[i + 3]);
                i += 4;
            }

            void VOID4(Action<int, int, int, int> func)
            {
                PRINT_E(4);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4]);
                i += 5;
            }

            void VOID5(Action<int, int, int, int, int> func)
            {
                PRINT_E(5);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5]);
                i += 6;
            }

            void VOID6(Action<int, int, int, int, int, int> func)
            {
                PRINT_E(6);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6]);
                i += 7;
            }

            void VOID7(Action<int, int, int, int, int, int, int> func)
            {
                PRINT_E(7);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7]);
                i += 8;
            }

            void VOID8(Action<int, int, int, int, int, int, int, int> func)
            {
                PRINT_E(8);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], e[i + 8]);
                i += 9;
            }

            void VOID9(Action<int, int, int, int, int, int, int, int, int> func)
            {
                PRINT_E(9);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], e[i + 8], e[i + 9]);
                i += 10;
            }

            void VOID10(Action<int, int, int, int, int, int, int, int, int, int> func)
            {
                PRINT_E(10);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], e[i + 8], e[i + 9], e[i + 10]);
                i += 11;
            }

            void VOID11(Action<int, int, int, int, int, int, int, int, int, int, int> func)
            {
                PRINT_E(11);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], e[i + 8], e[i + 9], e[i + 10], e[i + 11]);
                i += 12;
            }

            void VOID12(Action<int, int, int, int, int, int, int, int, int, int, int, int> func)
            {
                PRINT_E(12);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], e[i + 8], e[i + 9], e[i + 10], e[i + 11], e[i + 12]);
                i += 13;
            }

            void VOID13(Action<int, int, int, int, int, int, int, int, int, int, int, int, int> func)
            {
                PRINT_E(13);
                func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], e[i + 8], e[i + 9], e[i + 10], e[i + 11], e[i + 12], e[i + 13]);
                i += 14;
            }

            ///////////////////////////////////////////////////////////////////////////
            // BOOL 系列函数（对应 C 宏版本）
            // 布尔函数返回 true / false，决定跳转逻辑
            ///////////////////////////////////////////////////////////////////////////

            void BOOL(Func<bool> func)
            {
                PRINT_E(0);
                if (func())
                    i += e[i + 1];
                else
                    i += e[i + 2];
                i += 3;
            }

            void BOOL1(Func<int, bool> func)
            {
                PRINT_E(1);
                if (func(e[i + 1]))
                    i += e[i + 2];
                else
                    i += e[i + 3];
                i += 4;
            }

            void BOOL2(Func<int, int, bool> func)
            {
                PRINT_E(2);
                if (func(e[i + 1], e[i + 2]))
                    i += e[i + 3];
                else
                    i += e[i + 4];
                i += 5;
            }

            void BOOL2_2(Func<int, int, bool> func)
            {
                PRINT_E(2);
                if (func(e[i + 1], e[i + 4]))
                    i += e[i + 2];
                else
                    i += e[i + 3];
                i += 5;
            }

            void BOOL3(Func<int, int, int, bool> func)
            {
                PRINT_E(3);
                if (func(e[i + 1], e[i + 2], e[i + 3]))
                    i += e[i + 4];
                else
                    i += e[i + 5];
                i += 6;
            }

            void BOOL4(Func<int, int, int, int, bool> func)
            {
                PRINT_E(4);
                if (func(e[i + 1], e[i + 2], e[i + 3], e[i + 4]))
                    i += e[i + 5];
                else
                    i += e[i + 6];
                i += 7;
            }

            void BOOL5(Func<int, int, int, int, int, bool> func)
            {
                PRINT_E(5);
                if (func(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5]))
                    i += e[i + 6];
                else
                    i += e[i + 7];
                i += 8;
            }

            void RUN_INSTRUCT(string name, Action instructAction)
            {
                Console.Write($"{name}: ");
                instructAction();
            }

            ///////////////////////////////////////////////////////////////////////
            // 模拟 REGISTER_INSTRUCT 宏 — 用 switch case 实现
            ///////////////////////////////////////////////////////////////////////

            while (i < e.Count && loop_)
            {
                switch (e[i])
                {
                    case 1: /* 执行旧对话指令（3个参数） */
                    RUN_INSTRUCT("oldTalk", () => VOID3(oldTalk));
                    break;

                    case 2: /* 添加物品（2个参数） */
                    RUN_INSTRUCT("addItem", () => VOID2(addItem));
                    break;

                    case 3: /* 修改事件（13个参数） */
                    RUN_INSTRUCT("modifyEvent", () => VOID13(modifyEvent));
                    break;

                    case 4: /* 检查是否正在使用物品（1个参数，返回布尔值） */
                    RUN_INSTRUCT("isUsingItem", () => BOOL1(isUsingItem));
                    break;

                    case 5: /* 询问是否战斗（无参数，返回布尔值） */
                    RUN_INSTRUCT("askBattle", () => BOOL(() => askBattle()));
                    break;

                    case 6: /* 尝试战斗（2个参数，特殊顺序，返回布尔值） */
                    RUN_INSTRUCT("tryBattle", () => BOOL2_2(tryBattle));
                    break;

                    case 8: /* 更改主地图音乐（1个参数） */
                    RUN_INSTRUCT("changeMainMapMusic", () => VOID1(changeMainMapMusic));
                    break;

                    case 9: /* 询问是否加入队伍（无参数，返回布尔值） */
                    RUN_INSTRUCT("askJoin", () => BOOL(() => askJoin()));
                    break;

                    case 10: /* 加入队伍（1个参数） */
                    RUN_INSTRUCT("join", () => VOID1(join));
                    break;

                    case 11: /* 询问是否休息（无参数，返回布尔值） */
                    RUN_INSTRUCT("askRest", () => BOOL(() => askRest()));
                    break;

                    case 12: /* 执行休息（无参数） */
                    RUN_INSTRUCT("rest", () => VOID(() => rest(), 0));
                    break;

                    case 13: /* 场景变亮（无参数） */
                    RUN_INSTRUCT("lightScence", () => VOID(() => lightScene(), 0));
                    break;

                    case 14: /* 场景变暗（无参数） */
                    RUN_INSTRUCT("darkScence", () => VOID(() => darkScene(), 0));
                    break;

                    case 15: /* 角色死亡（无参数） */
                    RUN_INSTRUCT("dead", () => VOID(() => dead(), 0));
                    break;

                    case 16: /* 检查是否在队伍中（1个参数，返回布尔值） */
                    RUN_INSTRUCT("inTeam", () => BOOL1(inTeam));
                    break;

                    case 17: /* 设置子地图层数据（5个参数） */
                    RUN_INSTRUCT("setSubMapLayerData", () => VOID5(setSubMapLayerData));
                    break;

                    case 18: /* 检查是否有物品（1个参数，返回布尔值） */
                    RUN_INSTRUCT("haveItemBool", () => BOOL1(haveItemBool));
                    break;

                    case 19: /* 设置场景位置（2个参数） */
                    RUN_INSTRUCT("oldSetScencePosition", () => VOID2(oldSetScencePosition));
                    break;

                    case 20: /* 检查队伍是否已满（无参数，返回布尔值） */
                    RUN_INSTRUCT("teamIsFull", () => BOOL(() => teamIsFull()));
                    break;

                    case 21: /* 离开队伍（1个参数） */
                    RUN_INSTRUCT("leaveTeam", () => VOID1(leaveTeam));
                    break;

                    case 22: /* 清空所有MP（无参数） */
                    RUN_INSTRUCT("zeroAllMP", () => VOID(() => zeroAllMP(), 0));
                    break;

                    case 23: /* 设置角色使用毒药（2个参数） */
                    RUN_INSTRUCT("setRoleUsePoison", () => VOID2(setRoleUsePoison));
                    break;

                    case 25: /* 子地图视角切换（4个参数） */
                    RUN_INSTRUCT("subMapViewFromTo", () => VOID4(subMapViewFromTo));
                    break;

                    case 26: /* 添加3个事件编号（5个参数） */
                    RUN_INSTRUCT("add3EventNum", () => VOID5(add3EventNum));
                    break;

                    case 27: /* 播放动画（3个参数） */
                    RUN_INSTRUCT("playAnimation", () => VOID3(playAnimation));
                    break;

                    case 28: /* 检查角色道德（3个参数，返回布尔值） */
                    RUN_INSTRUCT("checkRoleMorality", () => BOOL3(checkRoleMorality));
                    break;

                    case 29: /* 检查角色攻击（3个参数，返回布尔值） */
                    RUN_INSTRUCT("checkRoleAttack", () => BOOL3(checkRoleAttack));
                    break;

                    case 30: /* 角色行走（4个参数） */
                    RUN_INSTRUCT("walkFromTo", () => VOID4(walkFromTo));
                    break;

                    case 31: /* 检查是否有足够金钱（1个参数，返回布尔值） */
                    RUN_INSTRUCT("checkEnoughMoney", () => BOOL1(checkEnoughMoney));
                    break;

                    case 32: /* 添加物品（无提示，2个参数） */
                    RUN_INSTRUCT("addItemWithoutHint", () => VOID2(addItemWithoutHint));
                    break;

                    case 33: /* 学习旧魔法（3个参数） */
                    RUN_INSTRUCT("oldLearnMagic", () => VOID3(oldLearnMagic));
                    break;

                    case 34: /* 增加智商（2个参数） */
                    RUN_INSTRUCT("addIQ", () => VOID2(addIQ));
                    break;

                    case 35: /* 设置角色魔法（4个参数） */
                    RUN_INSTRUCT("setRoleMagic", () => VOID4(setRoleMagic));
                    break;

                    case 36: /* 检查角色性别（1个参数，返回布尔值） */
                    RUN_INSTRUCT("checkRoleSexual", () => BOOL1(checkRoleSexual));
                    break;

                    case 37: /* 增加道德值（1个参数） */
                    RUN_INSTRUCT("addMorality", () => VOID1(addMorality));
                    break;

                    case 38: /* 更改子地图图片（4个参数） */
                    RUN_INSTRUCT("changeSubMapPic", () => VOID4(changeSubMapPic));
                    break;

                    case 39: /* 打开子地图（1个参数） */
                    RUN_INSTRUCT("openSubMap", () => VOID1(openSubMap));
                    break;

                    case 40: /* 设置朝向（1个参数） */
                    RUN_INSTRUCT("setTowards", () => VOID1(setTowards));
                    break;

                    case 41: /* 角色添加物品（3个参数） */
                    RUN_INSTRUCT("roleAddItem", () => VOID3(roleAddItem));
                    break;

                    case 42: /* 检查队伍中是否有女性（无参数，返回布尔值） */
                    RUN_INSTRUCT("checkFemaleInTeam", () => BOOL(() => checkFemaleInTeam()));
                    break;

                    case 43: /* 检查是否有物品（1个参数，返回布尔值） */
                    RUN_INSTRUCT("haveItemBool", () => BOOL1(haveItemBool));
                    break;

                    case 44: /* 播放2个动画（6个参数） */
                    RUN_INSTRUCT("play2Amination", () => VOID6(play2Amination));
                    break;

                    case 45: /* 增加速度（2个参数） */
                    RUN_INSTRUCT("addSpeed", () => VOID2(addSpeed));
                    break;

                    case 46: /* 增加最大MP（2个参数） */
                    RUN_INSTRUCT("addMaxMP", () => VOID2(addMaxMP));
                    break;

                    case 47: /* 增加攻击力（2个参数） */
                    RUN_INSTRUCT("addAttack", () => VOID2(addAttack));
                    break;

                    case 48: /* 增加最大HP（2个参数） */
                    RUN_INSTRUCT("addMaxHP", () => VOID2(addMaxHP));
                    break;

                    case 49: /* 设置MP类型（2个参数） */
                    RUN_INSTRUCT("setMPType", () => VOID2(setMPType));
                    break;

                    case 50: /* 特殊指令50：检查是否有5个物品（条件分支） */
                    if (e[i + 1] > 128)
                        RUN_INSTRUCT("checkHave5Item", () => BOOL5(checkHave5Item));
                    else
                    {
                        int temp = e[i + 8];
                        instruct_50e(e[i + 1], e[i + 2], e[i + 3], e[i + 4], e[i + 5], e[i + 6], e[i + 7], ref temp);
                        e[i + 8] = temp;
                        i += 8;
                    }
                    break;

                    case 51: /* 询问软星（无参数） */
                    RUN_INSTRUCT("askSoftStar", () => VOID(() => askSoftStar(), 0));
                    break;

                    case 52: /* 显示道德值（无参数） */
                    RUN_INSTRUCT("showMorality", () => VOID(() => showMorality(), 0));
                    break;

                    case 53: /* 显示声望（无参数） */
                    RUN_INSTRUCT("showFame", () => VOID(() => showFame(), 0));
                    break;

                    case 54: /* 打开所有子地图（无参数） */
                    RUN_INSTRUCT("openAllSubMap", () => VOID(() => openAllSubMap(), 0));
                    break;

                    case 55: /* 检查事件ID（2个参数，返回布尔值） */
                    RUN_INSTRUCT("checkEventID", () => BOOL2(checkEventID));
                    break;

                    case 56: /* 增加声望（1个参数） */
                    RUN_INSTRUCT("addFame", () => VOID1(addFame));
                    break;

                    case 57: /* 打破石门（无参数） */
                    RUN_INSTRUCT("breakStoneGate", () => VOID(() => breakStoneGate(), 0));
                    break;

                    case 58: /* 争夺第一（无参数） */
                    RUN_INSTRUCT("fightForTop", () => VOID(() => fightForTop(), 0));
                    break;

                    case 59: /* 全员离开（无参数） */
                    RUN_INSTRUCT("allLeave", () => VOID(() => allLeave(), 0));
                    break;

                    case 7:
                    case -1: /* 结束事件循环 */
                    i += 1;
                    loop_ = false;
                    break;

                    default: /* 未知指令，跳过 */
                    i += 1;
                    break;
                }
            }

            if (talk_box_ != null)
                Element.removeFromRoot(talk_box_);
            if(talk_box_up_ != null)
                talk_box_up_.setContent("");
            if(talk_box_down_ != null)
                talk_box_down_.setContent("");

            return true;
        }


        public SubMapInfo getSubMapRecordFromID(int submap_id)
        {
            var submap_record = Save.getInstance().GetSubMapInfo(submap_id);
            if (submap_record == null) { submap_record = subscene_.getMapInfo(); }
            return submap_record;
        }


        public void CallLeaveEvent(Role role)
        {
            CallEvent(GetLeaveEvent(role));
        }

        public int GetLeaveEvent(Role role)
        {
            for (int i = 0; i < leave_event_id_.Count; i++)
            {
                if (leave_event_id_[i] == role.ID)
                {
                    return leave_event_0_ + 2 * i;
                }
            }
            return -1;
        }

        public void AddItemWithoutHint(int item_id, int count)
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

        /// <summary>
        /// 整理物品包
        /// </summary>
        public void arrangeBag()
        {
            /////////////////////////////////////////////////////////////////////////
            // 创建一个临时字典，用于统计每种物品的总数量（item_id → count）
            /////////////////////////////////////////////////////////////////////////
            var itemCount = new Dictionary<int, int>();
            var save = Save.getInstance();
            for (int i = 0; i < Constant.ITEM_IN_BAG_COUNT; i++)
            {
                var item = save.protagonistInformation.Items[i];
                if (item.item_id >= 0 && item.count > 0)
                {
                    if (!itemCount.ContainsKey(item.item_id))
                        itemCount[item.item_id] = 0;
                    itemCount[item.item_id] += item.count;
                }

                // 清空当前背包格子
                item.item_id = -1;
                item.count = 0;
                save.protagonistInformation.Items[i] = item;
            }
            int k = 0;
            foreach (var i in itemCount)
            {
                save.protagonistInformation.Items[k].item_id = i.Key;
                save.protagonistInformation.Items[k].count = i.Value;
                k++;
            }
        }











    }
}
