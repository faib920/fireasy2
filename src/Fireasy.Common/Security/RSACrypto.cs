// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Fireasy.Common.Security
{
    internal class RSACrypto : AsymmetricCrypto
    {
        private readonly RSACryptoServiceProvider _rsa = null;

        public RSACrypto()
            : base("RSA")
        {
            _rsa = new RSACryptoServiceProvider();
        }

        /// <summary>
        /// 生成公钥。
        /// </summary>
        /// <returns></returns>
        public override string GeneratePublicKey()
        {
            return ToXmlString(false);
        }

        /// <summary>
        /// 生成私钥。
        /// </summary>
        /// <returns></returns>
        public override string GeneratePrivateKey()
        {
            return ToXmlString(true);
        }

        /// <summary>
        /// 对流内的数据进行加密。
        /// </summary>
        /// <param name="sourceStream">要加密的源流对象。</param>
        /// <param name="destStream">加密后的目标流对象。</param>
        public override void Encrypt(Stream sourceStream, Stream destStream)
        {
            using (var m1 = sourceStream.CopyToMemory())
            using (var m2 = new MemoryStream(Encrypt(m1.ToArray())))
            {
                var buffer = new byte[4096];
                int count;
                while ((count = m2.Read(buffer, 0, 4096)) > 0)
                {
                    destStream.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 对字节数组进行加密。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override byte[] Encrypt(byte[] source)
        {
            FromXmlString(PublicKey);
            return _rsa.Encrypt(source, false);
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
            using (var m1 = sourceStream.CopyToMemory())
            using (var m2 = new MemoryStream(Decrypt(m1.ToArray())))
            {
                var buffer = new byte[4096];
                int count;
                while ((count = m2.Read(buffer, 0, 4096)) > 0)
                {
                    destStream.Write(buffer, 0, count);
                }
            }
        }

        /// <summary>
        /// 对字节数组进行解密。
        /// </summary>
        /// <param name="cipherData"></param>
        /// <returns></returns>
        public override byte[] Decrypt(byte[] cipherData)
        {
            FromXmlString(PrivateKey);
            return _rsa.Decrypt(cipherData, false);
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
        public override byte[] CreateSignature(byte[] source, string aigorithm)
        {
            FromXmlString(PrivateKey);
            return _rsa.SignHash(source, aigorithm);
        }

        /// <summary>
        /// 验证签名。
        /// </summary>
        /// <param name="source">要签名的数据。</param>
        /// <param name="signature"><paramref name="source"/> 的签名数据。</param>
        /// <returns></returns>
        public override bool VerifySignature(byte[] source, byte[] signature, string aigorithm)
        {
            FromXmlString(PublicKey);
            return _rsa.VerifyHash(source, aigorithm, signature);
        }

        private string ToXmlString(bool includePrivateParameters)
        {
#if !NETSTANDARD
            return _rsa.ToXmlString(includePrivateParameters);
#else
            var parameters = _rsa.ExportParameters(includePrivateParameters);

            var sb = new StringBuilder();
            sb.Append("<RSAKeyValue>");
            // Add the modulus
            sb.Append($"<Modulus>{Convert.ToBase64String(parameters.Modulus)}</Modulus>");
            // Add the exponent
            sb.Append($"<Exponent>{Convert.ToBase64String(parameters.Exponent)}</Exponent>");

            if (includePrivateParameters)
            {
                // Add the private components
                sb.Append($"<P>{Convert.ToBase64String(parameters.P)}</P>");
                sb.Append($"<Q>{Convert.ToBase64String(parameters.Q)}</Q>");
                sb.Append($"<DP>{Convert.ToBase64String(parameters.DP)}</DP>");
                sb.Append($"<DQ>{Convert.ToBase64String(parameters.DQ)}</DQ>");
                sb.Append($"<InverseQ>{Convert.ToBase64String(parameters.InverseQ)}</InverseQ>");
                sb.Append($"<D>{Convert.ToBase64String(parameters.D)}</D>");
            }

            sb.Append("</RSAKeyValue>");

            return sb.ToString();
#endif
        }

        private void FromXmlString(string xmlString)
        {
#if !NETSTANDARD
            _rsa.FromXmlString(xmlString);
#else
            var parameters = new RSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (!xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                throw new Exception("Invalid XML RSA key.");
            }

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Modulus":
                        parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "Exponent":
                        parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "P":
                        parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "Q":
                        parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "DP":
                        parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "DQ":
                        parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "InverseQ":
                        parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "D":
                        parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                }
            }

            _rsa.ImportParameters(parameters);
#endif
        }
    }
}
