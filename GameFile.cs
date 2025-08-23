using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace kysSharp
{
    class GameFile
    {
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
