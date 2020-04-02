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
        private static readonly object locker = new object();
        private object instance;
        private readonly Func<IResolver, object> instanceCreator;

        internal SingletonRegistration(Container container, Type serviceType, object instance) :
            base(container, serviceType, instance.GetType())
        {
            this.instance = instance;
        }

        internal SingletonRegistration(Container container, Type serviceType, Func<IResolver, object> instanceCreator) :
            base(container, serviceType, (Type)null)
        {
            this.instanceCreator = instanceCreator;
        }
        public override Lifetime Lifetime => Lifetime.Singleton;

        internal override Expression BuildExpression()
        {
            lock (locker)
            {
                if (instance == null && instanceCreator != null)
                {
                    instance = instanceCreator(container);
                }
            }

            return Expression.Constant(instance, ServiceType);
        }
    }
}
