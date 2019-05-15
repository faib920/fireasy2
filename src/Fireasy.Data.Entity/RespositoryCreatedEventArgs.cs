// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    public class RespositoryCreatedEventArgs
    {
        public Type EntityType { get; set; }

        public bool Succeed { get; set; }

        public Exception Exception { get; set; }
    }
}
