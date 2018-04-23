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
    public abstract class BaseTableGenerateProvider : ITableGenerateProvider
    {
        IProvider IProviderService.Provider { get; set; }

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
