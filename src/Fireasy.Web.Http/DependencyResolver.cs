// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Ioc;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace Fireasy.Web.Http
{
    /// <summary>
    /// 依赖项目解释器。
    /// </summary>
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

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            yield return container.Resolve(serviceType);
        }
    }
}
