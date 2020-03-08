// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Fireasy.Common.Security;

namespace Fireasy.Data
{
    /// <summary>
    /// 连接字符串加密/解密辅助类。
    /// </summary>
    internal sealed class ConnectionStringEncryptHelper
    {
        /// <summary>
        /// 对连接字符串进行加密。
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal static string Encrypt(string connectionString)
        {
            var encrypt = CryptographyFactory.Create(CryptoAlgorithm.RC2) as SymmetricCrypto;
            var keys = new byte[8];
            var rnd = new Random();
            rnd.NextBytes(keys);

            encrypt.CryptKey = keys;
            var data = encrypt.Encrypt(connectionString, Encoding.GetEncoding(0));

            var array = new List<byte>();
            array.AddRange(new [] { keys[1], keys[3], keys[0], keys[6] });
            array.AddRange(data);
            array.AddRange(new[] { keys[7], keys[2], keys[5], keys[4] });
            return ToHex(array.ToArray());
        }

        /// <summary>
        /// 从数据中解密出连接字符串。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string Decrypt(string data)
        {
            var bytes = ToBytes(data);
            var l = bytes.Length;
            var keys = new[] { bytes[2], bytes[0], bytes[l - 3], bytes[1], 
                bytes[l - 1], bytes[l - 2], bytes[3], bytes[l - 4] };

            var encrypt = CryptographyFactory.Create(CryptoAlgorithm.RC2) as SymmetricCrypto;
            encrypt.CryptKey = keys;

            var bd = new byte[l - 8];
            Array.Copy(bytes, 4, bd, 0, bd.Length);
            return encrypt.Decrypt(bd, Encoding.GetEncoding(0));
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }

            return sb.ToString();
        }

        private static byte[] ToBytes(string str)
        {
            var l = str.Length;
            var bytes = new byte[l / 2];

            for (var i = 0; i < l; i += 2)
            {
                bytes[i / 2] = byte.Parse(str.Substring(i, 2), NumberStyles.HexNumber);
            }

            return bytes;
        }
    }
}
