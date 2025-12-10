using kysSharp;
using kysSharp.Types;
using SDL;
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

        public void setRole(Role r) => role_ = r;
        public int runAsRole(Role r)
        {
            setRole(r);
            return run();
        }

        public void setBattleScene(BattleScene b) => battle_scene_ = b;

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
            if (role_?.Moved!=0 || role_?.PhysicalPower < 10)
                childs_text_["移動"].setVisible(false);
            if (role_?.GetLearnedMagicCount() <= 0 || role_?.PhysicalPower < 20)
                childs_text_["武學"].setVisible(false);
            if (role_?.UsePoison <= 0 || role_?.PhysicalPower < 30)
                childs_text_["用毒"].setVisible(false);
            if (role_?.Detoxification <= 0 || role_?.PhysicalPower < 30)
                childs_text_["解毒"].setVisible(false);
            if (role_?.Medcine <= 0 || role_?.PhysicalPower < 10)
                childs_text_["醫療"].setVisible(false);
            if (role_?.HiddenWeapon <= 15 || role_?.PhysicalPower < 10)
                childs_text_["暗器"].setVisible(false);

            // 禁用等待
            childs_text_["等待"].setVisible(false);

            setFontSize(20);
            arrange(0, 0, 0, 28);
            pass_child_ = findFirstVisibleChild();
            forcePassChild();

            ///////////////////////////////////////////////////////////////////////
            // 若 Moved == 0（未移动），则将 AI_Action 重置为 -1
            ///////////////////////////////////////////////////////////////////////
            if (role_?.Moved == 0)
            {
                role_.AI_Action = -1;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 事件响应（AI 或 玩家）
        /////////////////////////////////////////////////////////////////////////
        public override void dealEvent(SDL_Event e)
        {
            if (battle_scene_ == null) return;

            ArgumentNullException.ThrowIfNull(role_);
            if (role_.isAuto())
            {
                int act = AutoSelect(role_);
                setResult(act);
                setAllChildState(State.Normal);
                childs_[act].setState(State.Press);
                setExit(true);
                setVisible(false); // AI不显示菜单
            }
            else
            {
                base.dealEvent(e);
            }
        }

        // "0移動", "1武學", "2用毒", "3解毒", "4醫療", "5暗器", "6藥品", "7等待", "8狀態", "9自動", "10結束"
        public int AutoSelect(Role role)
        {
            // 1. 分出友方与敌方 ------------------------------------------
            List<Role> friends = new();
            List<Role> enemies = new();

            foreach (var r in battle_scene_.battle_roles_)
            {
                if (r.Team == role.Team)
                    friends.Add(r);
                else
                    enemies.Add(r);
            }

            List<AIAction> aiActions = new();

            // **************************************************************
            // 2. AI 初始化：Clone 临时角色、初始化 AI action
            // **************************************************************
            if (role.AI_Action == -1)
            {
                Role roleTemp = role.Clone();   // C++: auto _role_temp = *role;
                                                //      role_temp = &_role_temp;

                // 初始化 AI 行为
                role.AI_Action = getResultFromString("結束");
                role.AI_MoveX = role.X();
                role.AI_MoveY = role.Y();
                role.AI_ActionX = role.X();
                role.AI_ActionY = role.Y();
                role.AI_Magic = null;
                role.AI_Item = null;

                // ----------------------------------------------------------
                // 3. 计算可移动范围
                // ----------------------------------------------------------
                battle_scene_.calSelectLayer(role, 0, battle_scene_.calMoveStep(role));

                // ==========================================================
                // 4. 吃药行为
                // ==========================================================
                string actionText = "藥品";
                if (childs_text_[actionText].getVisible() &&
                    (role.HP < 0.2 * role.MaxHP || role.MP < 0.2 * role.MaxMP || role.PhysicalPower < 0.2 * Constant.MAX_PHYSICAL_POWER))
                {
                    var items = BattleItemMenu.GetAvaliableItems(role, 2);

                    foreach (var item in items)
                    {
                        AIAction aa = new()
                        {
                            Action = getResultFromString(actionText),
                            point = 0
                        };

                        if (item.AddHP > 0)
                            aa.point += Math.Min(item.AddHP, role.MaxHP - role.HP) - item.AddHP / 10;

                        if (item.AddMP > 0)
                            aa.point += Math.Min(item.AddMP, role.MaxMP - role.MP) / 2 - item.AddMP / 10;

                        else if (item.AddPhysicalPower > 0)
                            aa.point += Math.Min(item.AddPhysicalPower, Constant.MAX_PHYSICAL_POWER - role.PhysicalPower);

                        if (aa.point > 0)
                        {
                            aa.item = item;
                            aa.point *= 1.5;

                            getFarthestToAll(role, enemies, ref aa.MoveX, ref aa.MoveY);
                            aiActions.Add(aa);
                        }
                    }
                }

                // ==========================================================
                // 5. Moral >= 50 : 解毒 & 医疗
                // ==========================================================
                if (role.Morality > 50)
                {
                    // ---------- 解毒 ----------
                    actionText = "解毒";
                    if (childs_text_[actionText].getVisible())
                    {
                        foreach (var r2 in friends)
                        {
                            if (r2.Poison > 50)
                            {
                                AIAction aa = new();
                                calAIActionNearest(r2, ref aa);

                                int actionDis = battle_scene_.calActionStep(role.Detoxification);
                                if (actionDis >= CalNeedActionDistance(aa))
                                {
                                    aa.Action = getResultFromString(actionText);
                                    aa.point = r2.Poison;
                                    aiActions.Add(aa);
                                }
                            }
                        }
                    }

                    // ---------- 医疗 ----------
                    actionText = "醫療";
                    if (childs_text_[actionText].getVisible())
                    {
                        foreach (var r2 in friends)
                        {
                            if (r2.HP < 0.2 * r2.MaxHP)
                            {
                                AIAction aa = new();
                                calAIActionNearest(r2, ref aa);

                                int actionDis = battle_scene_.calActionStep(role.Medcine);
                                if (actionDis >= CalNeedActionDistance(aa))
                                {
                                    aa.Action = getResultFromString(actionText);
                                    aa.point = r2.Medcine;
                                    aiActions.Add(aa);
                                }
                            }
                        }
                    }
                }

                // ==========================================================
                // 6. Moral < 50 : 用毒
                // ==========================================================
                else
                {
                    actionText = "用毒";
                    if (childs_text_[actionText].getVisible())
                    {
                        var r2 = GetNearestRole(role, enemies);

                        if (r2 != null)
                        {
                            AIAction aa = new();
                            calAIActionNearest(r2, ref aa);

                            int actionDis = battle_scene_.calActionStep(role.UsePoison);
                            if (actionDis >= CalNeedActionDistance(aa))
                            {
                                aa.Action = getResultFromString(actionText);
                                aa.point = Math.Min(Constant.MAX_POISON - r2.Poison, role.UsePoison) / 2;

                                if (r2.HP < 10)
                                    aa.point = 1;

                                aiActions.Add(aa);
                            }
                        }
                    }
                }

                // ==========================================================
                // 7. 暗器
                // ==========================================================
                actionText = "暗器";
                if (childs_text_[actionText].getVisible())
                {
                    var r2 = GetNearestRole(role, enemies);
                    
                    if(r2!=null)
                    {
                        AIAction aa = new();
                        calAIActionNearest(r2, ref aa, roleTemp);

                        int actionDis = battle_scene_.calActionStep(role.HiddenWeapon);

                        if (actionDis >= CalNeedActionDistance(aa))
                        {
                            aa.Action = getResultFromString(actionText);

                            var items = BattleItemMenu.GetAvaliableItems(role, 3);
                            foreach (var item in items)
                            {
                                aa.point = battle_scene_.calHiddenWeaponHurt(roleTemp, r2, item);

                                if (aa.point > r2.HP)
                                    aa.point = r2.HP * 1.25 - 10; // 稍微降低暗器价值

                                aa.item = item;
                                aiActions.Add(aa);
                            }
                        }
                    }                    
                }

                // ==========================================================
                // 8. 武学
                // ==========================================================
                actionText = "武學";
                if (childs_text_[actionText].getVisible())
                {
                    var r2 = GetNearestRole(role, enemies);
                    
                    if(r2!=null)
                    {
                        AIAction aa = new()
                        {
                            Action = getResultFromString(actionText)
                        };

                        calAIActionNearest(r2, ref aa, roleTemp);

                        // 遍历所有武学
                        for (int i = 0; i < Constant.MAGIC_COUNT; i++)
                        {
                            int maxHurt = -1;

                            var magic = Save.getInstance().GetRoleLearnedMagic(ref role, i);
                            if (magic == null)
                                continue;

                            int levelIndex = role.GetRoleMagicLevelIndex(i);

                            // Magic 可视范围
                            battle_scene_.calSelectLayerByMagic(aa.MoveX, aa.MoveY, role.Team, magic, levelIndex);

                            // 遍历所有点进行伤害评估
                            for (int ix = 0; ix < BattleConstant.BATTLEMAP_COORD_COUNT; ix++)
                            {
                                for (int iy = 0; iy < BattleConstant.BATTLEMAP_COORD_COUNT; iy++)
                                {
                                    if (!battle_scene_.canSelect(ix, iy))
                                        continue;

                                    battle_scene_.calEffectLayer(aa.MoveX, aa.MoveY, ix, iy, magic, levelIndex);

                                    int totalHurt = battle_scene_.calMagicHurtAllEnemies(roleTemp, magic, true);

                                    if (totalHurt > maxHurt)
                                    {
                                        maxHurt = totalHurt;
                                        aa.magic = magic;
                                        aa.ActionX = ix;
                                        aa.ActionY = iy;
                                    }
                                }
                            }

                            aa.point = maxHurt;
                            if (role.AttackTwice != 0)
                                aa.point *= 2;

                            aiActions.Add(aa);
                        }
                    }
                }

                // ==========================================================
                // 9. 求最大评分行为
                // ==========================================================
                double maxPoint = -1;
                var rand = new Random();

                for (int i = 0; i < aiActions.Count; i++)
                {
                    var aa = aiActions[i];           // aa 是 struct 的副本（可修改）
                    Console.WriteLine($"AI {GameUtil.EraseModredundantChar(role.strName)}: {getStringFromResult(aa.Action)}");
                    if (aa.item != null) Console.Write($"{GameUtil.EraseModredundantChar(aa.item.strName)} ");
                    if (aa.magic != null) Console.Write($"{GameUtil.EraseModredundantChar(aa.magic.strName)} ");

                    aa.point += rand.NextDouble();   // 修改副本
                    Console.WriteLine($"评分{aa.point:F2}");

                    if (aa.point < 1)
                    {
                        aa.Action = getResultFromString("結束");
                    }

                    if (aa.point > maxPoint)
                    {
                        maxPoint = aa.point;
                        SetAIActionToRole(aa, role);
                    }

                    aiActions[i] = aa;               // 把修改后的副本写回列表
                }
            }

            // ==========================================================
            // 10. 返回最终指令
            // ==========================================================
            if (role.Moved==0)
                return getResultFromString("移動");
            else
                return role.AI_Action;
        }


        /////////////////////////////////////////////////////////////////////////
        // 计算距离层（BFS 扩散）
        /////////////////////////////////////////////////////////////////////////
        public void calDistanceLayer(int startX, int startY, int maxStep = 64)
        {
            // 初始化所有格子为未访问状态（maxStep + 1 表示未访问）
            distance_layer_.SetAll((short)(maxStep + 1));

            // BFS 队列
            List<Point> current = new();
            distance_layer_.Data(startX, startY) = 0;
            current.Add(new Point(startX, startY));

            int count = 0;
            int step = 0;

            while (step <= maxStep)
            {
                List<Point> next = new();

                // 局部函数（对应 C++ 的 lambda）
                void CheckNext(Point p)
                {
                    // 未访问过且可行走
                    if (distance_layer_.Data(p.x, p.y) == (short)maxStep + 1 &&
                        battle_scene_.canWalk(p.x, p.y))
                    {
                        distance_layer_.Data(p.x, p.y) = (short)(step + 1);
                        next.Add(p);
                        count++;
                    }
                }

                // 扩散
                foreach (var p in current)
                {
                    distance_layer_.Data(p.x, p.y) = (short)step;

                    CheckNext(new Point(p.x - 1, p.y));
                    CheckNext(new Point(p.x + 1, p.y));
                    CheckNext(new Point(p.x, p.y - 1));
                    CheckNext(new Point(p.x, p.y + 1));

                    // 防止死循环
                    if (count >= distance_layer_.SquareSize)
                        break;
                }

                if (next.Count == 0)
                    break; // 没新格子，结束 BFS

                current = next;
                step++;
            }
        }


        public void getFarthestToAll(Role role, List<Role> roles, ref int x, ref int y)
        {
            // 用系统随机数即可（不需要自定义梅森旋转法）
            Random rand = new();

            double maxDis = 0.0;

            for (int ix = 0; ix < BattleConstant.BATTLEMAP_COORD_COUNT; ix++)
            {
                for (int iy = 0; iy < BattleConstant.BATTLEMAP_COORD_COUNT; iy++)
                {
                    if (battle_scene_.canSelect(ix, iy))
                    {
                        // 当前得分 = 随机微扰 + 到所有敌方的距离和
                        double curDis = rand.NextDouble();

                        foreach (var r2 in roles)
                        {
                            curDis += battle_scene_.calDistance(ix, iy, r2.X(), r2.Y());
                        }

                        if (curDis > maxDis)
                        {
                            maxDis = curDis;
                            x = ix;
                            y = iy;
                        }
                    }
                }
            }
        }


        public void getNearestPosition(int x0, int y0, ref int x, ref int y)
        {
            // 普通随机即可作为微扰（不需要Mersenne Twister）
            Random rand = new();

            // 计算 “到 x0,y0 的距离层”
            calDistanceLayer(x0, y0);

            double minDis = BattleConstant.BATTLEMAP_COORD_COUNT * BattleConstant.BATTLEMAP_COORD_COUNT;

            for (int ix = 0; ix < BattleConstant.BATTLEMAP_COORD_COUNT; ix++)
            {
                for (int iy = 0; iy < BattleConstant.BATTLEMAP_COORD_COUNT; iy++)
                {
                    if (battle_scene_.canSelect(ix, iy))
                    {
                        double curDis = (double)distance_layer_.Data(ix, iy) + rand.NextDouble();

                        if (curDis < minDis)
                        {
                            minDis = curDis;
                            x = ix;
                            y = iy;
                        }
                    }
                }
            }
        }


        public Role? GetNearestRole(Role role, List<Role> roles)
        {
            int minDis = 4096;
            Role? nearest = null;

            foreach (var r in roles)
            {
                int curDis = battle_scene_.calDistance(role, r);
                if (curDis < minDis)
                {
                    minDis = curDis;
                    nearest = r;
                }
            }

            return nearest;
        }


        public void calAIActionNearest(Role r2, ref AIAction aa, Role? rTemp=null)
        {
            // 找到距离 r2 最近且可移动的格子
            getNearestPosition(r2.X(), r2.Y(), ref aa.MoveX, ref aa.MoveY);

            // 行动目标点设为 r2 所在格
            aa.ActionX = r2.X();
            aa.ActionY = r2.Y();

            // 若提供临时角色，则更新它的位置
            if (rTemp != null)
            {
                rTemp.SetPositionOnly(aa.MoveX, aa.MoveY);
            }
        }


        public int CalNeedActionDistance(AIAction aa)
        {
            return battle_scene_.calDistance(aa.MoveX, aa.MoveY, aa.ActionX, aa.ActionY);
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
}
