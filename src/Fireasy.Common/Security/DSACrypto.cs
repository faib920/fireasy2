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
using System.Xml;

namespace Fireasy.Common.Security
{
    internal class DSACrypto : AsymmetricCrypto
    {
        private readonly DSACryptoServiceProvider _dsa = null;

        public DSACrypto()
            : base("DSA")
        {
            _dsa = new DSACryptoServiceProvider();
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
        public override byte[] CreateSignature(byte[] source, string aigorithm)
        {
            FromXmlString(PrivateKey);
            return _dsa.SignHash(source, aigorithm);
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
            return _dsa.VerifyHash(source, aigorithm, signature);
        }


        private string ToXmlString(bool includePrivateParameters)
        {
#if !NETSTANDARD
            return _dsa.ToXmlString(includePrivateParameters);
#else
            var parameters = _dsa.ExportParameters(includePrivateParameters);

            var sb = new StringBuilder();
            sb.Append("<DSAKeyValue>");

            // Add P, Q, G and Y
            sb.Append($"<P>{Convert.ToBase64String(parameters.P)}</P>");
            sb.Append($"<Q>{Convert.ToBase64String(parameters.Q)}</Q>");
            sb.Append($"<G>{Convert.ToBase64String(parameters.G)}</G>");
            sb.Append($"<Y>{Convert.ToBase64String(parameters.Y)}</Y>");

            // Add optional components if present
            if (parameters.J != null)
            {
                sb.Append($"<J>{Convert.ToBase64String(parameters.J)}</J>");
            }

            if ((parameters.Seed != null))
            {  // note we assume counter is correct if Seed is present
                sb.Append($"<Seed>{Convert.ToBase64String(parameters.Seed)}</Seed>");
                sb.Append($"<PgenCounter>{Convert.ToBase64String(ConvertIntToByteArray(parameters.Counter))}</PgenCounter>");
            }

            if (includePrivateParameters)
            {
                // Add the private component
                sb.Append($"<X>{Convert.ToBase64String(parameters.X)}</X>");
            }

            sb.Append("</DSAKeyValue>");

            return sb.ToString();
#endif
        }

        private void FromXmlString(string xmlString)
        {
#if !NETSTANDARD
            _dsa.FromXmlString(xmlString);
#else
            var parameters = new DSAParameters();

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (!xmlDoc.DocumentElement.Name.Equals("DSAKeyValue"))
            {
                throw new Exception("Invalid XML DSA key.");
            }

            foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "P":
                        parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "Q":
                        parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "G":
                        parameters.G = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "Y":
                        parameters.Y = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "J":
                        parameters.J = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "Seed":
                        parameters.Seed = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                    case "PgenCounter":
                        parameters.Counter = ConvertByteArrayToInt(Convert.FromBase64String(node.InnerText));
                        break;
                    case "X":
                        parameters.X = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText));
                        break;
                }
            }

            _dsa.ImportParameters(parameters);
#endif
        }

        private byte[] ConvertIntToByteArray(int dwInput)
        {
            var temp = new byte[8]; // int can never be greater than Int64
            int t1;  // t1 is remaining value to account for
            int t2;  // t2 is t1 % 256
            int i = 0;

            if (dwInput == 0)
            {
                return new byte[1];
            }

            t1 = dwInput;
            while (t1 > 0)
            {
                t2 = t1 % 256;
                temp[i] = (byte)t2;
                t1 = (t1 - t2) / 256;
                i++;
            }

            // Now, copy only the non-zero part of temp and reverse
            var output = new byte[i];

            // copy and reverse in one pass
            for (var j = 0; j < i; j++)
            {
                output[j] = temp[i - j - 1];
            }

            return output;
        }

        private int ConvertByteArrayToInt(byte[] input)
        {
            // Input to this routine is always big endian
            var dwOutput = 0;
            for (var i = 0; i < input.Length; i++)
            {
                dwOutput *= 256;
                dwOutput += input[i];
            }

            return (dwOutput);
        }
    }
}
