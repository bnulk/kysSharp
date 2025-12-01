using kysSharp.Types;
using SDL;
using System;
using System.Collections.Generic;
using System.Text;

namespace kysSharp
{
    public class BattleItemMenu : UIItem
    {
        private Role role_ = null;

        public BattleItemMenu()
        {
            setSelectUser(false);
        }

        public override void dealEvent(SDL_Event e)
        {
            if (role_ == null) return;
            if (role_.isAuto())
            {
                if (role_.AI_Item != null)
                {
                    current_item_ = role_.AI_Item;
                    setExit(true);
                }
            }
            else base.dealEvent(e);
        }

        public void SetRole(Role r) => role_ = r;
        public Role GetRole() => role_;

        public void AddItem(Item item, int count)
        {
            if (role_.Team == 0)
                Event.getInstance().AddItemWithoutHint(item.ID, count);
            else
                Event.getInstance().roleAddItem(role_.ID, item.ID, count);
        }

        public List<Item> GetAvaliableItems()
        {
            if (role_.Team == 0)
            {
                getItemsByType(force_item_type_);
            }
            else
            {
                available_items_.Clear();
                for (int i = 0; i < Constant.ROLE_TAKING_ITEM_COUNT; i++)
                {
                    var item = Save.getInstance().GetItem(role_.TakingItem[i]);
                    if (getItemDetailType(item) == force_item_type_)
                        available_items_.Add(item);
                }
            }
            return available_items_;
        }

        public static List<Item> GetAvaliableItems(Role role, int type)
        {
            var menu = new BattleItemMenu();
            menu.SetRole(role);
            menu.setForceItemType(type);
            var items = menu.GetAvaliableItems();
            return items;
        }


    }
}
