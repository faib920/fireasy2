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
    /// 包装 <see cref="ConstructorInfo"/> 对象，创建一个委托来提升构造函数的执行。
    /// </summary>
    public class ConstructorInvoker
    {
        private Func<object[], object> invoker;

        /// <summary>
        /// 
        /// </summary>
        public ConstructorInfo ConstructorInfo { get; private set; }

        /// <summary>
        /// 初始化 <see cref="ConstructorInvoker"/> 类的新实例。
        /// </summary>
        /// <param name="constructorInfo">要包装的 <see cref="ConstructorInfo"/> 对象。</param>
        public ConstructorInvoker(ConstructorInfo constructorInfo)
        {
            ConstructorInfo = constructorInfo;
            invoker = InitializeInvoker(constructorInfo);
        }

        private Func<object[], object> InitializeInvoker(ConstructorInfo constructorInfo)
        {
            var targetParameterExpression = Expression.Parameter(typeof(object), "s");
            var argsParameterExpression = Expression.Parameter(typeof(object[]), "args");

            var callExpression = InvokerBuilder.BuildMethodCall(constructorInfo, typeof(object), targetParameterExpression, argsParameterExpression);
            var lambdaExpression = Expression.Lambda(typeof(Func<object[], object>), callExpression, argsParameterExpression);
            var compiled = (Func<object[], object>)lambdaExpression.Compile();
            return compiled;
        }

        /// <summary>
        /// 使用指定的参数执行构造函数。
        /// </summary>
        /// <param name="parameters">构造函数的参数。</param>
        /// <returns></returns>
        public object Invoke(params object[] parameters)
        {
            return invoker(parameters);
        }
    }
}
