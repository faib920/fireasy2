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
    /// 提供对数据加密及解密的一组方法。
    /// </summary>
    public interface ICryptoProvider
    {
        /// <summary>
        /// 对字节数组进行加密。
        /// </summary>
        /// <param name="array">要加密的字节数组。</param>
        /// <returns></returns>
        byte[] Encrypt(byte[] array);

        /// <summary>
        /// 对文本进行加密。
        /// </summary>
        /// <param name="source">要加密的文本。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        byte[] Encrypt(string source, Encoding encoding);

        /// <summary>
        /// 对流内的数据进行加密。
        /// </summary>
        /// <param name="sourceStream">要加密的源流对象。</param>
        /// <param name="destStream">加密后的目标流对象。</param>
        void Encrypt(Stream sourceStream, Stream destStream);

        /// <summary>
        /// 对字节数组进行解密。
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        byte[] Decrypt(byte[] array);

        /// <summary>
        /// 将字节数组进行解密为文本。
        /// </summary>
        /// <param name="array">密文字节数组。</param>
        /// <param name="encoding">采用的字符编码。</param>
        /// <returns></returns>
        string Decrypt(byte[] array, Encoding encoding);

        /// <summary>
        /// 对流内的数据进行解密。
        /// </summary>
        /// <param name="sourceStream">要解密的源流对象。</param>
        /// <param name="destStream">加解后的目标流对象。</param>
        void Decrypt(Stream sourceStream, Stream destStream);
    }
}
