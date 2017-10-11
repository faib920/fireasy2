// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;

namespace Fireasy.Common.Ioc.Registrations
{
    internal class SingletonRegistration : AbstractRegistration
    {
        private readonly object instance;

        internal SingletonRegistration(Type serviceType, object instance) :
            base(serviceType)
        {
            this.instance = instance;
        }

        internal SingletonRegistration(Type serviceType, Func<object> instanceCreator) :
            this(serviceType, instanceCreator())
        {
        }

        internal override Expression BuildExpression()
        {
            return Expression.Constant(instance, ServiceType);
        }

    }

    internal class SingletonRegistration<TService> : AbstractRegistration where TService : class
    {
        private readonly TService instance;

        internal SingletonRegistration(TService instance) :
            base(typeof(TService))
        {
            this.instance = instance;
        }

        internal SingletonRegistration(Func<TService> instanceCreator) :
            this (instanceCreator())
        {
        }

        internal override Expression BuildExpression()
        {
            return Expression.Constant(instance, ServiceType);
        }
    }
}
