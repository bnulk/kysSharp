using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace kysSharp
{
    class GameFile
    {

        /// <summary>
        /// 获取idx文件中的内容
        /// </summary>
        /// <param name="filename_idx">idx文件名</param>
        /// <param name="filename_grp">grp文件名</param>
        /// <param name="offset">偏移值</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static byte[] GetIdxContent(string filename_idx, string filename_grp, ref List<int> offset, ref List<int> length)
        {
            int[] Ridx;

            int len = 0;
            readFile(filename_idx, out Ridx, out len);

            offset = new List<int>();
            length = new List<int>();
            offset.Add(0);

            for (int i = 0; i < len; i++)
            {
                offset.Add((int)Ridx[i]);
                length.Add((int)(offset[i + 1] - offset[i]));
            }
            int total_length = offset[offset.Count - 1];

            byte[] Rgrp = new byte[total_length];
            readFile(filename_grp, out Rgrp, total_length);

            return Rgrp;
        }

        /// <summary>
        /// 按字节数组读取二进制文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="s">字节数组</param>
        /// <param name="lenOfByte">文件字节数</param>
        /// <returns></returns>
        public static bool readFile(string filename, out byte[] s, out int numberOfByte)
        {
            StringBuilder tmpString = new StringBuilder();

            try
            {
                //根据指定文件路径创建文件流
                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);                                 //创建BinaryReader对象以读取二进制文件
                numberOfByte = Convert.ToInt32(fileStream.Length);
                s = new byte[numberOfByte];
                for (int i = 0; i < numberOfByte; i++)
                {
                    s[i] = binaryReader.ReadByte();
                }
                binaryReader.Close();
                fileStream.Close();
            }
            catch
            {
                Console.WriteLine("Can not open file " + filename.ToString());
                s = new byte[0];
                numberOfByte = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 按字节数组读取二进制文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="s">字节数组</param>
        /// <param name="lenOfByte">文件字节数</param>
        /// <returns></returns>
        public static bool readFile(string filename, out byte[] s, int numberOfByte)
        {
            StringBuilder tmpString = new StringBuilder();

            try
            {
                //根据指定文件路径创建文件流
                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);                                 //创建BinaryReader对象以读取二进制文件
                s = new byte[numberOfByte];
                for (int i = 0; i < numberOfByte; i++)
                {
                    s[i] = binaryReader.ReadByte();
                }
                binaryReader.Close();
                fileStream.Close();
            }
            catch
            {
                Console.WriteLine("Can not open file " + filename.ToString());
                s = new byte[0];
                numberOfByte = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 按Short数组读取二进制文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="s">short数组</param>
        /// <param name="lenOfShort">short数组个数</param>
        /// <returns></returns>
        public static void readFile(string filename, out short[] s, int lenOfShort)
        {
            StringBuilder tmpString = new StringBuilder();

            try
            {
                //根据指定文件路径创建文件流
                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);                                 //创建BinaryReader对象以读取二进制文件

                s = new short[lenOfShort];
                for (int i = 0; i < lenOfShort; i++)
                {
                    s[i] = binaryReader.ReadInt16();
                }
                binaryReader.Close();
                fileStream.Close();
            }
            catch
            {
                Console.WriteLine("Can not open file " + filename.ToString());
                s = new short[0];
                return;
            }
        }

        /// <summary>
        /// 按整型数组读取二进制文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="s">整型数组</param>
        /// <param name="lenOfInt">整型数组长度</param>
        /// <returns></returns>
        public static bool readFile(string filename, out int[] s, out int lenOfInt)
        {
            StringBuilder tmpString = new StringBuilder();

            try
            {
                //根据指定文件路径创建文件流
                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);                                 //创建BinaryReader对象以读取二进制文件
                lenOfInt = Convert.ToInt32(fileStream.Length);
                lenOfInt = lenOfInt / 4;
                s = new int[lenOfInt];
                for (int i = 0; i < lenOfInt; i++)
                {
                    s[i] = binaryReader.ReadInt32();
                }
                binaryReader.Close();
                fileStream.Close();
            }
            catch
            {
                Console.WriteLine("Can not open file " + filename.ToString());
                s = new int[0];
                lenOfInt = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取idx文件中的内容
        /// </summary>
        /// <param name="filename_idx">idx文件名</param>
        /// <param name="filename_grp">grp文件名</param>
        /// <param name="offset">偏移值</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public static byte[] getIdxContent(string filename_idx, string filename_grp, ref List<int> offset, ref List<int> length)
        {
            int[] Ridx;

            int len = 0;
            readFile(filename_idx, out Ridx, out len);

            offset = new List<int>();
            length = new List<int>();
            offset.Add(0);

            for (int i = 0; i < len; i++)
            {
                offset.Add((int)Ridx[i]);
                length.Add((int)(offset[i + 1] - offset[i]));
            }
            int total_length = offset[offset.Count - 1];

            byte[] Rgrp = new byte[total_length];
            readFile(filename_grp, out Rgrp, total_length);

            return Rgrp;
        }

        public static bool readFile(string filename, out short[] s, out int lenOfShort)
        {
            StringBuilder tmpString = new StringBuilder();

            try
            {
                //根据指定文件路径创建文件流
                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fileStream);                                 //创建BinaryReader对象以读取二进制文件
                lenOfShort = Convert.ToInt32(fileStream.Length);
                int tmpLen = lenOfShort / 2;
                s = new short[tmpLen];
                for (int i = 0; i < tmpLen; i++)
                {
                    s[i] = binaryReader.ReadInt16();
                }
                binaryReader.Close();
                fileStream.Close();
            }
            catch
            {
                Console.WriteLine("Can not open file " + filename.ToString());
                s = new short[0];
                lenOfShort = 0;
                return false;
            }

            return true;
        }

        public static string GetFileTime(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                    return "--------------------"; // 文件不存在则返回固定长度占位符

                DateTime fileTime = File.GetLastWriteTime(filename);

                // 格式化输出，与C++版"%Y-%m-%d  %H:%M:%S"完全一致
                string formatted = fileTime.ToString("yyyy-MM-dd  HH:mm:ss");

                // 输出调试信息，对应 C++ 的 printf
                Console.WriteLine($"{filename}:{formatted}");

                return formatted;
            }
            catch
            {
                return "--------------------";
            }
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数名称：WriteFile
        // 功能说明：安全写入文件（支持异常捕获与路径自动创建）
        // 参数说明：
        //   filename —— 文件路径（string）
        //   data     —— 要写入的字节数组
        //   length   —— 要写入的字节数
        // 返回值：
        //   true  —— 写入成功
        //   false —— 写入失败（异常已捕获）
        /////////////////////////////////////////////////////////////////////////
        public static bool WriteFile(string filename, byte[] data, int length)
        {
            try
            {
                /////////////////////////////////////////////////////////////////////////
                // 1️⃣ 参数合法性检查
                /////////////////////////////////////////////////////////////////////////
                if (data == null)
                {
                    Console.Error.WriteLine("WriteFile Error: data buffer is null.");
                    return false;
                }

                if (length <= 0 || length > data.Length)
                {
                    Console.Error.WriteLine($"WriteFile Error: invalid length ({length}).");
                    return false;
                }

                /////////////////////////////////////////////////////////////////////////
                // 2️⃣ 自动创建目录（若目录不存在）
                /////////////////////////////////////////////////////////////////////////
                string? dir = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                /////////////////////////////////////////////////////////////////////////
                // 3️⃣ 文件写入（覆盖模式）
                /////////////////////////////////////////////////////////////////////////
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Write(data, 0, length);
                }

                /////////////////////////////////////////////////////////////////////////
                // 4️⃣ 写入成功
                /////////////////////////////////////////////////////////////////////////
                return true;
            }
            catch (Exception ex)
            {
                /////////////////////////////////////////////////////////////////////////
                // 5️⃣ 错误处理（防止程序崩溃）
                /////////////////////////////////////////////////////////////////////////
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Console.Error.WriteLine($"[{time}] WriteFile Failed: {filename}");
                Console.Error.WriteLine($"Reason: {ex.Message}");
                return false;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // 函数：WriteFile
        // 功能：将 short[] 数据以二进制形式写入指定文件。
        // 参数：
        //   filename  —— 目标文件路径
        //   data      —— 要写入的 short 数组
        //   length    —— 要写入的 short 元素数量
        // 返回值：
        //   无（写入失败会抛出异常）
        // 说明：
        //   每个 short 占 2 字节，以 Little-Endian 顺序写入。
        //   如果文件不存在，会自动创建；若存在，则覆盖。
        ///////////////////////////////////////////////////////////////////////
        public static void WriteFile(string filename, short[] data, int length)
        {
            try
            {
                // 检查参数有效性
                if (data == null)
                    throw new ArgumentNullException(nameof(data), "输入数据不能为空。");

                if (length > data.Length)
                    throw new ArgumentOutOfRangeException(nameof(length), "写入长度超过数组大小。");

                // 将 short 数组转为字节数组（每个 short 两字节）
                byte[] bytes = new byte[length * 2];
                Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);

                // 打开文件（覆盖写入）
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameFile.WriteFile] 写入文件失败：{filename}");
                Console.WriteLine($"错误信息：{ex.Message}");
                throw; // 让上层处理错误（可根据需要去掉）
            }
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 从二进制文件读取内容并填充为结构体列表（等价于 C++: readFileToVector）
        /// </summary>
        /// <typeparam name="T">目标结构体类型，必须是不可为 null 的 unmanaged 类型</typeparam>
        /// <param name="filename">文件路径</param>
        /// <returns>结构体列表</returns>
        ///////////////////////////////////////////////////////////////////////
        public static List<T> ReadFileToList<T>(string filename) where T : unmanaged
        {
            List<T> result = new();

            if (!File.Exists(filename))
                return result;

            // 1️⃣ 读取文件全部字节
            byte[] buffer = File.ReadAllBytes(filename);

            // 2️⃣ 每个结构体的大小
            int structSize = Marshal.SizeOf<T>();

            // 3️⃣ 计算总数
            int count = buffer.Length / structSize;

            // 4️⃣ 依次解析为结构体
            for (int i = 0; i < count; i++)
            {
                int offset = i * structSize;
                T obj = BytesToStruct<T>(buffer, offset);
                result.Add(obj);
            }

            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 将字节数组的一部分转换为结构体（等价于 readDataToVector 内部逻辑）
        /// </summary>
        ///////////////////////////////////////////////////////////////////////
        public static T BytesToStruct<T>(byte[] data, int offset) where T : unmanaged
        {
            unsafe
            {
                fixed (byte* p = &data[offset])
                {
                    return *(T*)p;
                }
            }
        }

























        /// <summary>
        /// 获取泛型的字节数
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <returns>字节数</returns>
        public static int GetSize<T>()
        {
            if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
            {
                return 1;
            }
            else if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort) || typeof(T) == typeof(char))
            {
                return 2;
            }
            else if (typeof(T) == typeof(uint) || typeof(T) == typeof(int) || typeof(T) == typeof(float))
            {
                return 4;
            }
            else if (typeof(T) == typeof(ulong) || typeof(T) == typeof(long) || typeof(T) == typeof(double))
            {
                return 8;
            }
            return 0;
        }



    }
}
