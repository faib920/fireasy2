// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 提供对象的二进制序列化与反序列化方法。
    /// </summary>
    public interface IBinarySerializer
    {
        /// <summary>
        /// 将对象序列化为字节数组表示。
        /// </summary>
        /// <typeparam name="T">对象的类型。</typeparam>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的字节数组。</returns>
        byte[] Serialize<T>(T obj);

        /// <summary>
        /// 从一个字节数组中反序列化对象。
        /// </summary>
        /// <typeparam name="T">能够反序列化的对象类型。 </typeparam>
        /// <param name="data">字节数组。</param>
        /// <returns>类型为 <typeparamref name="T"/> 的对象。</returns>
        T Deserialize<T>(byte[] data);
    }
}
