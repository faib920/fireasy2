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
using Fireasy.Data.Schema;
using Fireasy.Data.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
        /// <param name="metadata">实体元数据。</param>
        /// <param name="tableName">数据表名称。</param>
        public bool TryCreate(IDatabase database, EntityMetadata metadata, string tableName)
        {
            var syntax = database.Provider.GetService<ISyntaxProvider>();
            if (syntax != null)
            {
                var properties = PropertyUnity.GetPersistentProperties(metadata.EntityType).ToList();
                if (properties.Count > 0)
                {
                    var commands = BuildCreateTableCommands(syntax, tableName, properties);
                    BatchExecute(database, commands);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 尝试添加新的字段。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="metadata">实体元数据。</param>
        /// <param name="tableName">数据表名称。</param>
        public IList<IProperty> TryAddFields(IDatabase database, EntityMetadata metadata, string tableName)
        {
            var schema = database.Provider.GetService<ISchemaProvider>();
            var syntax = database.Provider.GetService<ISyntaxProvider>();
            if (schema == null || syntax == null)
            {
                return null;
            }

            //查询目前数据表中的所有字段
            var columns = schema.GetSchemas<Column>(database, s => s.TableName == tableName).Select(s => s.Name).ToArray();
            
            //筛选出新的字段
            var properties = PropertyUnity.GetPersistentProperties(metadata.EntityType)
                .Where(s => !columns.Contains(s.Info.FieldName, StringComparer.CurrentCultureIgnoreCase)).ToList();

            if (properties.Count != 0)
            {
                var commands = BuildAddFieldCommands(syntax, tableName, properties);
                BatchExecute(database, commands);
            }

            return properties;
        }

        /// <summary>
        /// 判断实体类型对应的数据表是否已经存在。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="tableName">数据表名称。</param>
        /// <returns></returns>
        public bool IsExists(IDatabase database, string tableName)
        {
            var syntax = database.Provider.GetService<ISyntaxProvider>();
            if (!string.IsNullOrEmpty(tableName) && syntax != null)
            {
                //判断表是否存在
                SqlCommand sql = syntax.ExistsTable(tableName);
                using (var reader = database.ExecuteReader(sql))
                {
                    if (reader.Read())
                    {
                        return IsExistsTable(reader);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 构建创建表的语句。
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        protected abstract SqlCommand[] BuildCreateTableCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties);

        protected abstract SqlCommand[] BuildAddFieldCommands(ISyntaxProvider syntax, string tableName, IList<IProperty> properties);

        protected virtual bool IsExistsTable(IDataReader reader)
        {
            return reader.GetInt32(0) > 0;
        }

        protected string Quote(ISyntaxProvider syntax, string name)
        {
            return DbUtility.FormatByQuote(syntax, name);
        }

        /// <summary>
        /// 处理主键。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="syntax"></param>
        /// <param name="property"></param>
        protected virtual void ProcessPrimaryKeyField(StringBuilder builder, ISyntaxProvider syntax, IProperty property)
        {
        }

        /// <summary>
        /// 将字段生成添加到 builder 中。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="syntax"></param>
        /// <param name="property"></param>
        protected virtual void AppendFieldToBuilder(StringBuilder builder, ISyntaxProvider syntax, IProperty property)
        {
            builder.AppendFormat(" {0}", Quote(syntax, property.Info.FieldName));

            //数据类型及长度精度等
            builder.AppendFormat(" {0}", syntax.Column((DbType)property.Info.DataType,
                property.Info.Length,
                property.Info.Precision,
                property.Info.Scale));

            //主键
            if (property.Info.IsPrimaryKey)
            {
                ProcessPrimaryKeyField(builder, syntax, property);
            }

            //自增
            if (property.Info.GenerateType == IdentityGenerateType.AutoIncrement &&
                !string.IsNullOrEmpty(syntax.IdentityColumn))
            {
                builder.AppendFormat(" {0}", syntax.IdentityColumn);
            }

            //不可空
            if (!property.Info.IsNullable)
            {
                builder.AppendFormat(" not null");
            }

            //默认值
            if (!PropertyValue.IsEmpty(property.Info.DefaultValue))
            {
                if (property.Type == typeof(string))
                {
                    builder.AppendFormat(" default '{0}'", property.Info.DefaultValue);
                }
                else if (property.Type.IsEnum)
                {
                    builder.AppendFormat(" default {0}", (int)property.Info.DefaultValue.GetValue());
                }
                else if (property.Type == typeof(bool) || property.Type == typeof(bool?))
                {
                    builder.AppendFormat(" default {0}", (bool)property.Info.DefaultValue ? 1 : 0);
                }
                else
                {
                    builder.AppendFormat(" default {0}", property.Info.DefaultValue);
                }
            }
        }

        private void BatchExecute(IDatabase database, SqlCommand[] commands)
        {
            commands.ForEach(s => database.ExecuteNonQuery(s));
        }
    }
}
