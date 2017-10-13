// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Ioc;
using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;

namespace Fireasy.Web.Http
{
    /// <summary>
    /// <see cref="IHttpController"/> 的激活器。
    /// </summary>
    public class HttpControllerActivator : IHttpControllerActivator
    {
        private Container container;

        /// <summary>
        /// 初始化 <see cref="HttpControllerActivator"/> 类的新实例。
        /// </summary>
        /// <param name="container">指定的IOC容器。</param>
        public HttpControllerActivator(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// 创建 <see cref="IHttpController"/> 实例。
        /// </summary>
        /// <param name="request"></param>
        /// <param name="controllerDescriptor"></param>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
        {
            return container.Resolve(controllerType) as IHttpController;
        }
    }
}
