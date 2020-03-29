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
    /// MsSql相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class MsSqlSchema : SchemaBase
    {
        public MsSqlSchema()
        {
            AddRestriction<Database>(s => s.Name);
            AddRestriction<Table>(s => s.Name, s => s.Type);
            AddRestriction<Column>(s => s.TableName, s => s.Name);
            AddRestriction<View>(s => s.Name);
            AddRestriction<ViewColumn>(s => s.ViewName, s => s.Name);
            AddRestriction<User>(s => s.Name);
            AddRestriction<Procedure>(s => s.Name, s => s.Type);
            AddRestriction<ProcedureParameter>(s => s.ProcedureName, s => s.Name);
            AddRestriction<Index>(s => s.TableName, s => s.Name);
            AddRestriction<IndexColumn>(s => s.TableName, s => s.Name, s => s.ColumnName);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);

            AddDataType("bit", DbType.Boolean, typeof(bool));
            AddDataType("smallint", DbType.Int16, typeof(short));
            AddDataType("tinyint", DbType.SByte, typeof(sbyte));
            AddDataType("int", DbType.Int32, typeof(int));
            AddDataType("bigint", DbType.Int64, typeof(long));
            AddDataType("real", DbType.Single, typeof(float));
            AddDataType("float", DbType.Single, typeof(float));
            AddDataType("money", DbType.Decimal, typeof(decimal));
            AddDataType("smallmoney", DbType.Decimal, typeof(decimal));
            AddDataType("varbinary", DbType.Binary, typeof(byte[]));
            AddDataType("timestamp", DbType.Binary, typeof(byte[]));
            AddDataType("binary", DbType.Binary, typeof(byte[]));
            AddDataType("image", DbType.Binary, typeof(byte[]));
            AddDataType("char", DbType.String, typeof(string));
            AddDataType("nchar", DbType.String, typeof(string));
            AddDataType("varchar", DbType.String, typeof(string));
            AddDataType("nvarchar", DbType.String, typeof(string));
            AddDataType("text", DbType.String, typeof(string));
            AddDataType("ntext", DbType.String, typeof(string));
            AddDataType("xml", DbType.Xml, typeof(string));
            AddDataType("decimal", DbType.Decimal, typeof(decimal));
            AddDataType("numeric", DbType.Decimal, typeof(decimal));
            AddDataType("uniqueidentifier", DbType.Guid, typeof(Guid));
            AddDataType("datetime", DbType.DateTime, typeof(DateTime));
            AddDataType("smalldatetime", DbType.DateTime, typeof(DateTime));
            AddDataType("date", DbType.Date, typeof(DateTime));
            AddDataType("time", DbType.Time, typeof(DateTime));
            AddDataType("datetime2", DbType.DateTime2, typeof(DateTime));
            AddDataType("datetimeoffset", DbType.Int64, typeof(DateTimeOffset));
        }

        protected override IEnumerable<Database> GetDatabases(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT NAME AS DATABASE_NAME, CRDATE AS CREATE_DATE FROM MASTER..SYSDATABASES WHERE (NAME = @NAME OR (@NAME IS NULL))";

            restrictionValues.Parameterize(parameters, "NAME", nameof(Database.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Database
            {
                Name = wrapper.GetString(reader, 0),
                CreateDate = wrapper.GetDateTime(reader, 1)
            });
        }

        protected override IEnumerable<User> GetUsers(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT UID, NAME AS USER_NAME, CREATEDATE, UPDATEDATE FROM SYSUSERS WHERE (NAME = @NAME OR (@NAME IS NULL))";

            restrictionValues.Parameterize(parameters, "NAME", nameof(User.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new User
            {
                Name = wrapper.GetString(reader, 1),
                CreateDate = wrapper.GetDateTime(reader, 2)
            });
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
SELECT T.TABLE_CATALOG, 
  T.TABLE_SCHEMA, 
  T.TABLE_NAME, 
  T.TABLE_TYPE,
  (SELECT VALUE FROM ::FN_LISTEXTENDEDPROPERTY('MS_Description','user',T.TABLE_SCHEMA,'table',T.TABLE_NAME,NULL,NULL)) COMMENTS
FROM 
  INFORMATION_SCHEMA.TABLES T
WHERE TABLE_TYPE <> 'view'
  AND (T.TABLE_CATALOG = '{connpar.Database}')
  AND (T.TABLE_NAME = @NAME OR (@NAME IS NULL))
  AND ((T.TABLE_TYPE = 'BASE TABLE' AND (@TABLETYPE IS NULL OR @TABLETYPE = 0)) OR (T.TABLE_TYPE = 'SYSTEM TABLE' AND @TABLETYPE = 1))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Table.Name))
                .Parameterize(parameters, "TABLETYPE", nameof(Table.Type));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                Type = wrapper.GetString(reader, 3) == "BASE TABLE" ? TableType.BaseTable : TableType.SystemTable,
                Description = wrapper.GetString(reader, 4)
            });
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
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
  JOIN INFORMATION_SCHEMA.TABLES O
    ON O.TABLE_CATALOG = T.TABLE_CATALOG AND O.TABLE_SCHEMA = T.TABLE_SCHEMA AND T.TABLE_NAME = O.TABLE_NAME
