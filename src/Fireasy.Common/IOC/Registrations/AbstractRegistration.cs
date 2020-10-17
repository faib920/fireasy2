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
    internal abstract class AbstractRegistration : IRegistration
    {
        private static readonly object _locker = new object();
        private Func<IResolver, object> _instanceCreator;
        protected readonly Container _container;
        protected readonly ParameterExpression _parameter = Expression.Parameter(typeof(IResolver), "r");

        protected AbstractRegistration(Container container, Type serviceType, Type implementationType)
        {
            _container = container;
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        protected AbstractRegistration(Container container, Type serviceType, Func<IResolver, object> instanceCreator)
        {
            _container = container;
            ServiceType = serviceType;
            _instanceCreator = instanceCreator;
        }

        public virtual Lifetime Lifetime { get; }

        public Type ServiceType { get; private set; }

        public Type ImplementationType { get; set; }

        public object Resolve(IResolver resolver)
        {
            lock (_locker)
            {
                if (_instanceCreator == null)
                {
                    _instanceCreator = BuildInstanceCreator();
                }
            }

            return _instanceCreator(resolver);
        }

        private Func<IResolver, object> BuildInstanceCreator()
        {
            var expression = BuildExpression();
            var newInstanceMethod = Expression.Lambda<Func<IResolver, object>>(expression, _parameter);
            return newInstanceMethod.Compile();
        }

        internal abstract Expression BuildExpression();
    }
}
