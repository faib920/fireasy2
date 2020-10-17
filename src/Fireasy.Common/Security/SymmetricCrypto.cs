// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Fireasy.Common.Security
{
    /// <summary>
    /// 提供对称加密和解密的方法。
    /// </summary>
    public class SymmetricCrypto : ICryptoProvider
    {
        private readonly string _algorithmName;

        /// <summary>
        /// 初始化 <see cref="SymmetricCrypto"/> 类的新实例。
        /// </summary>
        /// <param name="algorithmName">算法名称。</param>
        public SymmetricCrypto(string algorithmName)
        {
            Guard.ArgumentNull(algorithmName, nameof(algorithmName));
            _algorithmName = algorithmName;
            SetDefaultKeyIV();
        }

        /// <summary>
        /// 获取或设置密钥。
        /// </summary>
        public byte[] CryptKey { get; set; }

        /// <summary>
        /// 获取或设置初始化向量。
        /// </summary>
        public byte[] CryptIV { get; set; }

        /// <summary>
        /// 设置加解密的密钥。
        /// </summary>
        /// <param name="key"></param>
        public virtual void SetKey(string key)
        {
            var sourceArray = CryptographyFactory.Create(CryptoAlgorithm.MD5).Encrypt(key, Encoding.GetEncoding(0));
            CryptKey = new byte[8];
            CryptIV = new byte[8];
            Array.Copy(sourceArray, 0, CryptKey, 0, 8);
            Array.Copy(sourceArray, 8, CryptIV, 0, 8);
        }

        /// <summary>
        /// 设置默认的Key和IV。
        /// </summary>
        protected virtual void SetDefaultKeyIV()
        {
            CryptKey = new byte[] { 53, 211, 34, 65, 171, 43, 21, 134 };
            CryptIV = new byte[] { 12, 64, 134, 43, 58, 154, 200, 48 };
        }

        /// <summary>
        /// 创建加密的转换器。
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        protected virtual ICryptoTransform CreateEncryptor(SymmetricAlgorithm algorithm)
        {
            return algorithm.CreateEncryptor(CryptKey, CryptIV);
        }

        /// <summary>
        /// 创建解密的转换器。
        /// </summary>
        /// <param name="algorithm"></param>
        /// <returns></returns>
        protected virtual ICryptoTransform CreateDecryptor(SymmetricAlgorithm algorithm)
        {
            return algorithm.CreateDecryptor(CryptKey, CryptIV);
        }

        /// <summary>
        /// 对流内的数据进行加密。
        /// </summary>
        /// <param name="sourceStream">要加密的源流对象。</param>
        /// <param name="destStream">加密后的目标流对象。</param>
        public virtual void Encrypt(Stream sourceStream, Stream destStream)
        {
            if (!sourceStream.CanRead || !destStream.CanWrite)
            {
                throw new Exception(SR.GetString(SRKind.SourceCanotReadDestCanotWrite));
            }

            using (var algorithm = CreateAlgorithm(_algorithmName))
            {
                if (algorithm != null)
                {
                    algorithm.Mode = CipherMode.ECB;
                }

                var transform = CreateEncryptor(algorithm);
                var cryptStream = new CryptoStream(destStream, transform, CryptoStreamMode.Write);
                var buffer = new byte[4096];
                int count;
                while ((count = sourceStream.Read(buffer, 0, 4096)) > 0)
                {
                    cryptStream.Write(buffer, 0, count);
                }

                cryptStream.FlushFinalBlock();
            }
        }

        /// <summary>
        /// 对字节数组进行加密。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public virtual byte[] Encrypt(byte[] array)
        {
            byte[] result;
            using (var outStream = new MemoryStream())
            {
                using (var inStream = new MemoryStream(array))
                {
                    Encrypt(inStream, outStream);
                }

                result = outStream.ToArray();
            }

            return result;
        }

        /// <summary>
        /// 对文本进行加密。
        /// </summary>
        /// <param name="source">要加密的文本。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public byte[] Encrypt(string source, Encoding encoding)
        {
            return Encrypt(encoding.GetBytes(source));
        }

        /// <summary>
        /// 对流内的数据进行解密。
        /// </summary>
        /// <param name="sourceStream">要解密的源流对象。</param>
        /// <param name="destStream">加解后的目标流对象。</param>
        public virtual void Decrypt(Stream sourceStream, Stream destStream)
        {
            if (!sourceStream.CanRead || !destStream.CanWrite)
            {
                throw new Exception(SR.GetString(SRKind.SourceCanotReadDestCanotWrite));
            }

            using (var algorithm = CreateAlgorithm(_algorithmName))
            {
                if (algorithm != null)
                {
                    algorithm.Mode = CipherMode.ECB;
                }

                var transform = CreateDecryptor(algorithm);
                var cryptStream = new CryptoStream(destStream, transform, CryptoStreamMode.Write);
                var buffer = new byte[4096];
                int count;
                while ((count = sourceStream.Read(buffer, 0, 4096)) > 0)
                {
                    cryptStream.Write(buffer, 0, count);
                }
                cryptStream.FlushFinalBlock();
            }
        }

        /// <summary>
        /// 对字节数组进行解密。
        /// </summary>
        /// <param name="cipherData"></param>
        /// <returns></returns>
        public virtual byte[] Decrypt(byte[] cipherData)
        {
            byte[] result;
            using (var outStream = new MemoryStream())
            {
                using (var inStream = new MemoryStream(cipherData))
                {
                    Decrypt(inStream, outStream);
                }

                result = outStream.ToArray();
            }

            return result;
        }

        /// <summary>
        /// 将字节数组进行解密为文本。
        /// </summary>
        /// <param name="cipherData">密文字节数组。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public string Decrypt(byte[] cipherData, Encoding encoding)
        {
            return encoding.GetString(Decrypt(cipherData));
        }

        private SymmetricAlgorithm CreateAlgorithm(string algorithmName)
        {
#if !NETSTANDARD
            return SymmetricAlgorithm.Create(algorithmName);
#endif
            switch (algorithmName.ToUpper())
            {
                case "AES":
                    return Aes.Create();
                case "DES":
                    return DES.Create();
                case "RC2":
                    return RC2.Create();
                case "RIJNDAEL":
                    return Rijndael.Create();
                case "TRIPLEDES":
                    return TripleDES.Create();
                default:
                    return null;
            }
        }
    }
}
