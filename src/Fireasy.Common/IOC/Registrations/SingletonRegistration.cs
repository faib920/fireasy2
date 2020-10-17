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
        private static readonly object _locker = new object();
        private object _instance;
        private readonly Func<IResolver, object> _instanceCreator;

        internal SingletonRegistration(Container container, Type serviceType, object instance) :
            base(container, serviceType, instance.GetType())
        {
            _instance = instance;
        }

        internal SingletonRegistration(Container container, Type serviceType, Func<IResolver, object> instanceCreator) :
            base(container, serviceType, (Type)null)
        {
            _instanceCreator = instanceCreator;
        }
        public override Lifetime Lifetime => Lifetime.Singleton;

        internal override Expression BuildExpression()
        {
            lock (_locker)
            {
                if (_instance == null && _instanceCreator != null)
                {
                    _instance = _instanceCreator(_container);
                }
            }

            return Expression.Constant(_instance, ServiceType);
        }
    }
}
