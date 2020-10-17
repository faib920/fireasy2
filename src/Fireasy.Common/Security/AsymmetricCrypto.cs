// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.IO;
using System.Text;

namespace Fireasy.Common.Security
{
    /// <summary>
    /// 提供非对称加密和解密的方法。
    /// </summary>
    public abstract class AsymmetricCrypto : ICryptoProvider
    {
        private readonly string _algorithmName;

        /// <summary>
        /// 初始化 <see cref="SymmetricCrypto"/> 类的新实例。
        /// </summary>
        /// <param name="algorithmName">算法名称。</param>
        public AsymmetricCrypto(string algorithmName)
        {
            Guard.ArgumentNull(algorithmName, nameof(algorithmName));
            _algorithmName = algorithmName;
        }

        /// <summary>
        /// 设置或获取公钥。
        /// </summary>
        public virtual string PublicKey { get; set; }

        /// <summary>
        /// 获取或设置密钥。
        /// </summary>
        public virtual string PrivateKey { get; set; }

        /// <summary>
        /// 生成公钥。
        /// </summary>
        /// <returns></returns>
        public abstract string GeneratePublicKey();

        /// <summary>
        /// 生成私钥。
        /// </summary>
        /// <returns></returns>
        public abstract string GeneratePrivateKey();

        /// <summary>
        /// 对流内的数据进行加密。
        /// </summary>
        /// <param name="sourceStream">要加密的源流对象。</param>
        /// <param name="destStream">加密后的目标流对象。</param>
        public abstract void Encrypt(Stream sourceStream, Stream destStream);

        /// <summary>
        /// 对字节数组进行加密。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public abstract byte[] Encrypt(byte[] array);

        /// <summary>
        /// 对文本进行加密。
        /// </summary>
        /// <param name="source">要加密的文本。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public abstract byte[] Encrypt(string source, Encoding encoding);

        /// <summary>
        /// 对流内的数据进行解密。
        /// </summary>
        /// <param name="sourceStream">要解密的源流对象。</param>
        /// <param name="destStream">加解后的目标流对象。</param>
        public abstract void Decrypt(Stream sourceStream, Stream destStream);

        /// <summary>
        /// 对字节数组进行解密。
        /// </summary>
        /// <param name="cipherData"></param>
        /// <returns></returns>
        public abstract byte[] Decrypt(byte[] cipherData);

        /// <summary>
        /// 将字节数组进行解密为文本。
        /// </summary>
        /// <param name="cipherData">密文字节数组。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public abstract string Decrypt(byte[] cipherData, Encoding encoding);

        /// <summary>
        /// 对数组进行签名。
        /// </summary>
        /// <param name="source">要签名的数据。</param>
        /// <returns></returns>
        public abstract byte[] CreateSignature(byte[] source, string aigorithm);

        /// <summary>
        /// 验证签名。
        /// </summary>
        /// <param name="source">要签名的数据。</param>
        /// <param name="signature"><paramref name="source"/> 的签名数据。</param>
        /// <returns></returns>
        public abstract bool VerifySignature(byte[] source, byte[] signature, string aigorithm);
    }
}
