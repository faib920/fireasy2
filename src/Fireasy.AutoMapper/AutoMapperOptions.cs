// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using AutoMapper;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fireasy.AutoMapper
{
    public class AutoMapperOptions
    {
        public List<Action<IMapperConfigurationExpression>> Configurators { get; } = new List<Action<IMapperConfigurationExpression>>();

        /// <summary>
        /// 添加配置文件。
        /// </summary>
        /// <typeparam name="TProfile"></typeparam>
        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            Configurators.Add(c => c.AddProfile<TProfile>());
        }

        /// <summary>
        /// 添加程序集里的 <see cref="Profile"/> 及定义了 <see cref="AutoMapAttribute"/> 特性的类。
        /// </summary>
        /// <param name="assembly"></param>
        [Obsolete]
        public void AddAssembly(Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            Configurators.Add(c => c.AddMaps(assembly));
        }

        /// <summary>
        /// 添加程序集里的 <see cref="Profile"/> 及定义了 <see cref="AutoMapAttribute"/> 特性的类。
        /// </summary>
        /// <param name="assemblies">要搜索的程序集数组，如果没有指定，则为当前调用的程序集。</param>
        public void AddProfiles(params Assembly[] assemblies)
        {
            if (assemblies == null)
            {
                Configurators.Add(c => c.AddMaps(Assembly.GetCallingAssembly()));
            }
            else
            {
                assemblies.ForEach(s => Configurators.Add(c => c.AddMaps(s)));
            }
        }
    }
}
#endif