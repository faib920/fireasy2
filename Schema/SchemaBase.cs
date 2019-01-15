// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.RecordWrapper;
using Fireasy.Data.Schema.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 一个抽象类，提供获取数据库架构的方法。
    /// </summary>
    public abstract class SchemaBase : ISchemaProvider
    {
        private readonly Dictionary<Type, Dictionary<string, int>> dicRestrIndex = new Dictionary<Type, Dictionary<string, int>>();

        public IProvider Provider { get; set; }

        /// <summary>
        /// 获取指定类型的数据库架构信息。
        /// </summary>
        /// <typeparam name="T">架构信息的类型。</typeparam>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="predicate">用于测试架构信息是否满足条件的函数。</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetSchemas<T>(IDatabase database, Expression<Func<T, bool>> predicate = null) where T : ISchemaMetadata
        {
            var indexes = dicRestrIndex.TryGetValue(typeof(T), () => new Dictionary<string, int>());
            var restrictionValues = SchemaQueryTranslator.GetRestriction(indexes, typeof(T), predicate);

            using (var connection = database.CreateConnection())
            {
                try
                {
                    switch (GetCategory<T>())
                    {
                        case SchemaCategory.Database:
                            return (IEnumerable<T>)GetDatabases(database, restrictionValues);
                        case SchemaCategory.DataType:
                            return (IEnumerable<T>)GetDataTypes(database, restrictionValues);
                        case SchemaCategory.MetadataCollection:
                            return (IEnumerable<T>)GetMetadataCollections(database, restrictionValues);
                        case SchemaCategory.ReservedWord:
                            return (IEnumerable<T>)GetReservedWords(database, restrictionValues);
                        case SchemaCategory.Table:
                            return (IEnumerable<T>)GetTables(database, restrictionValues);
                        case SchemaCategory.Column:
                            return (IEnumerable<T>)GetColumns(database, restrictionValues);
                        case SchemaCategory.View:
                            return (IEnumerable<T>)GetViews(database, restrictionValues);
                        case SchemaCategory.ViewColumn:
                            return (IEnumerable<T>)GetViewColumns(database, restrictionValues);
                        case SchemaCategory.Index:
                            return (IEnumerable<T>)GetIndexs(database, restrictionValues);
                        case SchemaCategory.IndexColumn:
                            return (IEnumerable<T>)GetIndexColumns(database, restrictionValues);
                        case SchemaCategory.Procedure:
                            return (IEnumerable<T>)GetProcedures(database, restrictionValues);
                        case SchemaCategory.ProcedureParameter:
                            return (IEnumerable<T>)GetProcedureParameters(database, restrictionValues);
                        case SchemaCategory.ForeignKey:
                            return (IEnumerable<T>)GetForeignKeys(database, restrictionValues);
                        case SchemaCategory.User:
                            return (IEnumerable<T>)GetUsers(database, restrictionValues);
                        default:
                            return Enumerable.Empty<T>();
                    }
                }
                catch (Exception ex)
                {
                    throw new SchemaException(typeof(T).Name, ex);
                }
                finally
                {
                    connection.TryClose();
                }
            }
        }

        /// <summary>
        /// 为 <typeparamref name="T"/> 类型添加约定限定的索引位置。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="restrs"></param>
        protected void AddRestrictionIndex<T>(params Expression<Func<T, string>>[] restrs) where T : ISchemaMetadata
        {
            Guard.ArgumentNull(restrs, nameof(restrs));
            var indexes = dicRestrIndex.TryGetValue(typeof(T), () => new Dictionary<string, int>());
            for (var i = 0; i < restrs.Length; i++)
            {
                if (restrs[i] == null)
                {
                    continue;
                }

                if (!(restrs[i].Body is MemberExpression mbr))
                {
                    continue;
                }

                indexes.Add(mbr.Member.Name, i);
            }
        }

        /// <summary>
        /// 获取 <see cref="Column"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="Procedure"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Procedure> GetProcedures(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="ProcedureParameter"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ProcedureParameter> GetProcedureParameters(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="Table"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Table> GetTables(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="User"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<User> GetUsers(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="View"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<View> GetViews(IDatabase database, string[] restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="ViewColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ViewColumn> GetViewColumns(IDatabase database, string[] restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="DataBase"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<DataBase> GetDatabases(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="ReservedWord"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ReservedWord> GetReservedWords(IDatabase database, string[] restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="Restriction"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Restriction> GetRestrictions(IDatabase database, string[] restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="ForeignKey"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="DataType"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<DataType> GetDataTypes(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="Index"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Index> GetIndexs(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="IndexColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<IndexColumn> GetIndexColumns(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="MetadataCollection"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<MetadataCollection> GetMetadataCollections(IDatabase database, string[] restrictionValues)
        {
            yield break;
        }

        protected void ParameteRestrition(ParameterCollection parameters, string name, int index, string[] restrictionValues)
        {
            if (restrictionValues.Length >= index + 1)
            {
                parameters.Add(name, restrictionValues[index]);
            }
            else
            {
                parameters.Add(name, DBNull.Value);
            }
        }

        protected IEnumerable<T> ParseMetadata<T>(IDatabase database, SqlCommand sql, ParameterCollection parameters, Func<IRecordWrapper, IDataReader, T> parser)
        {
            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return parser(wrapper, reader);
                }
            }
        }

        protected IEnumerable<T> GetSchemas<T>(IDatabase database, string collectionName, string[] restrictionValues, Func<DataRow, T> parser)
        {
            using (var connection = database.CreateConnection())
            {
                var table = connection.GetSchema(collectionName, restrictionValues);

                foreach (DataRow row in table.Rows)
                {
                    yield return parser(row);
                }
            }
        }

        private SchemaCategory GetCategory<T>()
        {
#if NET35
            try
            {
                return (SchemaCategory)Enum.Parse(typeof(SchemaCategory), typeof(T).Name);
            }
            catch
            {
                return SchemaCategory.Unknow;
            }
#else
            SchemaCategory category = SchemaCategory.Unknow;
            Enum.TryParse(typeof(T).Name, out category);
            return category;
#endif
        }
    }
}
