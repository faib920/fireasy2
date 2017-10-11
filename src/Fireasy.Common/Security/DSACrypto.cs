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
    internal class DSACrypto : AsymmetricCrypto
    {
        private DSACryptoServiceProvider dsa = null;

        public DSACrypto()
            : base("DSA")
        {
            dsa = new DSACryptoServiceProvider();
        }

        /// <summary>
        /// 生成公钥。
        /// </summary>
        /// <returns></returns>
        public override string GeneratePublicKey()
        {
            return dsa.ToXmlString(false);
        }

        /// <summary>
        /// 生成私钥。
        /// </summary>
        /// <returns></returns>
        public override string GeneratePrivateKey()
        {
            return dsa.ToXmlString(true);
        }

        /// <summary>
        /// 对流内的数据进行加密。
        /// </summary>
        /// <param name="sourceStream">要加密的源流对象。</param>
        /// <param name="destStream">加密后的目标流对象。</param>
        public override void Encrypt(Stream sourceStream, Stream destStream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 对字节数组进行加密。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override byte[] Encrypt(byte[] source)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 对文本进行加密。
        /// </summary>
        /// <param name="source">要加密的文本。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public override byte[] Encrypt(string source, Encoding encoding)
        {
            return Encrypt(encoding.GetBytes(source));
        }

        /// <summary>
        /// 对流内的数据进行解密。
        /// </summary>
        /// <param name="sourceStream">要解密的源流对象。</param>
        /// <param name="destStream">加解后的目标流对象。</param>
        public override void Decrypt(Stream sourceStream, Stream destStream)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 对字节数组进行解密。
        /// </summary>
        /// <param name="cipherData"></param>
        /// <returns></returns>
        public override byte[] Decrypt(byte[] cipherData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 将字节数组进行解密为文本。
        /// </summary>
        /// <param name="cipherData">密文字节数组。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        public override string Decrypt(byte[] cipherData, Encoding encoding)
        {
            return encoding.GetString(Decrypt(cipherData));
        }

        /// <summary>
        /// 对数组进行签名。
        /// </summary>
        /// <param name="source">要签名的数据。</param>
        /// <returns></returns>
        public override byte[] CreateSignature(byte[] source)
        {
            dsa.FromXmlString(PrivateKey);
            var md5 = CryptographyFactory.Create(CryptoAlgorithm.SHA1);
            return dsa.SignHash(md5.Encrypt(source), "SHA1");
        }

        /// <summary>
        /// 验证签名。
        /// </summary>
        /// <param name="source">要签名的数据。</param>
        /// <param name="signature"><paramref name="source"/> 的签名数据。</param>
        /// <returns></returns>
        public override bool VerifySignature(byte[] source, byte[] signature)
        {
            dsa.FromXmlString(PublicKey);
            var md5 = CryptographyFactory.Create(CryptoAlgorithm.SHA1);
            return dsa.VerifyHash(md5.Encrypt(source), "SHA1", signature);
        }    
    }
}
