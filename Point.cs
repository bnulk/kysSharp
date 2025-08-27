using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    /////////////////////////////////////////////////////////////////////////
    // Point 类
    // 表示一个二维点，包含 x 和 y 坐标
    /////////////////////////////////////////////////////////////////////////
    public class Point
    {
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;

        // 默认构造函数
        public Point() { }

        // 带参构造函数
        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    /////////////////////////////////////////////////////////////////////////
    // PointEx 类，继承 Point
    // 扩展了路径搜索相关的数据 (A* 算法常用)
    /////////////////////////////////////////////////////////////////////////
    public class PointEx : Point
    {
        public int step = 0;
        public int g = 0, h = 0, f = 0;
        public int Gx = 0, Gy = 0;

        public Towards? towards;
        public PointEx? parent;             // C# 引用类型，允许 null
        public PointEx?[] child = new PointEx?[4];

        // 构造函数
        public PointEx() : base() { }

        ///////////////////////////////////////////////////////////////////////////
        // 删除以该节点为根的树 (递归删除子节点)
        ///////////////////////////////////////////////////////////////////////////
        public void delTree(PointEx? node)
        {
            if (node == null) return;
            for (int i = 0; i < 4; i++)
            {
                if (node != null && node.child[i] != null)
                {
                    delTree(node.child[i]);
                    node.child[i] = null;  // 手动释放引用，等待 GC 回收
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        // 比较函数，相当于重载 C++ 中的 "小于" 运算符
        ///////////////////////////////////////////////////////////////////////////
        public bool lessThan(PointEx myPoint)
        {
            return f > myPoint.f;   // 注意：这里和名字相反，按 C++ 原逻辑
        }

        ///////////////////////////////////////////////////////////////////////////
        // 启发式函数 (例如 A* 搜索中常用曼哈顿距离)
        ///////////////////////////////////////////////////////////////////////////
        public int heuristic(int Fx, int Fy)
        {
            return Math.Abs(Fx - this.x) + Math.Abs(Fy - this.y);
        }
    }
}
