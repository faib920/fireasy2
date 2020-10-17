// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Ioc;
using System.Linq;
using System.Reflection;
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
#else
using System.Web.Mvc;
#endif
namespace Fireasy.Web.Mvc
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// 向 IOC 容器注册所有控制器类型。
        /// </summary>
        /// <param name="container"></param>
        /// <param name="assembly"></param>
        public static void RegisterControllers(this Container container, Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes().Where(s => typeof(Controller).IsAssignableFrom(s)))
            {
                container.RegisterTransient(type, type);
            }
        }
    }
}