WHERE (O.TABLE_TYPE <> 'view' AND T.TABLE_CATALOG = '{connpar.Database}') AND
  (T.TABLE_NAME = @TABLENAME OR (@TABLENAME IS NULL)) AND 
  (T.COLUMN_NAME = @COLUMNNAME OR (@COLUMNNAME IS NULL))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 3),
                DataType = wrapper.GetString(reader, 4),
                Length = wrapper.GetInt64(reader, 5),
                NumericPrecision = wrapper.GetInt32(reader, 6),
                NumericScale = wrapper.GetInt32(reader, 7),
                IsNullable = wrapper.GetString(reader, 8) == "YES",
                IsPrimaryKey = wrapper.GetInt32(reader, 9) > 0,
                Default = wrapper.GetString(reader, 10),
                Description = wrapper.GetString(reader, 11),
                Autoincrement = wrapper.GetInt32(reader, 12) == 1
            });
        }

        protected override IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
SELECT T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME, 
  (SELECT VALUE FROM ::FN_LISTEXTENDEDPROPERTY('MS_Description','user',t.TABLE_SCHEMA,'view',T.TABLE_NAME,NULL,NULL)) COMMENTS
FROM 
  INFORMATION_SCHEMA.TABLES T
WHERE TABLE_TYPE = 'view'
  AND (T.TABLE_CATALOG = '{connpar.Database}')
  AND (T.TABLE_NAME = @NAME OR (@NAME IS NULL))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues.Parameterize(parameters, "NAME", nameof(View.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new View
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                Description = wrapper.GetString(reader, 3)
            });
        }

        protected override IEnumerable<ViewColumn> GetViewColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
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
       0 AUTOINC
  FROM INFORMATION_SCHEMA.COLUMNS T
  JOIN INFORMATION_SCHEMA.TABLES O
    ON O.TABLE_CATALOG = T.TABLE_CATALOG AND O.TABLE_SCHEMA = T.TABLE_SCHEMA AND T.TABLE_NAME = O.TABLE_NAME
WHERE (O.TABLE_TYPE = 'view' AND T.TABLE_CATALOG = '{connpar.Database}') AND
  (T.TABLE_NAME = @TABLENAME OR (@TABLENAME IS NULL)) AND 
  (T.COLUMN_NAME = @COLUMNNAME OR (@COLUMNNAME IS NULL))
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(ViewColumn.ViewName))
                .Parameterize(parameters, "COLUMNNAME", nameof(ViewColumn.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ViewColumn
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                ViewName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 3),
                DataType = wrapper.GetString(reader, 4),
                Length = wrapper.GetInt64(reader, 5),
                NumericPrecision = wrapper.GetInt32(reader, 6),
                NumericScale = wrapper.GetInt32(reader, 7),
                IsNullable = wrapper.GetString(reader, 8) == "YES",
                IsPrimaryKey = wrapper.GetInt32(reader, 9) > 0,
                Default = wrapper.GetString(reader, 10),
                Description = wrapper.GetString(reader, 11),
                Autoincrement = wrapper.GetInt32(reader, 12) == 1
            });
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
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
   (TC.CONSTRAINT_CATALOG = '{connpar.Database}') AND 
   (TC.TABLE_NAME = @TABLENAME OR @TABLENAME IS NULL) AND 
   (TC.CONSTRAINT_NAME = @NAME OR @NAME IS NULL)";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(ForeignKey.TableName))
                .Parameterize(parameters, "NAME", nameof(ForeignKey.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                TableName = wrapper.GetString(reader, 3),
                ColumnName = wrapper.GetString(reader, 4),
                PKTable = wrapper.GetString(reader, 5),
                PKColumn = wrapper.GetString(reader, 6),
            });
        }

        protected override IEnumerable<Index> GetIndexs(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
SELECT DISTINCT
  DB_NAME() AS CONSTRAINT_CATALOG,
  CONSTRAINT_SCHEMA = USER_NAME(O.UID),
  CONSTRAINT_NAME = X.NAME,
  TABLE_CATALOG  = DB_NAME(),
  TABLE_SCHEMA = USER_NAME(O.UID),
  TABLE_NAME = O.NAME,
  INDEX_NAME = X.NAME
FROM SYSOBJECTS O, SYSINDEXES X, SYSINDEXKEYS XK
WHERE O.TYPE IN ('U') AND X.ID = O.ID  AND O.ID = XK.ID AND X.INDID = XK.INDID AND XK.KEYNO &LT; = X.KEYCNT AND
 (DB_NAME() = '{connpar.Database}') AND
 (O.NAME = @TABLE OR (@TABLE IS NULL)) AND
 (X.NAME = @NAME OR (@NAME IS NULL)) ORDER BY TABLE_NAME, INDEX_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLE", nameof(Index.TableName))
                .Parameterize(parameters, "NAME", nameof(Index.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Index
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 5),
                Name = wrapper.GetString(reader, 6)
            });
        }

        protected override IEnumerable<IndexColumn> GetIndexColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
