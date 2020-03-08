// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Fireasy.Common.Extensions;
using Fireasy.Common.Aop;

namespace Fireasy.Common.Ioc.Registrations
{
    internal class TransientRegistration<TService, TImplementation> : AbstractRegistration
        where TImplementation : class, TService
        where TService : class
    {
        private static readonly MethodInfo MthResolve = typeof(Container).GetMethod(nameof(Container.Resolve), new[] { typeof(Type) });

        public TransientRegistration()
            : base(typeof(TService), typeof(TImplementation))
        {
        }

        internal override Expression BuildExpression()
        {
            var newExpression = BuildNewExpression();
            var instanceInitializer = Container.GetInitializer<TImplementation>();
            var initExpression = BuildMemberInitExpression(newExpression);
            return instanceInitializer != null ? BuildExpressionWithInstanceInitializer(initExpression, instanceInitializer) : initExpression;
        }

        private Expression BuildExpressionWithInstanceInitializer(Expression expression,
            Action<TImplementation> instanceInitializer)
        {
            Func<TImplementation, TImplementation> instanceCreatorWithInitializer = instance =>
                {
                    instanceInitializer(instance);
                    return instance;
                };

            return Expression.Invoke(Expression.Constant(instanceCreatorWithInitializer), expression);
        }

        private NewExpression BuildNewExpression()
        {
            //检查是否可构造
            Helpers.CheckConstructable(typeof(TImplementation));
            //获取默认的构造函数
            ConstructorInfo constructor = null;

            if (typeof(IAopSupport).IsAssignableFrom(typeof(TImplementation)))
            {
                constructor = AspectFactory.GetProxyType(typeof(TImplementation)).GetConstructors().Single();
            }
            else
            {
                constructor = typeof(TImplementation).GetConstructors().Single();
            }

            var arguments =
                from parameter in constructor.GetParameters()
                select BuildParameterExpression(parameter.ParameterType);

            return Expression.New(constructor, arguments.ToArray());
        }

        private Expression BuildParameterExpression(Type parameterType)
        {
            var regs = Container.GetRegistrations(parameterType);
            if (regs.Any())
            {
                if (Container.IsEnumerableResolve(parameterType))
                {
                    var elementType = parameterType.GetEnumerableElementType();
                    return Expression.NewArrayInit(elementType, regs.Select(s => (s as AbstractRegistration).BuildExpression()));
                }

                return (regs.LastOrDefault() as AbstractRegistration).BuildExpression();
            }

            return Expression.Constant(null, parameterType);
        }

        private Expression BuildMemberInitExpression(NewExpression newExpression)
        {
            var parameters = newExpression.Constructor.GetParameters();

            var bindings = from s in ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             where s.CanWrite && Container.GetRegistrations(s.PropertyType).Any()
                                && !s.IsDefined<IgnoreInjectPropertyAttribute>()
                                && !parameters.Any(t => t.ParameterType == s.PropertyType) //如果构造器里已经有这个类型，则不使用属性注入
                                select (MemberBinding)Expression.Bind(s, Expression.Convert(
                                    Expression.Call(Container.GetParameterExpression(), MthResolve, new Expression[]
                                        {
                                            Expression.Constant(s.PropertyType)
                                        }) , s.PropertyType));

            return Expression.MemberInit(newExpression, bindings);
        }
    }
}
