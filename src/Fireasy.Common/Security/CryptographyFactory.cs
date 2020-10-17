// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

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

            throw new NotSupportedException(SR.GetString(SRKind.AlgorithmNotSupported, algorithmName));
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

        /// <summary>
        /// 创建一个 <see cref="SymmetricCrypto"/> 的实例。
        /// </summary>
        /// <param name="algorithm">密码算法的类型。</param>
        /// <returns></returns>
        public static AsymmetricCrypto CreateAsymmetric(CryptoAlgorithm algorithm)
        {
            if (algorithm != CryptoAlgorithm.RSA &&
                algorithm != CryptoAlgorithm.DSA)
            {
                throw new NotSupportedException(SR.GetString(SRKind.AlgorithmNotSupported, algorithm));
            }

            return Create(algorithm.ToString()) as AsymmetricCrypto;
        }

        /// <summary>
        /// 创建一个 <see cref="SymmetricCrypto"/> 的实例。
        /// </summary>
        /// <param name="algorithm">密码算法的类型。</param>
        /// <returns></returns>
        public static SymmetricCrypto CreateSymmetric(CryptoAlgorithm algorithm)
        {
            if (algorithm != CryptoAlgorithm.DES &&
                algorithm != CryptoAlgorithm.AES &&
                algorithm != CryptoAlgorithm.RC2 &&
                algorithm != CryptoAlgorithm.RC4 &&
                algorithm != CryptoAlgorithm.Rijndael &&
                algorithm != CryptoAlgorithm.TripleDES)
            {
                throw new NotSupportedException(SR.GetString(SRKind.AlgorithmNotSupported, algorithm));
            }

            return Create(algorithm.ToString()) as SymmetricCrypto;
        }

        /// <summary>
        /// 创建一个 <see cref="HashCrypto"/> 的实例。
        /// </summary>
        /// <param name="algorithm">密码算法的类型。</param>
        /// <returns></returns>
        public static HashCrypto CreateHash(CryptoAlgorithm algorithm)
        {
            if (algorithm != CryptoAlgorithm.MD5 &&
                algorithm != CryptoAlgorithm.SHA1 &&
                algorithm != CryptoAlgorithm.SHA256 &&
                algorithm != CryptoAlgorithm.SHA384 &&
                algorithm != CryptoAlgorithm.SHA512)
            {
                throw new NotSupportedException(SR.GetString(SRKind.AlgorithmNotSupported, algorithm));
            }

            return Create(algorithm.ToString()) as HashCrypto;
        }
    }
}
