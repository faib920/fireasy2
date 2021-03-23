// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.IO;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 一个抽象类，将对象序列化为二进制数据，也用于将二进制数据反序列化为对象。
    /// </summary>
    public abstract class BinarySerializer : IBinarySerializer
    {
        /// <summary>
        /// 获取或设置序列化令牌。
        /// </summary>
        public SerializeToken Token { get; set; }

        SerializeOption ISerializer.Option { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// 将一个对象序列化为字节数组。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的字节数组。</returns>
        public abstract byte[] Serialize<T>(T obj);

        /// <summary>
        /// 将一个对象序列化后输出到磁盘文件。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <param name="filePath">保存的文件路径。</param>
        public void Serialize<T>(T obj, string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            var bytes = Serialize(obj);
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// 从一个字节数组中反序列化对象。
        /// </summary>
        /// <param name="bytes">字节数组。</param>
        /// <returns>反序列化后的对象。</returns>
        public abstract T Deserialize<T>(byte[] bytes);

        /// <summary>
        /// 从一个字节数组中反序列化对象。
        /// </summary>
        /// <param name="bytes">字节数组。</param>
        /// <param name="type"></param>
        /// <returns>反序列化后的对象。</returns>
        public abstract object Deserialize(byte[] bytes, Type type);

        /// <summary>
        /// 从一个磁盘文件反序列化对象。
        /// </summary>
        /// <param name="filePath">要读取的文件路径。</param>
        /// <returns>反序列化后的对象。</returns>
        public T Deserialize<T>(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var mstream = new MemoryStream();
            const int bufferSize = 102400;
            var bytes = new byte[bufferSize];
            int length;
            while ((length = stream.Read(bytes, 0, bufferSize)) > 0)
            {
                mstream.Write(bytes, 0, length);
            }

            return Deserialize<T>(mstream.ToArray());
        }
    }
}
