using System;
using System.Collections.Generic;
using System.Text;

namespace kysSharp.Types
{
    public class MapSquareRole
    {
        public Role[] data_;
        public int line_;

        public Role[] Data_ { get => data_; set => data_ = value; }
        public int Line_ { get => line_; set => line_ = value; }

        public MapSquareRole()
        {
            data_ = new Role[0];
            line_ = 0;
        }

        public MapSquareRole(int size)
        {
            data_ = new Role[size * size];
            line_ = size;
        }

        //不会保留原始数据
        public void Resize(int x)
        {
            data_ = new Role[x * x];
            line_ = x;
        }

        //原始函数为MAP_INT& data(int x, int y) { return data_[x + line_ * y]; }，接收数据用
        public void SetData(int x, int y, Role acceptData)
        {
            data_[x + line_ * y] = acceptData;
        }
        public Role GetData(int x, int y)
        {
            return data_[x + line_ * y];
        }

        public Role GetData(int x)
        {
            return data_[x];
        }

        //原始函数为MAP_INT& data(int x) { return data_[x]; }，接收数据用
        public void SetData(int x, Role acceptData)
        {
            data_[x] = acceptData;
        }
        public Role SetData(int x)
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

        public void SetAll(Role v)
        {
            Data_ = new Role[SquareSize()];
            for (int i = 0; i < SquareSize(); i++)
            {
                Data_[i] = (Role)v;
            }
        }
        public void CopyTo(ref MapSquareRole ms)
        {
            for (int i = 0; i < SquareSize(); i++)
            {
                ms.data_[i] = data_[i];
            }
        }
        public void CopyFrom(MapSquareRole ms)
        {
            for (int i = 0; i < SquareSize(); i++)
            {
                data_[i] = ms.data_[i];
            }
        }
    }
}
