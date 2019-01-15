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
        private static object locker = new object();
        private object instance;
        private Func<object> instanceCreator;

        internal SingletonRegistration(Type serviceType, object instance) :
            base(serviceType, instance.GetType())
        {
            this.instance = instance;
        }

        internal SingletonRegistration(Type serviceType, Type implementationType, Func<object> instanceCreator) :
            base(serviceType, implementationType)
        {
            this.instanceCreator = instanceCreator;
        }

        internal override Expression BuildExpression()
        {
            lock (locker)
            {
                if (instance == null && instanceCreator != null)
                {
                    instance = instanceCreator();
                }
            }

            return Expression.Constant(instance, ServiceType);
        }
    }
}
