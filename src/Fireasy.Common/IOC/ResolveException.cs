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
    public sealed class ResolveException : Exception
    {
        public ResolveException(string message)
            : base(message)
        {
        }
    }
}
