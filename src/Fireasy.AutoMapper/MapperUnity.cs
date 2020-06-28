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

namespace Fireasy.AutoMapper
{
    public static class MapperUnity
    {
        private static readonly List<Action<IMapperConfigurationExpression>> configurators = new List<Action<IMapperConfigurationExpression>>();
        private static readonly object locker = new object();
        private static IMapper mapper;

        /// <summary>
        /// 添加配置文件。
        /// </summary>
        /// <typeparam name="TProfile"></typeparam>
        public static void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            configurators.Add(c => c.AddProfile<TProfile>());
        }

        /// <summary>
        /// 获取 <see cref="IMapper"/> 实例，此实例为单例。
        /// </summary>
        /// <returns></returns>
        public static IMapper GetMapper()
        {
            return SingletonLocker.Lock(ref mapper, locker, () =>
            {
                var mapperConfiguration = new MapperConfiguration(c =>
                {
                    configurators.ForEach(s => s(c));
                });

                return mapperConfiguration.CreateMapper();
            });
        }
    }
}
