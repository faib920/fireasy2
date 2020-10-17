﻿// -----------------------------------------------------------------------
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
using System.Reflection;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 一个抽象类，提供获取数据库架构的方法。
    /// </summary>
    public abstract class SchemaBase : ISchemaProvider
    {
        private readonly Dictionary<Type, List<MemberInfo>> _dicRestrMbrs = new Dictionary<Type, List<MemberInfo>>();
        private readonly List<DataType> _dataTypes = new List<DataType>();

        IProvider IProviderService.Provider { get; set; }

        protected ConnectionParameter GetConnectionParameter(IDatabase database)
        {
            return database.Provider.GetConnectionParameter(database.ConnectionString);
        }

        /// <summary>
        /// 获取指定类型的数据库架构信息。
        /// </summary>
        /// <typeparam name="T">架构信息的类型。</typeparam>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="predicate">用于测试架构信息是否满足条件的函数。</param>
        /// <returns></returns>
        public virtual IEnumerable<T> GetSchemas<T>(IDatabase database, Expression<Func<T, bool>> predicate = null) where T : ISchemaMetadata
        {
            var restrictionValues = SchemaQueryTranslator.GetRestrictions<T>(predicate, _dicRestrMbrs);

            using var connection = database.CreateConnection();
            try
            {
                return GetCategory<T>() switch
                {
                    SchemaCategory.Database => (IEnumerable<T>)GetDatabases(database, restrictionValues),
                    SchemaCategory.DataType => (IEnumerable<T>)GetDataTypes(database, restrictionValues),
                    SchemaCategory.MetadataCollection => (IEnumerable<T>)GetMetadataCollections(database, restrictionValues),
                    SchemaCategory.ReservedWord => (IEnumerable<T>)GetReservedWords(database, restrictionValues),
                    SchemaCategory.Table => (IEnumerable<T>)GetTables(database, restrictionValues),
                    SchemaCategory.Column => (IEnumerable<T>)GetColumns(database, restrictionValues),
                    SchemaCategory.View => (IEnumerable<T>)GetViews(database, restrictionValues),
                    SchemaCategory.ViewColumn => (IEnumerable<T>)GetViewColumns(database, restrictionValues),
                    SchemaCategory.Index => (IEnumerable<T>)GetIndexs(database, restrictionValues),
                    SchemaCategory.IndexColumn => (IEnumerable<T>)GetIndexColumns(database, restrictionValues),
                    SchemaCategory.Procedure => (IEnumerable<T>)GetProcedures(database, restrictionValues),
                    SchemaCategory.ProcedureParameter => (IEnumerable<T>)GetProcedureParameters(database, restrictionValues),
                    SchemaCategory.ForeignKey => (IEnumerable<T>)GetForeignKeys(database, restrictionValues),
                    SchemaCategory.User => (IEnumerable<T>)GetUsers(database, restrictionValues),
                    _ => Enumerable.Empty<T>(),
                };
            }
            catch (Exception ex)
            {
                throw new SchemaException(typeof(T).Name, ex);
            }
        }

        /// <summary>
        /// 为 <typeparamref name="T"/> 类型添加约定限定。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="restrs"></param>
        protected void AddRestriction<T>(params Expression<Func<T, object>>[] restrs) where T : ISchemaMetadata
        {
            Guard.ArgumentNull(restrs, nameof(restrs));

            var indexes = _dicRestrMbrs.TryGetValue(typeof(T), () => new List<MemberInfo>());
            indexes.Clear();

            for (int i = 0, n = restrs.Length; i < n; i++)
            {
                var mbr = restrs[i].Body as MemberExpression;
                if (mbr == null && restrs[i].Body.NodeType == ExpressionType.Convert)
                {
                    mbr = (restrs[i].Body as UnaryExpression).Operand as MemberExpression;
                }

                if (mbr == null)
                {
                    continue;
                }

                if (mbr.Member is PropertyInfo property)
                {
                    indexes.Add(property);
                }
            }
        }

        /// <summary>
        /// 添加数据类型。
        /// </summary>
        /// <param name="name">类型名称。</param>
        /// <param name="dbType"></param>
        /// <param name="systemType"></param>
        protected void AddDataType(string name, DbType dbType, Type systemType)
        {
            _dataTypes.Add(new DataType { Name = name, DbType = dbType, SystemType = systemType });
        }

        /// <summary>
        /// 获取 <see cref="Column"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="Procedure"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Procedure> GetProcedures(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="ProcedureParameter"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ProcedureParameter> GetProcedureParameters(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="Table"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="User"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<User> GetUsers(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="View"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="ViewColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ViewColumn> GetViewColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="Database"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Database> GetDatabases(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="ReservedWord"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ReservedWord> GetReservedWords(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="Restriction"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Restriction> GetRestrictions(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;

        }

        /// <summary>
        /// 获取 <see cref="ForeignKey"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="DataType"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<DataType> GetDataTypes(IDatabase database, RestrictionDictionary restrictionValues)
        {
            return _dataTypes;
        }

        /// <summary>
        /// 获取 <see cref="Index"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Index> GetIndexs(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="IndexColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<IndexColumn> GetIndexColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="MetadataCollection"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<MetadataCollection> GetMetadataCollections(IDatabase database, RestrictionDictionary restrictionValues)
        {
            yield break;
        }

        protected IEnumerable<T> ExecuteAndParseMetadata<T>(IDatabase database, SpecialCommand sql, ParameterCollection parameters, Func<IRecordWrapper, IDataReader, T> parser)
        {
            using var reader = database.ExecuteReader(sql, parameters: parameters);
            var wrapper = database.Provider.GetService<IRecordWrapper>();
            while (reader.Read())
            {
                yield return parser(wrapper, reader);
            }
        }

        protected virtual Column SetColumnType(Column column)
        {
            if (column.NumericPrecision > 0 && column.NumericScale != null && column.DataType.IndexOf("int", StringComparison.CurrentCultureIgnoreCase) == -1)
            {
                column.ColumnType = $"{column.DataType}({column.NumericPrecision},{column.NumericScale})";
            }
            else if (column.NumericPrecision > 0)
            {
                column.ColumnType = $"{column.DataType}({column.NumericPrecision})";
            }
            else if (column.Length > 0)
            {
                column.ColumnType = $"{column.DataType}({column.Length})";
            }
            else
            {
                column.ColumnType = column.DataType;
            }

            return column;
        }

        private SchemaCategory GetCategory<T>()
        {
            Enum.TryParse(typeof(T).Name, out SchemaCategory category);
            return category;
        }
    }
}
