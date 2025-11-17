using kysSharp;
using kysSharp.Types;
using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using static kysSharp.GameRandom;

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
        public MapSquare earth_layer_, building_layer_, select_layer_, effect_layer_;
        //角色层
        public MapSquareRole role_layer_;

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

            earth_layer_ = new MapSquare(COORD_COUNT);
            building_layer_ = new MapSquare(COORD_COUNT);
            select_layer_ = new MapSquare(COORD_COUNT);
            effect_layer_ = new MapSquare(COORD_COUNT);
            role_layer_ = new MapSquareRole(COORD_COUNT);

            battle_menu_ = new BattleActionMenu();
            battle_menu_.SetBattleScene(this);
            battle_menu_.setPosition(160, 200);

            head_self_ = new Head();
            addChild(head_self_);

            battle_cursor_ = new BattleCursor();
            battle_cursor_.SetBattleScene(this);

            save_ = Save.getInstance();
        }

        public BattleScene(int id) : this()
        {
            SetID(id);
        }

        /////////////////////////////////////////////////////////////////////////
        // 初始化与绘制逻辑
        /////////////////////////////////////////////////////////////////////////
        public void SetID(int id)
        {
            battle_id_ = id;
            info_ = BattleMap.getInstance().GetBattleInfo(id) ?? new BattleInfo();

            BattleMap.getInstance().CopyLayerData((int)info_.BattleFieldID, 0, earth_layer_);
            BattleMap.getInstance().CopyLayerData((int)info_.BattleFieldID, 1, building_layer_);

            //role_layer_.SetAll(null);
            select_layer_.SetAll(-1);
            effect_layer_.SetAll(-1);
        }

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
                        int num = earth_layer_.GetData(ix, iy) / 2;
                        SDL_Color color = new SDL_Color() { r=255,g=255,b=255,a=255};

                        /*
                        if (battle_cursor_.IsRunning() && !r0.IsAuto())
                        {
                            if (select_layer_.Data(ix, iy) < 0)
                                color = new BP_Color(64, 64, 64, 255);
                            else
                                color = new BP_Color(128, 128, 128, 255);

                            if (battle_cursor_.Mode == BattleCursor.Action)
                            {
                                if (HaveEffect(ix, iy))
                                {
                                    color = CanSelect(ix, iy)
                                        ? new BP_Color(192, 192, 192, 255)
                                        : new BP_Color(160, 160, 160, 255);
                                }
                            }
                            if (ix == select_x_ && iy == select_y_)
                                color = new BP_Color(255, 255, 255, 255);
                        }
                        */

                        if (num > 0)
                            TextureManager.getInstance().renderTexture("smap", num, p.x, p.y, color);
                    }
                }
            }

            // 建筑层和人物层绘制略……
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
            List<int> frames= new List<int>();
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
            return effect_layer_.GetData(x, y) >= 0; 
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






























    }
}
