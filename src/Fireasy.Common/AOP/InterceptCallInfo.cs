// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Reflection;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 用于通知客户端的拦截信息。无法继承此类。
    /// </summary>
    public sealed class InterceptCallInfo
    {
        /// <summary>
        /// 获取或设置定义的类型。
        /// </summary>
        public Type DefinedType { get; set; }

        /// <summary>
        /// 获取或设置当前被拦截的方法或属性。
        /// </summary>
        public MemberInfo Member { get; set; }

        /// <summary>
        /// 获取或设置方法的返回类型。
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// 获取或设置当前被拦截的目标对象。
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// 获取或设置拦截的类型。
        /// </summary>
        public InterceptType InterceptType { get; set; }

        /// <summary>
        /// 获取或设置方法的参数数组。
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// 获取或设置方法的返回值。
        /// </summary>
        public object ReturnValue { get; set; }

        /// <summary>
        /// 获取或设置触发的异常信息。
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 获取或设置取消 Before 事件之后调用基类的方法。
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// 获取或设置是否中断后继拦截器的执行。
        /// </summary>
        public bool Break { get; set; }
    }
}
