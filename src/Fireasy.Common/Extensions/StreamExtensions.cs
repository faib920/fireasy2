// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 流的扩展。
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// 从流对象中分段读取字节数组。
        /// </summary>
        /// <param name="stream">流对象。</param>
        /// <param name="readAction">每次读到的数据放入缓冲区。</param>
        /// <param name="bufferSize">缓冲区大小。</param>
        public static void Read(this Stream stream, Action<byte[], int, int> readAction, int bufferSize = 20480)
        {
            if (!stream.CanRead)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.StreamNotSupportRead));
            }

            var offset = 0;
            var bytesRead = 0;
            var buffer = new byte[bufferSize];
            while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                readAction(buffer, offset, bytesRead);
                offset += bytesRead;
            }
        }

        /// <summary>
        /// 将一个流对象的内容写入到目标流中。
        /// </summary>
        /// <param name="stream">流对象。</param>
        /// <param name="targetStream">目标流。</param>
        /// <param name="bufferSize">缓冲区大小。</param>
        /// <returns></returns>
        public static Stream WriteTo(this Stream stream, Stream targetStream, int bufferSize = 20480)
        {
            if (!stream.CanRead)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.StreamNotSupportRead));
            }

            if (!targetStream.CanWrite)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.StreamNotSupportWrite));
            }

            var buffer = new byte[bufferSize];
            var bytesRead = 0;

            while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                targetStream.Write(buffer, 0, bytesRead);
            }

            return stream;
        }

        /// <summary>
        /// 将流内容复制到 <see cref="MemoryStream"/> 对象中。
        /// </summary>
        /// <param name="stream">流对象。</param>
        /// <returns></returns>
        public static MemoryStream CopyToMemory(this Stream stream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream;
        }
    }
}
