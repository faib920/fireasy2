// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Extensions;

namespace Fireasy.Data
{
    public static class RowMapperFactory
    {
        public static IDataRowMapper<T> CreateRowMapper<T>()
        {
            return typeof(T).IsDbTypeSupported() ? 
                (IDataRowMapper<T>)SingleValueRowMapper<T>.Create() : new DefaultRowMapper<T>();
        }
    }
}
