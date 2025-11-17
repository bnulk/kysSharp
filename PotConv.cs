using System.Text;

namespace kysSharp
{
    class PotConv
    {
        public static void FromCP950ToString(sbyte[] s, ref string str)
        {
            int length = s.Length;
            byte[] tmpByte = new byte[length];
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding big5 = Encoding.GetEncoding(950);
            for (int i = 0; i < length; i++)
            {
                tmpByte[i] = (byte)s[i];
            }
            str = big5.GetString(tmpByte);
        }

        /////////////////////////////////////////////////////////////////////////
        // 函数：Cp950ToCp936
        // 功能：将台湾繁体编码（CP950）转换为简体中文（CP936/GBK）
        /////////////////////////////////////////////////////////////////////////
        public static string Cp950ToCp936(byte[] data, int startIndex,int length)
        {
            /*
            int end = startIndex;
            while (end < data.Length && data[end] != 0) end++;
            byte[] sub = new byte[end - startIndex];
            Array.Copy(data, startIndex, sub, 0, sub.Length);
            string text = Encoding.GetEncoding("big5").GetString(sub);
            return text;
            */

            // 安全性检查
            if (data == null || data.Length == 0)
                return string.Empty;

            if (startIndex < 0 || length <= 0 || startIndex + length > data.Length)
                return string.Empty;

            // 从指定位置复制字节
            byte[] subData = new byte[length];
            Buffer.BlockCopy(data, startIndex, subData, 0, length);

            // 转换为字符串（UTF-8）
            string text = Encoding.GetEncoding("big5").GetString(subData);
            return text;
        }

        ////////////////////////////////////////////////////////////////////////
        // 函数：Cp950ToCp936
        // 功能：将台湾繁体编码（CP950）转换为简体中文（CP936/GBK）
        // 参数：
        //   data        —— 输入的 sbyte[] 数据
        //   startIndex  —— 起始索引
        //   length      —— 读取长度
        // 返回：
        //   转换后的简体中文字符串
        ////////////////////////////////////////////////////////////////////////
        public static string Cp950ToCp936(sbyte[] data, int startIndex, int length)
        {
            /////////////////////////////////////////////////////////////////////////
            // 参数检查
            /////////////////////////////////////////////////////////////////////////
            if (data == null || data.Length == 0)
                return string.Empty;

            if (startIndex < 0 || length <= 0 || startIndex + length > data.Length)
                return string.Empty;

            /////////////////////////////////////////////////////////////////////////
            // 将 sbyte[] 转换为 byte[]
            // 因为 Encoding.GetString() 只接受无符号字节（byte[]）
            /////////////////////////////////////////////////////////////////////////
            byte[] byteData = new byte[length];
            Buffer.BlockCopy(data, startIndex, byteData, 0, length);

            /////////////////////////////////////////////////////////////////////////
            // 使用 Big5 解码（繁体编码）
            /////////////////////////////////////////////////////////////////////////
            string text = Encoding.GetEncoding("big5").GetString(byteData);

            /////////////////////////////////////////////////////////////////////////
            // 若要进一步转为简体（CP936/GBK），可以使用 Encoding 转码：
            /////////////////////////////////////////////////////////////////////////
            byte[] gbkBytes = Encoding.Convert(Encoding.GetEncoding("big5"), Encoding.GetEncoding("gbk"), byteData);
            string simplified = Encoding.GetEncoding("gbk").GetString(gbkBytes);

            return simplified;
        }

        ////////////////////////////////////////////////////////////////////////
        // 函数：Cp950ToCp936
        // 功能：将台湾繁体编码（CP950）转换为简体中文（CP936/GBK）
        // 参数：
        //   data        —— 输入的 sbyte* 指针
        //   startIndex  —— 起始偏移量
        //   length      —— 要读取的字节数
        // 返回：
        //   转换后的简体中文字符串
        ////////////////////////////////////////////////////////////////////////
        public static unsafe string Cp950ToCp936(sbyte* data, int startIndex, int length)
        {
            /////////////////////////////////////////////////////////////////////////
            // 参数检查
            /////////////////////////////////////////////////////////////////////////
            if (data == null || length <= 0)
                return string.Empty;

            /////////////////////////////////////////////////////////////////////////
            // 分配托管缓冲区（byte[]）
            /////////////////////////////////////////////////////////////////////////
            byte[] buffer = new byte[length];

            /////////////////////////////////////////////////////////////////////////
            // 从非托管指针复制数据到托管数组
            // 注意：sbyte* 转为 byte* 时只需 reinterpret，不做数值转换
            /////////////////////////////////////////////////////////////////////////
            byte* src = (byte*)(data + startIndex);
            fixed (byte* dst = buffer)
            {
                Buffer.MemoryCopy(src, dst, length, length);
            }

            /////////////////////////////////////////////////////////////////////////
            // 使用 Big5 编码（CP950）解码为繁体中文字符串
            /////////////////////////////////////////////////////////////////////////
            string traditional = Encoding.GetEncoding("big5").GetString(buffer);

            /////////////////////////////////////////////////////////////////////////
            // 若希望进一步转换为简体中文（CP936 / GBK）
            /////////////////////////////////////////////////////////////////////////
            byte[] gbkBytes = Encoding.Convert(
                Encoding.GetEncoding("big5"),
                Encoding.GetEncoding("gbk"),
                buffer
            );

            string simplified = Encoding.GetEncoding("gbk").GetString(gbkBytes);

            return simplified;
        }



        /// <summary>
        /// 二进制数据（字节数组）转换为 CP936 编码的字符串
        /// </summary>
        /// <param name="binaryData">二进制数据（字节数组）</param>
        /// <returns></returns>
        public static void ConvertToStringCP936(byte[] binaryData, ref string result)
        {
            // 获取CP936编码器
            Encoding cp936 = Encoding.GetEncoding(936);

            // 使用CP936编码器将字节数组转换为字符串
            result = cp936.GetString(binaryData);
        }

        public static void ConvertToStringCP936(sbyte[] sbyteData, ref string result)
        {
            // 将 sbyte[] 转换为 byte[]
            byte[] byteData = new byte[sbyteData.Length];
            for (int i = 0; i < sbyteData.Length; i++)
            {
                byteData[i] = (byte)(sbyteData[i] & 0xFF); // 将 sbyte 转换为 byte
            }

            // 获取 CP936 编码器
            Encoding cp936 = Encoding.GetEncoding(936);

            // 使用 CP936 编码器将字节数组转换为字符串
            result = cp936.GetString(byteData);
        }
    }
}
