// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Reflection;

namespace Fireasy.Web.MicroServices
{
    /// <summary>
    /// 微服务设置参数。
    /// </summary>
    public class MicroServiceOption
    {
        /// <summary>
        /// 获取或设置服务路径。
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 获取或设置是否需要身份验证。
        /// </summary>
        public bool UseAuthentication { get; set; }

        /// <summary>
        /// 获取或设置 IP 白名单，用分号分隔。
        /// </summary>
        public string IpWhiteAddress { get; set; }

        /// <summary>
        /// 添加程序集中的处理类。
        /// </summary>
        /// <param name="assembly"></param>
        public void AddHandlers(Assembly assembly)
        {
            ServiceManager.AddHandlers(assembly);
        }

        /// <summary>
        /// 添加程序集中的处理类。
        /// </summary>
        /// <param name="assemblyName"></param>
        public void AddHandlers(string assemblyName)
        {
            ServiceManager.AddHandlers(Assembly.Load(assemblyName));
        }
    }
}
