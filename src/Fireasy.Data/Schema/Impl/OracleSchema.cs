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

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// Oracle相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class OracleSchema : SchemaBase
    {
        public OracleSchema()
        {
            AddRestriction<Database>(s => s.Name);
            AddRestriction<Table>(s => s.Name, s => s.Type);
            AddRestriction<Column>(s => s.TableName, s => s.Name);
            AddRestriction<View>(s => s.Name);
            AddRestriction<ViewColumn>(s => s.ViewName, s => s.Name);
            AddRestriction<User>(s => s.Name);
            AddRestriction<Procedure>(s => s.Name);
            AddRestriction<ProcedureParameter>(s => s.ProcedureName);
            AddRestriction<Index>(s => s.TableName, s => s.Name);
            AddRestriction<IndexColumn>(s => s.TableName, s => s.IndexName, s => s.ColumnName);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);

            AddDataType("long", DbType.Int64, typeof(long));
            AddDataType("interval year to month", DbType.Int64, typeof(long));
            AddDataType("float", DbType.Single, typeof(float));
            AddDataType("binary_float", DbType.Single, typeof(float));
            AddDataType("binary_double", DbType.Double, typeof(double));
            AddDataType("number", DbType.Decimal, typeof(decimal));
            AddDataType("bfile", DbType.Binary, typeof(byte[]));
            AddDataType("blob", DbType.Binary, typeof(byte[]));
            AddDataType("raw", DbType.Binary, typeof(byte[]));
            AddDataType("long raw", DbType.Binary, typeof(byte[]));
            AddDataType("char", DbType.String, typeof(string));
            AddDataType("nchar", DbType.String, typeof(string));
            AddDataType("varchar2", DbType.String, typeof(string));
            AddDataType("nvarchar2", DbType.String, typeof(string));
            AddDataType("clob", DbType.String, typeof(string));
            AddDataType("nclob", DbType.String, typeof(string));
            AddDataType("xmltype", DbType.String, typeof(string));
            AddDataType("rowid", DbType.String, typeof(string));
            AddDataType("date", DbType.Date, typeof(DateTime));
            AddDataType("timestamp with time zone", DbType.DateTime, typeof(DateTime));
            AddDataType("timestamp with local time zone", DbType.DateTime, typeof(DateTime));
            AddDataType("timestamp", DbType.DateTime, typeof(DateTime));
            AddDataType("interval day to second", DbType.Int64, typeof(TimeSpan));
        }

        protected override IEnumerable<Database> GetDatabases(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT NAME, CREATED FROM V$DATABASE T
WHERE (T.NAME = :NAME OR (:NAME IS NULL))";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Database.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Database
            {
                Name = wrapper.GetString(reader, 0),
                CreateDate = wrapper.GetDateTime(reader, 1)
            });
        }

        protected override IEnumerable<User> GetUsers(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT USERNAME FROM USER_USERS T
WHERE (T.USERNAME = :USERNAME OR (:USERNAME IS NULL))";

            restrictionValues
                .Parameterize(parameters, "USERNAME", nameof(User.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new User
            {
                Name = wrapper.GetString(reader, 0)
            });
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT * FROM (
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
) T
 WHERE (T.OWNER = '{connpar.UserId.ToUpper()}') AND 
  (T.TABLE_NAME = :TABLENAME OR (:TABLENAME IS NULL)) AND
  ((T.TYPE = 'USER' AND (:TABLETYPE IS NULL OR :TABLETYPE = 0)) OR (T.TYPE = 'SYSTEM' AND :TABLETYPE = 1))
  ORDER BY OWNER, TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Table.Name))
                .Parameterize(parameters, "TABLETYPE", nameof(Table.Type));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
            {
                Schema = wrapper.GetString(reader, 0),
                Name = wrapper.GetString(reader, 1),
                Type = wrapper.GetString(reader, 2) == "USER" ? TableType.BaseTable : TableType.SystemTable,
                Description = wrapper.GetString(reader, 3)
            });
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
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
  JOIN ALL_TABLES V
    ON T.OWNER = V.OWNER
   AND T.TABLE_NAME = V.TABLE_NAME
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
 WHERE (T.OWNER = '{connpar.UserId.ToUpper()}') AND 
   (T.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL) AND 
   (T.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
 ORDER BY T.OWNER, T.TABLE_NAME, T.COLUMN_ID";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => SetColumnType(new Column
            {
                Schema = wrapper.GetString(reader, 0),
                TableName = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                DataType = wrapper.GetString(reader, 3),
                Length = reader.IsDBNull(4) ? (long?)null : wrapper.GetInt32(reader, 4),
                NumericPrecision = reader.IsDBNull(5) ? (int?)null : wrapper.GetInt32(reader, 5),
                NumericScale = reader.IsDBNull(6) ? (int?)null : wrapper.GetInt32(reader, 6),
                IsNullable = wrapper.GetString(reader, 7) == "Y",
                IsPrimaryKey = wrapper.GetString(reader, 8) == "Y",
                Default = wrapper.GetString(reader, 9),
                Description = wrapper.GetString(reader, 10),
            }));
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
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
   and (FKCON.OWNER = '{connpar.UserId.ToUpper()}')
   AND (FKCON.TABLE_NAME = :TABLENAME OR :TABLENAME is null) AND (FKCON.CONSTRAINT_NAME = :CONSTRAINTNAME OR :CONSTRAINTNAME is null)";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(ForeignKey.TableName))
                .Parameterize(parameters, "CONSTRAINTNAME", nameof(ForeignKey.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
            {
                Schema = reader["FOREIGN_KEY_OWNER"].ToString(),
                Name = reader["FOREIGN_KEY_CONSTRAINT_NAME"].ToString(),
                TableName = reader["FOREIGN_KEY_TABLE_NAME"].ToString().Replace("\"", ""),
                ColumnName = reader["FOREIGN_KEY_COLUMN_NAME"].ToString(),
                PKTable = reader["PRIMARY_KEY_TABLE_NAME"].ToString(),
                PKColumn = reader["PRIMARY_KEY_COLUMN_NAME"].ToString()
            });
        }

        protected override IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT * FROM (
    SELECT T.OWNER,
       T.VIEW_NAME,
       C.COMMENTS
  FROM ALL_VIEWS T
  JOIN ALL_TAB_COMMENTS C
    ON T.OWNER = C.OWNER
   AND T.VIEW_NAME = C.TABLE_NAME
) T
 WHERE (T.OWNER = '{connpar.UserId.ToUpper()}') AND 
  (T.VIEW_NAME = :VIEWNAME OR (:VIEWNAME IS NULL))
  ORDER BY OWNER, VIEW_NAME";

            restrictionValues
                .Parameterize(parameters, "VIEWNAME", nameof(Table.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new View
            {
                Schema = wrapper.GetString(reader, 0),
                Name = wrapper.GetString(reader, 1),
                Description = wrapper.GetString(reader, 2)
            });
        }

        protected override IEnumerable<ViewColumn> GetViewColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT T.OWNER,
       T.TABLE_NAME,
       T.COLUMN_NAME,
       T.DATA_TYPE AS DATATYPE,
       T.DATA_LENGTH AS LENGTH,
       T.DATA_PRECISION AS PRECISION,
       T.DATA_SCALE AS SCALE,
       T.NULLABLE AS NULLABLE,
       C.COMMENTS
  FROM ALL_TAB_COLUMNS T
  JOIN ALL_VIEWS V
    ON T.OWNER = V.OWNER
   AND T.TABLE_NAME = V.VIEW_NAME
  LEFT JOIN ALL_COL_COMMENTS C
    ON T.OWNER = C.OWNER
   AND T.TABLE_NAME = C.TABLE_NAME
   AND T.COLUMN_NAME = C.COLUMN_NAME
  LEFT JOIN ALL_TAB_COLUMNS D
    ON T.OWNER = D.OWNER
   AND T.TABLE_NAME = D.TABLE_NAME
   AND T.COLUMN_NAME = D.COLUMN_NAME
 WHERE (T.OWNER = '{connpar.UserId.ToUpper()}') AND 
   (T.TABLE_NAME = :VIEWNAME OR :VIEWNAME IS NULL) AND 
   (T.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
 ORDER BY T.OWNER, T.TABLE_NAME, T.COLUMN_ID";

            restrictionValues
                .Parameterize(parameters, "VIEWNAME", nameof(ViewColumn.ViewName))
                .Parameterize(parameters, "COLUMNNAME", nameof(ViewColumn.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ViewColumn
            {
                Schema = wrapper.GetString(reader, 0),
                ViewName = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                DataType = wrapper.GetString(reader, 3),
                Length = reader.IsDBNull(4) ? (long?)null : wrapper.GetInt32(reader, 4),
                NumericPrecision = reader.IsDBNull(5) ? (int?)null : wrapper.GetInt32(reader, 5),
                NumericScale = reader.IsDBNull(6) ? (int?)null : wrapper.GetInt32(reader, 6),
                IsNullable = wrapper.GetString(reader, 7) == "Y",
                Description = wrapper.GetString(reader, 8),
            });
        }

        protected override IEnumerable<Index> GetIndexs(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT T.OWNER,
       T.INDEX_NAME, 
       T.TABLE_NAME,
       T.UNIQUENESS
 FROM ALL_INDEXES T
 WHERE (T.OWNER = '{connpar.UserId.ToUpper()}') AND 
   (T.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL) AND 
   (T.INDEX_NAME = :INDEXNAME OR :INDEXNAME IS NULL)
 ORDER BY T.OWNER, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Index.TableName))
                .Parameterize(parameters, "INDEXNAME", nameof(Index.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Index
            {
                Schema = wrapper.GetString(reader, 0),
                Name = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 2),
                IsUnique = wrapper.GetString(reader, 3) == "UNIQUE"
            });
        }

        protected override IEnumerable<IndexColumn> GetIndexColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT T.INDEX_OWNER,
       T.INDEX_NAME, 
       T.TABLE_NAME,
       T.COLUMN_NAME
 FROM ALL_IND_COLUMNS T
 WHERE (T.INDEX_OWNER = '{connpar.UserId.ToUpper()}') AND 
   (T.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL) AND 
   (T.INDEX_NAME = :INDEXNAME OR :INDEXNAME IS NULL) AND
   (T.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL) 
 ORDER BY T.INDEX_OWNER, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(IndexColumn.TableName))
                .Parameterize(parameters, "INDEXNAME", nameof(IndexColumn.IndexName))
                .Parameterize(parameters, "COLUMNNAME", nameof(IndexColumn.ColumnName));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new IndexColumn
            {
                Schema = wrapper.GetString(reader, 0),
                TableName = wrapper.GetString(reader, 1),
                IndexName = wrapper.GetString(reader, 2),
                ColumnName = wrapper.GetString(reader, 3)
            });
        }
    }
}
