// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Security.Cryptography;

namespace Fireasy.Common.Security
{
    /// <summary>
    /// 密码算法工厂。
    /// </summary>
    public static class CryptographyFactory
    {
        /// <summary>
        /// 根据密码算法创建一个 <see cref="ICryptoProvider"/> 实例。
        /// </summary>
        /// <param name="algorithmName">密码算法的简称。</param>
        /// <returns>一个 <see cref="ICryptoProvider"/> 对象。</returns>
        /// <exception cref="NotSupportedException">指定的 <paramref name="algorithmName"/> 不支持。</exception>
        public static ICryptoProvider Create(string algorithmName)
        {
            switch (algorithmName.ToUpper())
            {
                case "DES":
                case "RC2":
                case "RIJNDAEL":
                    return new SymmetricCrypto(algorithmName);
                case "RC4":
                    return new RC4Crypto();
                case "TRIPLEDES":
                    return new TripleDESCrypto();
                case "AES":
                    return new AESCrypto();
                case "MD5":
                case "SHA1":
                case "SHA256":
                case "SHA384":
                case "SHA512":
                    return new HashCrypto(algorithmName);
                case "RSA":
                    return new RSACrypto();
                case "DSA":
                    return new DSACrypto();
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// 根据密码算法创建一个 <see cref="ICryptoProvider"/> 实例。
        /// </summary>
        /// <param name="algorithm">密码算法的类型。</param>
        /// <returns>一个 <see cref="ICryptoProvider"/> 对象。</returns>
        /// <exception cref="NotSupportedException">指定的 <paramref name="algorithm"/> 不支持。</exception>
        public static ICryptoProvider Create(CryptoAlgorithm algorithm)
        {
            return Create(algorithm.ToString());
        }
    }
}
