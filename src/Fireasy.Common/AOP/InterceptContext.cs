// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 拦截器的上下文信息。无法继承此类。
    /// </summary>
    public sealed class InterceptContext
    {
        public InterceptContext(MemberInfo member, object target)
        {
            Member = member;
            Target = target;
        }

        /// <summary>
        /// 获取当前的成员。
        /// </summary>
        public MemberInfo Member { get; private set; }

        /// <summary>
        /// 获取当前被拦截的目标对象。
        /// </summary>
        public object Target { get; private set; }

    }
}
