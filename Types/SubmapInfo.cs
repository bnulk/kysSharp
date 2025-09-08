
namespace kysSharp.Types
{
    /// <summary>
    /// 实际的场景数据
    /// </summary>
    public class SubmapInfo : SubmapInfoSave
    {
        public MAP_INT[,] layer_data_;
        public SubmapEvent[] events_;

        public SubmapInfo()
        {
            layer_data_ = new MAP_INT[Constant.SUBMAP_LAYER_COUNT, Constant.SUBMAP_COORD_COUNT * Constant.SUBMAP_COORD_COUNT];
            events_ = new SubmapEvent[Constant.SUBMAP_EVENT_COUNT];
        }



        //原始函数为MAP_INT& LayerData(int layer, int x, int y) { return layer_data_[layer][x + y * SUBMAP_COORD_COUNT]; }
        public void SetLayerData(int layer, int x, int y, MAP_INT acceptData)
        {
            layer_data_[layer, x + y * Constant.SUBMAP_COORD_COUNT] = acceptData;
        }
        public MAP_INT GetLayerData(int layer, int x, int y)
        {
            return layer_data_[layer, x + y * Constant.SUBMAP_COORD_COUNT];
        }

        public void SetEarth(int x, int y, MAP_INT acceptData)
        {
            SetLayerData(0, x, y, acceptData);
        }
        public MAP_INT GetEarth(int x, int y)
        {
            return GetLayerData(0, x, y);
        }

        public void SetBuilding(int x, int y, MAP_INT acceptData)
        {
            SetLayerData(1, x, y, acceptData);
        }
        public MAP_INT GetBuilding(int x, int y)
        {
            return GetLayerData(1, x, y);
        }

        public void SetDecoration(int x, int y, MAP_INT acceptData)
        {
            SetLayerData(2, x, y, acceptData);
        }
        public MAP_INT GetDecoration(int x, int y)
        {
            return GetLayerData(2, x, y);
        }
        public void SetEventIndex(int x, int y, MAP_INT acceptData)
        {
            SetLayerData(3, x, y, acceptData);
        }
        public MAP_INT GetEventIndex(int x, int y)
        {
            return GetLayerData(3, x, y);
        }
        public void SetBuildingHeight(int x, int y, MAP_INT acceptData)
        {
            SetLayerData(4, x, y, acceptData);
        }
        public MAP_INT GetBuildingHeight(int x, int y)
        {
            return GetLayerData(4, x, y);
        }
        public void SetDecorationHeight(int x, int y, MAP_INT acceptData)
        {
            SetLayerData(5, x, y, acceptData);
        }
        public MAP_INT GetDecorationHeight(int x, int y)
        {
            return GetLayerData(5, x, y);
        }

        public SubmapEvent? Event(int x, int y)
        {
            int i = GetEventIndex(x, y);
            return Event(i);
        }

        public SubmapEvent? Event(int i)
        {
            if (i < 0 || i >= Constant.SUBMAP_EVENT_COUNT)
            {
                return null;
            }
            return events_[i];
        }





    }
}
