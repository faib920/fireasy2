// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Fireasy.Common.Ioc
{
    internal class ResolveScope : Scope<ResolveScope>
    {
        public IRegistration Registration { get; set; }

        public Lazy<object> Creator { get; set; }
    }
}
