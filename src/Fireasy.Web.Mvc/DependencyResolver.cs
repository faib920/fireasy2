// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Extensions;
using Fireasy.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Fireasy.Web.Mvc
{
    public class DependencyResolver : IDependencyResolver
    {
        private Container container;

        /// <summary>
        /// 初始化 <see cref="DependencyResolver"/> 类的新实例。
        /// </summary>
        /// <param name="container">指定的IOC容器。</param>
        public DependencyResolver(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// 初始化 <see cref="DependencyResolver"/> 类的新实例。
        /// </summary>
        public DependencyResolver()
        {
        }

        public object GetService(Type serviceType)
        {
            if (container == null)
            {
                if (serviceType.IsInterface || serviceType.IsAbstract)
                {
                    return null;
                }

                return serviceType.New();
            }

            if (!container.IsRegistered(serviceType))
            {
                return null;
            }

            return container.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (container == null || !container.IsRegistered(serviceType))
            {
                yield break;
            }

            yield return container.Resolve(serviceType);
        }
    }
}
#endif