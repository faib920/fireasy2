// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Entity.Metadata;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;

namespace Fireasy.Data.Entity.Generation
{
    /// <summary>
    /// 基础的数据表生成器。
    /// </summary>
    public abstract class BaseTableGenerateProvider : ITableGenerateProvider
    {
        IProvider IProviderService.Provider { get; set; }

        /// <summary>
        /// 尝试创建实体类型对应的数据表。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="entityType">实体类型。</param>
        public void TryCreate(IDatabase database, Type entityType)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            var syntax = database.Provider.GetService<ISyntaxProvider>();
            if (metadata != null && syntax != null)
            {
                var sqls = BuildCreateTableCommands(syntax, metadata);
                sqls.ForEach(s => database.ExecuteNonQuery(s));
            }
        }

        /// <summary>
        /// 判断实体类型对应的数据表是否已经存在。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="entityType">实体类型。</param>
        /// <returns></returns>
        public bool IsExists(IDatabase database, Type entityType)
        {
            var metadata = EntityMetadataUnity.GetEntityMetadata(entityType);
            var syntax = database.Provider.GetService<ISyntaxProvider>();
            if (metadata != null && syntax != null)
            {
                //判断表是否存在
                SqlCommand sql = syntax.ExistsTable(metadata.TableName);
                return database.ExecuteScalar<int>(sql) != 0;
            }

            return false;
        }

        /// <summary>
        /// 构建创建表的语句。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        protected abstract SqlCommand[] BuildCreateTableCommands(ISyntaxProvider syntax, EntityMetadata metadata);
    }
}
