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
        private Func<Container, object> instanceCreator;

        protected AbstractRegistration(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public Type ServiceType { get; private set; }

        public Type ImplementationType { get; set; }

        internal Container Container { get; private set; }

        public object Resolve()
        {
            lock (locker)
            {
                if (instanceCreator == null)
                {
                    instanceCreator = BuildInstanceCreator();
                }
            }

            return instanceCreator(Container);
        }

        private Func<Container, object> BuildInstanceCreator()
        {
            var expression = BuildExpression();
            var newInstanceMethod = Expression.Lambda<Func<Container, object>>(expression, Container.GetParameterExpression());
            return newInstanceMethod.Compile();
        }

        internal abstract Expression BuildExpression();

        internal AbstractRegistration SetContainer(Container container)
        {
            Container = container;
            return this;
        }
    }
}
