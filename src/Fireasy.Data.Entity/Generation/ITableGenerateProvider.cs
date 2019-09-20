// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Provider;
using System;
using System.Collections.Generic;

namespace Fireasy.Data.Entity.Generation
{
    /// <summary>
    /// 用于生成数据表的服务。
    /// </summary>
    public interface ITableGenerateProvider : IProviderService
    {
        /// <summary>
        /// 判断实体类型对应的数据表是否已经存在。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="tableName">数据表名称。</param>
        /// <returns></returns>
        bool IsExists(IDatabase database, string tableName);

        /// <summary>
        /// 尝试创建实体类型对应的数据表。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="metadata">实体元数据。</param>
        /// <param name="tableName">数据表名称。</param>
        bool TryCreate(IDatabase database, EntityMetadata metadata, string tableName);

        /// <summary>
        /// 尝试添加新的字段。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="metadata">实体元数据。</param>
        /// <param name="tableName">数据表名称。</param>
        IList<IProperty> TryAddFields(IDatabase database, EntityMetadata metadata, string tableName);
    }
}
