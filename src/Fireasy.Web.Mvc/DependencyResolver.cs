// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Fireasy.Web.Mvc
{
    public class DependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// 初始化 <see cref="DependencyResolver"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        public DependencyResolver(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 初始化 <see cref="DependencyResolver"/> 类的新实例。
        /// </summary>
        public DependencyResolver()
        {
        }

        public object GetService(Type serviceType)
        {
            if (serviceProvider == null)
            {
                if (serviceType.IsInterface || serviceType.IsAbstract)
                {
                    return null;
                }

                return serviceType.New();
            }

            return serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            yield return serviceProvider.GetService(serviceType);
        }
    }
}
#endif