using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Data
{
    internal static class RowMapperFactory
    {
        internal static IDataRowMapper<T> CreateRowMapper<T>()
        {
            var elementType = typeof(T);
            Type generalType;
            if (elementType.IsPrimitive || elementType == typeof(string))
            {
                generalType = typeof(SingleValueRowMapper<>);
            }
            else
            {
                generalType = typeof(DefaultRowMapper<>);
            }

            return generalType.MakeGenericType(elementType).New<IDataRowMapper<T>>();
        }
    }
}
