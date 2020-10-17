// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Aop;
using Fireasy.Common.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Ioc.Registrations
{
    internal class TransientRegistration<TService, TImplementation> : AbstractRegistration
        where TImplementation : class, TService
        where TService : class
    {
        public TransientRegistration(Container container)
            : base(container, typeof(TService), typeof(TImplementation))
        {
        }

        public TransientRegistration(Container container, Func<IResolver, TService> instanceCreator)
            : base(container, typeof(TService), instanceCreator)
        {
        }
        public override Lifetime Lifetime => Lifetime.Transient;

        internal override Expression BuildExpression()
        {
            var newExp = BuildNewExpression();
            var initMbrExp = BuildMemberInitExpression(newExp);
            var initializer = _container.GetInitializer<TImplementation>();
            return initializer != null ? BuildExpressionWithInstanceInitializer(initMbrExp, initializer) : initMbrExp;
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
            ConstructorInfo[] constructors = null;

            if (typeof(IAopSupport).IsAssignableFrom(typeof(TImplementation)))
            {
                constructors = AspectFactory.GetProxyType(typeof(TImplementation)).GetConstructors();
            }
            else
            {
                constructors = typeof(TImplementation).GetConstructors();
            }

            if (constructors.Length != 1)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.NoDefaultConstructor, typeof(TImplementation)));
            }

            var constructor = constructors[0];

            foreach (var par in constructor.GetParameters())
            {
                if (!_container.IsRegistered(par.ParameterType))
                {
                    throw new InvalidOperationException(SR.GetString(SRKind.NotFoundRegisterForType, par.ParameterType));
                }
            }

            var arguments =
                from parameter in constructor.GetParameters()
                select BuildParameterExpression(parameter.ParameterType);

            return Expression.New(constructor, arguments.ToArray());
        }

        private Expression BuildParameterExpression(Type parameterType)
        {
            return Expression.Call(_parameter, nameof(IResolver.Resolve), new Type[] { parameterType });
        }

        private Expression BuildMemberInitExpression(NewExpression newExpression)
        {
            var parameters = newExpression.Constructor.GetParameters();

            var bindings = from s in ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           where s.CanWrite && _container.GetRegistrations(s.PropertyType).Any()
                              && !s.IsDefined<IgnoreInjectPropertyAttribute>()
                              && !parameters.Any(t => t.ParameterType == s.PropertyType) //如果构造器里已经有这个类型，则不使用属性注入
                           select (MemberBinding)Expression.Bind(s, BuildParameterExpression(s.PropertyType));

            return Expression.MemberInit(newExpression, bindings);
        }
    }
}