SELECT DISTINCT
  DB_NAME() AS CONSTRAINT_CATALOG,
  CONSTRAINT_SCHEMA = USER_NAME(O.UID),
  CONSTRAINT_NAME = X.NAME,
  TABLE_CATALOG  = DB_NAME(),
  TABLE_SCHEMA = USER_NAME(O.UID),
  TABLE_NAME = O.NAME,
  COLUMN_NAME = C.NAME,
  ORDINAL_POSITION = CONVERT(INT, XK.KEYNO),
  KEYTYPE = C.XTYPE, 
  INDEX_NAME = X.NAME
FROM SYSOBJECTS O, SYSINDEXES X, SYSCOLUMNS C, SYSINDEXKEYS XK
WHERE O.TYPE IN ('U') AND X.ID = O.ID  AND O.ID = C.ID AND O.ID = XK.ID AND X.INDID = XK.INDID AND C.COLID = XK.COLID AND XK.KEYNO <= X.KEYCNT AND PERMISSIONS(O.ID, C.NAME) <> 0 
  AND (DB_NAME() = '{connpar.Database}')
  AND (USER_NAME()= @OWNER OR (@OWNER IS NULL))
  AND (X.NAME = @CONSTRAINTNAME OR (@CONSTRAINTNAME IS NULL))
  AND (C.NAME = @COLUMN OR (@COLUMN IS NULL))
ORDER BY TABLE_NAME, INDEX_NAME";

            restrictionValues
                .Parameterize(parameters, "TABLE", nameof(IndexColumn.TableName))
                .Parameterize(parameters, "CONSTRAINTNAME", nameof(IndexColumn.IndexName))
                .Parameterize(parameters, "COLUMN", nameof(IndexColumn.ColumnName));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new IndexColumn
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 5),
                IndexName = wrapper.GetString(reader, 2),
                ColumnName = wrapper.GetString(reader, 6)
            });
        }

        protected override IEnumerable<Procedure> GetProcedures(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
SELECT
  SPECIFIC_CATALOG,
  SPECIFIC_SCHEMA,
  SPECIFIC_NAME,
  ROUTINE_CATALOG,
  ROUTINE_SCHEMA,
  ROUTINE_NAME,
  ROUTINE_TYPE,
  CREATED,
  LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE (SPECIFIC_CATALOG = '{connpar.Database}')
  AND (SPECIFIC_NAME = @NAME OR (@NAME IS NULL))
  AND (ROUTINE_TYPE = @TYPE OR (@TYPE IS NULL))
ORDER BY SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Procedure.Name))
                .Parameterize(parameters, "TYPE", nameof(Procedure.Type));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Procedure
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                Type = wrapper.GetString(reader, 6)
            });
        }

        protected override IEnumerable<ProcedureParameter> GetProcedureParameters(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
SELECT
  SPECIFIC_CATALOG,
  SPECIFIC_SCHEMA,
  SPECIFIC_NAME,
  ORDINAL_POSITION,
  PARAMETER_MODE,
  IS_RESULT,
  AS_LOCATOR,
  PARAMETER_NAME,
  CASE WHEN DATA_TYPE IS NULL THEN USER_DEFINED_TYPE_NAME WHEN DATA_TYPE = 'TABLE TYPE' THEN USER_DEFINED_TYPE_NAME ELSE DATA_TYPE END AS DATA_TYPE,
  CHARACTER_MAXIMUM_LENGTH,
  CHARACTER_OCTET_LENGTH,
  COLLATION_CATALOG,
  COLLATION_SCHEMA,
  COLLATION_NAME,
  CHARACTER_SET_CATALOG,
  CHARACTER_SET_SCHEMA,
  CHARACTER_SET_NAME,
  NUMERIC_PRECISION,
  NUMERIC_PRECISION_RADIX,
  NUMERIC_SCALE,
  DATETIME_PRECISION,
  INTERVAL_TYPE,
  INTERVAL_PRECISION
FROM INFORMATION_SCHEMA.PARAMETERS
WHERE (SPECIFIC_CATALOG = '{connpar.Database}')
  AND (SPECIFIC_NAME = @NAME OR (@NAME IS NULL))
  AND (PARAMETER_NAME = @PARAMETER OR (@PARAMETER IS NULL))
ORDER BY SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, PARAMETER_NAME";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(ProcedureParameter.ProcedureName))
                .Parameterize(parameters, "PARAMETER", nameof(ProcedureParameter.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ProcedureParameter
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                ProcedureName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 7),
                Direction = wrapper.GetString(reader, 5) == "YES" ? ParameterDirection.ReturnValue : (wrapper.GetString(reader, 4) == "IN" ? ParameterDirection.Input : ParameterDirection.Output),
                NumericPrecision = wrapper.GetInt32(reader, 17),
                NumericScale = wrapper.GetInt32(reader, 19),
                DataType = wrapper.GetString(reader, 8),
                Length = wrapper.GetInt64(reader, 9)
            });
        }
    }
}
