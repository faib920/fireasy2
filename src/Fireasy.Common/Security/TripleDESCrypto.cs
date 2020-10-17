// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Text;
namespace Fireasy.Common.Security
{
    internal class TripleDESCrypto : SymmetricCrypto
    {
        public TripleDESCrypto()
            : base("TripleDES")
        {
        }

        protected override void SetDefaultKeyIV()
        {
            CryptKey = new byte[] { 89, 44, 53, 65, 132, 45, 199, 34, 34, 122, 35, 87, 34, 124, 233, 53, 233, 44, 56, 89, 56, 26, 98, 252 };
            CryptIV = new byte[] { 12, 64, 134, 43, 58, 154, 200, 48 };
        }

        public override void SetKey(string key)
        {
            var sourceArray = CryptographyFactory.Create(CryptoAlgorithm.MD5).Encrypt(key, Encoding.GetEncoding(0));
            CryptKey = new byte[24];
            CryptIV = new byte[8];

            Array.Copy(sourceArray, 0, CryptKey, 0, 8);
            sourceArray = Xor(sourceArray, 62, 8);
            Array.Copy(sourceArray, 0, CryptKey, 8, 8);
            sourceArray = Xor(sourceArray, 216, 8);
            Array.Copy(sourceArray, 0, CryptKey, 16, 8);
            Array.Copy(sourceArray, 8, CryptIV, 0, 8);
        }

        private byte[] Xor(byte[] bytes, byte b, int length)
        {
            for (var i = 0; i < Math.Min(length, bytes.Length); i++)
            {
                bytes[i] ^= b;
            }

            return bytes;
        }
    }
}
