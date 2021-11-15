// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Query
{
    public sealed class QueryExecuteException : Exception
    {
        public QueryExecuteException(string message, Exception exp = null)
            : base (message, exp)
        {
        }
    }
}
