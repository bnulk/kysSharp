using System;

namespace kysSharp.Types
{
    [Serializable]
    public class MapSquare<T>
    {
        public T[] data_;
        public int line_;

        public MapSquare()
        {
            data_ = Array.Empty<T>();
            line_ = 0;
        }

        public MapSquare(int size)
        {
            data_ = new T[size * size];
            line_ = size;
        }

        // 不保留原始数据，完全等价 C++
        public void Resize(int size)
        {
            data_ = new T[size * size];
            line_ = size;
        }

        // 和 C++ 一样： data[x + line*y]
        public ref T Data(int x, int y)
        {
            return ref data_[x + line_ * y];
        }

        public ref T Data(int index)
        {
            return ref data_[index];
        }

        public int Size => line_;
        public T[] Data_ => data_;

        public int SquareSize => data_.Length;

        public void SetAll(T value)
        {
            for (int i = 0; i < data_.Length; i++)
                data_[i] = value;
        }

        public void CopyTo(MapSquare<T> ms)
        {
            if (ms.data_.Length != data_.Length)
                ms.Resize(line_);

            for (int i = 0; i < data_.Length; i++)
                ms.data_[i] = data_[i];
        }

        public void CopyFrom(MapSquare<T> ms)
        {
            if (data_.Length != ms.data_.Length)
                Resize(ms.line_);

            for (int i = 0; i < data_.Length; i++)
                data_[i] = ms.data_[i];
        }






    }
}
