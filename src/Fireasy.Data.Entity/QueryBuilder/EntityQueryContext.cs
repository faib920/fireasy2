// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Syntax;

namespace Fireasy.Data.Entity.QueryBuilder
{
    internal sealed class EntityQueryContext
    {
        internal ParameterCollection Parameters { get; set; }
        internal IDatabase Database { get; set; }
        internal ISyntaxProvider Syntax { get; set; }
        internal EntityPersistentEnvironment Environment { get; set; }
    }
}
