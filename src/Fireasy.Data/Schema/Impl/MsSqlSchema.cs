using Fireasy.Data.RecordWrapper;
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

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// MsSql相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class MsSqlSchema : SchemaBase
    {
        public MsSqlSchema()
        {
            AddRestrictionIndex<DataBase>(s => s.Name);
            AddRestrictionIndex<Table>(s => s.Catalog, s => s.Schema, s => s.Name, s => s.Type);
            AddRestrictionIndex<Column>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<View>(s => s.Catalog, s => s.Schema, s => s.Name);
            AddRestrictionIndex<ViewColumn>(s => s.Catalog, s => s.Schema, s => s.ViewName, s => s.Name);
            AddRestrictionIndex<User>(s => s.Name);
            AddRestrictionIndex<Procedure>(s => s.Catalog, s => s.Schema, s => s.Name, s => s.Type);
            AddRestrictionIndex<ProcedureParameter>(s => s.Catalog, s => s.Schema, s => s.ProcedureName, s => s.Name);
            AddRestrictionIndex<Index>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<IndexColumn>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name, s => s.ColumnName);
            AddRestrictionIndex<ForeignKey>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
        }

        protected override IEnumerable<T> GetSchemas<T>(IDatabase database, SchemaCategory category, string[] restrictionValues)
        {
            switch (category)
            {
                case SchemaCategory.Table:
                    return GetTables(database, restrictionValues).Cast<T>();
                case SchemaCategory.View:
                    return GetViews(database, restrictionValues).Cast<T>();
                case SchemaCategory.Column:
                    return GetColumns(database, restrictionValues).Cast<T>();
                case SchemaCategory.ForeignKey:
                    return GetForeignKeys(database, restrictionValues).Cast<T>();
            }

            return base.GetSchemas<T>(database, category, restrictionValues);
        }

        /// <summary>
        /// 获取 <see cref="ProcedureParameter"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<ProcedureParameter> GetProcedureParameters(DataTable table, Action<ProcedureParameter, DataRow> action)
        {
            return base.GetProcedureParameters(table, (t, r) =>
                {
                    if (r["IS_RESULT"].ToString() == "YES")
                    {
                        t.Direction = ParameterDirection.ReturnValue;
                    }
                });
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
                        Catalog = row["CONSTRAINT_CATALOG"].ToString(),
                        Schema = row["CONSTRAINT_SCHEMA"].ToString(),
                        Name = row["INDEX_NAME"].ToString(),
                        TableName = row["TABLE_NAME"].ToString().Replace("\"", "")
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
                        Catalog = row["CONSTRAINT_CATALOG"].ToString(),
                        Schema = row["CONSTRAINT_SCHEMA"].ToString(),
                        Name = row["CONSTRAINT_NAME"].ToString(),
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

        private IEnumerable<Table> GetTables(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.TABLE_CATALOG, 
  T.TABLE_SCHEMA, 
  T.TABLE_NAME, 
  T.TABLE_TYPE,
  (SELECT VALUE FROM ::FN_LISTEXTENDEDPROPERTY('MS_Description','user',T.TABLE_SCHEMA,'table',T.TABLE_NAME,NULL,NULL)) COMMENTS
FROM 
  INFORMATION_SCHEMA.TABLES T
WHERE TABLE_TYPE <> 'view' AND (TABLE_CATALOG = @CATALOG OR (@CATALOG IS NULL)) AND 
  (T.TABLE_SCHEMA = @OWNER OR (@OWNER IS NULL)) AND 
  (T.TABLE_NAME = @NAME OR (@NAME IS NULL)) AND 
  (T.TABLE_TYPE = @TABLETYPE OR (@TABLETYPE IS NULL))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "Catalog", 0, restrictionValues);
            ParameteRestrition(parameters, "Owner", 1, restrictionValues);
            ParameteRestrition(parameters, "Name", 2, restrictionValues);
            ParameteRestrition(parameters, "TableType", 3, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new Table
                        {
                            Catalog = wrapper.GetString(reader, 0),
                            Schema = wrapper.GetString(reader, 1),
                            Name = wrapper.GetString(reader, 2),
                            Type = wrapper.GetString(reader, 3),
                            Description = wrapper.GetString(reader, 4)
                        };
                }
            }
        }

        private IEnumerable<View> GetViews(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME, 
  (SELECT VALUE FROM ::FN_LISTEXTENDEDPROPERTY('MS_Description','user',t.TABLE_SCHEMA,'view',T.TABLE_NAME,NULL,NULL)) COMMENTS
FROM 
  INFORMATION_SCHEMA.TABLES T
WHERE TABLE_TYPE = 'view' AND
  (T.TABLE_CATALOG = @CATALOG OR (@CATALOG IS NULL)) AND 
  (T.TABLE_SCHEMA = @OWNER OR (@OWNER IS NULL)) AND 
  (T.TABLE_NAME = @NAME OR (@NAME IS NULL))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "OWNER", 1, restrictionValues);
            ParameteRestrition(parameters, "NAME", 2, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new View
                        {
                            Catalog = wrapper.GetString(reader, 0),
                            Schema = wrapper.GetString(reader, 1),
                            Name = wrapper.GetString(reader, 2),
                            Description = wrapper.GetString(reader, 3)
                        };
                }
            }
        }

        private IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.TABLE_CATALOG,
       T.TABLE_SCHEMA,
       T.TABLE_NAME,
       T.COLUMN_NAME,
       T.DATA_TYPE AS DATATYPE,
       T.CHARACTER_MAXIMUM_LENGTH AS LENGTH,
       T.NUMERIC_PRECISION AS PRECISION,
       T.NUMERIC_SCALE AS SCALE,
       T.IS_NULLABLE AS NULLABLE,
       (SELECT COUNT(1) FROM SYSCOLUMNS A
            JOIN SYSINDEXKEYS B ON A.ID=B.ID AND A.COLID=B.COLID AND A.ID=OBJECT_ID(T.TABLE_NAME)
            JOIN SYSINDEXES C ON A.ID=C.ID AND B.INDID=C.INDID JOIN SYSOBJECTS D ON C.NAME=D.NAME AND D.XTYPE= 'PK' WHERE A.NAME = T.COLUMN_NAME) COLUMN_IS_PK,
       T.COLUMN_DEFAULT,
       (SELECT VALUE FROM ::FN_LISTEXTENDEDPROPERTY('MS_Description','user',T.TABLE_SCHEMA,'table',T.TABLE_NAME,'column',T.COLUMN_NAME)) COMMENTS,
       (SELECT C.COLSTAT FROM SYSCOLUMNS C
            LEFT JOIN SYSOBJECTS O ON C.ID = O.ID WHERE O.XTYPE='U' AND O.NAME = T.TABLE_NAME AND C.NAME = T.COLUMN_NAME) AUTOINC
  FROM INFORMATION_SCHEMA.COLUMNS T
