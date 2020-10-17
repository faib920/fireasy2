// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Ioc.Registrations
{
    internal class ScopedRegistration<TService, TImplementation> : TransientRegistration<TService, TImplementation>
        where TImplementation : class, TService
        where TService : class
    {
        public ScopedRegistration(Container container)
            : base(container)
        {
        }

        public override Lifetime Lifetime => Lifetime.Scoped;
    }
}
