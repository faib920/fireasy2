// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Common.Security
{
    internal class RC4Crypto : SymmetricCrypto
    {
        public RC4Crypto()
            : base("RC4")
        {
        }

        public override void SetKey(string key)
        {
            CryptKey = Encoding.GetEncoding(0).GetBytes(key);
        }

        protected override ICryptoTransform CreateEncryptor(SymmetricAlgorithm algorithm)
        {
            return new RC4CryptoTransform(CryptKey);
        }

        protected override ICryptoTransform CreateDecryptor(SymmetricAlgorithm algorithm)
        {
            return new RC4CryptoTransform(CryptKey);
        }
    }
}
