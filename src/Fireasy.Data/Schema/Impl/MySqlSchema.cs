// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Fireasy.Common.Extensions;
using Fireasy.Data.RecordWrapper;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// MySql相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class MySqlSchema : SchemaBase
    {
        public MySqlSchema()
        {
            AddRestrictionIndex<DataBase>(s => s.Name);
            AddRestrictionIndex<Table>(s => s.Catalog, s => s.Schema, s => s.Name, s => s.Type);
            AddRestrictionIndex<Column>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<View>(s => s.Catalog, s => s.Schema, s => s.Name);
            AddRestrictionIndex<ViewColumn>(s => s.Catalog, s => s.Schema, s => s.ViewName, s => s.Name);
            AddRestrictionIndex<User>(s => s.Name);
            AddRestrictionIndex<Procedure>(s => s.Catalog, s => s.Schema, s => s.Name, s => s.Type);
            AddRestrictionIndex<ProcedureParameter>(s => s.Catalog, s => s.Schema, s => s.ProcedureName, null, s => s.Name);
            AddRestrictionIndex<Index>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<IndexColumn>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name, s => s.ColumnName);
            AddRestrictionIndex<ForeignKey>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            //AddRestrictionIndex<Trigger>(s => s.Catalog, s => s.Schema, s => s.ObjectTable, s => s.Name);
        }

        /// <summary>
        /// 获取架构的名称。
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        protected override string GetSchemaCategoryName(SchemaCategory category)
        {
            switch (category)
            {
                case SchemaCategory.ForeignKey:
                    return "Foreign Keys";
                case SchemaCategory.ProcedureParameter:
                    return "Procedure Parameters";
            }
            return base.GetSchemaCategoryName(category);
        }
        
        /// <summary>
        /// 获取 <see cref="Table"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<Table> GetTables(DataTable table, Action<Table, DataRow> action)
        {
            return base.GetTables(table, (t, r) =>
                {
                    t.Description = r["TABLE_COMMENT"].ToString();
                });
        }

        /// <summary>
        /// 获取 <see cref="Column"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<Column> GetColumns(DataTable table, Action<Column, DataRow> action)
        {
            return base.GetColumns(table, (t, r) =>
                {
                    t.IsPrimaryKey = r["COLUMN_KEY"].ToString() == "PRI";
                    t.Description = r["COLUMN_COMMENT"].ToString();
                });
        }

        /// <summary>
        /// 获取 <see cref="Procedure"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<Procedure> GetProcedures(DataTable table, Action<Procedure, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Procedure
                    {
                        Catalog = row["ROUTINE_CATALOG"].ToString(),
                        Schema = row["ROUTINE_SCHEMA"].ToString(),
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
        /// 获取 <see cref="User"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<User> GetUsers(DataTable table, Action<User, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new User
                {
                    Name = row["USERNAME"].ToString()
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
        protected override IEnumerable<Index> GetIndexs(DataTable table, Action<Index, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Index
                    {
                        Catalog = row["INDEX_CATALOG"].ToString(),
                        Schema = row["INDEX_SCHEMA"].ToString(),
                        Name = row["INDEX_NAME"].ToString(),
                        TableName = row["TABLE_NAME"].ToString().Replace("\"", ""),
                        IsPrimaryKey = row["PRIMARY"].ToString() == "True",
                        IsUnique = row["UNIQUE"].ToString() == "True"
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }
        
        /// <summary>
        /// 获取 <see cref="IndexColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<IndexColumn> GetIndexColumns(DataTable table, Action<IndexColumn, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new IndexColumn
                    {
                        Catalog = row["INDEX_CATALOG"].ToString(),
                        Schema = row["INDEX_SCHEMA"].ToString(),
                        Name = row["INDEX_NAME"].ToString(),
                        TableName = row["TABLE_NAME"].ToString().Replace("\"", ""),
                        ColumnName = row["COLUMN_NAME"].ToString()
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
        protected override IEnumerable<ViewColumn> GetViewColumns(DataTable table, Action<ViewColumn, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ViewColumn
                    {
                        Catalog = row["VIEW_CATALOG"].ToString(),
                        Schema = row["VIEW_SCHEMA"].ToString(),
                        ViewName = row["VIEW_NAME"].ToString(),
                        Name = row["COLUMN_NAME"].ToString(),
                        DataType = row["DATA_TYPE"].ToString(),
                        NumericPrecision = row["NUMERIC_PRECISION"].To<int>(),
                        NumericScale = row["NUMERIC_SCALE"].To<int>(),
                        IsNullable = row["IS_NULLABLE"] != DBNull.Value && row["IS_NULLABLE"].To<bool>(),
                        Size = row["CHARACTER_MAXIMUM_LENGTH"].To<long>(),
                        Position = row["ORDINAL_POSITION"].To<int>()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        protected override IEnumerable<T> GetSchemas<T>(IDatabase database, SchemaCategory category, string[] restrictionValues)
        {
            switch (category)
            {
                case SchemaCategory.ForeignKey:
                    return GetForeignKeys(database, restrictionValues).Cast<T>();
            }

            return base.GetSchemas<T>(database, category, restrictionValues);
        }

        private IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT 
    T.CONSTRAINT_CATALOG, 
    T.CONSTRAINT_SCHEMA,  
    T.CONSTRAINT_NAME,
    T.TABLE_NAME, 
    T.COLUMN_NAME,     
    T.REFERENCED_TABLE_NAME, 
    T.REFERENCED_COLUMN_NAME 
FROM  
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE T
WHERE (T.CONSTRAINT_CATALOG = ?CATALOG OR ?CATALOG IS NULL) AND 
   (T.CONSTRAINT_SCHEMA = ?SCHEMA OR ?SCHEMA IS NULL) AND 
   (T.TABLE_NAME = ?TABLENAME OR ?TABLENAME IS NULL) AND 
   (T.CONSTRAINT_NAME = ?NAME OR ?NAME IS NULL) AND
   REFERENCED_TABLE_NAME IS NOT NULL";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "SCHEMA", 1, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 2, restrictionValues);
            ParameteRestrition(parameters, "NAME", 3, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new ForeignKey
                        {
                            Catalog = wrapper.GetString(reader, 0),
                            Schema = wrapper.GetString(reader, 1),
                            Name = wrapper.GetString(reader, 2),
                            TableName = wrapper.GetString(reader, 3),
                            ColumnName = wrapper.GetString(reader, 4),
                            PKTable = wrapper.GetString(reader, 5),
                            PKColumn = wrapper.GetString(reader, 6),
                        };
                }
            }

        }
    }
}
