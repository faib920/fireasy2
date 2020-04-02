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
        private static readonly object locker = new object();
        private Func<IResolver, object> instanceCreator;
        protected readonly Container container;
        protected readonly ParameterExpression parameter = Expression.Parameter(typeof(IResolver), "r");

        protected AbstractRegistration(Container container, Type serviceType, Type implementationType)
        {
            this.container = container;
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        protected AbstractRegistration(Container container, Type serviceType, Func<IResolver, object> instanceCreator)
        {
            this.container = container;
            ServiceType = serviceType;
            this.instanceCreator = instanceCreator;
        }

        public virtual Lifetime Lifetime { get; }

        public Type ServiceType { get; private set; }

        public Type ImplementationType { get; set; }

        public object Resolve(IResolver resolver)
        {
            lock (locker)
            {
                if (instanceCreator == null)
                {
                    instanceCreator = BuildInstanceCreator();
                }
            }

            return instanceCreator(resolver);
        }

        private Func<IResolver, object> BuildInstanceCreator()
        {
            var expression = BuildExpression();
            var newInstanceMethod = Expression.Lambda<Func<IResolver, object>>(expression, parameter);
            return newInstanceMethod.Compile();
        }

        internal abstract Expression BuildExpression();
    }
}
