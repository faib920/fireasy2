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
        private Func<Container, object> instanceCreator;

        protected AbstractRegistration(Type serviceType, Type componetType)
        {
            ServiceType = serviceType;
            ComponentType = componetType;
            ParameterExpression = Expression.Parameter(typeof(Container), "s");
        }

        public Type ServiceType { get; private set; }

        public Type ComponentType { get; set; }

        internal Container Container { get; set; }

        internal ParameterExpression ParameterExpression { get; private set; }

        public object Resolve()
        {
            if (instanceCreator == null)
            {
                instanceCreator = BuildInstanceCreator();
            }

            var instance = instanceCreator(Container);

            return instance;
        }

        private Func<Container, object> BuildInstanceCreator()
        {
            var expression = BuildExpression();
            var newInstanceMethod = Expression.Lambda<Func<Container, object>>(expression, ParameterExpression);
            return newInstanceMethod.Compile();
        }

        internal abstract Expression BuildExpression();
    }
}
