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
        private readonly Func<object[], object> _invoker;

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
            _invoker = GetConstructorDelegate(constructorInfo);
        }

        private Func<object[], object> GetConstructorDelegate(ConstructorInfo constructorInfo)
        {
            var targetParExp = Expression.Parameter(typeof(object), "s");
            var argsParExp = Expression.Parameter(typeof(object[]), "args");

            var callExp = InvokerBuilder.BuildMethodCall(constructorInfo, typeof(object), targetParExp, argsParExp);
            var lambdaExp = Expression.Lambda(typeof(Func<object[], object>), callExp, argsParExp);
            return (Func<object[], object>)lambdaExp.Compile();

            /*
            var dm = new DynamicMethod("CreateInstance", typeof(object),
                new Type[] { typeof(object[]) }, constructorInfo.DeclaringType, true);

            var parameters = constructorInfo.GetParameters();
            var emiter = new EmitHelper(dm.GetILGenerator());
            emiter.nop
                .For(0, parameters.Length, (e, i) => e.ldarg_1.ldc_i4(i).ldelem_ref
                    .Assert(parameters[i].ParameterType.IsValueType,
                        e1 => e1.unbox_any(parameters[i].ParameterType),
                        e1 => e1.castclass(parameters[i].ParameterType)))
                .newobj(constructorInfo)
                .ret();

            return (Func<object[], object>)dm.CreateDelegate(typeof(Func<object[], object>));
            */
        }

        /// <summary>
        /// 使用指定的参数执行构造函数。
        /// </summary>
        /// <param name="parameters">构造函数的参数。</param>
        /// <returns></returns>
        public object Invoke(params object[] parameters)
        {
            if (_invoker == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate, ConstructorInfo.Name));
            }

            return _invoker(parameters);
        }
    }
}
