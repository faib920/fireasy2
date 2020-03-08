// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Reflection;
using System;

namespace Fireasy.Data
{
    public static class RowMapperFactory
    {
        public static IDataRowMapper<T> CreateRowMapper<T>()
        {
            var elementType = typeof(T);

            var mapperType = ReflectionCache.GetMember("RowMapper", elementType, k =>
                {
                    Type generalType;
                    if (k.IsPrimitive || k == typeof(string))
                    {
                        generalType = typeof(SingleValueRowMapper<>);
                    }
                    else
                    {
                        generalType = typeof(DefaultRowMapper<>);
                    }

                    return generalType.MakeGenericType(k);
                });

            return mapperType.New<IDataRowMapper<T>>();
        }
    }
}
