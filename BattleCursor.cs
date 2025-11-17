//////////////////////////////////////////////////////////////////////
// BattleCursor.cs
// 战斗光标类：用于战斗场景中的目标选择与光标控制逻辑
//////////////////////////////////////////////////////////////////////

using kysSharp;
using kysSharp.Types;
using System;

namespace kysSharp
{
    ///////////////////////////////////////////////////////////////////////
    // BattleCursor 类
    // 说明：
    //   负责控制战斗时的光标移动、目标选择、以及 AI 的选择行为。
    //   对应原 C++ 类 BattleCursor。
    ///////////////////////////////////////////////////////////////////////
    public class BattleCursor : Element
    {
        public int[]? select_x_ = null;
        public int[]? select_y_ = null;
        public MapSquare? select_layer_ = null;
        public MapSquare? effect_layer_ = null;

        public Role? role_ = null;
        public Magic? magic_ = null;
        public int level_index_ = 0;

        private Head head_selected_;
        public Head HeadSelected => head_selected_;

        private UIStatus ui_status_;
        public UIStatus UIStatus => ui_status_;

        public const int Other = -1;
        public const int Move = 0;
        public const int Action = 1;
        public const int Check = 2;

        private int mode_ = Move;
        public int Mode
        {
            get => mode_;
            set => mode_ = value;
        }

        private BattleScene battle_scene_ = null;
        public void SetBattleScene(BattleScene b) => battle_scene_ = b;

        ///////////////////////////////////////////////////////////////////////
        // 构造与析构
        ///////////////////////////////////////////////////////////////////////
        public BattleCursor()
        {
            head_selected_ = new Head();
            addChild(head_selected_);

            ui_status_ = new UIStatus();
            ui_status_.setVisible(false);
            ui_status_.SetShowButton(false);
            addChild(ui_status_, 300, 0);
        }

        ~BattleCursor()
        {
            // 在 C# 中由 GC 负责内存释放
        }

        /*
        ///////////////////////////////////////////////////////////////////////
        // 设置角色与法术
        ///////////////////////////////////////////////////////////////////////
        public void SetRoleAndMagic(Role r, Magic? m = null, int l = 0)
        {
            role_ = r;
            magic_ = m;
            level_index_ = l;
            head_selected_.SetRole(ref r);
        }

        ///////////////////////////////////////////////////////////////////////
        // 事件分发
        ///////////////////////////////////////////////////////////////////////
        public override void dealEvent(Event e)
        {
            if (battle_scene_ == null) return;

            int x = -1, y = -1;

            if (!role_.IsAuto())
            {
                if (e.type == BP_EventType.KEYDOWN)
                {
                    int tw = battle_scene_.GetTowardsByKey(e.key.keysym.sym);

                    // 线型攻击的特殊处理
                    if (magic_ != null && magic_.AttackAreaType == 1)
                    {
                        Scene.GetTowardsPosition(role_.X, role_.Y, tw, out x, out y);
                    }
                    else
                    {
                        Scene.GetTowardsPosition(
                            battle_scene_.select_x_,
                            battle_scene_.select_y_,
                            tw,
                            out x, out y
                        );
                    }
                }
                else if (e.type == BP_EventType.MOUSEMOTION)
                {
                    if (magic_ != null && magic_.AttackAreaType == 1)
                    {
                        int tw = battle_scene_.GetTowardsByMouse(e.motion.x, e.motion.y);
                        Scene.GetTowardsPosition(role_.X, role_.Y, tw, out x, out y);
                    }
                    else
                    {
                        var p = battle_scene_.GetMousePosition(
                            e.motion.x, e.motion.y,
                            role_.X, role_.Y
                        );
                        x = p.x;
                        y = p.y;
                    }
                }
            }
            else
            {
                if (mode_ == Move)
                {
                    x = role_.AI_MoveX;
                    y = role_.AI_MoveY;
                    SetResult(0);
                    SetExit(true);
                }
                else if (mode_ == Action)
                {
                    x = role_.AI_ActionX;
                    y = role_.AI_ActionY;
                    SetResult(0);
                    SetExit(true);
                }
            }

            if (battle_scene_.CanSelect(x, y))
            {
                battle_scene_.SetSelectPosition(x, y);

                if (head_selected_.GetVisible())
                {
                    head_selected_.SetRole(battle_scene_.role_layer_.Data(x, y));
                }

                if (ui_status_.GetVisible())
                {
                    ui_status_.SetRole(battle_scene_.role_layer_.Data(x, y));
                }
            }

            if (mode_ == Move)
            {
                // 处理移动模式逻辑（留空）
            }
            else if (mode_ == Action)
            {
                battle_scene_.CalEffectLayer(
                    role_,
                    battle_scene_.select_x_,
                    battle_scene_.select_y_,
                    magic_,
                    level_index_
                );
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 移动事件处理
        ///////////////////////////////////////////////////////////////////////
        public void DealMoveEvent(BP_Event e)
        {
            // TODO: 未来实现移动模式下的光标处理逻辑
        }

        ///////////////////////////////////////////////////////////////////////
        // 攻击事件处理
        ///////////////////////////////////////////////////////////////////////
        public void DealActionEvent(BP_Event e)
        {
            // TODO: 未来实现攻击模式下的光标处理逻辑
        }

        ///////////////////////////////////////////////////////////////////////
        // 进入时的初始化
        ///////////////////////////////////////////////////////////////////////
        public override void OnEntrance()
        {
            Engine.GetInstance().GetPresentSize(out int w, out int h);
            head_selected_.SetPosition(w - 400, h - 150);
            battle_scene_.towards_ = role_.FaceTowards;
        }

        ///////////////////////////////////////////////////////////////////////
        // 按键回调
        ///////////////////////////////////////////////////////////////////////
        public override void OnPressedOK() => ExitWithResult(0);
        public override void OnPressedCancel() => ExitWithResult(-1);
        */



    }
}
