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
using System.Web.Mvc;
using System.Web.Routing;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 控制器创建者。
    /// </summary>
    public class ControllerActivator : IControllerActivator
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 初始化 <see cref="ControllerActivator"/> 类的新实例。
        /// </summary>
        /// <param name="serviceProvider">应用程序服务提供者实例。</param>
        public ControllerActivator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 初始化 <see cref="ControllerActivator"/> 类的新实例。
        /// </summary>
        public ControllerActivator()
        {
        }

        /// <summary>
        /// 创建控制器实例。
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public IController Create(RequestContext requestContext, Type controllerType)
        {
            if (_serviceProvider != null &&
                _serviceProvider.GetService(controllerType) is IController controller)
            {
                return controller;
            }

            if (controllerType.IsAbstract || controllerType.IsInterface)
            {
                return null;
            }

            return controllerType.New<IController>();
        }
    }
}
#endif