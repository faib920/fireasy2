// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Extensions
{
    public static class ByteArrayExtension
    {
        /// <summary>
        /// 将一组字节转换为使用十六进制字符串表示。比如 {255,254} 转为 FFFE。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="lower">是否小写。</param>
        /// <returns></returns>
        public static string ToHex(this byte[] bytes, bool lower = false)
        {
            if (bytes == null)
            {
                return null;
            }

            var chArray = new char[bytes.Length * 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                byte num2 = bytes[i];
                chArray[2 * i] = NibbleToHex((byte)(num2 >> 4), lower);
                chArray[(2 * i) + 1] = NibbleToHex((byte)(num2 & 15), lower);
            }

            return new string(chArray);
        }

        /// <summary>
        /// 将表示十六进制的字符串转换为字节数组。比如 FFFE 转为 {255,254}。
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] FromHex(this string hexString)
        {
            if ((hexString == null) || ((hexString.Length % 2) != 0))
            {
                return null;
            }

            var buffer = new byte[hexString.Length / 2];
            for (var i = 0; i < buffer.Length; i++)
            {
                var num2 = HexToInt(hexString[2 * i]);
                var num3 = HexToInt(hexString[(2 * i) + 1]);
                if ((num2 == -1) || (num3 == -1))
                {
                    return null;
                }
                buffer[i] = (byte)((num2 << 4) | num3);
            }

            return buffer;
        }

        private static char NibbleToHex(byte nibble, bool lower)
        {
            return ((nibble < 10) ? ((char)(nibble + 0x30)) : ((char)((nibble - 10) + 0x41 + (lower ? 0x20 : 0))));
        }

        private static int HexToInt(char h)
        {
            if ((h >= '0') && (h <= '9'))
            {
                return (h - '0');
            }
            if ((h >= 'a') && (h <= 'f'))
            {
                return ((h - 'a') + 10);
            }
            if ((h >= 'A') && (h <= 'F'))
            {
                return ((h - 'A') + 10);
            }
            return -1;
        }

    }
}
