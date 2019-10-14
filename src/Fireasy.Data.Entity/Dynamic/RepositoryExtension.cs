// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Dynamic
{
    public static class RepositoryExtension
    {
        public static IQueryable Where(this IRepository repository, Expression predicate)
        {
            return Where(repository as IQueryable, predicate);
        }

        public static IQueryable Where(this IQueryable queryable, Expression predicate)
        {
            if (queryable == null || predicate == null)
            {
                return queryable;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { queryable.ElementType },
                new[] { queryable.Expression, predicate });

            return queryable.Provider.CreateQuery(expression);
        }

        public static object FirstOrDefault(this IRepository repository, Expression predicate = null)
        {
            return FirstOrDefault(repository as IQueryable, predicate);
        }

        public static object FirstOrDefault(this IQueryable queryable, Expression predicate = null)
        {
            if (queryable == null)
            {
                return null;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.FirstOrDefault), new[] { queryable.ElementType },
                new[] { queryable.Expression, predicate });

            return queryable.Provider.Execute(expression);
        }

        public static IQueryable Select(this IQueryable queryable, params string[] selector)
        {
            if (queryable == null || selector == null)
            {
                return queryable;
            }

            var parameter = Expression.Parameter(queryable.ElementType, "s");

            var type = queryable.ElementType;
            var properties = type.GetProperties();

            var keyPairArray = new Expression[selector.Length];
            var constructor = typeof(KeyValuePair<string, object>).GetConstructors()[0];

            for (var i = 0; i < selector.Length; i++)
            {
                var property = properties.FirstOrDefault(s => s.Name == selector[i]);
                if (property != null)
                {
                    keyPairArray[i] = Expression.New(constructor, Expression.Constant(selector[i]),
                        Expression.Convert(Expression.MakeMemberAccess(parameter, property), typeof(object)));
                }
            }

            var arrayExp = Expression.NewArrayInit(typeof(KeyValuePair<string, object>), keyPairArray);

            var initExp = Expression.Convert(arrayExp, typeof(DynamicExpandoObject));

            var expression = Expression.Call(typeof(Queryable), "Select",
                new[] { type, typeof(DynamicExpandoObject) },
                new Expression[] {
                    Expression.Constant(queryable),
                    Expression.Lambda(initExp, parameter)
                });

            return queryable.Provider.CreateQuery(expression);
        }

        public static bool Any(this IRepository repository, Expression predicate)
        {
            return Any(repository as IQueryable, predicate);
        }

        public static bool Any(this IQueryable queryable, Expression predicate)
        {
            if (queryable == null)
            {
                return false;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.Any),
                new[] { queryable.ElementType },
                new[] { queryable.Expression, predicate });

            return (bool)queryable.Provider.Execute(expression);
        }

        public static bool All(this IRepository repository, Expression predicate)
        {
            return All(repository as IQueryable, predicate);
        }

        public static bool All(this IQueryable queryable, Expression predicate)
        {
            if (queryable == null)
            {
                return false;
            }

            var expression = Expression.Call(typeof(Queryable), nameof(Queryable.All),
                new[] { queryable.ElementType },
                new[] { queryable.Expression, predicate });

            return (bool)queryable.Provider.Execute(expression);
        }
    }
}
