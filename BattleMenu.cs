using kysSharp;
using kysSharp.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kysSharp
{
    ///////////////////////////////////////////////////////////////////////////
    // 战斗行动菜单（BattleActionMenu）
    // 负责控制战斗中角色的行为选择逻辑（包括 AI 自动行为）
    ///////////////////////////////////////////////////////////////////////////
    public class BattleActionMenu : MenuText
    {
        public Role? role_;
        public BattleScene battle_scene_;
        public MapSquare<MAP_INT> distance_layer_;

        public BattleActionMenu()
        {
            /////////////////////////////////////////////////////////////////////////
            // 构造函数
            /////////////////////////////////////////////////////////////////////////
            setStrings(new List<string> { "移動", "武學", "用毒", "解毒", "醫療", "暗器", "藥品", "等待", "狀態", "自動", "結束" });
            distance_layer_ = new MapSquare<MAP_INT>(BattleConstant.BATTLEMAP_COORD_COUNT);
        }

        ~BattleActionMenu()
        {
            distance_layer_ = null;
        }

        public void SetRole(Role r) => role_ = r;
        public int RunAsRole(Role r)
        {
            SetRole(r);
            return run();
        }

        public void SetBattleScene(BattleScene b) => battle_scene_ = b;

        /*
        /////////////////////////////////////////////////////////////////////////
        // 进入菜单时初始化
        /////////////////////////////////////////////////////////////////////////
        public override void onEntrance()
        {
            setVisible(true);
            foreach (var c in childs_)
            {
                c.setVisible(true);
                c.setState(State.Normal);
            }

            // 移动过则不可移动
            if (role_?.Moved || role_?.PhysicalPower < 10)
                childs_text_["移動"].setVisible(false);
            if (role_.GetLearnedMagicCount() <= 0 || role_.PhysicalPower < 20)
                childs_text_["武學"].setVisible(false);
            if (role_.UsePoison <= 0 || role_.PhysicalPower < 30)
                childs_text_["用毒"].setVisible(false);
            if (role_.Detoxification <= 0 || role_.PhysicalPower < 30)
                childs_text_["解毒"].setVisible(false);
            if (role_.Medcine <= 0 || role_.PhysicalPower < 10)
                childs_text_["醫療"].setVisible(false);
            if (role_.HiddenWeapon <= 15 || role_.PhysicalPower < 10)
                childs_text_["暗器"].setVisible(false);

            // 禁用等待
            childs_text_["等待"].setVisible(false);

            SetFontSize(20);
            Arrange(0, 0, 0, 28);
            pass_child_ = FindFirstVisibleChild();
            ForcePassChild();

            // 设置为未计算过AI的行动
            if (!role_.Moved)
                role_.AI_Action = -1;
        }

        /////////////////////////////////////////////////////////////////////////
        // 事件响应（AI 或 玩家）
        /////////////////////////////////////////////////////////////////////////
        public override void DealEvent(BP_Event e)
        {
            if (battle_scene_ == null) return;

            if (role_.IsAuto())
            {
                int act = AutoSelect(role_);
                SetResult(act);
                SetAllChildState(MenuState.Normal);
                childs_[act].SetState(MenuState.Press);
                SetExit(true);
                SetVisible(false); // AI不显示菜单
            }
            else
            {
                base.DealEvent(e);
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // AI 自动选择行为
        /////////////////////////////////////////////////////////////////////////
        public int AutoSelect(Role role)
        {
            List<Role> friends = new();
            List<Role> enemies = new();

            foreach (var r in battle_scene_.battle_roles_)
            {
                if (r.Team == role.Team)
                    friends.Add(r);
                else
                    enemies.Add(r);
            }

            List<AIAction> ai_actions = new();

            if (role.AI_Action == -1)
            {
                // 临时角色用于距离衰减计算
                Role role_temp = role.Clone();

                role.AI_Action = GetResultFromString("結束");
                role.AI_MoveX = role.X();
                role.AI_MoveY = role.Y();
                role.AI_ActionX = role.X();
                role.AI_ActionY = role.Y();
                role.AI_Magic = null;
                role.AI_Item = null;

                // 计算可移动范围
                battle_scene_.CalSelectLayer(role, 0, battle_scene_.CalMoveStep(role));

                /////////////////////////////////////////////////////////////////////////
                // 1. 吃药行为
                /////////////////////////////////////////////////////////////////////////
                string action_text = "藥品";
                if (childs_text_[action_text].GetVisible() &&
                    (role.HP < 0.2 * role.MaxHP || role.MP < 0.2 * role.MaxMP || role.PhysicalPower < 0.2 * Role.MAX_PHYSICAL_POWER))
                {
                    foreach (var item in BattleItemMenu.GetAvaliableItems(role, 2))
                    {
                        AIAction aa = new() { Action = GetResultFromString(action_text), item = item };
                        aa.point = 0;
                        if (item.AddHP > 0)
                            aa.point += Math.Min(item.AddHP, role.MaxHP - role.HP) - item.AddHP / 10;
                        if (item.AddMP > 0)
                            aa.point += Math.Min(item.AddMP, role.MaxMP - role.MP) / 2 - item.AddMP / 10;
                        else if (item.AddPhysicalPower > 0)
                            aa.point += Math.Min(item.AddPhysicalPower, Role.MAX_PHYSICAL_POWER - role.PhysicalPower);

                        if (aa.point > 0)
                        {
                            aa.point *= 1.5;
                            GetFarthestToAll(role, enemies, ref aa.MoveX, ref aa.MoveY);
                            ai_actions.Add(aa);
                        }
                    }
                }

                /////////////////////////////////////////////////////////////////////////
                // 2. 医疗、解毒、用毒、暗器、武学等
                /////////////////////////////////////////////////////////////////////////
                if (role.Morality > 50)
                {
                    // 解毒
                    if (childs_text_["解毒"].GetVisible())
                    {
                        foreach (var r2 in friends)
                        {
                            if (r2.Poison > 50)
                            {
                                AIAction aa = new();
                                CalAIActionNearest(r2, ref aa);
                                int act_dis = battle_scene_.CalActionStep(role.Detoxification);
                                if (act_dis >= CalNeedActionDistance(aa))
                                {
                                    aa.Action = GetResultFromString("解毒");
                                    aa.point = r2.Poison;
                                    ai_actions.Add(aa);
                                }
                            }
                        }
                    }
                    // 医疗
                    if (childs_text_["醫療"].GetVisible())
                    {
                        foreach (var r2 in friends)
                        {
                            if (r2.HP < 0.2 * r2.MaxHP)
                            {
                                AIAction aa = new();
                                CalAIActionNearest(r2, ref aa);
                                int act_dis = battle_scene_.CalActionStep(role.Medcine);
                                if (act_dis >= CalNeedActionDistance(aa))
                                {
                                    aa.Action = GetResultFromString("醫療");
                                    aa.point = r2.Medcine;
                                    ai_actions.Add(aa);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // 用毒
                    if (childs_text_["用毒"].GetVisible())
                    {
                        var r2 = GetNearestRole(role, enemies);
                        AIAction aa = new();
                        CalAIActionNearest(r2, ref aa);
                        int act_dis = battle_scene_.CalActionStep(role.UsePoison);
                        if (act_dis >= CalNeedActionDistance(aa))
                        {
                            aa.Action = GetResultFromString("用毒");
                            aa.point = Math.Min(Role.MAX_POISON - r2.Poison, role.UsePoison) / 2.0;
                            if (r2.HP < 10) aa.point = 1;
                            ai_actions.Add(aa);
                        }
                    }
                }

                /////////////////////////////////////////////////////////////////////////
                // 3. 暗器与武学略（同逻辑）
                /////////////////////////////////////////////////////////////////////////
                // ...
            }

            if (!role.Moved)
                return GetResultFromString("移動");
            else
                return role.AI_Action;
        }

        /////////////////////////////////////////////////////////////////////////
        // 计算距离层（BFS）
        /////////////////////////////////////////////////////////////////////////
        public void CalDistanceLayer(int x, int y, int max_step = 64)
        {
            distance_layer_.SetAll(max_step + 1);
            List<Point> cur = new() { new Point(x, y) };
            distance_layer_.Data(x, y, 0);

            int step = 0;
            while (step <= max_step)
            {
                List<Point> next = new();
                foreach (var p in cur)
                {
                    foreach (var d in Point.Directions4)
                    {
                        int nx = p.x + d.x, ny = p.y + d.y;
                        if (distance_layer_.Data(nx, ny) == max_step + 1 && battle_scene_.CanWalk(nx, ny))
                        {
                            distance_layer_.Data(nx, ny, step + 1);
                            next.Add(new Point(nx, ny));
                        }
                    }
                }
                if (next.Count == 0) break;
                cur = next;
                step++;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 获取距离所有敌人最远的可移动点
        /////////////////////////////////////////////////////////////////////////
        public void GetFarthestToAll(Role role, List<Role> roles, ref int x, ref int y)
        {
            double max_dis = 0;
            Random rand = new();
            for (int ix = 0; ix < BattleMap.BATTLEMAP_COORD_COUNT; ix++)
            {
                for (int iy = 0; iy < BattleMap.BATTLEMAP_COORD_COUNT; iy++)
                {
                    if (battle_scene_.CanSelect(ix, iy))
                    {
                        double cur = rand.NextDouble();
                        foreach (var r2 in roles)
                            cur += battle_scene_.CalDistance(ix, iy, r2.X(), r2.Y());
                        if (cur > max_dis)
                        {
                            max_dis = cur;
                            x = ix; y = iy;
                        }
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 获取距离某点最近的可行位置
        /////////////////////////////////////////////////////////////////////////
        public void GetNearestPosition(int x0, int y0, ref int x, ref int y)
        {
            Random rand = new();
            CalDistanceLayer(x0, y0);
            double min_dis = BattleMap.BATTLEMAP_COORD_COUNT * BattleMap.BATTLEMAP_COORD_COUNT;
            for (int ix = 0; ix < BattleMap.BATTLEMAP_COORD_COUNT; ix++)
            {
                for (int iy = 0; iy < BattleMap.BATTLEMAP_COORD_COUNT; iy++)
                {
                    if (battle_scene_.CanSelect(ix, iy))
                    {
                        double cur = distance_layer_.Data(ix, iy) + rand.NextDouble();
                        if (cur < min_dis)
                        {
                            min_dis = cur;
                            x = ix; y = iy;
                        }
                    }
                }
            }
        }

        public Role GetNearestRole(Role role, List<Role> roles)
        {
            int min_dis = 4096;
            Role result = null;
            foreach (var r in roles)
            {
                int d = battle_scene_.CalDistance(role, r);
                if (d < min_dis)
                {
                    result = r;
                    min_dis = d;
                }
            }
            return result;
        }

        public void CalAIActionNearest(Role r2, ref AIAction aa, Role r_temp = null)
        {
            GetNearestPosition(r2.X(), r2.Y(), ref aa.MoveX, ref aa.MoveY);
            aa.ActionX = r2.X();
            aa.ActionY = r2.Y();
            if (r_temp != null)
                r_temp.SetPositionOnly(aa.MoveX, aa.MoveY);
        }

        public int CalNeedActionDistance(AIAction aa)
        {
            return battle_scene_.CalDistance(aa.MoveX, aa.MoveY, aa.ActionX, aa.ActionY);
        }

        ///////////////////////////////////////////////////////////////////////////
        // AI 行为数据结构
        ///////////////////////////////////////////////////////////////////////////
        public struct AIAction
        {
            public int Action;
            public double point;
            public int MoveX, MoveY;
            public int ActionX, ActionY;
            public Magic magic;
            public Item item;
        }

        public void SetAIActionToRole(AIAction aa, Role role)
        {
            role.AI_Action = aa.Action;
            role.AI_MoveX = aa.MoveX;
            role.AI_MoveY = aa.MoveY;
            role.AI_ActionX = aa.ActionX;
            role.AI_ActionY = aa.ActionY;
            role.AI_Magic = aa.magic;
            role.AI_Item = aa.item;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // 魔法菜单
    ///////////////////////////////////////////////////////////////////////////
    public class BattleMagicMenu : MenuText
    {
        public Role role_ = null;
        public Magic magic_ = null;

        public override void OnEntrance()
        {
            SetVisible(true);
            List<string> magic_names = new();
            for (int i = 0; i < Role.ROLE_MAGIC_COUNT; i++)
            {
                var m = Save.GetInstance().GetRoleLearnedMagic(role_, i);
                if (m != null)
                    magic_names.Add($"{m.Name}{role_.GetRoleShowLearnedMagicLevel(i),7}");
            }
            SetStrings(magic_names);
            SetPosition(160, 200);
        }

        public override void DealEvent(BP_Event e)
        {
            if (role_ == null) return;
            if (role_.IsAuto())
            {
                magic_ = role_.AI_Magic;
                SetAllChildState(MenuState.Normal);
                SetResult(0);
                SetExit(true);
                SetVisible(false);
            }
            else base.DealEvent(e);
        }

        public override void OnPressedOK()
        {
            PressToResult();
            magic_ = Save.GetInstance().GetRoleLearnedMagic(role_, result_);
            if (magic_ != null) SetExit(true);
        }

        public override void OnPressedCancel()
        {
            magic_ = null;
            ExitWithResult(-1);
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // 物品菜单
    ///////////////////////////////////////////////////////////////////////////
    public class BattleItemMenu : UIItem
    {
        private Role role_ = null;

        public BattleItemMenu()
        {
            SetSelectUser(false);
        }

        public override void DealEvent(BP_Event e)
        {
            if (role_ == null) return;
            if (role_.IsAuto())
            {
                if (role_.AI_Item != null)
                {
                    current_item_ = role_.AI_Item;
                    SetExit(true);
                }
            }
            else base.DealEvent(e);
        }

        public void SetRole(Role r) => role_ = r;
        public Role GetRole() => role_;

        public void AddItem(Item item, int count)
        {
            if (role_.Team == 0)
                Event.GetInstance().AddItemWithoutHint(item.ID, count);
            else
                Event.GetInstance().RoleAddItem(role_.ID, item.ID, count);
        }

        public List<Item> GetAvaliableItems()
        {
            if (role_.Team == 0)
            {
                GetItemsByType(force_item_type_);
            }
            else
            {
                available_items_.Clear();
                for (int i = 0; i < Role.ROLE_TAKING_ITEM_COUNT; i++)
                {
                    var item = Save.GetInstance().GetItem(role_.TakingItem[i]);
                    if (GetItemDetailType(item) == force_item_type_)
                        available_items_.Add(item);
                }
            }
            return available_items_;
        }

        public static List<Item> GetAvaliableItems(Role role, int type)
        {
            var menu = new BattleItemMenu();
            menu.SetRole(role);
            menu.SetForceItemType(type);
            var items = menu.GetAvaliableItems();
            return items;
        }
        */


    }
}
