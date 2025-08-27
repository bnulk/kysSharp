namespace kysSharp.SystemUtils
{
    class ByteUtils
    {
        /// <summary>
        /// 把byte数组转变为整型数组
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">字节数</param>
        /// <returns>整型数组</returns>
        public static int[] ByteToInt32(byte[] bytes, int offset, int numberOfByte)
        {
            int[] s;
            int tmpLen;

            try
            {
                tmpLen = numberOfByte / 4;
                s = new int[tmpLen];
                for (int i = 0; i < tmpLen; i++)
                {
                    s[i] = System.BitConverter.ToInt32(bytes, i * 4 + offset);
                }
                return s;
            }
            catch
            {
                s = new int[0];
                return s;
            }
        }

        /// <summary>
        /// 把byte数组转变为16位整型数组
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="offset">偏移量</param>
        /// <param name="length">字节数</param>
        /// <returns>整型数组</returns>
        public static short[] ByteToInt16(byte[] bytes, int offset, int numberOfByte)
        {
            short[] s;
            int tmpLen;

            try
            {
                tmpLen = numberOfByte / 2;
                s = new short[tmpLen];
                for (int i = 0; i < tmpLen; i++)
                {
                    s[i] = System.BitConverter.ToInt16(bytes, i * 2 + offset);
                }
                return s;
            }
            catch
            {
                s = new short[0];
                return s;
            }
        }

        /// <summary>
        /// 把byte型数组变为sbyte型数组。
        /// 当 byte 小于 128 时其值保持不变，大于等于 128 时就将其减去 256
        /// </summary>
        /// <param name="bytes">byte型数组</param>
        /// <returns>sbyte型数组</returns>
        public static sbyte[] ByteToSbyte(byte[] bytes)
        {
            sbyte[] s = new sbyte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] > 127)
                {
                    s[i] = (sbyte)(bytes[i] - 256);
                }
                else
                {
                    s[i] = (sbyte)bytes[i];
                }
            }
            return s;
        }


    }
}
