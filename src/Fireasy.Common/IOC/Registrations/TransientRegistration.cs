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
        public TransientRegistration()
            : base(typeof(TService))
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
            var registration = Container.GetRegistration(parameterType).As<AbstractRegistration>();
            if (registration != null)
            {
                return registration.BuildExpression();
            }

            return Expression.Constant(null, parameterType);
        }

        private Expression BuildMemberInitExpression(NewExpression newExpression)
        {
            var parameters = newExpression.Constructor.GetParameters();
            var implType = typeof(TImplementation);

            var method = typeof(Container).GetMethod("Resolve", new [] { typeof(Type) });
            var bindings = from s in ServiceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             let registration = Container.GetRegistration(s.PropertyType)
                             let p = implType.GetProperty(s.Name)
                             where s.CanWrite && registration != null
                                && (p != null && !p.IsDefined<IgnoreInjectPropertyAttribute>())
                                && parameters.FirstOrDefault(t => t.ParameterType == s.PropertyType) == null
                                select (MemberBinding)Expression.Bind(s, Expression.Convert(
                                    Expression.Call(ParameterExpression, method, new Expression[]
                                        {
                                            Expression.Constant(s.PropertyType)
                                        }) , s.PropertyType));
            return Expression.MemberInit(newExpression, bindings);
        }
    }
}
