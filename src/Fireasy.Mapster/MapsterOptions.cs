// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Mapster;
using System;

namespace Fireasy.Mapster
{
    public class MapsterOptions
    {
        /// <summary>
        /// 为 <typeparamref name="TSource"/> 和 <typeparamref name="TDestination"/> 添加映射。
        /// </summary>
        /// <typeparam name="TSource">源类型。</typeparam>
        /// <typeparam name="TDestination">目标类型。</typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        public MapsterOptions Map<TSource, TDestination>(Action<TypeAdapterSetter<TSource, TDestination>> action)
        {
            var setter = TypeAdapterConfig<TSource, TDestination>.NewConfig();
            action?.Invoke(setter);

            return this;
        }
    }
}
