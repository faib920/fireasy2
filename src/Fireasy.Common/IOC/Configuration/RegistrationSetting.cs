// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Reflection;

namespace Fireasy.Common.Ioc.Configuration
{
    /// <summary>
    /// 表示服务与组件的注册键对。
    /// </summary>
    public class RegistrationSetting
    {
        /// <summary>
        /// 获取或设置服务的类型。
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// 获取或设置实现的类型。
        /// </summary>
        public Type ImplementationType { get; set; }

        /// <summary>
        /// 获取或设置程序集。
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <summary>
        /// 获取或设置是否单例。
        /// </summary>
        [Obsolete]
        public bool Singleton { get; set; }

        /// <summary>
        /// 获取或设置实例的生命周期。
        /// </summary>
        public Lifetime Lifetime { get; set; }

    }
}
