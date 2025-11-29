//////////////////////////////////////////////////////////////////////
// BattleCursor.cs
// 战斗光标类：用于战斗场景中的目标选择与光标控制逻辑
//////////////////////////////////////////////////////////////////////

using kysSharp;
using kysSharp.Types;
using SDL;
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
        public MapSquare<MAP_INT> select_layer_ = null;
        public MapSquare<MAP_INT> effect_layer_ = null;

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
        public void setBattleScene(BattleScene b) => battle_scene_ = b;

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

        ///////////////////////////////////////////////////////////////////////
        // 设置角色与法术
        ///////////////////////////////////////////////////////////////////////
        public void setRoleAndMagic(Role r, Magic? m = null, int l = 0)
        {
            role_ = r;
            magic_ = m;
            level_index_ = l;
            head_selected_.SetRole(ref r);
        }

        public void setMode(int m) { mode_ = m; }

        ///////////////////////////////////////////////////////////////////////
        // 事件分发
        ///////////////////////////////////////////////////////////////////////
        public override void dealEvent(SDL_Event e)
        {
            if (battle_scene_ == null) return;

            int x = -1, y = -1;

            if (role_ == null)
                return;

            if (!role_.isAuto())
            {
                if (e.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN)
                {
                    int tw = (int)battle_scene_.getTowardsByKey(e.key.key);

                    // 线型攻击的特殊处理
                    if (magic_ != null && magic_.AttackAreaType == 1)
                    {
                        Scene.getTowardsPosition(role_.X(), role_.Y(), (Towards)tw, ref x, ref y);
                    }
                    else
                    {
                        Scene.getTowardsPosition(
                            battle_scene_.select_x_,
                            battle_scene_.select_y_,
                            (Towards)tw,
                            ref x, ref y
                        );
                    }
                }
                else if (e.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_MOTION)
                {
                    if (magic_ != null && magic_.AttackAreaType == 1)
                    {
                        int tw = (int)battle_scene_.getTowardsByMouse((int)e.motion.x, (int)e.motion.y);
                        Scene.getTowardsPosition(role_.X(), role_.Y(), (Towards)tw, ref x, ref y);
                    }
                    else
                    {
                        var p = battle_scene_.getMousePosition(
                            (int)e.motion.x, (int)e.motion.y,
                            role_.X(), role_.Y()
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
                    setResult(0);
                    setExit(true);
                }
                else if (mode_ == Action)
                {
                    x = role_.AI_ActionX;
                    y = role_.AI_ActionY;
                    setResult(0);
                    setExit(true);
                }
            }

            if (battle_scene_.canSelect(x, y))
            {
                battle_scene_.setSelectPosition(x, y);

                if (head_selected_.getVisible())
                {
                    head_selected_.SetRole(ref battle_scene_.role_layer_.Data(x, y));
                }

                if (ui_status_.getVisible())
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
                battle_scene_.calEffectLayer(
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
        public void dealMoveEvent(SDL_Event e)
        {
            // TODO: 未来实现移动模式下的光标处理逻辑
        }

        ///////////////////////////////////////////////////////////////////////
        // 攻击事件处理
        ///////////////////////////////////////////////////////////////////////
        public void dealActionEvent(SDL_Event e)
        {
            // TODO: 未来实现攻击模式下的光标处理逻辑
        }

        ///////////////////////////////////////////////////////////////////////
        // 进入时的初始化
        ///////////////////////////////////////////////////////////////////////
        public override void onEntrance()
        {
            int w = 0;
            int h = 0;
            Engine.getInstance().getPresentSize(ref w, ref h);
            head_selected_.setPosition(w - 400, h - 150);
            if(role_!=null)
            {
                battle_scene_.towards_ = (Towards)role_.FaceTowards;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 按键回调
        ///////////////////////////////////////////////////////////////////////
        public override void onPressedOK() => ExitWithResult(0);
        public override void onPressedCancel() => ExitWithResult(-1);




    }
}
