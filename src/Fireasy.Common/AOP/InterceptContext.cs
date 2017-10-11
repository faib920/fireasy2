// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 拦截器的上下文信息。无法继承此类。
    /// </summary>
    public sealed class InterceptContext
    {
        /// <summary>
        /// 获取或设置拦截目标所定义的 <see cref="InterceptAttribute"/>。
        /// </summary>
        public InterceptAttribute Attribute { get; set; }

        /// <summary>
        /// 获取或设置当前被拦截的目标对象。
        /// </summary>
        public object Target { get; set; }

    }
}
