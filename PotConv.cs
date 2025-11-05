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
