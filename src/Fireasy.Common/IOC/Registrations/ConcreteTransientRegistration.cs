// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Ioc.Registrations
{
    internal class ConcreteTransientRegistration<TConcrete> :
        TransientRegistration<TConcrete, TConcrete>
        where TConcrete : class
    {
    }
}
