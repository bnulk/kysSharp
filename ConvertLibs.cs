using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace kysSharp
{
    class ConvertLibs
    {
        /////////////////////////////////////////////////////////////////////////
        // 函数功能: 从指定路径读取文本文件内容，并返回为字符串。
        // 使用说明: 
        //   - 输入参数 filename 为文件路径（包含文件名和扩展名）。
        //   - 如果文件存在且可读，则返回文件内容（字符串）。
        //   - 如果读取过程中发生异常（如文件不存在、权限不足等），
        //     将捕获异常，输出错误信息到控制台，并返回空字符串 ""。
        // 技术要点:
        //   - 使用 File.ReadAllText 方法，内部自动完成文件流的打开、读取、关闭。
        //   - 使用 try...catch 捕获异常，避免程序崩溃。
        //   - 使用 string.Empty 返回空字符串，保证调用者不会收到 null。
        /////////////////////////////////////////////////////////////////////////
        public static string ReadStringFromFile(string filename)
        {
            try
            {
                /////////////////////////////////////////////////////////////////////////
                // File.ReadAllText(filename)
                // --------------------------
                // .NET 提供的便捷方法，用于一次性读取整个文本文件内容。
                // 优点: 简洁，内部已处理文件流打开与释放。
                // 返回值: 文件中的所有内容，类型为 string。
                /////////////////////////////////////////////////////////////////////////
                return File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                /////////////////////////////////////////////////////////////////////////
                // 捕获所有异常:
                //   - 常见异常类型:
                //       FileNotFoundException   : 文件不存在
                //       UnauthorizedAccessException : 没有权限访问
                //       IOException             : 其他 I/O 错误
                //   - 这里统一用 Exception 捕获，保证健壮性。
                /////////////////////////////////////////////////////////////////////////

                /////////////////////////////////////////////////////////////////////////
                // Console.WriteLine(...) 
                // ----------------------
                // 输出错误信息到控制台，包含文件名和具体异常信息 (ex.Message)，
                // 便于调试和用户查看问题原因。
                /////////////////////////////////////////////////////////////////////////
                Console.WriteLine($"❌ Failed to read file: {filename}. Error: {ex.Message}");

                /////////////////////////////////////////////////////////////////////////
                // return string.Empty
                // -------------------
                // 在出现错误时返回空字符串 ""，而不是 null。
                // 好处: 避免调用者在处理结果时发生 NullReferenceException。
                /////////////////////////////////////////////////////////////////////////
                return string.Empty;
            }
        }

        public static int FindNumbers<T>(string s, ref List<T> data)
        {
            int n = 0;
            string str = "";
            bool haveNum = false;
            data = new List<T>();

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                bool findNumChar = (c >= '0' && c <= '9') || c == '.' || c == '-' || c == '+' || c == 'E' || c == 'e';
                if (findNumChar)
                {
                    str += c;
                    if (c >= '0' && c <= '9')
                    { haveNum = true; }
                }
                if (!findNumChar || i == s.Length - 1)
                {
                    if (str != "" && haveNum)
                    {
                        var f = (T)Convert.ChangeType(Convert.ToDouble(str), typeof(T));
                        data.Add(f);
                        n++;
                    }
                    str = "";
                    haveNum = false;
                }
            }
            return n;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 将字符串写入文件
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void WriteStringToFile(string str, string filename)
        {
            try
            {
                File.WriteAllText(filename, str, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error writing file {filename}: {ex.Message}");
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 替换字符串中第一个匹配项
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static int ReplaceString(ref string s, string oldString, string newString, int pos0 = 0)
        {
            int pos = s.IndexOf(oldString, pos0);
            if (pos >= 0)
            {
                s = s.Remove(pos, oldString.Length).Insert(pos, newString);
                return pos + newString.Length;
            }
            return -1;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 替换字符串中所有匹配项
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static int ReplaceAllString(ref string s, string oldString, string newString)
        {
            s = s.Replace(oldString, newString);
            return s.Length;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 替换文件中第一个匹配项
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void ReplaceStringInFile(string oldFilename, string newFilename, string oldString, string newString)
        {
            string s = ReadStringFromFile(oldFilename);
            if (s.Length <= 0) return;
            ReplaceString(ref s, oldString, newString);
            WriteStringToFile(s, newFilename);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 替换文件中所有匹配项
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void ReplaceAllStringInFile(string oldFilename, string newFilename, string oldString, string newString)
        {
            string s = ReadStringFromFile(oldFilename);
            if (s.Length <= 0) return;
            ReplaceAllString(ref s, oldString, newString);
            WriteStringToFile(s, newFilename);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 格式化字符串（相当于 C++ 的 sprintf）
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static string FormatString(string format, params object[] args)
        {
            return string.Format(format, args);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 在字符串后追加格式化内容
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void FormatAppendString(ref string str, string format, params object[] args)
        {
            str += string.Format(format, args);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        // 查找最后一次出现的位置
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static int FindTheLast(string s, string content)
        {
            return s.LastIndexOf(content, StringComparison.Ordinal);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 分割字符串
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static List<string> SplitString(string str, string pattern)
        {
            var result = new List<string>();
            if (pattern == "") { result.Add(str); return result; }

            string[] parts = str.Split(new string[] { pattern }, StringSplitOptions.None);
            result.AddRange(parts);
            return result;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        // 判断是否是英文字母、数字或括号字符
        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool IsProChar(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'A' && c <= 'z') ||
                   (c >= '(' && c <= ')');
        }













    }
}
