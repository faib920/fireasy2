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
    /// 扩展自默认控制器工厂的类。
    /// </summary>
    public class ControllerFactory : DefaultControllerFactory
    {
        private readonly Container _container;
        private IResolver _scope;

        /// <summary>
        /// 初始化 <see cref="ControllerFactory"/> 类的新实例。
        /// </summary>
        /// <param name="container">指定的IOC容器。</param>
        public ControllerFactory(Container container)
        {
            _container = container;
        }

        /// <summary>
        /// 初始化 <see cref="ControllerFactory"/> 类的新实例。
        /// </summary>
        public ControllerFactory()
        {
        }

        /// <summary>
        /// 初始化 <see cref="ControllerFactory"/> 类的新实例。
        /// </summary>
        /// <param name="activator">控制器的创建者。</param>
        public ControllerFactory(IControllerActivator activator)
            : base(activator)
        {
        }

        /// <summary>
        /// 获取控制器实例，并设置其 ActionInvoker 指向 <see cref="ControllerActionInvoker"/> 实例。
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            _scope = _container?.CreateScope();
            if (controllerType == null)
            {
                return base.GetControllerInstance(requestContext, controllerType);
            }

            //忽略
            if (controllerType.IsDefined<IgnoreFactoryAttribute>())
            {
                return base.GetControllerInstance(requestContext, controllerType);
            }

            if (_scope.GetRegistrations(controllerType) == null)
            {
                _container.Register(controllerType, Lifetime.Transient);
            }

            var controller = _scope?.Resolve(controllerType) as IController ?? base.GetControllerInstance(requestContext, controllerType);

            controller.As<Controller>(c =>
                {
                    c.ActionInvoker = new ControllerActionInvoker();
                });

            return controller;
        }

        /// <summary>
        /// 释放控制器实例。
        /// </summary>
        /// <param name="controller"></param>
        public override void ReleaseController(IController controller)
        {
            _scope?.Dispose();

            base.ReleaseController(controller);
        }
    }
}
#endif