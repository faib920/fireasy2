// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Provider
{
    /// <summary>
    /// 指定默认的 <see cref="IServiceProvider"/> 实现。
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DefaultProviderServiceAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="DefaultProviderServiceAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="serviceType"></param>
        public DefaultProviderServiceAttribute(Type serviceType)
        {
            ServiceType = serviceType;
        }

        /// <summary>
        /// 获取或设置 <see cref="IServiceProvider"/> 类型。
        /// </summary>
        public Type ServiceType { get; set; }
    }
}
