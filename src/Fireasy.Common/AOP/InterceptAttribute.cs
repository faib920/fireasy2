// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 表示一个属性或方法使用指定的拦截器注入一段执行代码。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class InterceptAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="InterceptAttribute"/> 类的新实例。
        /// </summary>
        public InterceptAttribute()
        {
        }

        /// <summary>
        /// 初始化 <see cref="InterceptAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="interceptorType">拦截器的类型。</param>
        /// <param name="allowThrowException">是否在 try 语句块内抛出异常。</param>
        public InterceptAttribute(Type interceptorType, bool allowThrowException = true)
        {
            InterceptorType = interceptorType;
            AllowThrowException = allowThrowException;
        }

        /// <summary>
        /// 获取或设置拦截器的类型。
        /// </summary>
        public Type InterceptorType { get; set; }

        /// <summary>
        /// 获取或设置是否在 try 语句块内抛出异常。如果为 false，异常将不被抛出，而是使用拦截器进行通知。
        /// </summary>
        public bool AllowThrowException { get; set; }
    }
}
