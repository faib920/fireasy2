using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// <see cref="ITextConverter"/> 的集合。
    /// </summary>
    public sealed class ConverterList : List<ITextConverter>
    {
        /// <summary>
        /// 获取指定类型的序列化转换器。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>一个 <see cref="ITextConverter"/> 对象。</returns>
        public ITextConverter GetConverter(Type type)
        {
            return this.FirstOrDefault(s => s.CanConvert(type));
        }

        /// <summary>
        /// 获取指定类型的序列化转换器。
        /// </summary>
        /// <param name="converterType"></param>
        /// <returns></returns>
        public IEnumerable<ITextConverter> GetConverters(Type converterType)
        {
            return this.Where(s => converterType.IsAssignableFrom(s.GetType()));
        }

        /// <summary>
        /// 获取指定类型的可写的序列化转换器。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <param name="convertTypes">转换器的类型。</param>
        /// <returns>一个 <see cref="ITextConverter"/> 对象。</returns>
        public ITextConverter GetWritableConverter(Type type, Type[] convertTypes)
        {
            return this.FirstOrDefault(s => s.CanConvert(type) && s.CanWrite && convertTypes.Any(t => t.IsAssignableFrom(s.GetType())));
        }

        /// <summary>
        /// 获取指定类型的可读的序列化转换器。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <param name="convertTypes">转换器的类型。</param>
        /// <returns>一个 <see cref="ITextConverter"/> 对象。</returns>
        public ITextConverter GetReadableConverter(Type type, Type[] convertTypes)
        {
            return this.FirstOrDefault(s => s.CanConvert(type) && s.CanRead && convertTypes.Any(t => t.IsAssignableFrom(s.GetType())));
        }
    }
}
