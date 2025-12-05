using kysSharp;
using kysSharp.Types;
using Microsoft.VisualBasic;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static kysSharp.GameRandom;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace kysSharp
{
    public class BattleScene : Scene
    {
        /////////////////////////////////////////////////////////////////////////
        // 成员变量定义
        /////////////////////////////////////////////////////////////////////////
        private Save save_;

        public List<Role> battle_roles_ = new();
        public List<Role> friends_ = new();                            //保存开始就参战的人物，用来计算失败经验

        public BattleActionMenu battle_menu_;
        public BattleCursor battle_cursor_;
        public Head head_self_;


        public int battle_id_ = 0;
        public BattleInfo info_;

        //地面层，建筑层，选择层（负值为不可选，0和正值为可选）
        public MapSquare<MAP_INT> earth_layer_, building_layer_, select_layer_, effect_layer_;
        //角色层
        public MapSquare<Role> role_layer_;

        public int select_state_ = 0; // 0-其他，1-选移动目标，2-选行动目标role_layer_
        public int select_x_ = 0, select_y_ = 0;

        // 动画与状态参数
        public int action_frame_ = 0;
        public int action_type_ = -1;
        public int show_number_y_ = 0;
        public int effect_id_ = -1;
        public int effect_frame_ = 0;
        public byte dead_alpha_ = 255;
        public const int animation_delay_ = 2;

        public bool fail_exp_ = false;

        /////////////////////////////////////////////////////////////////////////
        // 构造与析构
        /////////////////////////////////////////////////////////////////////////
        public BattleScene()
        {
            full_window_ = true;
            COORD_COUNT = BattleConstant.BATTLEMAP_COORD_COUNT;

            earth_layer_ = new MapSquare<MAP_INT>(COORD_COUNT);
            building_layer_ = new MapSquare<MAP_INT>(COORD_COUNT);
            select_layer_ = new MapSquare<MAP_INT>(COORD_COUNT);
            effect_layer_ = new MapSquare<MAP_INT>(COORD_COUNT);
            role_layer_ = new MapSquare<Role>(COORD_COUNT);

            battle_menu_ = new BattleActionMenu();
            battle_menu_.setBattleScene(this);
            battle_menu_.setPosition(160, 200);

            head_self_ = new Head();
            addChild(head_self_);

            battle_cursor_ = new BattleCursor();
            battle_cursor_.setBattleScene(this);

            save_ = Save.getInstance();
        }

        public BattleScene(int id) : this()
        {
            setID(id);
        }

        /////////////////////////////////////////////////////////////////////////
        // 初始化与绘制逻辑
        /////////////////////////////////////////////////////////////////////////
        public void setID(int id)
        {
            battle_id_ = id;
            info_ = BattleMap.getInstance().GetBattleInfo(id) ?? new BattleInfo();

            BattleMap.getInstance().CopyLayerData((int)info_.BattleFieldID, 0, earth_layer_);
            BattleMap.getInstance().CopyLayerData((int)info_.BattleFieldID, 1, building_layer_);

            //role_layer_.SetAll(null);
            select_layer_.SetAll(-1);
            effect_layer_.SetAll(-1);
        }

        public void setSelectPosition(int x, int y) { select_x_ = x; select_y_ = y; }
        public override void draw()
        {
            Role? r0 = battle_roles_.Count > 0 ? battle_roles_[0] : null;
            if (r0 == null) return;

            Engine.getInstance().setRenderAssistTexture();
            Engine.getInstance().fillColor(new SDL_Color() { r = 0, g = 0, b = 0, a = 255 }, 0, 0, render_center_x_ * 2, render_center_y_ * 2);

            for (int sum = -view_sum_region_; sum <= view_sum_region_ + 15; sum++)
            {
                for (int i = -view_width_region_; i <= view_width_region_; i++)
                {
                    int ix = man_x_ + i + (sum / 2);
                    int iy = man_y_ - i + (sum - sum / 2);
                    var p = getPositionOnRender(ix, iy, man_x_, man_y_);
                    p.x += x_;
                    p.y += y_;
                    if (!isOutLine(ix, iy))
                    {
                        int num = earth_layer_.Data(ix, iy) / 2;
                        SDL_Color color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };

                        if (battle_cursor_.isRunning() && !r0.isAuto())                   //如果是自动人物没有变暗的选择效果看着太乱
                        {
                            if (select_layer_.Data(ix, iy) < 0)
                                color = new SDL_Color() { r = 64, g = 64, b = 64, a = 255 };
                            else
                                color = new SDL_Color() { r = 128, g = 128, b = 128, a = 255 };

                            if (battle_cursor_.Mode == BattleCursor.Action)
                            {
                                if (haveEffect(ix, iy))
                                {
                                    color = canSelect(ix, iy)
                                        ? new SDL_Color() { r = 192, g = 192, b = 192, a = 255 }
                                        : new SDL_Color() { r = 160, g = 160, b = 160, a = 255 };
                                }
                            }
                            if (ix == select_x_ && iy == select_y_)
                                color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
                        }


                        if (num > 0)
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y, color);
                    }
                }
            }

            // 建筑层和人物层绘制

            for (int sum = -view_sum_region_; sum <= view_sum_region_ + 15; sum++)
            {
                for (int i = -view_width_region_; i <= view_width_region_; i++)
                {
                    int ix = man_x_ + i + (sum / 2);
                    int iy = man_y_ - i + (sum - sum / 2);

                    var p = getPositionOnRender(ix, iy, man_x_, man_y_);
                    p.x += x_;
                    p.y += y_;

                    if (!isOutLine(ix, iy))
                    {
                        int num = building_layer_.Data(ix, iy) / 2;
                        if (num > 0)
                        {
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y);
                        }

                        var r = role_layer_.Data(ix, iy);
                        if (r != null)
                        {
                            string path = Path.Combine("fight","fight"+ r.HeadID.ToString("000"));

                            SDL_Color color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
                            byte alpha = 255;

                            if (battle_cursor_.isRunning() && !r0.isAuto())
                            {
                                color = new SDL_Color() { r = 128, g = 128, b = 128, a = 255 };
                                if (inEffect(r0, r))
                                {
                                    color = new SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
                                }
                            }

                            int pic = (r == r0)
                                ? calRolePic(r, action_type_, action_frame_)
                                : calRolePic(r);

                            if (r.HP <= 0)
                                alpha = dead_alpha_;

                            TextureManager.getInstance().renderTexture(path, pic, p.x, p.y, color, alpha);
                        }

                        if (effect_id_ >= 0 && haveEffect(ix, iy))
                        {
                            string path = Path.Combine("eft", "eft" + effect_id_.ToString("000"));

                            num = effect_frame_ + RandomClassical.rand(10) - RandomClassical.rand(10);

                            TextureManager.getInstance().renderTexture(
                                path,
                                num,
                                p.x,
                                p.y,
                                new SDL_Color() { r = 255, g = 255, b = 255, a = 255 },
                            224
                            );
                        }
                    }
                }
            }

            Engine.getInstance().renderAssistTextureToWindow();
        }

        //是否输了也有经验
        public void setHaveFailExp(bool b) { fail_exp_ = b; }


        public override void onEntrance()
        {
            calViewRegion();
            Audio.getInstance().playMusic(info_.Music);

            // 注意：此时才能得到窗口大小，用来设置头像位置
            head_self_.setPosition(80, 100);

            readBattleInfo();

            // 初始状态
            foreach (var r in battle_roles_)
            {
                setRoleInitState(r);
            }

            // 排序
            sortRoles();
        }

        public override void onExit()
        {
            if (result_ == 0 || (result_ == 1 && fail_exp_))
            {
                calExpGot();
            }

            // 清空全部角色的位置层
            foreach (var r in Save.getInstance().GetRoles())
            {
                r.SetRolePoitionLayer(null);
            }
        }

        public unsafe void readBattleInfo()
        {
            // 设置全部角色的位置层，避免今后出错
            foreach (var r in Save.getInstance().GetRoles())
            {
                r.SetPoitionLayer(role_layer_);
                r.Team = 2;   // 先全部设置成不存在的阵营
                r.Auto = 1;
            }

            // ======================
            // 敌方角色处理
            // ======================
            for (int i = 0; i < BattleConstant.BATTLE_ENEMY_COUNT; i++)
            {
                var roleId = info_.Enemy[i];
                var r = Save.getInstance().GetRole(roleId);

                if (r != null)
                {
                    battle_roles_.Add(r);
                    r.SetPosition(info_.EnemyX[i], info_.EnemyY[i]);
                    r.Team = 1;

                    readFightFrame(r);

                    r.FaceTowards = RandomClassical.rand(4);
                }
            }

            // 视角转至第一个敌人
            if (battle_roles_.Count > 0)
            {
                man_x_ = battle_roles_[0].X();
                man_y_ = battle_roles_[0].Y();
            }
            else
            {
                man_x_ = COORD_COUNT / 2;
                man_y_ = COORD_COUNT / 2;
            }

            // ======================
            // 判断是不是有自动战斗人物
            // ======================

            if (info_.AutoTeamMate[0] >= 0)
            {
                for (int i = 1; i < Constant.TEAMMATE_COUNT; i++)
                {
                    var r = Save.getInstance().GetRole(info_.AutoTeamMate[i]);
                    if (r != null)
                        friends_.Add(r);
                }
            }
            else
            {
                // C++ 中的 new TeamMenu + delete
                // 在 C# 中用 using 或普通 new
                var teamMenu = new TeamMenu();
                teamMenu.SetMode(1);
                teamMenu.run();
                friends_ = teamMenu.GetRoles();   // 返回 List<Role>

                // C# 无需 delete，对象交给 GC
            }

            // ======================
            // 队友加入战斗角色列表
            // ======================
            for (int i = 0; i < friends_.Count; i++)
            {
                var r = friends_[i];
                if (r != null)
                {
                    battle_roles_.Add(r);
                    r.SetPosition(info_.TeamMateX[i], info_.TeamMateY[i]);
                    r.Team = 0;
                }
            }
        }

        public void readFightFrame(Role r)
        {
            // 若已经有数据则直接返回
            if (r.FightFrame[0] >= 0)
            {
                return;
            }

            // 初始化为 0
            for (int i = 0; i < 5; i++)
            {
                r.FightFrame[i] = 0;
            }

            // 组装文件路径
            string file = $"game/resource/fight/fight{r.HeadID:000}/fightframe.txt";

            // 读取整个文件内容
            string frameTxt = File.ReadAllText(file);

            // 提取所有数字
            List<int> frames = new List<int>();
            ConvertLibs.FindNumbers(frameTxt, ref frames);


            // 将读取到的数据写入 FightFrame 数组
            for (int i = 0; i < frames.Count / 2; i++)
            {
                int index = frames[i * 2];
                int value = frames[i * 2 + 1];
                r.FightFrame[index] = value;
            }
        }

        public void setRoleInitState(Role r)
        {
            r.Acted = 0;
            r.ExpGot = 0;
            r.ShowString = "";
            r.FightingFrame = 0;
            r.Moved = 0;
            r.AI_Action = -1;

            if (r.Team == 0)
            {
                r.Auto = 0;

                // HP / MP 不能低于 MaxHP/10，不能超过 MaxHP
                GameUtil.limit2(ref r.HP, r.MaxHP / 10, r.MaxHP);
                GameUtil.limit2(ref r.MP, r.MaxMP / 10, r.MaxMP);
            }
            else
            {
                // 敌方
                r.Auto = 1;

                // 敌方的特殊初始化
                r.PhysicalPower = 90;
                r.HP = r.MaxHP;
                r.MP = r.MaxMP;
                r.Poison = 0;
                r.Hurt = 0;
            }

            // 读取动作帧数
            readFightFrame(r);

            // 初始朝向：面对最近的敌人
            setFaceTowardsNearest(r);
            // r.FaceTowards = RandomClassical.Rand(4);  // 随机朝向（开发注释）
        }

        public void setFaceTowardsNearest(Role r, bool inEffect_ = false)
        {
            int minDistance = COORD_COUNT * COORD_COUNT;
            Role? rNear = null;

            foreach (var r1 in battle_roles_)
            {
                if (r1 == r)
                    continue; // 避免自己指向自己

                bool targetOk;

                if (!inEffect_)
                {
                    // 非效果范围，依阵营判断敌人
                    targetOk = (r.Team != r1.Team);
                }
                else
                {
                    // 效果范围模式，例如 buff/debuff 范围
                    targetOk = inEffect(r, r1);
                }

                if (!targetOk)
                    continue;

                int dis = calDistance(r, r1);

                if (dis < minDistance)
                {
                    minDistance = dis;
                    rNear = r1;
                }
            }

            if (rNear != null)
            {
                r.FaceTowards = (int)calTowards(r.X(), r.Y(), rNear.X(), rNear.Y());
            }
        }

        public bool inEffect(Role r1, Role r2)
        {
            if (haveEffect(r2.X(), r2.Y()))
            {
                if (r1.ActTeam == 0 && r1.Team == r2.Team)
                {
                    return true;
                }
                else if (r1.ActTeam != 0 && r1.Team != r2.Team)
                {
                    return true;
                }
            }
            return false;
        }

        //所在坐标是否有效果
        public bool haveEffect(int x, int y)
        {
            return effect_layer_.Data(x, y) >= 0;
        }

        public int calDistance(Role r1, Role r2)
        {
            return calDistance(r1.X(), r1.Y(), r2.X(), r2.Y());
        }

        public int calDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public void sortRoles()
        {
            // 按 Speed 降序排序（Speed 大的排前面）
            battle_roles_.Sort(CompareRole);
        }

        private int CompareRole(Role r1, Role r2)
        {
            // C# 的排序需要返回 int：
            // < 0 ：r1 在 r2 前
            // = 0 ：相等
            // > 0 ：r1 在 r2 后
            return r2.Speed.CompareTo(r1.Speed);
        }

        public bool canSelect(int x, int y)
        {
            return !isOutLine(x, y) && select_layer_.Data(x, y) >= 0;
        }

        public int calRolePic(Role r, int style = -1, int frame = 0)
        {
            // 如果该动作没有帧数，则视为无效动作
            if(style!=-1)
            {
                if (r.FightFrame[style] <= 0)
                {
                    style = -1;
                }
            }            

            // style = -1 ：自动根据 FightFrame 查找第一个有效动作
            if (style == -1)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (r.FightFrame[i] > 0)
                    {
                        return r.FightFrame[i] * r.FaceTowards;
                    }
                }
            }
            else
            {
                int total = 0;

                for (int i = 0; i < 5; i++)
                {
                    if (i == style)
                    {
                        // 找到目标动作：总偏移 + 本动作偏移 + 当前帧
                        return total + r.FightFrame[style] * r.FaceTowards + frame;
                    }

                    // 每个动作有 4 个方向，所以偏移为 FightFrame[i] * 4
                    total += r.FightFrame[i] * 4;
                }
            }

            // 没找到动作，直接返回面向方向
            return r.FaceTowards;
        }

        ///////////////////////////////////////////////////////////////////////
        // 处理事件：dealEvent
        // 对应 C++: void BattleScene::dealEvent(BP_Event& e)
        //
        // 主要逻辑：
        // 1. 选择人物数组中的第一个人。
        // 2. 若该人物已经行动过，则表示所有人都行动过；重置行动状态并重新排序。
        // 3. 更新人物位置、选中位置以及头像状态。
        // 4. 调用 action() 处理行动。
        // 5. 清除死亡角色。
        // 6. 检测战斗结果，若我方胜利则退出战斗。
        ///////////////////////////////////////////////////////////////////////
        public override void dealEvent(SDL_Event e)
        {
            // 选择位于人物数组中的第一个人
            var r = battle_roles_[0];

            // 若第一个人已经行动过，说明所有人都行动过，则重置并排序
            if (r.Acted != 0)
            {
                resetRolesAct();
                sortRoles();
                r = battle_roles_[0];
            }

            // 定位
            man_x_ = r.X();
            man_y_ = r.Y();
            select_x_ = r.X();
            select_y_ = r.Y();
            head_self_.SetRole(ref r);
            head_self_.setState(State.Pass);

            // 行动
            action(r);

            // 清除被击退/死亡人物
            clearDead();

            // 检测战斗结果
            int battle_result = checkResult();

            // 我方胜 >= 0
            if (battle_result >= 0)
            {
                result_ = battle_result;
                setExit(true);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 处理事件：dealEvent2
        // 对应 C++: void BattleScene::dealEvent2(BP_Event& e)
        //
        // 逻辑：
        // 若检测到取消按键，则将我方队伍 Team == 0 的角色 Auto 标记清除。
        ///////////////////////////////////////////////////////////////////////
        public override void dealEvent2(SDL_Event e)
        {
            if (isPressCancel(e))
            {
                foreach (var r in battle_roles_)
                {
                    if (r.Team == 0)
                    {
                        r.Auto = 0;
                    }
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 重置所有角色的行动状态：ResetRolesAct
        // 对应 C++: void BattleScene::resetRolesAct()
        //
        // 功能说明：
        // 1. 将 Acted 设为 0 —— 表示未行动。
        // 2. 将 Moved 设为 0 —— 表示未移动。
        // 3. 调用 r.SetPosition(r.X(), r.Y()) —— 重新确认当前位置，通常用于触发
        //    内部更新（如网格刷新、碰撞刷新或内部状态重建）。
        ///////////////////////////////////////////////////////////////////////
        public void resetRolesAct()
        {
            foreach (var r in battle_roles_)
            {
                r.Acted = 0;
                r.Moved = 0;

                // C++: r->setPosition(r->X(), r->Y());
                r.SetPosition(r.X(), r.Y());
            }
        }

        public Role getSelectedRole()
        {
            return role_layer_.Data(select_x_, select_y_);
        }

        ///////////////////////////////////////////////////////////////////////
        // 行动主控：Action
        // 对应 C++：void BattleScene::action(Role* r)
        //
        // 整体流程：
        // 1. 调用 battle_menu_ 以角色身份运行，得到用户选择结果（字符串）。
        // 2. 根据字符串分支执行不同的行动逻辑。
        // 3. 如果此角色成功进行了一次行动(Acted != 0)，则将其移到队列末尾，
        //    并触发 poisonEffect(r)。
        //
        // 说明：
        // 原代码使用字符串进行判断（例如 "移動"、"武學" 等），C# 版本保持一致。
        // 如果之后需要优化，可以改为 enum，但当前严格保持原逻辑。
        ///////////////////////////////////////////////////////////////////////
        public void action(Role r)
        {
            // C++：battle_menu_->runAsRole(r);
            battle_menu_.runAsRole(r);

            // C++：std::string str = battle_menu_->getResultString();
            string str = battle_menu_.getResultString();

            ///////////////////////////////////////////////////////////////////////
            // 根据返回字符串决定行动
            ///////////////////////////////////////////////////////////////////////
            switch (str)
            {
                case "移動": actMove(r); break;
                case "武學": actUseMagic(r); break;
                case "用毒": actUsePoison(r); break;
                case "解毒": actDetoxification(r); break;
                case "醫療": actMedicine(r); break;
                case "暗器": actUseHiddenWeapon(r); break;
                case "藥品": actUseDrug(r); break;
                case "等待": actWait(r); break;
                case "狀態": actStatus(r); break;
                case "自動": actAuto(r); break;
                case "結束": actRest(r); break;
            }

            ///////////////////////////////////////////////////////////////////////
            // 若此角色完成行动，则放到队尾（保持轮转机制与原 C++ 相同）
            ///////////////////////////////////////////////////////////////////////
            if (r.Acted != 0)
            {
                // C++:
                // battle_roles_.erase(battle_roles_.begin());
                // battle_roles_.push_back(r);

                var first = battle_roles_[0];
                battle_roles_.RemoveAt(0);
                battle_roles_.Add(first);

                // C++：poisonEffect(r);
                poisonEffect(r);
            }
        }

        public void actUsePoison(Role r)
        {
            calSelectLayer(r, 1, calActionStep(r.UsePoison));
            battle_cursor_.setMode(BattleCursor.Action);
            battle_cursor_.setRoleAndMagic(r);

            r.ActTeam = 1;

            int selected = battle_cursor_.run();
            if (selected >= 0)
            {
                var r2 = getSelectedRole();
                if (r2 != null)
                {
                    int v = GameUtil.UsePoison(ref r, ref r2);
                    r2.ShowString = v.ToString();
                    r2.ShowColor = new SDL_Color() { r = 20, g = 255, b = 20, a = 255 };
                }

                r.PhysicalPower = GameUtil.limit(r.PhysicalPower - 3, 0, Constant.MAX_PHYSICAL_POWER);

                actionAnimation(r, 0, 30);
                showNumberAnimation();

                r.Acted = 1;
            }
        }

        public void actDetoxification(Role r)
        {
            calSelectLayer(r, 1, calActionStep(r.Detoxification));
            battle_cursor_.setMode(BattleCursor.Action);
            battle_cursor_.setRoleAndMagic(r);

            r.ActTeam = 0;

            int selected = battle_cursor_.run();
            if (selected >= 0)
            {
                var r2 = getSelectedRole();
                if (r2 != null)
                {
                    int v = GameUtil.Detoxification(ref r, ref r2);
                    r2.ShowString = "-" + v.ToString();
                    r2.ShowColor = new SDL_Color() { r = 20, g = 200, b = 255, a = 255 };
                }

                r.PhysicalPower = GameUtil.limit(r.PhysicalPower - 5, 0, Constant.MAX_PHYSICAL_POWER);

                actionAnimation(r, 0, 36);
                showNumberAnimation();

                r.Acted = 1;
            }
        }

        public void actMedicine(Role r)
        {
            calSelectLayer(r, 1, calActionStep(r.Medcine));

            battle_cursor_.setMode(BattleCursor.Action);
            battle_cursor_.setRoleAndMagic(r);

            r.ActTeam = 0;

            int selected = battle_cursor_.run();
            if (selected >= 0)
            {
                var r2 = getSelectedRole();
                if (r2 != null)
                {
                    int v = GameUtil.Medicine(ref r, ref r2);
                    r2.ShowString = "+"+ (Math.Abs(v)).ToString();
                    r2.ShowColor = new SDL_Color() { r=255,g=255,b=200,a=255};
                }

                r.PhysicalPower = GameUtil.limit(r.PhysicalPower - 5, 0, Constant.MAX_PHYSICAL_POWER);

                actionAnimation(r, 0, 0);
                showNumberAnimation();

                r.Acted = 1;
            }
        }

        public void actUseHiddenWeapon(Role r)
        {
            var item_menu = new BattleItemMenu();
            item_menu.setRole(r);
            item_menu.setForceItemType(3);
            item_menu.runAtPosition(300, 0);

            var item = item_menu.getCurrentItem();
            if (item != null)
            {
                calSelectLayer(r, 1, calActionStep(r.HiddenWeapon));
                battle_cursor_.setMode(BattleCursor.Action);
                battle_cursor_.setRoleAndMagic(r);
                r.ActTeam = 1;

                int selected = battle_cursor_.run();
                if (selected >= 0)
                {
                    var r2 = getSelectedRole();
                    int v = 0;

                    if (r2 != null)
                    {
                        v = calHiddenWeaponHurt(r, r2, item);
                        r2.ShowString = "-" + v.ToString();
                        r2.ShowColor = new SDL_Color() { r = 255, g = 20, b = 20, a = 255 };
                    }

                    showMagicName(GameUtil.EraseModredundantChar(item.strName));
                    r.PhysicalPower = GameUtil.limit(r.PhysicalPower - 5, 0, Constant.MAX_PHYSICAL_POWER);

                    actionAnimation(r, 0, item.HiddenWeaponEffectID);

                    if (r2 != null)
                    {
                        r2.HP = GameUtil.limit(r2.HP - v, 0, r2.MaxHP);
                    }

                    showNumberAnimation();
                    item_menu.AddItem(item, -1);
                    r.Acted = 1;
                }
            }

            // C++ delete 转为 C# 不需要，但若类有 Dispose，可调用
            // item_menu.Dispose();
        }

        public void actUseDrug(Role r)
        {
            var item_menu = new BattleItemMenu();
            item_menu.setForceItemType(2);
            item_menu.setRole(r);
            item_menu.runAtPosition(300, 0);

            var item = item_menu.getCurrentItem();
            if (item != null)
            {
                // Role r0 = *r;   ← C++ 是拷贝构造，这里需要复制一个值拷贝
                Role r0 = r.Clone();   // 你需要在 Role 类中实现 Clone()

                GameUtil.UseItem(ref r, ref item);

                var df = new ShowRoleDifference(r0, r);
                df.setText(GameUtil.EraseModredundantChar(r.strName)+ "服用"+GameUtil.EraseModredundantChar(item.strName));
                df.SetBlackScreen(false);
                df.SetShowHead(false);
                df.setPosition(250, 220);
                df.setStayFrame(40);
                df.run();
                // C# 无需 delete
                // df.Dispose(); // 若有 IDisposable 则调用

                item_menu.AddItem(item, -1);
                r.Acted = 1;
            }

            // C# 不需要 delete
            // item_menu.Dispose();
        }

        public void actWait(Role r)
        {
            // 等待，将自己插入到最后一个没行动的人的后面
            for (int i = 1; i < battle_roles_.Count; i++)
            {
                if (battle_roles_[i].Acted == 0)
                {
                    // C++: battle_roles_.erase(battle_roles_.begin());
                    battle_roles_.RemoveAt(0);

                    // C++: battle_roles_.insert(battle_roles_.begin() + i, r);
                    battle_roles_.Insert(i, r);
                }
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        /// <param name="r">角色</param>
        public void actStatus(Role r)
        {
            head_self_.setVisible(false);
            battle_cursor_.getHead().setVisible(false);
            battle_cursor_.getUIStatus().setVisible(true);

            calSelectLayer(r, 2, 0);
            battle_cursor_.setRoleAndMagic(r);
            battle_cursor_.setMode(BattleCursor.Check);
            battle_cursor_.run();

            head_self_.setVisible(true);
            battle_cursor_.getHead().setVisible(true);
            battle_cursor_.getUIStatus().setVisible(false);
        }

        public void actUseMagic(Role r)
        {
            var magicMenu = new BattleMagicMenu();

            while (true)
            {
                magicMenu.runAsRole(r);
                var magic = magicMenu.getMagic();

                if (magic == null)
                {
                    break;
                }

                r.ActTeam = 1;

                // level_index表示从0到9，而level从0到999
                int levelIndex = r.GetMagicLevelIndex(magic.ID);
                calSelectLayerByMagic(r.X(), r.Y(), r.Team, magic, levelIndex);

                // 选择目标
                battle_cursor_.setMode(BattleCursor.Action);
                battle_cursor_.setRoleAndMagic(r, magic, levelIndex);
                int selected = battle_cursor_.run();

                // 取消选择目标则重新进入选武功
                if (selected < 0)
                {
                    continue;
                }
                else
                {
                    for (int i = 0; i <= GameUtil.sign(r.AttackTwice); i++)
                    {
                        // 播放攻击画面，计算伤害
                        showMagicName(GameUtil.EraseModredundantChar(magic.strName));
                        r.PhysicalPower = GameUtil.limit(r.PhysicalPower - 3, 0, Constant.MAX_PHYSICAL_POWER);
                        r.MP = GameUtil.limit(r.MP - magic.CalNeedMP(levelIndex), 0, r.MaxMP);
                        useMagicAnimation(r, magic);
                        calMagicHurtAllEnemies(r, magic);
                        showNumberAnimation();

                        // 武学等级增加
                        int index = 1 + r.GetMagicOfRoleIndex(magic);
                        if (index >= 0)
                        {
                            r.MagicLevel[index] += RandomClassical.rand(2);
                            GameUtil.limit2(ref r.MagicLevel[index], 0, Constant.MAX_MAGIC_LEVEL);
                        }
                    }

                    r.Acted = 1;
                    break;
                }
            }
        }

        // 使用武学动画
        public void useMagicAnimation(Role r, Magic m)
        {
            if (r != null && m != null)
            {
                Audio.getInstance().playAsound(m.SoundID);   //这里播放音效严格说不正确，不管了
                actionAnimation(r, m.MagicType, m.EffectID);
            }
        }

        public void actionAnimation(Role r, int style, int effect_id, int shake = 0)
        {
            // 若角色不在当前选择的坐标上，则重新计算朝向
            if (r.X() != select_x_ || r.Y() != select_x_)
            {
                r.FaceTowards = (int)calTowards(r.X(), r.Y(), select_x_, select_x_);
            }

            // 自动战斗：自动朝向最近敌人
            if (r.isAuto())
            {
                setFaceTowardsNearest(r, true);
            }

            // 动作帧数
            int frame_count = r.FightFrame[style];
            action_type_ = style;

            // 播放动作动画
            for (action_frame_ = 0; action_frame_ < frame_count; action_frame_++)
            {
                drawAndPresent(animation_delay_);
            }

            action_frame_ = frame_count - 1;
            effect_id_ = effect_id;

            // 生成特效纹理组路径
            string path = Path.Combine("eft", "eft" + effect_id_.ToString("000"));

            // 特效帧数
            int effect_count = TextureManager.getInstance().getTextureGroupCount(path);

            // 播放特效音效
            Audio.getInstance().playEsound(effect_id_);

            // 播放特效动画
            for (effect_frame_ = 0; effect_frame_ < effect_count + 10; effect_frame_++)
            {
                // 有震屏参数
                if (shake > 0)
                {
                    x_ = RandomClassical.rand(shake) - RandomClassical.rand(shake);
                    y_ = RandomClassical.rand(shake) - RandomClassical.rand(shake);
                }

                drawAndPresent(animation_delay_);
            }

            // 动画复位
            action_frame_ = 0;
            action_type_ = -1;
            effect_frame_ = 0;
            effect_id_ = -1;
            x_ = 0;
            y_ = 0;
        }

        public int calMagiclHurtAllEnemies(Role r, Magic m, bool simulation)
        {
            int total = 0;

            foreach (var r2 in battle_roles_)
            {
                // 非我方且被击中（即所在位置的效果层非负）
                if (r2.Team != r.Team && haveEffect(r2.X(), r2.Y()))
                {
                    int hurt = calMagicHurt(r, r2, m);

                    if (!simulation)
                    {
                        r2.ShowString = "-" + hurt.ToString();
                        r2.ShowColor = new SDL_Color() { r = 255, g = 20, b = 20, a = 255 };
                        r2.HP = GameUtil.limit(r2.HP - hurt, 0, r2.MaxHP);
                        r.ExpGot += hurt / 10;
                    }
                    else
                    {
                        if (hurt >= r2.HP)
                        {
                            hurt = (int)(1.25 * r2.HP);
                        }
                    }

                    total += hurt;
                }
            }

            return total;
        }

        public void showNumberAnimation()
        {
            // 判断是否有需要显示的数字
            bool need_show = false;
            foreach (var r in battle_roles_)
            {
                if (!string.IsNullOrEmpty(r.ShowString))
                {
                    need_show = true;
                    break;
                }
            }
            if (!need_show) { return; }

            int size = 28;

            for (int i = 0; i <= 10; i++)
            {
                // C# 中需要使用 Action<object?>
                Action<object?> drawNumber = (_) =>
                {
                    foreach (var r in battle_roles_)
                    {
                        if (!string.IsNullOrEmpty(r.ShowString))
                        {
                            var p = getPositionOnWindow(r.X(), r.Y(), man_x_, man_y_);
                            int x = p.x - size * r.ShowString.Length / 4;
                            int y = p.y - 75 - i * 2;

                            GameFont.getInstance().draw(
                                r.ShowString,
                                size,
                                x,
                                y,
                                r.ShowColor,
                                (byte)(255 - 20 * i)
                            );
                        }
                    }
                };

                drawAndPresent(animation_delay_, drawNumber);
            }

            // 清除所有人的显示
            foreach (var r in battle_roles_)
            {
                r.ShowString = string.Empty;
            }
        }


        public void showMagicName(string name)
        {
            var magicName = new TextBox();
            magicName.setText(name);
            magicName.setPosition(450, 150);
            magicName.setFontSize(20);
            magicName.setStayFrame(40);
            magicName.run();
        }


        ///////////////////////////////////////////////////////////////////////
        // 中毒效果：PoisonEffect
        // 对应 C++：void BattleScene::poisonEffect(Role* r)
        //
        // 功能说明：
        // 1. 对角色 r 的中毒状态进行处理。
        // 2. 抗毒值（AntiPoison）会自动减少中毒值。
        // 3. 中毒值限制在 0~MAX_POISON 之间。
        // 4. HP 会扣除中毒值的三分之一，但最低保留 1 点血。
        ///////////////////////////////////////////////////////////////////////
        public void poisonEffect(Role r)
        {
            if (r != null)
            {
                // 抗毒高者会自动解毒
                r.Poison -= r.AntiPoison;

                // 限制中毒值在 [0, MAX_POISON] 之间
                GameUtil.limit2(ref r.Poison, 0, Constant.MAX_POISON);

                // 扣除血量
                r.HP -= r.Poison / 3;

                // 最低扣到1点
                if (r.HP < 1)
                {
                    r.HP = 1;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 清理死亡角色：ClearDead
        // 对应 C++：void BattleScene::clearDead()
        //
        // 功能说明：
        // 1. 检查是否存在 HP <= 0 的角色。
        // 2. 若存在，则播放退场渐隐动画（alpha 从 255 降到 0）。
        // 3. 将死亡角色从 battle_roles_ 中移除，并把其坐标设置为 (-1, -1)。
        // 4. 保留所有存活角色。
        ///////////////////////////////////////////////////////////////////////
        public void clearDead()
        {
            // 判断是否有人死亡
            bool foundDead = false;
            foreach (var r in battle_roles_)
            {
                if (r.HP <= 0)
                {
                    foundDead = true;
                    break;
                }
            }

            if (!foundDead)
                return;

            // 退场动画，逐步降低 dead_alpha_
            for (int i = 0; i <= 25; i++)
            {
                dead_alpha_ = (byte)(255 - i * 10);
                if (dead_alpha_ < 0)
                    dead_alpha_ = 0;

                drawAndPresent(animation_delay_);
            }

            // 动画结束，重置 alpha
            dead_alpha_ = 255;

            // 保留存活角色，并把死亡角色移除
            var alive = new List<Role>();

            foreach (var r in battle_roles_)
            {
                if (r.HP > 0)
                {
                    alive.Add(r);
                }
                else
                {
                    // 死亡角色移到地图外
                    r.SetPosition(-1, -1);
                }
            }

            battle_roles_ = alive;
        }

        ///////////////////////////////////////////////////////////////////////
        // 检查战斗结果：CheckResult
        //
        // 对应 C++：int BattleScene::checkResult()
        //
        // 功能说明：
        // 1. 检查是否有一方全灭。
        // 2. 返回值含义：
        //    - 返回 0  → 我方胜（team == 0 全体存活）
        //    - 返回 1  → 敌方胜（team == 0 全灭）
        //    - 返回 -1 → 胜负未分
        //
        // 判断逻辑说明：
        // team0 = 我方存活人数
        // battle_roles_.Count = 当前战场总存活人数
        //
        // 如果我方人数 == 总人数，则说明全是我方 → 我方胜。
        // 如果我方人数 == 0，则我方全灭 → 敌方胜。
        // 否则战斗继续。
        ///////////////////////////////////////////////////////////////////////
        public int checkResult()
        {
            int team0 = getTeamMateCount(0);

            if (team0 == battle_roles_.Count)
                return 0;   // 我方胜利

            if (team0 == 0)
                return 1;   // 敌方胜利

            return -1;       // 胜负未分，继续战斗
        }

        ///////////////////////////////////////////////////////////////////////
        // 获取指定队伍人数：GetTeamMateCount
        //
        // 对应 C++：int BattleScene::getTeamMateCount(int team)
        //
        // 功能说明：
        // 统计 battle_roles_ 中属于指定队伍 team 的人物数量。
        // team 示例：
        //   0 → 我方
        //   1 → 敌方
        //
        // 实现逻辑：
        // 遍历角色列表，判断 r.Team 是否等于 team，
        // 若相等则计数器加 1。
        ///////////////////////////////////////////////////////////////////////
        public int getTeamMateCount(int team)
        {
            int count = 0;

            foreach (var r in battle_roles_)
            {
                if (r.Team == team)
                {
                    count++;
                }
            }

            return count;
        }

        public void actAuto(Role r)
        {
            foreach (var role in battle_roles_)
            {
                role.Auto = 1;
            }
        }

        public void actRest(Role r)
        {
            if (r?.Moved == 0)
            {
                r.PhysicalPower = GameUtil.limit(r.PhysicalPower + 5, 0, Constant.MAX_PHYSICAL_POWER);
                r.HP = GameUtil.limit(r.HP + (int)(0.05 * r.MaxHP), 0, r.MaxHP);
                r.MP = GameUtil.limit(r.MP + (int)(0.05 * r.MaxMP), 0, r.MaxMP);
            }

            if (r != null)
                r.Acted = 1;
        }

        public void actMove(Role r)
        {
            if (r == null)
                return;

            int step = calMoveStep(r);
            calSelectLayer(r, 0, step);

            battle_cursor_.setRoleAndMagic(r);
            battle_cursor_.setMode(BattleCursor.Move);

            // run() == 0 表示成功执行移动
            if (battle_cursor_.run() == 0)
            {
                r.PhysicalPower = GameUtil.limit(r.PhysicalPower - 2, 0, Constant.MAX_PHYSICAL_POWER);

                // 保存移动前的位置
                r.SetPrevPosition(r.X(), r.Y());

                // 执行动画：从旧位置到 select_x_/select_y_
                moveAnimation(r, select_x_, select_y_);

                // Moved = 1 表示“已经移动过”
                r.Moved = 1;
            }
        }


        /// <summary>
        /// 依据能力值计算行动的范围步数
        /// </summary>
        /// <param name="ability">能力值</param>
        /// <returns></returns>
        public int calActionStep(int ability)
        { 
            return ability / 15 + 1; 
        }


        /// <summary>
        /// 计算可移动步数(考虑装备)
        /// </summary>
        /// <param name="r">角色</param>
        /// <returns></returns>
        public int calMoveStep(Role r)
        {
            if (r == null)
                return 0;

            // 如果已经移动过，则不可再移动
            if (r.Moved != 0)
                return 0;

            int speed = r.Speed;

            // 装备 0
            if (r.Equip0 >= 0)
            {
                var item0 = Save.getInstance().GetItem(r.Equip0);
                speed += item0.AddSpeed;
            }

            // 装备 1
            if (r.Equip1 >= 0)
            {
                var item1 = Save.getInstance().GetItem(r.Equip1);
                speed += item1.AddSpeed;
            }

            // 原版公式：speed / 15 + 1
            return speed / 15 + 1;
        }

        public void calSelectLayerByMagic(int x, int y, int team, Magic magic, int levelIndex)
        {
            int dis = magic.SelectDistance[levelIndex];

            switch (magic.AttackAreaType)
            {
                case 0:
                case 3:
                    // 与 C++：calSelectLayer(x, y, team, 1, dis)
                    calSelectLayer(x, y, team, 1, dis);
                    break;

                case 1:
                    // 与 C++：calSelectLayer(x, y, team, 3, dis)
                    calSelectLayer(x, y, team, 3, dis);
                    break;

                default:
                    // 与 C++：calSelectLayer(x, y, team, 4, dis)
                    calSelectLayer(x, y, team, 4, dis);
                    break;
            }
        }


        public void calSelectLayer(int x, int y, int team, int mode, int step = 0)
        {
            if (mode == 0)
            {
                select_layer_.SetAll(-1);

                List<Point> calStack = new List<Point>();
                select_layer_.Data(x, y) = (short)step;  // 正确写法

                calStack.Add(new Point(x, y));

                int count = 0;

                while (step >= 0)
                {
                    List<Point> nextStack = new List<Point>();

                    void CheckNext(Point p1)
                    {
                        // 未计算过且可以走
                        if (select_layer_.Data(p1.x, p1.y) == -1 && canWalk(p1.x, p1.y))
                        {
                            select_layer_.Data(p1.x, p1.y) = (short)(step - 1);
                            nextStack.Add(p1);
                            count++;
                        }
                    }

                    foreach (var p in calStack)
                    {
                        // 若在敌方旁边，但不是起点 — 则不向外扩散
                        if (!isNearEnemy(team, p.x, p.y) || (p.x == x && p.y == y))
                        {
                            CheckNext(new Point(p.x - 1, p.y));
                            CheckNext(new Point(p.x + 1, p.y));
                            CheckNext(new Point(p.x, p.y - 1));
                            CheckNext(new Point(p.x, p.y + 1));
                        }

                        // 计算上限，避免无限循环
                        if (count >= COORD_COUNT * COORD_COUNT)
                            break;
                    }

                    if (nextStack.Count == 0)
                        break;

                    calStack = nextStack;
                    step--;
                }
            }
            else if (mode == 1)
            {
                for (int ix = 0; ix < COORD_COUNT; ix++)
                {
                    for (int iy = 0; iy < COORD_COUNT; iy++)
                    {
                        select_layer_.Data(ix, iy) = (short)(step - calDistance(ix, iy, x, y));
                    }
                }
            }
            else if (mode == 2)
            {
                select_layer_.SetAll(0);
            }
            else if (mode == 3)
            {
                for (int ix = 0; ix < COORD_COUNT; ix++)
                {
                    for (int iy = 0; iy < COORD_COUNT; iy++)
                    {
                        int dx = Math.Abs(ix - x);
                        int dy = Math.Abs(iy - y);

                        if ((dx == 0 && dy <= step) ||
                            (dy == 0 && dx <= step))
                        {
                            select_layer_.Data(ix, iy) = 0;
                        }
                        else
                        {
                            select_layer_.Data(ix, iy) = -1;
                        }
                    }
                }

                select_layer_.Data(x, y) = -1; // 原位禁止移动
            }
            else
            {
                select_layer_.SetAll(-1);
            }
        }

        public void calSelectLayer(Role r, int mode, int step = 0)
        {
            if (r == null)
                return;

            calSelectLayer(r.X(), r.Y(), r.Team, mode, step);
        }

        /////////////////////////////////////////////////////////////////////////
        // r1 使用武学 magic 攻击 r2 的伤害（返回正值）
        /////////////////////////////////////////////////////////////////////////
        public int calMagicHurt(Role r1, Role r2, Magic magic)
        {
            int levelIndex = Save.getInstance().GetRoleLearnedMagicLevelIndex(ref r1, ref magic);

            int attack = r1.Attack + magic.Attack[levelIndex] / 3;
            int defence = r2.Defence;

            // 装备加成
            if (r1.Equip0 >= 0)
            {
                var i = Save.getInstance().GetItem(r1.Equip0);
                attack += i.AddAttack;
            }
            if (r1.Equip1 >= 0)
            {
                var i = Save.getInstance().GetItem(r1.Equip1);
                attack += i.AddAttack;
            }
            if (r2.Equip0 >= 0)
            {
                var i = Save.getInstance().GetItem(r2.Equip0);
                defence += i.AddDefence;
            }
            if (r2.Equip1 >= 0)
            {
                var i = Save.getInstance().GetItem(r2.Equip1);
                defence += i.AddDefence;
            }

            // 基础伤害
            int v = attack - defence;

            // 距离衰减
            int dis = calDistance(r1, r2);
            double decay = Math.Exp((dis - 1) / 10.0);
            v = (int)(v / decay);

            // 随机扰动
            v += GameRandom.RandomClassical.rand(10) - GameRandom.RandomClassical.rand(10);

            // 最低伤害：如果 <10，则 1~10
            if (v < 10)
            {
                v = 1 + GameRandom.RandomClassical.rand(10);
            }

            return v;
        }

        /////////////////////////////////////////////////////////////////////////
        // r 使用武学 m 对所有敌人造成伤害
        // simulation == true  → 仅模拟，用于 AI 评分
        /////////////////////////////////////////////////////////////////////////
        public int calMagicHurtAllEnemies(Role r, Magic m, bool simulation=false)
        {
            int total = 0;

            foreach (var r2 in battle_roles_)
            {
                // 目标必须：不是同队 & 在效果层内
                if (r2.Team != r.Team && haveEffect(r2.X(), r2.Y()))
                {
                    int hurt = calMagicHurt(r, r2, m);

                    if (!simulation)
                    {
                        // 显示伤害文字
                        r2.ShowString = $"-{hurt}";
                        r2.ShowColor = new SDL_Color() { r = 255, g = 0, b = 0, a = 255 };

                        // 扣血
                        r2.HP = GameUtil.limit(r2.HP - hurt, 0, r2.MaxHP);

                        // 经验奖励（与原版一致）
                        r.ExpGot += hurt / 10;
                    }
                    else
                    {
                        // AI 模拟时：如果能打死，算 1.25×HP 的收益（与原版完全一致）
                        if (hurt >= r2.HP)
                        {
                            hurt = (int)(1.25 * r2.HP);
                        }
                    }

                    total += hurt;
                }
            }
            return total;
        }


        public int calHiddenWeaponHurt(Role r1, Role r2, Item item)
        {
            // v = r1->HiddenWeapon - item->AddHP;
            int v = r1.HiddenWeapon - item.AddHP;

            // 计算距离
            int dis = calDistance(r1, r2);

            // v = v / exp((dis - 1) / 10)
            double decay = Math.Exp((dis - 1) / 10.0);
            v = (int)(v / decay);

            // 随机扰动：相当于 rand(10) - rand(10);
            v += GameRandom.RandomClassical.rand(10) - GameRandom.RandomClassical.rand(10);

            // v < 1 则 v = 1
            if (v < 1)
                v = 1;

            return v;
        }




        public void walk(Role r, int x, int y, Towards t)
        {
            if (canWalk(x, y))
            {
                r.SetPosition(x, y);
            }
        }

        public override bool canWalk(int x, int y)
        {
            if (isOutLine(x, y) || isBuilding(x, y) || isWater(x, y) || isRole(x, y))
                return false;

            return true;
        }

        public bool isBuilding(int x, int y)
        {
            return building_layer_.Data(x, y) > 0;
        }

        public bool isWater(int x, int y)
        {
            int num = earth_layer_.Data(x, y) / 2;

            if ((num >= 179 && num <= 181) ||
                num == 261 || num == 511 ||
                (num >= 662 && num <= 665) ||
                num == 674)
            {
                return true;
            }

            return false;
        }

        public bool isRole(int x, int y)
        {
            return role_layer_.Data(x, y) != null;
        }

        public override bool isOutScreen(int x, int y)
        {
            return (Math.Abs(man_x_ - x) >= 16 || Math.Abs(man_y_ - y) >= 20);
        }

        public bool isNearEnemy(int team, int x, int y)
        {
            foreach (var r1 in battle_roles_)
            {
                if (team != r1.Team && calDistance(r1.X(), r1.Y(), x, y) <= 1)
                    return true;
            }

            return false;
        }

        public void moveAnimation(Role r, int x, int y)
        {
            if (r == null)
                return;

            // 从目标格开始往回回溯路径
            List<Point> way = new List<Point>();

            // 局部函数，等价于 C++ 的 lambda（捕获外部变量 way）
            bool CheckNext(Point p1, int step)
            {
                if (canSelect(p1.x, p1.y) &&
                    select_layer_.Data(p1.x, p1.y) == (short)step)
                {
                    way.Add(p1);
                    return true;
                }
                return false;
            }

            // ① 将终点加入路径
            way.Add(new Point(x, y));

            // 起点步数
            int startStep = select_layer_.Data(r.X(), r.Y());
            // 终点步数
            int endStep = select_layer_.Data(x, y);

            // ② 从终点逐步反向往起点回溯
            for (int step = endStep; step < startStep; step++)
            {
                int cx = way[way.Count - 1].x;
                int cy = way[way.Count - 1].y;

                // 四方向寻找下一个点
                if (CheckNext(new Point(cx - 1, cy), step + 1)) continue;
                if (CheckNext(new Point(cx + 1, cy), step + 1)) continue;
                if (CheckNext(new Point(cx, cy - 1), step + 1)) continue;
                if (CheckNext(new Point(cx, cy + 1), step + 1)) continue;
            }

            // ③ 反向播放移动动画（倒序 way）
            for (int i = way.Count - 2; i >= 0; i--)
            {
                Point p = way[i];

                // 设置朝向
                r.FaceTowards = (int)calTowards(r.X(), r.Y(), p.x, p.y);

                // 设置位置
                r.SetPosition(p.x, p.y);

                // 每步呈现
                drawAndPresent(2);
            }

            // 最终位置
            r.SetPosition(x, y);
            r.Moved = 1;

            // 清空选中层
            select_layer_.SetAll(-1);
        }

        public void calEffectLayer(Role r, int selectX, int selectY, Magic m = null, int levelIndex = 0)
        {
            calEffectLayer(r.X(), r.Y(), selectX, selectY, m, levelIndex);
        }

        public void calEffectLayer(int x, int y, int selectX, int selectY, Magic m = null, int levelIndex = 0)
        {
            effect_layer_.SetAll(-1);

            // 若无武学（m == null）或 AttackAreaType == 0，则只选择 selectX, selectY 这一点
            if (m == null || m.AttackAreaType == 0)
            {
                effect_layer_.Data(selectX, selectY) = 0;
                return;
            }

            // AttackAreaType == 1 ：方向型直线攻击
            if (m.AttackAreaType == 1)
            {
                int tw = (int)calTowards(x, y, selectX, selectY);
                int dis = m.SelectDistance[levelIndex];

                for (int ix = x - dis; ix <= x + dis; ix++)
                {
                    for (int iy = y - dis; iy <= y + dis; iy++)
                    {
                        if (!isOutLine(ix, iy) &&
                            (x == ix || y == iy) &&
                            (int)calTowards(x, y, ix, iy) == tw)
                        {
                            effect_layer_.Data(ix, iy) = 0;
                        }
                    }
                }
            }
            // AttackAreaType == 2 ：十字攻击
            else if (m.AttackAreaType == 2)
            {
                int dis = m.SelectDistance[levelIndex];

                for (int ix = x - dis; ix <= x + dis; ix++)
                {
                    for (int iy = y - dis; iy <= y + dis; iy++)
                    {
                        if (!isOutLine(ix, iy) &&
                            (x == ix || y == iy))
                        {
                            effect_layer_.Data(ix, iy) = 0;
                        }
                    }
                }
            }
            // AttackAreaType == 3 ：圆形区域攻击（以 selectX, selectY 为中心）
            else if (m.AttackAreaType == 3)
            {
                int dis = m.AttackDistance[levelIndex];

                for (int ix = selectX - dis; ix <= selectX + dis; ix++)
                {
                    for (int iy = selectY - dis; iy <= selectY + dis; iy++)
                    {
                        if (!isOutLine(ix, iy))
                        {
                            effect_layer_.Data(ix, iy) = 0;
                        }
                    }
                }
            }
        }

        public void calExpGot()
        {
            head_self_.setVisible(false);

            List<Role> alive_teammate = new List<Role>();
            if (result_ == 0)
            {
                foreach (var r in battle_roles_)
                {
                    if (r.Team == 0)
                    {
                        alive_teammate.Add(r);
                    }
                }
            }
            else
            {
                alive_teammate = friends_;
            }

            if (alive_teammate.Count == 0) { return; }

            // 还在场的人获得经验
            foreach (var r in alive_teammate)
            {
                r.ExpGot += info_.Exp / alive_teammate.Count;
            }

            var show_exp = new ShowExp();
            show_exp.setRoles(alive_teammate);
            show_exp.run();
            // C# 不需要 delete

            // 升级，修炼物品
            var diff = new ShowRoleDifference();
            for(int i=0;i<alive_teammate.Count;i++)
            {
                var r = alive_teammate[i];
                // Role r0 = *r; ← C++ 拷贝，这里需要深拷贝
                Role r0 = r.Clone();

                var item = Save.getInstance().GetItem(r.PracticeItem);

                if (r.Level >= Constant.MAX_LEVEL)
                {
                    // 已满级，全加到物品经验
                    r.ExpForItem += r.ExpGot;
                }
                else if (item != null)
                {
                    // 未满级，平分经验
                    r.Exp += r.ExpGot / 2;
                    r.ExpForItem += r.ExpGot / 2;
                }
                else
                {
                    // 其余情况全加到人物经验
                    r.Exp += r.ExpGot;
                }

                // 避免越界
                if (r.Exp < r0.Exp) r.Exp = Constant.MAX_EXP;
                if (r.ExpForItem < r0.ExpForItem) r.ExpForItem = Constant.MAX_EXP;

                // 升级
                int change = 0;
                while (GameUtil.CanLevelUp(ref r))
                {
                    GameUtil.LevelUp(ref r);
                    change++;
                }
                if (change > 0)
                {
                    diff.SetTwinRole(r0, r);
                    diff.setText("升級");
                    diff.run();
                }

                // 修炼秘笈
                if (item != null)
                {
                    r0 = r.Clone();
                    change = 0;

                    while (GameUtil.CanFinishedItem(ref r))
                    {
                        GameUtil.UseItem(ref r, ref item);
                        change++;
                    }

                    if (change > 0)
                    {
                        diff.SetTwinRole(r0, r);
                        diff.setText("修煉"+ GameUtil.EraseModredundantChar(item.strName) + "成功");
                        diff.run();
                    }
                }
            }

            // C# 不需要 delete diff
        }








































    }
}
