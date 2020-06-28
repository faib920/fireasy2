// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;

namespace Fireasy.Common.Mapper
{
    /// <summary>
    /// 缺省的对象映射器。
    /// </summary>
    public class DefaultObjectMapper : IObjectMapper
    {
        public readonly static IObjectMapper Default = new DefaultObjectMapper();

        /// <summary>
        /// 将源对象转换为目标类型。
        /// </summary>
        /// <typeparam name="TSource">源对象类型。</typeparam>
        /// <typeparam name="TDestination">目标对象类型。</typeparam>
        /// <param name="source">源对象。</param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return source.To<TSource, TDestination>();
        }

        /// <summary>
        /// 将源对象转换为目标类型。
        /// </summary>
        /// <typeparam name="TSource">源对象类型。</typeparam>
        /// <typeparam name="TDestination">目标对象类型。</typeparam>
        /// <param name="source">源对象。</param>
        /// <param name="destination">目标对象。</param>
        /// <returns></returns>
        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return source.MapTo(destination);
        }
    }
}
