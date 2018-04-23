// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.Schema.Linq;

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
            var category = GetCategory<T>();

            return GetSchemas<T>(database, category, restrictionValues);
        }

        protected virtual IEnumerable<T> GetSchemas<T>(IDatabase database, SchemaCategory category, string[] restrictionValues)
        {
            using (var connection = database.CreateConnection())
            {
                var collectionName = GetSchemaCategoryName(category);
                DataTable table;
                try
                {
                    connection.TryOpen();
                    table = DoGetSchemas<T>(connection, collectionName, restrictionValues);
                    BeforeReturnSchemaElements<T>(connection, restrictionValues);
                }
                catch (NotSupportedException ex)
                {
                    throw new SchemaNotSupportedtException(collectionName, ex);
                }
                catch (Exception ex)
                {
                    throw new SchemaException(collectionName, ex);
                }
                finally
                {
                    connection.TryClose();
                }

                return ReturnSchemaElements(category, table).Cast<T>();
            }
        }

        /// <summary>
        /// 获取指定类型的数据库架构信息。
        /// </summary>
        /// <param name="database">提供给当前插件的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="collectionName">架构信息类别名称。</param>
        /// <param name="restrictionValues">列限制数组。</param>
        /// <returns></returns>
        public virtual DataTable GetSchema(IDatabase database, string collectionName, string[] restrictionValues)
        {
            DataTable table;
            using (var connection = database.CreateConnection())
            {
                try
                {
                    connection.TryOpen();
                    table = connection.GetSchema(collectionName, restrictionValues);
                }
                catch (NotSupportedException ex)
                {
                    throw new SchemaNotSupportedtException(collectionName, ex);
                }
                catch (Exception ex)
                {
                    throw new SchemaException(collectionName, ex);
                }
                finally
                {
                    connection.TryClose();
                }
            }

            return table;
        }

        /// <summary>
        /// 在枚举构架元素之前进行一些初始化的操作。
        /// </summary>
        /// <typeparam name="T">架构的类型。</typeparam>
        /// <param name="connection">数据库链接对象。</param>
        /// <param name="restrictionValues">限制数组。</param>
        protected virtual void BeforeReturnSchemaElements<T>(DbConnection connection, string[] restrictionValues)
        {
        }

        /// <summary>
        /// 自定义获取架构的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">当前的 <see cref="DbConnection"/> 对象。</param>
        /// <param name="collectionName">指定要返回的架构的名称。</param>
        /// <param name="restrictionValues">限制的数组。</param>
        /// <returns></returns>
        protected virtual DataTable DoGetSchemas<T>(DbConnection connection, string collectionName, string[] restrictionValues)
        {
            return connection.GetSchema(collectionName, restrictionValues);
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
                var mbr = restrs[i].Body as MemberExpression;
                if (mbr == null)
                {
                    continue;
                }
                indexes.Add(mbr.Member.Name, i);
            }
        }

        /// <summary>
        /// 返回架构中的元素。
        /// </summary>
        /// <param name="category"></param>
        /// <param name="table">记录架构信息的数据表。</param>
        /// <returns></returns>
        protected IEnumerable ReturnSchemaElements(SchemaCategory category, DataTable table)
        {
            IEnumerable enumer = null;
            switch (category)
            {
                case SchemaCategory.Database:
                    enumer = GetDatabases(table, null);
                    break;
                case SchemaCategory.Column:
                    enumer = GetColumns( table, null);
                    break;
                case SchemaCategory.DataType:
                    enumer = GetDataTypes(table, null);
                    break;
                case SchemaCategory.ForeignKey:
                    enumer = GetForeignKeys(table, null);
                    break;
                case SchemaCategory.IndexColumn:
                    enumer = GetIndexColumns(table, null);
                    break;
                case SchemaCategory.Index:
                    enumer = GetIndexs(table, null);
                    break;
                case SchemaCategory.MetadataCollection:
                    enumer = GetMetadataCollections(table, null);
                    break;
                case SchemaCategory.ProcedureParameter:
                    enumer = GetProcedureParameters(table, null);
                    break;
                case SchemaCategory.Procedure:
                    enumer = GetProcedures(table, null);
                    break;
                case SchemaCategory.ReservedWord:
                    enumer = GetReservedWords(table, null);
                    break;
                case SchemaCategory.Restriction:
                    enumer = GetRestrictions(table, null);
                    break;
                case SchemaCategory.Table:
                    enumer = GetTables(table, null);
                    break;
                case SchemaCategory.User:
                    enumer = GetUsers(table, null);
                    break;
                case SchemaCategory.ViewColumn:
                    enumer = GetViewColumns(table, null);
                    break;
                case SchemaCategory.View:
                    enumer = GetViews(table, null);
                    break;
                //case SchemaCategory.Trigger:
                //    enumer = GetTriggers(table, null);
                //    break;
            }

            return enumer;
        }

        /// <summary>
        /// 获取架构的名称。
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        protected virtual string GetSchemaCategoryName(SchemaCategory category)
        {
            switch (category)
            {
                case SchemaCategory.Database:
                    return "DATABASES";
                case SchemaCategory.Column:
                    return "COLUMNS";
                case SchemaCategory.DataType:
                    return "DATATYPES";
                case SchemaCategory.ForeignKey:
                    return "FOREIGNKEYS";
                case SchemaCategory.IndexColumn:
                    return "INDEXCOLUMNS";
                case SchemaCategory.Index:
                    return "INDEXES";
                case SchemaCategory.MetadataCollection:
                    return "METADATACOLLECTIONS";
                case SchemaCategory.ProcedureParameter:
                    return "PROCEDUREPARAMETERS";
                case SchemaCategory.Procedure:
                    return "PROCEDURES";
                case SchemaCategory.ReservedWord:
                    return "RESERVEDWORDS";
                case SchemaCategory.Restriction:
                    return "RESTRICTIONS";
                case SchemaCategory.Table:
                    return "TABLES";
                case SchemaCategory.User:
                    return "USERS";
                case SchemaCategory.ViewColumn:
                    return "VIEWCOLUMNS";
                case SchemaCategory.View:
                    return "VIEWS";
                //case SchemaCategory.Trigger:
                //    return "TRIGGERS";
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取 <see cref="Column"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Column> GetColumns(DataTable table, Action<Column, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Column
                    {
                        Catalog = row["TABLE_CATALOG"].ToString(),
                        Schema = row["TABLE_SCHEMA"].ToString(),
                        TableName = row["TABLE_NAME"].ToString(),
                        Name = row["COLUMN_NAME"].ToString(),
                        Default = row["COLUMN_DEFAULT"].ToString(),
                        DataType = row["DATA_TYPE"].ToString(),
                        NumericPrecision = row["NUMERIC_PRECISION"].To<int>(),
                        NumericScale = row["NUMERIC_SCALE"].To<int>(),
                        IsNullable = row["IS_NULLABLE"] != DBNull.Value && row["IS_NULLABLE"].To<bool>(),
                        Length = row["CHARACTER_MAXIMUM_LENGTH"].To<long>(),
                        Position = row["ORDINAL_POSITION"].To<int>(),
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="Procedure"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Procedure> GetProcedures(DataTable table, Action<Procedure, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Procedure
                    {
                        Catalog = row["SPECIFIC_CATALOG"].ToString(),
                        Schema = row["SPECIFIC_SCHEMA"].ToString(),
                        Name = row["SPECIFIC_NAME"].ToString(),
                        Type = row["ROUTINE_TYPE"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="ProcedureParameter"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ProcedureParameter> GetProcedureParameters(DataTable table, Action<ProcedureParameter, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ProcedureParameter
                    {
                        Catalog = row["SPECIFIC_CATALOG"].ToString(),
                        Schema = row["SPECIFIC_SCHEMA"].ToString(),
                        ProcedureName = row["SPECIFIC_NAME"].ToString(),
                        Direction = row["PARAMETER_MODE"].ToString() == "OUT" ? ParameterDirection.Output : ParameterDirection.Input,
                        Name = row["PARAMETER_NAME"].ToString(),
                        DataType = row["DATA_TYPE"].ToString(),
                        Length = row["CHARACTER_MAXIMUM_LENGTH"].To<long>(),
                        NumericScale = row["NUMERIC_SCALE"].To<int>(),
                        NumericPrecision = row["NUMERIC_PRECISION"].To<int>(),
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="Table"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Table> GetTables(DataTable table, Action<Table, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Table
                    {
                        Catalog = row["TABLE_CATALOG"].ToString(),
                        Schema = row["TABLE_SCHEMA"].ToString(),
                        Name = row["TABLE_NAME"].ToString(),
                        Type = row["TABLE_TYPE"].ToString(),
                        Description = ""
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="User"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<User> GetUsers(DataTable table, Action<User, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new User
                    {
                        Name = row["USER_NAME"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="View"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<View> GetViews(DataTable table, Action<View, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new View
                    {
                        Catalog = row["TABLE_CATALOG"].ToString(),
                        Schema = row["TABLE_SCHEMA"].ToString(),
                        Name = row["TABLE_NAME"].ToString(),
                        Description = ""
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="ViewColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ViewColumn> GetViewColumns(DataTable table, Action<ViewColumn, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ViewColumn
                    {
                        Catalog = row["VIEW_CATALOG"].ToString(),
                        Schema = row["VIEW_SCHEMA"].ToString(),
                        ViewName = row["TABLE_NAME"].ToString(),
                        Name = row["COLUMN_NAME"].ToString(),
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="DataBase"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<DataBase> GetDatabases(DataTable table, Action<DataBase, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new DataBase
                    {
                        Name = row["DATABASE_NAME"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="ReservedWord"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ReservedWord> GetReservedWords(DataTable table, Action<ReservedWord, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ReservedWord
                    {
                        Word = row["ReservedWord"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="Restriction"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Restriction> GetRestrictions(DataTable table, Action<Restriction, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Restriction
                    {
                        CollectionName = row["COLLECTIONNAME"].ToString(),
                        Name = row["RESTRICTIONNAME"].ToString(),
                        Number = row["RESTRICTIONNUMBER"].To<int>()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="ForeignKey"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<ForeignKey> GetForeignKeys(DataTable table, Action<ForeignKey, DataRow> action)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取 <see cref="DataType"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<DataType> GetDataTypes(DataTable table, Action<DataType, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new DataType
                    {
                        Name = row["TypeName"].ToString(),
                        DbType = ((DbType)row["ProviderDbType"]).ToString(),
                        SystemType = Type.GetType(row["DataType"].ToString(), false, true),
                        CreateFormat = row["CreateFormat"].ToString(),
                        CreateParameters = row["CreateParameters"].ToString(),
                        IsAutoincrementable = row["IsAutoincrementable"].To<bool>(),
                        IsBestMatch = row["IsBestMatch"].To<bool>(),
                        IsCaseSensitive = row["IsCaseSensitive"].To<bool>(),
                        IsFixedLength = row["IsFixedLength"].To<bool>(),
                        IsFixedPrecisionScale = row["IsFixedPrecisionScale"].To<bool>(),
                        IsLong = row["IsLong"].To<bool>(),
                        IsSearchable = row["IsSearchable"].To<bool>(),
                        IsSearchableWithLike = row["IsSearchableWithLike"].To<bool>(),
                        IsUnsigned = row["IsUnsigned"].To<bool>(),
                        MaximumScale = row["MaximumScale"].To<int>(),
                        MinimumScale = row["MinimumScale"].To<int>(),
                        IsConcurrencyType = row["IsConcurrencyType"].To<bool>(),
                        IsLiteralSupported = row["IsLiteralSupported"].To<bool>(),
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="Index"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Index> GetIndexs(DataTable table, Action<Index, DataRow> action)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="IndexColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<IndexColumn> GetIndexColumns(DataTable table, Action<IndexColumn, DataRow> action)
        {
            yield break;
        }

        /// <summary>
        /// 获取 <see cref="MetadataCollection"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<MetadataCollection> GetMetadataCollections(DataTable table, Action<MetadataCollection, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new MetadataCollection
                    {
                        CollectionName = row["CollectionName"].ToString(),
                        NumberOfRestrictions = row["NumberOfRestrictions"].To<int>(),
                        NumberOfIdentifierParts = row["NumberOfIdentifierParts"].To<int>()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /*
        /// <summary>
        /// 获取 <see cref="Trigger"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected virtual IEnumerable<Trigger> GetTriggers(DataTable table, Action<Trigger, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Trigger
                    {
                        Catalog = row["TRIGGER_CATALOG"].ToString(),
                        Schema = row["TRIGGER_SCHEMA"].ToString(),
                        Name = row["TRIGGER_NAME"].ToString(),
                        ObjectTable = row["EVENT_OBJECT_TABLE"].ToString()
                    };

                switch (row["EVENT_MANIPULATION"].ToString())
                {
                    case "INSERT":
                        item.Manipulation = TriggerManipulation.Insert;
                        break;
                    case "DELETE":
                        item.Manipulation = TriggerManipulation.Delete;
                        break;
                    case "UPDATE":
                        item.Manipulation = TriggerManipulation.Update;
                        break;
                }

                switch (row["ACTION_TIMING"].ToString())
                {
                    case "AFTER":
                        item.Timing = TriggerTiming.After;
                        break;
                    case "BEFORE":
                        item.Timing = TriggerTiming.Before;
                        break;
                }

                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }
         */

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
            Enum.TryParse<SchemaCategory>(typeof(T).Name, out category);
            return category;
#endif
        }
    }
}
