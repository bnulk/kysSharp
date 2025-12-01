using kysSharp.Types;
using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace kysSharp
{
    public class BattleMagicMenu : MenuText
    {
        public Role role_ = null;
        public Magic magic_ = null;

        public override void onEntrance()
        {
            setVisible(true);
            List<string> magic_names = new();
            for (int i = 0; i < Constant.ROLE_MAGIC_COUNT; i++)
            {
                var m = Save.getInstance().GetRoleLearnedMagic(ref role_, i);
                if (m != null)
                    magic_names.Add($"{m.Name}{role_.GetRoleShowLearnedMagicLevel(i),7}");
            }
            setStrings(magic_names);
            setPosition(160, 200);
        }

        public override void dealEvent(SDL_Event e)
        {
            if (role_ == null) return;
            
            if (role_.isAuto())
            {
                ArgumentNullException.ThrowIfNull(role_.AI_Magic);
                magic_ = role_.AI_Magic;
                setAllChildState(State.Normal);
                setResult(0);
                setExit(true);
                setVisible(false);
            }
            else base.dealEvent(e);
        }

        public override void onPressedOK()
        {
            pressToResult();
            magic_ = Save.getInstance().GetRoleLearnedMagic(ref role_, result_);
            if (magic_ != null) setExit(true);
        }

        public override void onPressedCancel()
        {
            magic_ = null;
            ExitWithResult(-1);
        }
    }
}
