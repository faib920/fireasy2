﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Data.Provider;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 提供获取数据库架构的方法。
    /// </summary>
    public interface ISchemaProvider : IProviderService
    {
        /// <summary>
        /// 获取指定类型的数据库架构信息。
        /// </summary>
        /// <typeparam name="T">架构信息的类型。</typeparam>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="predicate">用于限制查询的表达式。此方法只识别 x => x.TableName == "table" && x.Name == "name" 这样的表达式。</param>
        /// <returns></returns>
        IEnumerable<T> GetSchemas<T>(IDatabase database, Expression<Func<T, bool>> predicate = null) where T : ISchemaMetadata;
    }
}
