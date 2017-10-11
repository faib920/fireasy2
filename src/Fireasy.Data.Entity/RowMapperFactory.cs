// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 数据行映射器的工厂。
    /// </summary>
    internal static class RowMapperFactory
    {
        /// <summary>
        /// 根据数据类型创建相应的映射器。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IDataRowMapper CreateMapper(Type type)
        {
            Type mapperType;
            if (type.IsAnonymousType())
            {
                mapperType = typeof(AnonymousRowMapper<>);
            }
            else if (typeof(IEntity).IsAssignableFrom(type))
            {
                mapperType = typeof(EntityRowMapper<>);
            }
            else if (type.IsDbTypeSupported())
            {
                mapperType = typeof(SingleValueRowMapper<>);
            }
            else
            {
                mapperType = typeof(DefaultRowMapper<>);
            }

            return mapperType.MakeGenericType(type).New<IDataRowMapper>();
        }

        /// <summary>
        /// 根据数据类型创建相应的映射器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static IDataRowMapper<T> CreateMapper<T>()
        {
            return (IDataRowMapper<T>)CreateMapper(typeof(T));
        }
    }
}
