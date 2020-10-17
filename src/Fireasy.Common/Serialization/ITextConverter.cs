// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 提供对象和文本的转换，以实现自定义序列化和反序列化过程。
    /// </summary>
    public interface ITextConverter
    {
        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        bool CanConvert(Type type);

        /// <summary>
        /// 获取是否可以使用 ReadObject 方法。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 获取是否可使用 WriteObject 方法。
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// 将对象写为文本。
        /// </summary>
        /// <param name="serializer">一个 <see cref="ITextSerializer"/> 对象。</param>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>表示对象的文本。</returns>
        string WriteObject(ITextSerializer serializer, object obj);

        /// <summary>
        /// 从文本中读取对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="ITextSerializer"/> 对象。</param>
        /// <param name="dataType">将要读取的类型。</param>
        /// <param name="text">表示对象的文本。</param>
        /// <returns>反序列化后的对象。</returns>
        object ReadObject(ITextSerializer serializer, Type dataType, string text);
    }
}
