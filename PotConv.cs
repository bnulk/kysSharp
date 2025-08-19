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
    }
}
