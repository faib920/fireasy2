// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using AutoMapper;
using Fireasy.Common.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fireasy.AutoMapper
{
    public static class MapperUnity
    {
        private static readonly List<Action<IMapperConfigurationExpression>> _configurators = new List<Action<IMapperConfigurationExpression>>();
        private static readonly object _locker = new object();
        private static IMapper _mapper;

        /// <summary>
        /// 添加配置文件。
        /// </summary>
        /// <typeparam name="TProfile"></typeparam>
        public static void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            _configurators.Add(c => c.AddProfile<TProfile>());
        }

        /// <summary>
        /// 添加程序集里的 <see cref="Profile"/> 及定义了 <see cref="AutoMapAttribute"/> 特性的类。
        /// </summary>
        /// <param name="assembly"></param>
        public static void AddAssembly(Assembly assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            _configurators.Add(c => c.AddMaps(assembly));
        }

        /// <summary>
        /// 获取 <see cref="IMapper"/> 实例，此实例为单例。
        /// </summary>
        /// <returns></returns>
        public static IMapper GetMapper()
        {
            return SingletonLocker.Lock(ref _mapper, _locker, () =>
            {
                var mapperConfiguration = new MapperConfiguration(c =>
                {
                    _configurators.ForEach(s => s(c));
                });

                return mapperConfiguration.CreateMapper();
            });
        }
    }
}
