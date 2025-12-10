using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace kysSharp
{
    internal class GameRandom
    {
        public enum RandomType
        {
            Uniform = 0,             // 均匀分布 [a, b)
            Normal = 1,              // 正态分布 N(a, b)，其中 a 为均值，b 为标准差
        }

        public sealed class RandomX<T> where T : IFloatingPoint<T>
        {
            ///////////////////////////////////////////////////////////////////////
            // 字段：分布类型与两个随机引擎（高质量 / 快速）
            ///////////////////////////////////////////////////////////////////////
            private RandomType _type = RandomType.Uniform;

            // 高质量引擎（对应 C++ 的 std::mt19937）
            private Random _rng = new Random();

            // 快速引擎（对应 C++ 的 std::minstd_rand0）
            private Lcg31 _rngFast = new Lcg31(1u);

            ///////////////////////////////////////////////////////////////////////
            // 分布参数
            // - 对 Uniform ：区间 [a, b)
            // - 对 Normal  ：均值 = a，标准差 = b（需 b > 0）
            ///////////////////////////////////////////////////////////////////////
            private T _a = T.Zero;
            private T _b = T.One;

            ///////////////////////////////////////////////////////////////////////
            // Box–Muller 变换缓存：一次生成两个正态数，复用一个以减少计算
            ///////////////////////////////////////////////////////////////////////
            private bool _hasSpare = false;
            private double _spare = 0.0;
            private bool _hasSpareFast = false;
            private double _spareFast = 0.0;

            ///////////////////////////////////////////////////////////////////////
            // 构造
            ///////////////////////////////////////////////////////////////////////
            public RandomX() { }

            ///////////////////////////////////////////////////////////////////////
            // 设置分布类型
            ///////////////////////////////////////////////////////////////////////
            public void SetRandomType(RandomType t) => _type = t;

            ///////////////////////////////////////////////////////////////////////
            // 设置分布参数：
            //   - Uniform: [a, b)
            //   - Normal : mean=a, stddev=b
            ///////////////////////////////////////////////////////////////////////
            public void SetParameter(T a, T b)
            {
                _a = a;
                _b = b;
            }

            ///////////////////////////////////////////////////////////////////////
            // 采样（高质量引擎）
            ///////////////////////////////////////////////////////////////////////
            public T Next()
            {
                return _type == RandomType.Uniform
                    ? NextUniform(useFast: false)
                    : NextNormal(useFast: false);
            }

            ///////////////////////////////////////////////////////////////////////
            // 采样（快速引擎）
            ///////////////////////////////////////////////////////////////////////
            public T NextFast()
            {
                return _type == RandomType.Uniform
                    ? NextUniform(useFast: true)
                    : NextNormal(useFast: true);
            }

            ///////////////////////////////////////////////////////////////////////
            // 统一播种（无参：使用加密随机数生成种子；有参：固定种）
            // - 同时播种高质量与快速引擎
            ///////////////////////////////////////////////////////////////////////
            public void SetSeed()
            {
                byte[] b = new byte[4];
                RandomNumberGenerator.Fill(b);
                int seed = BitConverter.ToInt32(b, 0);

                _rng = new Random(seed);
                _rngFast.Seed((uint)(1 + Math.Abs(seed % 2147483646)));
                _hasSpare = _hasSpareFast = false;
            }

            public void SetSeed(int seed)
            {
                _rng = new Random(seed);
                _rngFast.Seed((uint)(1 + Math.Abs(seed % 2147483646)));
                _hasSpare = _hasSpareFast = false;
            }

            ///////////////////////////////////////////////////////////////////////
            // 私有：均匀分布采样
            ///////////////////////////////////////////////////////////////////////
            private T NextUniform(bool useFast)
            {
                // 检查区间
                if (_b <= _a)
                    throw new ArgumentException("均匀分布参数非法：要求 b > a。");

                double u = useFast ? _rngFast.NextDouble() : _rng.NextDouble(); // [0,1)
                return _a + (_b - _a) * T.CreateChecked(u);
            }

            ///////////////////////////////////////////////////////////////////////
            // 私有：正态分布采样（Box–Muller）
            ///////////////////////////////////////////////////////////////////////
            private T NextNormal(bool useFast)
            {
                double mean = double.CreateChecked(_a);
                double stddev = double.CreateChecked(_b);
                if (stddev <= 0)
                    throw new ArgumentException("正态分布参数非法：标准差 b 必须 > 0。");

                if (!useFast)
                {
                    if (_hasSpare)
                    {
                        _hasSpare = false;
                        return T.CreateChecked(mean + stddev * _spare);
                    }

                    double u1, u2;
                    do { u1 = _rng.NextDouble(); } while (u1 <= double.Epsilon); // (0,1]
                    u2 = _rng.NextDouble();

                    double r = Math.Sqrt(-2.0 * Math.Log(u1));
                    double theta = 2.0 * Math.PI * u2;
                    double z0 = r * Math.Cos(theta);
                    double z1 = r * Math.Sin(theta);

                    _spare = z1;  // 缓存第二个
                    _hasSpare = true;

                    return T.CreateChecked(mean + stddev * z0);
                }
                else
                {
                    if (_hasSpareFast)
                    {
                        _hasSpareFast = false;
                        return T.CreateChecked(mean + stddev * _spareFast);
                    }

                    double u1, u2;
                    do { u1 = _rngFast.NextDouble(); } while (u1 <= double.Epsilon);
                    u2 = _rngFast.NextDouble();

                    double r = Math.Sqrt(-2.0 * Math.Log(u1));
                    double theta = 2.0 * Math.PI * u2;
                    double z0 = r * Math.Cos(theta);
                    double z1 = r * Math.Sin(theta);

                    _spareFast = z1;
                    _hasSpareFast = true;

                    return T.CreateChecked(mean + stddev * z0);
                }
            }

            ///////////////////////////////////////////////////////////////////////
            // 内部：minstd_rand0 的简单实现（与 C++ 示例的“快速引擎”呼应）
            ///////////////////////////////////////////////////////////////////////
            private struct Lcg31
            {
                private const uint Modulus = 2147483647; // 2^31 - 1（梅森素数）
                private const uint Multiplier = 16807;      // minstd_rand0 的乘子
                private uint _state;

                public Lcg31(uint seed)
                {
                    _state = seed == 0 ? 1u : seed % Modulus;
                }

                public void Seed(uint seed)
                {
                    _state = seed == 0 ? 1u : seed % Modulus;
                }

                public uint NextUInt()
                {
                    _state = (uint)(((ulong)_state * Multiplier) % Modulus);
                    return _state;
                }

                public double NextDouble()
                {
                    // 映射到 (0,1)，与 C++ 实现的语义接近
                    uint x = NextUInt();
                    return (x - 1) / 2147483646.0;
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        // 兼容 C 风格的“古典”接口（示意），对应 C++ RandomClassical
        ///////////////////////////////////////////////////////////////////////////
        public static class RandomClassical
        {
            private static Random? _rng;

            ///////////////////////////////////////////////////////////////////////
            // srand(): 若无参，使用当前时间种子；有参则固定种子
            ///////////////////////////////////////////////////////////////////////
            public static void srand()
            {
                _rng = new Random(); // 时间种子
            }

            public static void srand(int seed)
            {
                _rng = new Random(seed);
            }

            ///////////////////////////////////////////////////////////////////////
            // rand(n): 返回 [0, n) 的整数；n<=0 时返回 0
            // 注意：同样存在取模偏差（与 C 的 rand()%n 一致）
            ///////////////////////////////////////////////////////////////////////
            public static int rand(int n)
            {
                if (n <= 0) return 0;
                if (_rng == null) _rng = new Random();
                return _rng.Next(n);
            }

            public static int randNextNormalInt0ToN(int n)
            {
                if (n <= 1) return 0;

                double mean = n / 2.0;
                double std = n / 10.0;     // 建议值，可根据需要调整，分母越大分布越集中

                if (_rng != null)
                {
                    double z = _rng.Next();         // 正态浮点
                    int v = (int)Math.Round(z);     // 四舍五入
                    return Math.Clamp(v, 0, n); // 限制到 [0, n]
                }
                else
                {
                    int v = (int)n / 2;
                    return v;
                }
                
            }
        }
    }


    /*
     * 使用示例
    static void Main()
    {
        var rngF = new RandomX<float>();    // 等价于 C++ template<T=float> 的默认
        rngF.SetSeed(12345);                // 固定种子：可复现实验
        rngF.SetRandomType(RandomType.Uniform);
        rngF.SetParameter(0f, 1f);          // 均匀分布 [0,1)
        float u = rngF.Next();              // 高质量引擎
        float uf = rngF.NextFast();         // 快速引擎（LCG）

        var rngD = new RandomX<double>();
        rngD.SetSeed();                     // 无参：使用加密随机获得的随机种子
        rngD.SetRandomType(RandomType.Normal);
        rngD.SetParameter(0.0, 1.0);        // 正态(均值0, 标准差1)
        double z1 = rngD.Next();            // 高质量引擎采样
        double z2 = rngD.NextFast();        // 快速引擎采样

        // 古典接口
        RandomClassical.srand();
        int k = RandomClassical.rand(10);   // [0,10)
    }
    */
















}
