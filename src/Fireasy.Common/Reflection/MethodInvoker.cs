// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Reflection
{
    /// <summary>
    /// 包装 <see cref="MethodInfo"/> 对象，创建一个委托来提升方法的执行。
    /// </summary>
    public class MethodInvoker
    {
        private readonly Func<object, object[], object> _invoker;

        /// <summary>
        /// 获取要包装的 <see cref="MethodInfo"/> 对象。
        /// </summary>
        public MethodInfo MethodInfo { get; private set; }

        /// <summary>
        /// 初始化 <see cref="MethodInvoker"/> 类的新实例。
        /// </summary>
        /// <param name="methodInfo">要包装的 <see cref="MethodInfo"/> 对象。</param>
        public MethodInvoker(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            _invoker = CreateInvokeDelegate(methodInfo);
        }

        /// <summary>
        /// 使用指定的参数调用当前实例的方法。
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <param name="parameters">方法的参数。</param>
        /// <returns></returns>
        public object Invoke(object instance, params object[] parameters)
        {
            if (_invoker == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate, MethodInfo.Name));
            }

            return _invoker(instance, parameters);
        }

        private static Func<object, object[], object> CreateInvokeDelegate(MethodInfo methodInfo)
        {
            var targetParExp = Expression.Parameter(typeof(object), "s");
            var argsParExp = Expression.Parameter(typeof(object[]), "args");

            var callExp = InvokerBuilder.BuildMethodCall(methodInfo, typeof(object), targetParExp, argsParExp);
            var lambdaExp = Expression.Lambda(typeof(Func<object, object[], object>), callExp, targetParExp, argsParExp);

            return (Func<object, object[], object>)lambdaExp.Compile();
        }
    }
}
