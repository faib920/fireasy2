// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Query
{
    internal class QueryProviderCache
    {
        private static readonly ConcurrentDictionary<Type, Func<IQueryProvider, Expression, IQueryable>> _cache =
            new ConcurrentDictionary<Type, Func<IQueryProvider, Expression, IQueryable>>();

        internal static IQueryable Create(Type type, IQueryProvider queryProvider, Expression expression)
        {
            var func = _cache.GetOrAdd(type, key =>
                {
                    var rcons = typeof(QuerySet<>).MakeGenericType(key).GetConstructors()[1];
                    var parExp1 = Expression.Parameter(typeof(IQueryProvider));
                    var parExp2 = Expression.Parameter(typeof(Expression));
                    var newExp = Expression.New(rcons, parExp1, parExp2);
                    var lambdaExp = Expression.Lambda<Func<IQueryProvider, Expression, IQueryable>>(newExp, parExp1, parExp2);
                    return lambdaExp.Compile();
                });

            return func(queryProvider, expression);
        }
    }
}
