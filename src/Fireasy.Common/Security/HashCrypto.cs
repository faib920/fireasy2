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
    /// 提供基于散列的加密方法。
    /// </summary>
    public class HashCrypto : ICryptoProvider
    {
        private readonly string _algorithmName;

        /// <summary>
        /// 初始化 <see cref="SymmetricCrypto"/> 类的新实例。
        /// </summary>
        /// <param name="algorithmName">算法名称。</param>
        public HashCrypto(string algorithmName)
        {
            Guard.ArgumentNull(algorithmName, nameof(algorithmName));
            _algorithmName = algorithmName;
        }

        /// <summary>
        /// 对字节数组进行加密。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public virtual byte[] Encrypt(byte[] array)
        {
#if NETSTANDARD
            var algorithm = new HashAlgorithmName(_algorithmName);
            using var hasher = IncrementalHash.CreateHash(algorithm);
            hasher.AppendData(array);
            return hasher.GetHashAndReset();
#else
            using var algorithm = HashAlgorithm.Create(_algorithmName);
            return algorithm.ComputeHash(array, 0, array.Length);
#endif
        }

        /// <summary>
        /// 对流内的数据进行解密。
        /// </summary>
        /// <param name="sourceStream">要解密的源流对象。</param>
        /// <param name="destStream">加解后的目标流对象。</param>
        public virtual void Encrypt(Stream sourceStream, Stream destStream)
        {
            if (!sourceStream.CanRead || !destStream.CanWrite)
            {
                throw new Exception(SR.GetString(SRKind.SourceCanotReadDestCanotWrite));
            }

            using var algorithm = HashAlgorithm.Create(_algorithmName);
            using var memoryStream = new MemoryStream();
            var buffer = new byte[4096];
            int count;
            while ((count = sourceStream.Read(buffer, 0, 4096)) > 0)
            {
                memoryStream.Write(buffer, 0, count);
            }

            buffer = algorithm.ComputeHash(memoryStream.ToArray());
            destStream.Write(buffer, 0, buffer.Length);
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
        /// 对字节数组进行解密。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public virtual byte[] Decrypt(byte[] array)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 对流内的数据进行解密。
        /// </summary>
        /// <param name="sourceStream">要解密的源流对象。</param>
        /// <param name="destStream">加解后的目标流对象。</param>
        public virtual void Decrypt(Stream sourceStream, Stream destStream)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 将字节数组进行解密为文本。
        /// </summary>
        /// <param name="array">密文字节数组。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public string Decrypt(byte[] array, Encoding encoding)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// 计算 Hash 值。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public virtual byte[] ComputeHash(byte[] array)
        {
            return Encrypt(array);
        }
    }
}
