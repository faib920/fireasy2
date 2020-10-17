// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using AutoMapper;
using System;
using System.Collections.Generic;

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
    }
}
