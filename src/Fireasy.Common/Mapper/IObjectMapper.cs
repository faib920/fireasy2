// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Mapper
{
    /// <summary>
    /// 提供对象的映射转换。
    /// </summary>
    public interface IObjectMapper
    {
        /// <summary>
        /// 将源对象转换为目标类型。
        /// </summary>
        /// <typeparam name="TSource">源对象类型。</typeparam>
        /// <typeparam name="TDestination">目标对象类型。</typeparam>
        /// <param name="source">源对象。</param>
        /// <returns></returns>
        TDestination Map<TSource, TDestination>(TSource source);

        /// <summary>
        /// 将源对象转换为目标类型。
        /// </summary>
        /// <typeparam name="TSource">源对象类型。</typeparam>
        /// <typeparam name="TDestination">目标对象类型。</typeparam>
        /// <param name="source">源对象。</param>
        /// <param name="destination">目标对象。</param>
        /// <returns></returns>
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
    }
}
