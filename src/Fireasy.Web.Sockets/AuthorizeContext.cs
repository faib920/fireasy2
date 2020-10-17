// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Reflection;
using System.Security.Principal;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 认证的上下文。
    /// </summary>
    public class AuthorizeContext
    {
        /// <summary>
        /// 初始化 <see cref="AuthorizeContext"/> 类的新实例。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="user"></param>
        /// <param name="arguments"></param>
        internal AuthorizeContext(IPrincipal user, MethodInfo method, object[] arguments)
        {
            User = user;
            Method = method;
            Arguments = arguments;
        }

        /// <summary>
        /// 获取当前调用的方法。
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// 获取当前用户。
        /// </summary>
        public IPrincipal User { get; }

        /// <summary>
        /// 获取调用的参数。
        /// </summary>
        public object[] Arguments { get; }
    }
}
