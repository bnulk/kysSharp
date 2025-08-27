using System;
using MAP_INT = System.Int16;

namespace kysSharp.Types
{
    [Serializable]
    public class MapSquare
    {
        private MAP_INT[] data_;
        private MAP_INT line_;

        public MAP_INT[] Data_ { get => data_; set => data_ = value; }
        public MAP_INT Line_ { get => line_; set => line_ = value; }

        public MapSquare()
        {
            data_= new MAP_INT[0];
            line_ = 0;
        }

        public MapSquare(int size)
        {
            data_ = new MAP_INT[size * size];
            line_ = (MAP_INT)size;
        }

        //不会保留原始数据
        public void Resize(int x)
        {
            Data_ = new MAP_INT[x * x];
            Line_ = (MAP_INT)x;
        }

        //原始函数为MAP_INT& data(int x, int y) { return data_[x + line_ * y]; }，接收数据用
        public void SetData(int x, int y, MAP_INT acceptData)
        {
            data_[x + Line_ * y] = acceptData;
        }
        public MAP_INT GetData(int x, int y)
        {
            return data_[x + Line_ * y];
        }

        public MAP_INT GetData(int x)
        {
            return data_[x];
        }

        //原始函数为MAP_INT& data(int x) { return data_[x]; }，接收数据用
        public void SetData(int x, MAP_INT acceptData)
        {
            data_[x] = acceptData;
        }
        public MAP_INT SetData(int x)
        {
            return data_[x];
        }

        public int Size()
        {
            return Line_;
        }

        public int SquareSize()
        {
            return Line_ * Line_;
        }

        public void SetAll(int v)
        {
            Data_ = new MAP_INT[SquareSize()];
            for (int i = 0; i < SquareSize(); i++)
            {
                Data_[i] = (MAP_INT)v;
            }
        }
        public void CopyTo(ref MapSquare ms)
        {
            for (int i = 0; i < SquareSize(); i++)
            {
                ms.Data_[i] = Data_[i];
            }
        }
        public void CopyFrom(MapSquare ms)
        {
            for (int i = 0; i < SquareSize(); i++)
            {
                Data_[i] = ms.Data_[i];
            }
        }
    }



}
