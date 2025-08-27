using MAP_INT = System.Int16;

namespace kysSharp.Types
{
    /// <summary>
    /// 场景事件数据
    /// </summary>
    public class SubmapEvent
    {
        private MAP_INT X_, Y_;

        //event1为主动触发，event2为物品触发，event3为经过触发
        public MAP_INT CannotWalk, Index, Event1, Event2, Event3, CurrentPic, EndPic, BeginPic, PicDelay;

        public void GetX(MAP_INT x)
        {
            X_ = x;
        }

        public void GetY(MAP_INT y)
        {
            Y_ = y;
        }

        public MAP_INT X()
        {
            return X_;
        }

        public MAP_INT Y()
        {
            return Y_;
        }

        /// <summary>
        /// 设置某个事件的坐标，在一些MOD里面此语句有错误
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="submap_record"></param>
        public void SetPosition(int x, int y, SubmapInfo submap_record)
        {
            if (x < 0) { x = X_; }
            if (y < 0) { y = Y_; }
            var index = submap_record.GetEventIndex(X_, Y_);
            submap_record.SetEventIndex(X_, Y_, -1);
            X_ = (MAP_INT)x;
            Y_ = (MAP_INT)y;
            submap_record.SetEventIndex(X_, Y_, index);
        }

        public void SetPic(int pic)
        {
            BeginPic = (MAP_INT)pic;
            CurrentPic = (MAP_INT)pic;
            EndPic = (MAP_INT)pic;
        }


    }
}
