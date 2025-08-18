using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal class SimpleRandom
    {
        // 生成均值 mu，标准差 sigma 的正态分布随机数
        static double NextGaussian(double mu = 0, double sigma = 1)
        {
            Random rand = new Random();
            // 生成 (0,1) 区间的随机数
            double u1 = 1.0 - rand.NextDouble(); // 避免 log(0)
            double u2 = 1.0 - rand.NextDouble();

            // Box-Muller 变换
            double standardNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);

            // 转换为指定均值、标准差
            return mu + sigma * standardNormal;
        }
    }
}
