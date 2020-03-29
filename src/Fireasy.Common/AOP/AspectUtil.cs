// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Aop
{
    /// <summary>
    /// 辅助类。
    /// </summary>
    public static class AspectUtil
    {
        /// <summary>
        /// 获取所有拦截特性。
        /// </summary>
        /// <param name="callInfo"></param>
        /// <returns></returns>
        public static InterceptAttribute[] GetInterceptAttributes(InterceptCallInfo callInfo)
        {
            return callInfo.Member.GetCustomAttributes<InterceptAttribute>(true)
                .Union(callInfo.DefinedType.GetCustomAttributes<InterceptAttribute>(true))
                .ToArray();
        }
    }
}
