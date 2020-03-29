// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 基于二进制的压缩序列化方法，对象序列化后进行压缩处理。
    /// </summary>
    public sealed class BinaryCompressSerializer : BinarySerializer
    {
        /// <summary>
        /// 将一个对象序列化为字节数组。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的字节数组。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public override byte[] Serialize<T>(T obj)
        {
            try
            {
                using var stream = new MemoryStream();
                if (Token != null && Token.Data != null && Token.Data.Length > 0)
                {
                    stream.Write(Token.Data, 0, Token.Data.Length);
                }

                using (var zipStream = new DeflateStream(stream, CompressionMode.Compress))
                {
                    var bin = new BinaryFormatter();
                    bin.Serialize(zipStream, obj);
                }

                return stream.ToArray();
            }
            catch (Exception ex)
            {
                throw new SerializationException(SR.GetString(SRKind.SerializationError), ex);
            }
        }

        /// <summary>
        /// 从一个字节数组反序列化对象。
        /// </summary>
        /// <param name="bytes">字节数组。</param>
        /// <returns>反序列化后的对象。</returns>
        [SuppressMessage("Microsoft.Usage", "CA2202")]
        public override T Deserialize<T>(byte[] bytes)
        {
            return (T)Deserialize(bytes, typeof(T));
        }

        /// <summary>
        /// 从一个字节数组中反序列化对象。
        /// </summary>
        /// <param name="bytes">字节数组。</param>
        /// <param name="type"></param>
        /// <returns>反序列化后的对象。</returns>
        public override object Deserialize(byte[] bytes, Type type)
        {
            byte[] data;

            if (Token != null && Token.Data != null && Token.Data.Length > 0)
            {
                data = new byte[bytes.Length - Token.Data.Length];
                Array.Copy(bytes, Token.Data.Length, data, 0, data.Length);

                if (Token.Data.Where((t, i) => t != bytes[i]).Any())
                {
                    throw new SerializationException(SR.GetString(SRKind.SerializationTokenInvalid));
                }
            }
            else
            {
                data = bytes;
            }

            try
            {
                using var stream = new MemoryStream(data);
                using var zipStream = new DeflateStream(stream, CompressionMode.Decompress);
                var bin = new BinaryFormatter
                {
                    Binder = new IgnoreSerializationBinder()
                };
                return bin.Deserialize(zipStream);
            }
            catch (Exception ex)
            {
                throw new SerializationException(SR.GetString(SRKind.DeserializationError), ex);
            }
        }
    }
}
