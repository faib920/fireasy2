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
using System.Web.Mvc;
using System.Web.Routing;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 控制器创建者。
    /// </summary>
    public class ControllerActivator : System.Web.Mvc.IControllerActivator
    {
        private Container container;

        /// <summary>
        /// 初始化 <see cref="ControllerActivator"/> 类的新实例。
        /// </summary>
        /// <param name="container">指定的IOC容器。</param>
        public ControllerActivator(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// 初始化 <see cref="DependencyResolver"/> 类的新实例。
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
            if (container != null)
            {
                return container.Resolve(controllerType) as IController;
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