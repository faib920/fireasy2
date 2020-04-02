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
    internal class FuncRegistration<TService> : AbstractRegistration where TService : class
    {
        private readonly Func<IResolver, object> instanceCreator;
        private readonly Lifetime lifetime;

        internal FuncRegistration(Container container, Func<IResolver, object> instanceCreator, Lifetime lifetime)
            : base(container, typeof(TService), (Type)null)
        {
            this.instanceCreator = instanceCreator;
            this.lifetime = lifetime;
        }

        public override Lifetime Lifetime => lifetime;

        internal override Expression BuildExpression()
        {
            return Expression.Invoke(Expression.Constant(instanceCreator), parameter);
        }
    }
}
