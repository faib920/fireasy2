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
    public class ControllerFactory : System.Web.Mvc.DefaultControllerFactory
    {
        private Container container;

        /// <summary>
        /// 初始化 <see cref="ControllerFactory"/> 类的新实例。
        /// </summary>
        /// <param name="container">指定的IOC容器。</param>
        public ControllerFactory(Container container)
        {
            this.container = container;
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
            : base (activator)
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
            if (controllerType == null)
            {
                return base.GetControllerInstance(requestContext, controllerType);
            }

            //忽略
            if (controllerType.IsDefined<IgnoreFactoryAttribute>())
            {
                return base.GetControllerInstance(requestContext, controllerType);
            }

            var controller = container != null ? 
                container.Resolve(controllerType) as IController : base.GetControllerInstance(requestContext, controllerType);
            
            controller.As<System.Web.Mvc.Controller>(c =>
                {
                    c.ActionInvoker = ControllerActionInvoker.Instance;
                });

            return controller;
        }

#if PLUGIN
        /// <summary>
        /// 根据控制器名称获得控制器类型。
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerName">控制器名称。</param>
        /// <returns>控制器类型。</returns>
        private Type GetControllerTypeFromPlugins(RequestContext requestContext, string controllerName)
        {
            if (!requestContext.RouteData.Values.ContainsKey("pluginName"))
            {
                return null;
            }

            var pluginName = requestContext.RouteData.GetRequiredString("pluginName");

            foreach (var plugin in PluginManager.GetPlugins())
            {
                if (!string.Equals(pluginName, plugin.Plugin.Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                var type = plugin.GetControllerType(controllerName + "Controller");

                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
#endif
    }
}
#endif