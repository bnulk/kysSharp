using kysSharp.Types;

namespace kysSharp
{
    class Event
    {
        private static Event event_ = new Event();
        private List<int> offset_ = new List<int>();
        private List<int> length_ = new List<int>();
        private List<List<int>> kdef_ = new List<List<int>>();

        private int leave_event_0_;
        private List<int> leave_event_id_ = new List<int>();

        //两个对话，用于上面和下面，两个可以同时显示
        //视需要可增加更多
        private Element? talk_box_;
        //private Talk? talk_box_up_ = null;
        //private Talk? talk_box_down_ = null;

        //专用于显示确认和取消选项
        private MenuText? menu2_ = null;
        //专用于显示一个文本框
        private TextBox? text_box_ = null;
        private int event_id_ = -1;

        //private SubScene subscene_;
        private int submap_id_;
        private int x_, y_;
        private int event_index_;
        private int item_id_;
        private Item? item_;
        //Save* save_;
        private bool loop_;

        private int[] x50 = new int[65535];

        public static Event GetInstance()
        {
            return event_;
        }

        public bool CallEvent(int event_id, Element? subscene=null, int supmap_id=-1, int item_id=-1, int event_index=-1, int x=-1, int y=-1) //调用指令的内容写这里
        {
            return true;
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
                //ArrangeBag();
            }
        }

        /// <summary>
        /// 整理物品包
        /// </summary>
        public void ArrangeBag()
        {
            Dictionary<int, int> item_count = new Dictionary<int, int>();
            var save = Save.getInstance();
            for (int i = 0; i < Constant.ITEM_IN_BAG_COUNT; i++)
            {
                if (save.protagonistInformation.Items[i].item_id >= 0 && save.protagonistInformation.Items[i].count > 0)
                {
                    item_count[save.protagonistInformation.Items[i].item_id] += save.protagonistInformation.Items[i].count;
                }
                save.protagonistInformation.Items[i].item_id = -1;
                save.protagonistInformation.Items[i].count = 0;
            }
            int k = 0;
            foreach (var i in item_count)
            {
                save.protagonistInformation.Items[k].item_id = i.Key;
                save.protagonistInformation.Items[k].count = i.Value;
                k++;
            }
        }











    }
}
