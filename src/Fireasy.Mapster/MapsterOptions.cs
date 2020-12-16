// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Mapster;
using System;
using System.Reflection;

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
        public MapsterOptions CreateMap<TSource, TDestination>(Action<TypeAdapterSetter<TSource, TDestination>> action)
        {
            var setter = TypeAdapterConfig<TSource, TDestination>.NewConfig();
            action?.Invoke(setter);

            return this;
        }

        /// <summary>
        /// 添加程序集中的 <see cref="IRegister"/> 类。
        /// </summary>
        /// <param name="assemblies">要搜索的程序集数组，如果没有指定，则为当前调用的程序集。</param>
        /// <returns></returns>
        public MapsterOptions AddRegisters(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetCallingAssembly());
            }
            else
            {
                TypeAdapterConfig.GlobalSettings.Scan(assemblies);
            }

            return this;
        }
    }
}
#endif
