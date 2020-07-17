// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 提供一个适用于 Json 和 Xml 序列化的值类型转换器。
    /// </summary>
    public abstract class ValueConverter : ITextConverter
    {
        /// <summary>
        /// 判断类型是否可转换。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool CanConvert(Type type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取是否可以使用 ReadObject 方法。
        /// </summary>
        public virtual bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// 获取是否可以使用 WriteObject 方法。
        /// </summary>
        public virtual bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// 将对象写为文本。
        /// </summary>
        /// <param name="serializer">一个 <see cref="ITextSerializer"/> 对象。</param>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>表示对象的文本。</returns>
        public abstract string WriteObject(ITextSerializer serializer, object obj);

        /// <summary>
        /// 从文本中读取对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="ITextSerializer"/> 对象。</param>
        /// <param name="dataType">将要读取的类型。</param>
        /// <param name="text">表示对象的文本。</param>
        /// <returns>反序列化后的对象。</returns>
        public abstract object ReadObject(ITextSerializer serializer, Type dataType, string text);
    }

    /// <summary>
    /// 为 <typeparamref name="T"/> 提供一个适用于 Json 和 Xml 序列化的值类型转换器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueConverter<T> : ValueConverter
    {
        /// <summary>
        /// 判断类型是否可转换。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool CanConvert(Type type)
        {
            return type == typeof(T) ||
                (type.IsNullableType() && type.GetNonNullableType() == typeof(T));
        }
    }
}
