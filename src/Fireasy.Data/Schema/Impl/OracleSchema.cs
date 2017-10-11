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
using Fireasy.Common.Extensions;
using System.Data.Common;
using System.Linq;
using Fireasy.Data.RecordWrapper;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// Oracle相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class OracleSchema : SchemaBase
    {
        public OracleSchema()
        {
            AddRestrictionIndex<DataBase>(s => s.Name);
            AddRestrictionIndex<Table>(s => s.Schema, s => s.Name);
            AddRestrictionIndex<Column>(s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<View>(s => s.Schema, s => s.Name);
            AddRestrictionIndex<User>(s => s.Name);
            AddRestrictionIndex<Procedure>(s => s.Schema, s => s.Name);
            AddRestrictionIndex<ProcedureParameter>(s => s.Schema, s => s.ProcedureName);
            AddRestrictionIndex<Index>(s => s.Schema, s => s.Name, null, s => s.TableName);
            AddRestrictionIndex<IndexColumn>(s => s.Schema, s => s.Name, null, s => s.TableName, s => s.ColumnName);
            AddRestrictionIndex<ForeignKey>(s => s.Schema, s => s.TableName, s => s.Name);
        }

        protected override IEnumerable<T> GetSchemas<T>(IDatabase database, SchemaCategory category, string[] restrictionValues)
        {
            switch (category)
            {
                case SchemaCategory.Table:
                    return GetTables(database, restrictionValues).Cast<T>();
                case SchemaCategory.Column:
                    return GetColumns(database, restrictionValues).Cast<T>();
                case SchemaCategory.ForeignKey:
                    return GetForeignKeys(database, restrictionValues).Cast<T>();
            }

            return base.GetSchemas<T>(database, category, restrictionValues);
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
                        Name = row["NAME"].ToString()
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
        protected override IEnumerable<View> GetViews(DataTable table, Action<View, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new View
                    {
                        Schema = row["OWNER"].ToString(),
                        Name = row["VIEW_NAME"].ToString(),
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
        protected override IEnumerable<Procedure> GetProcedures(DataTable table, Action<Procedure, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Procedure
                    {
                        Schema = row["OWNER"].ToString(),
                        Name = row["OBJECT_NAME"].ToString()
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
        protected override IEnumerable<ProcedureParameter> GetProcedureParameters(DataTable table, Action<ProcedureParameter, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ProcedureParameter
                    {
                        Schema = row["OWNER"].ToString(),
                        ProcedureName = row["OBJECT_NAME"].ToString(),
                        Name = row["ARGUMENT_NAME"].ToString(),
                        DataType = row["DATA_TYPE"].ToString(),
                        Direction = GetDirection(row["IN_OUT"].ToString()),
                        NumericPrecision = row["DATA_PRECISION"].To<int>(),
                        NumericScale = row["DATA_SCALE"].To<int>(),
                        Length = row["DATA_LENGTH"].To<int>(),
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
                        Schema = row["OWNER"].ToString(),
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
                        Schema = row["INDEX_OWNER"].ToString(),
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

        private ParameterDirection GetDirection(string direction)
        {
            switch (direction)
            {
                case "IN": return ParameterDirection.Input;
                case "OUT": return ParameterDirection.Output;
                case "IN/OUT": return ParameterDirection.InputOutput;
                default:
                    throw new NotImplementedException();
            }
        }

        private IEnumerable<Table> GetTables(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.OWNER,
       T.TABLE_NAME,
       DECODE(T.OWNER,
              'SYS',
              'SYSTEM',
              'SYSTEM',
              'SYSTEM',
              'SYSMAN',
              'SYSTEM',
              'CTXSYS',
              'SYSTEM',
              'MDSYS',
              'SYSTEM',
              'OLAPSYS',
              'SYSTEM',
              'ORDSYS',
              'SYSTEM',
              'OUTLN',
              'SYSTEM',
              'WKSYS',
              'SYSTEM',
              'WMSYS',
              'SYSTEM',
              'XDB',
              'SYSTEM',
              'ORDPLUGINS',
              'SYSTEM',
              'USER') AS TYPE,
       C.COMMENTS
  FROM ALL_TABLES T
  JOIN ALL_TAB_COMMENTS C
    ON T.OWNER = C.OWNER
   AND T.TABLE_NAME = C.TABLE_NAME
 WHERE (T.OWNER = :OWNER OR :OWNER IS NULL) AND 
   (T.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL)
  ORDER BY OWNER, TABLE_NAME";

            ParameteRestrition(parameters, "OWNER", 0, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 1, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new Table
                        {
                            Schema = wrapper.GetString(reader, 0),
                            Name = wrapper.GetString(reader, 1),
                            Type = wrapper.GetString(reader, 2),
                            Description = wrapper.GetString(reader, 3)
                        };
                }
            }
        }

        private IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.OWNER,
       T.TABLE_NAME,
       T.COLUMN_NAME,
       T.DATA_TYPE AS DATATYPE,
       T.DATA_LENGTH AS LENGTH,
       T.DATA_PRECISION AS PRECISION,
       T.DATA_SCALE AS SCALE,
       T.NULLABLE AS NULLABLE,
       (CASE
         WHEN P.OWNER IS NULL THEN
          'N'
         ELSE
          'Y'
       END) PK,
       D.DATA_DEFAULT,
       C.COMMENTS
  FROM ALL_TAB_COLUMNS T
  LEFT JOIN ALL_COL_COMMENTS C
    ON T.OWNER = C.OWNER
   AND T.TABLE_NAME = C.TABLE_NAME
   AND T.COLUMN_NAME = C.COLUMN_NAME
  LEFT JOIN ALL_TAB_COLUMNS D
    ON T.OWNER = D.OWNER
   AND T.TABLE_NAME = D.TABLE_NAME
   AND T.COLUMN_NAME = D.COLUMN_NAME
  LEFT JOIN (SELECT AU.OWNER, AU.TABLE_NAME, CU.COLUMN_NAME
               FROM ALL_CONS_COLUMNS CU, ALL_CONSTRAINTS AU
              WHERE CU.OWNER = AU.OWNER
                AND CU.CONSTRAINT_NAME = AU.CONSTRAINT_NAME
                AND AU.CONSTRAINT_TYPE = 'P') P
    ON T.OWNER = P.OWNER
   AND T.TABLE_NAME =P.TABLE_NAME
   AND T.COLUMN_NAME = P.COLUMN_NAME
 WHERE (T.OWNER = :OWNER OR :OWNER IS NULL) AND 
   (T.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL) AND 
   (T.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
 ORDER BY T.OWNER, T.TABLE_NAME, T.COLUMN_ID";

            ParameteRestrition(parameters, "OWNER", 0, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 1, restrictionValues);
            ParameteRestrition(parameters, "COLUMNNAME", 2, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new Column
                        {
                            Schema = wrapper.GetString(reader, 0),
                            TableName = wrapper.GetString(reader, 1),
                            Name = wrapper.GetString(reader, 2),
                            DataType = wrapper.GetString(reader, 3),
                            Length = wrapper.GetInt32(reader, 4),
                            NumericPrecision = wrapper.GetInt32(reader, 5),
                            NumericScale = wrapper.GetInt32(reader, 6),
                            IsNullable = wrapper.GetString(reader, 7) == "Y",
                            IsPrimaryKey = wrapper.GetString(reader, 8) == "Y",
                            Default = wrapper.GetString(reader, 9),
                            Description = wrapper.GetString(reader, 10),
                        };
                }
            }
        }

        private IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT PKCON.CONSTRAINT_NAME AS PRIMARY_KEY_CONSTRAINT_NAME,
       PKCON.OWNER AS PRIMARY_KEY_OWNER,
       PKCON.TABLE_NAME AS PRIMARY_KEY_TABLE_NAME,
       FKCON.OWNER AS FOREIGN_KEY_OWNER,
       FKCON.CONSTRAINT_NAME AS FOREIGN_KEY_CONSTRAINT_NAME,
       FKCON.TABLE_NAME AS FOREIGN_KEY_TABLE_NAME,
       FKCON.SEARCH_CONDITION,
       FKCON.R_OWNER,
       FKCON.R_CONSTRAINT_NAME,
       FKCON.DELETE_RULE,
       FKCON.STATUS,
       (SELECT cu.COLUMN_NAME
          FROM ALL_CONS_COLUMNS CU, ALL_CONSTRAINTS AU
         WHERE CU.OWNER = AU.OWNER
           AND CU.CONSTRAINT_NAME = AU.CONSTRAINT_NAME
           AND AU.CONSTRAINT_TYPE = 'P'
           and au.constraint_name = FKCON.r_constraint_name
           and FKCON.owner = au.OWNER
           and rownum = 1) PRIMARY_KEY_COLUMN_NAME,
       (SELECT cu.COLUMN_NAME
          FROM ALL_CONS_COLUMNS CU, ALL_CONSTRAINTS AU
         WHERE CU.OWNER = AU.OWNER
           AND CU.CONSTRAINT_NAME = AU.CONSTRAINT_NAME
           AND AU.CONSTRAINT_TYPE = 'R'
           and au.constraint_name = FKCON.CONSTRAINT_NAME
           and PKCON.owner = au.OWNER
           and rownum = 1) FOREIGN_KEY_COLUMN_NAME
  FROM ALL_CONSTRAINTS FKCON, ALL_CONSTRAINTS PKCON
 WHERE PKCON.OWNER = FKCON.R_OWNER
   AND PKCON.CONSTRAINT_NAME = FKCON.R_CONSTRAINT_NAME
   AND FKCON.CONSTRAINT_TYPE = 'R'
   and (FKCON.OWNER = :OWNER OR :OWNER is null)
   AND (FKCON.TABLE_NAME = :TABLENAME OR :TABLENAME is null) AND (FKCON.CONSTRAINT_NAME = :CONSTRAINTNAME OR :CONSTRAINTNAME is null)";

            ParameteRestrition(parameters, "OWNER", 0, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 1, restrictionValues);
            ParameteRestrition(parameters, "CONSTRAINTNAME", 2, restrictionValues);

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new ForeignKey
                        {
                            Schema = reader["FOREIGN_KEY_OWNER"].ToString(),
                            Name = reader["FOREIGN_KEY_CONSTRAINT_NAME"].ToString(),
                            TableName = reader["FOREIGN_KEY_TABLE_NAME"].ToString().Replace("\"", ""),
                            ColumnName = reader["FOREIGN_KEY_COLUMN_NAME"].ToString(),
                            PKTable = reader["PRIMARY_KEY_TABLE_NAME"].ToString(),
                            PKColumn = reader["PRIMARY_KEY_COLUMN_NAME"].ToString()
                        };
                }
            }
        }
    }
}
