// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Ioc;
using System.Threading.Tasks;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 提供对类成员进行拦截的方法。
    /// </summary>
    [IgnoreRegister]
    public interface IInterceptor
    {
        /// <summary>
        /// 使用上下文对象对当前的拦截器进行初始化。
        /// </summary>
        /// <param name="context">包含拦截定义的上下文。</param>
        void Initialize(InterceptContext context);

        /// <summary>
        /// 将自定义方法注入到当前的拦截点。
        /// </summary>
        /// <param name="info">拦截调用信息。</param>
        void Intercept(InterceptCallInfo info);
    }
}
