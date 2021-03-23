// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Serialization;
using System;
using System.Text;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示编码过的数据。无法继承此类。
    /// </summary>
    public sealed class CodedData : ITextSerializable, IFormattable
    {
        /// <summary>
        /// 将字符串转换为 <see cref="CodedData"/> 对象。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static implicit operator CodedData(string str)
        {
            return new CodedData(Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// 将 <see cref="CodedData"/> 转换为字符串。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static explicit operator string(CodedData data)
        {
            return data == null || data.Data == null ? string.Empty : Encoding.UTF8.GetString(data.Data);
        }

        public static bool operator ==(CodedData v1, string v2)
        {
            return v1.ToString() == v2;
        }

        public static bool operator !=(CodedData v1, string v2)
        {
            return v1.ToString() != v2;
        }

        public static bool operator ==(CodedData v1, CodedData v2)
        {
            return v1.ToString() == v2.ToString();
        }

        public static bool operator !=(CodedData v1, CodedData v2)
        {
            return v1.ToString() != v2.ToString();
        }

        /// <summary>
        /// 初始化类 <see cref="CodedData"/> 的新实例。
        /// </summary>
        public CodedData()
        {
            Data = new byte[0];
        }

        /// <summary>
        /// 初始化类 <see cref="CodedData"/> 的新实例。
        /// </summary>
        /// <param name="content"></param>
        public CodedData(string content)
            : this(Encoding.UTF8.GetBytes(content))
        {
        }

        /// <summary>
        /// 初始化类 <see cref="CodedData"/> 的新实例。
        /// </summary>
        /// <param name="data"></param>
        public CodedData(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// 获取或设置二进制数据。
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 输出字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Data == null ? string.Empty : Encoding.UTF8.GetString(Data);
        }

        string IFormattable.ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }

        string ITextSerializable.Serialize(ITextSerializer serializer)
        {
            return serializer.Serialize(Encoding.UTF8.GetString(Data));
        }

        void ITextSerializable.Deserialize(ITextSerializer serializer, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Data = Encoding.UTF8.GetBytes(text);
            }
        }
    }
}