WHERE (T.TABLE_CATALOG = @CATALOG OR (@CATALOG IS NULL)) AND 
  (T.TABLE_SCHEMA = @OWNER OR (@OWNER IS NULL)) AND 
  (T.TABLE_NAME = @TABLENAME OR (@TABLENAME IS NULL)) AND 
  (T.COLUMN_NAME = @COLUMNNAME OR (@COLUMNNAME IS NULL))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "OWNER", 1, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 2, restrictionValues);
            ParameteRestrition(parameters, "COLUMNNAME", 3, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new Column
                        {
                            Catalog = wrapper.GetString(reader, 0),
                            Schema = wrapper.GetString(reader, 1),
                            TableName = wrapper.GetString(reader, 2),
                            Name = wrapper.GetString(reader, 3),
                            DataType = wrapper.GetString(reader, 4),
                            Length = wrapper.GetInt32(reader, 5),
                            NumericPrecision = wrapper.GetInt32(reader, 6),
                            NumericScale = wrapper.GetInt32(reader, 7),
                            IsNullable = wrapper.GetString(reader, 8) == "YES",
                            IsPrimaryKey = wrapper.GetInt32(reader, 9) > 0,
                            Default = wrapper.GetString(reader, 10),
                            Description = wrapper.GetString(reader, 11),
                            Autoincrement = wrapper.GetInt32(reader, 12) == 1
                        };
                }
            }
        }

        private IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT
  TC.CONSTRAINT_CATALOG, 
  TC.CONSTRAINT_SCHEMA, 
  TC.CONSTRAINT_NAME, 
  TC.TABLE_NAME,
  C.COLUMN_NAME,
  FKCU.TABLE_NAME REFERENCED_TABLE_NAME, 
  FKCU.COLUMN_NAME AS REFERENCED_COLUMN_NAME
FROM [INFORMATION_SCHEMA].[COLUMNS] C
LEFT JOIN [INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] KCU
  ON KCU.TABLE_SCHEMA = C.TABLE_SCHEMA
  AND KCU.TABLE_NAME = C.TABLE_NAME
  AND KCU.COLUMN_NAME = C.COLUMN_NAME
LEFT JOIN [INFORMATION_SCHEMA].[TABLE_CONSTRAINTS] TC
  ON TC.CONSTRAINT_SCHEMA = KCU.CONSTRAINT_SCHEMA
AND TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
LEFT JOIN [INFORMATION_SCHEMA].[REFERENTIAL_CONSTRAINTS] FC
  ON KCU.CONSTRAINT_SCHEMA = FC.CONSTRAINT_SCHEMA
AND KCU.CONSTRAINT_NAME = FC.CONSTRAINT_NAME
LEFT JOIN [INFORMATION_SCHEMA].[KEY_COLUMN_USAGE] FKCU
  ON FKCU.CONSTRAINT_SCHEMA = FC.UNIQUE_CONSTRAINT_SCHEMA
  AND FKCU.CONSTRAINT_NAME = FC.UNIQUE_CONSTRAINT_NAME
WHERE TC.CONSTRAINT_TYPE = 'FOREIGN KEY' AND
   (TC.CONSTRAINT_CATALOG = @CATALOG OR @CATALOG IS NULL) AND 
   (TC.CONSTRAINT_SCHEMA = @SCHEMA OR @SCHEMA IS NULL) AND 
   (TC.TABLE_NAME = @TABLENAME OR @TABLENAME IS NULL) AND 
   (TC.CONSTRAINT_NAME = @NAME OR @NAME IS NULL)";

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
