﻿// -----------------------------------------------------------------------
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
            var initializer = container.GetInitializer<TImplementation>();
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
            ConstructorInfo constructor = null;

            if (typeof(IAopSupport).IsAssignableFrom(typeof(TImplementation)))
            {
                constructors = AspectFactory.GetProxyType(typeof(TImplementation)).GetConstructors();
            }
            else
            {
                constructors = typeof(TImplementation).GetConstructors();
            }

            //寻找最匹配的构造方法
            foreach (var con in constructors.OrderBy(s => s.GetParameters().Length))
            {
                if (con.GetParameters().All(s => container.IsRegistered(s.ParameterType)))
                {
                    constructor = con;
                    break;
                }
            }

            if (constructor == null)
            {
                throw new InvalidOperationException(SR.GetString(SRKind.NoDefaultConstructor));
            }

            var arguments =
                from parameter in constructor.GetParameters()
                select BuildParameterExpression(parameter.ParameterType);

            return Expression.New(constructor, arguments.ToArray());
        }

        private Expression BuildParameterExpression(Type parameterType)
        {
            return Expression.Call(parameter, nameof(IResolver.Resolve), new Type[] { parameterType });
        }

        private Expression BuildMemberInitExpression(NewExpression newExpression)
        {
            var parameters = newExpression.Constructor.GetParameters();

            var bindings = from s in ImplementationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                             where s.CanWrite && container.GetRegistrations(s.PropertyType).Any()
                                && !s.IsDefined<IgnoreInjectPropertyAttribute>()
                                && !parameters.Any(t => t.ParameterType == s.PropertyType) //如果构造器里已经有这个类型，则不使用属性注入
                                select (MemberBinding)Expression.Bind(s, BuildParameterExpression(s.PropertyType));

            return Expression.MemberInit(newExpression, bindings);
        }
    }
}
