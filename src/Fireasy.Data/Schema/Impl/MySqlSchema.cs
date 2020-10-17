// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920?126.com"
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
    /// MySql相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class MySqlSchema : SchemaBase
    {
        public MySqlSchema()
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
            AddRestriction<IndexColumn>(s => s.TableName, s => s.IndexName, s => s.ColumnName);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);

            AddDataType("bit", DbType.Boolean, typeof(bool));
            AddDataType("smallint", DbType.Int16, typeof(short));
            AddDataType("tinyint", DbType.Byte, typeof(byte));
            AddDataType("int", DbType.Int32, typeof(int));
            AddDataType("mediumint", DbType.Int32, typeof(int));
            AddDataType("bigint", DbType.Int64, typeof(long));
            AddDataType("float", DbType.Single, typeof(float));
            AddDataType("double", DbType.Double, typeof(double));
            AddDataType("decimal", DbType.Decimal, typeof(decimal));
            AddDataType("binary", DbType.Binary, typeof(byte[]));
            AddDataType("varbinary", DbType.Binary, typeof(byte[]));
            AddDataType("blob", DbType.Binary, typeof(byte[]));
            AddDataType("tinyblob", DbType.Binary, typeof(byte[]));
            AddDataType("mediumblob", DbType.Binary, typeof(byte[]));
            AddDataType("longblob", DbType.Binary, typeof(byte[]));
            AddDataType("timestamp", DbType.Binary, typeof(byte[]));
            AddDataType("binary", DbType.Binary, typeof(byte[]));
            AddDataType("image", DbType.Binary, typeof(byte[]));
            AddDataType("char", DbType.String, typeof(string));
            AddDataType("nchar", DbType.String, typeof(string));
            AddDataType("varchar", DbType.String, typeof(string));
            AddDataType("nvarchar", DbType.String, typeof(string));
            AddDataType("set", DbType.String, typeof(string));
            AddDataType("enum", DbType.String, typeof(string));
            AddDataType("tinytext", DbType.String, typeof(string));
            AddDataType("text", DbType.String, typeof(string));
            AddDataType("mediumtext", DbType.String, typeof(string));
            AddDataType("longtext", DbType.String, typeof(string));
            AddDataType("date", DbType.Date, typeof(DateTime));
            AddDataType("datetime", DbType.DateTime, typeof(DateTime));
            AddDataType("time", DbType.Int64, typeof(TimeSpan));
        }

        protected override IEnumerable<Database> GetDatabases(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var sql = "SHOW DATABASES";

            if (restrictionValues.TryGetValue(nameof(Database.Name), out string dbName))
            {
                sql += $" LIKE '{dbName}'";
            }

            return ExecuteAndParseMetadata(database, sql, null, (wrapper, reader) => new Database
            {
                Name = wrapper.GetString(reader, 0)
            });
        }

        protected override IEnumerable<User> GetUsers(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = "SELECT HOST, USER FROM MYSQL.USER WHERE (NAME = ?NAME OR ?NAME IS NULL)";

            restrictionValues.Parameterize(parameters, "NAME", nameof(User.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new User
            {
                Name = wrapper.GetString(reader, 1)
            });
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT
  TABLE_CATALOG,
  TABLE_SCHEMA,
  TABLE_NAME,
  TABLE_TYPE,
  TABLE_COMMENT
FROM INFORMATION_SCHEMA.TABLES T
WHERE (T.TABLE_SCHEMA = '{connpar.Database}')
  AND T.TABLE_TYPE <> 'VIEW'
  AND (T.TABLE_NAME = ?NAME OR ?NAME IS NULL)
  AND ((T.TABLE_TYPE = 'BASE TABLE' AND (@TABLETYPE IS NULL OR @TABLETYPE = 0)) OR (T.TABLE_TYPE = 'SYSTEM TABLE' AND @TABLETYPE = 1))
ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Table.Name))
                .Parameterize(parameters, "TABLETYPE", nameof(Table.Type));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
            {
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

            SpecialCommand sql = $@"
SELECT T.TABLE_CATALOG,
       T.TABLE_SCHEMA,
       T.TABLE_NAME,
       T.COLUMN_NAME,
       T.DATA_TYPE,
       T.CHARACTER_MAXIMUM_LENGTH,
       T.NUMERIC_PRECISION,
       T.NUMERIC_SCALE,
       T.IS_NULLABLE,
       T.COLUMN_KEY,
       T.COLUMN_DEFAULT,
       T.COLUMN_COMMENT,
       T.EXTRA,
       T.COLUMN_TYPE
FROM INFORMATION_SCHEMA.COLUMNS T
JOIN INFORMATION_SCHEMA.TABLES O
  ON O.TABLE_SCHEMA = T.TABLE_SCHEMA AND O.TABLE_NAME = T.TABLE_NAME
WHERE (T.TABLE_SCHEMA = '{connpar.Database}') AND 
  (T.TABLE_NAME = ?TABLENAME OR ?TABLENAME IS NULL) AND 
  (T.COLUMN_NAME = ?COLUMNNAME OR ?COLUMNNAME IS NULL)
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
                Length = reader.IsDBNull(5) ? (long?)null : wrapper.GetInt64(reader, 5),
                NumericPrecision = reader.IsDBNull(6) ? (int?)null : wrapper.GetInt32(reader, 6),
                NumericScale = reader.IsDBNull(7) ? (int?)null : wrapper.GetInt32(reader, 7),
                IsNullable = wrapper.GetString(reader, 8) == "YES",
                IsPrimaryKey = wrapper.GetString(reader, 9) == "PRI",
                Default = wrapper.GetString(reader, 10),
                Description = wrapper.GetString(reader, 11),
                Autoincrement = !wrapper.IsDbNull(reader, 12) && wrapper.GetString(reader, 12).IndexOf("auto_increment") != -1,
                ColumnType = wrapper.GetString(reader, 13)
            });
        }

        protected override IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME
FROM 
  INFORMATION_SCHEMA.VIEWS T
WHERE (T.TABLE_SCHEMA = '{connpar.Database}') AND 
  (T.TABLE_NAME = ?NAME OR ?NAME IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues.Parameterize(parameters, "NAME", nameof(View.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new View
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2)
            });
        }

        protected override IEnumerable<ViewColumn> GetViewColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT T.TABLE_CATALOG,
       T.TABLE_SCHEMA,
       T.TABLE_NAME,
       T.COLUMN_NAME,
       T.DATA_TYPE,
       T.CHARACTER_MAXIMUM_LENGTH,
       T.NUMERIC_PRECISION,
       T.NUMERIC_SCALE,
       T.IS_NULLABLE,
       T.COLUMN_KEY,
       T.COLUMN_DEFAULT,
       T.COLUMN_COMMENT,
       T.EXTRA
FROM INFORMATION_SCHEMA.COLUMNS T
JOIN INFORMATION_SCHEMA.VIEWS O
  ON O.TABLE_SCHEMA = T.TABLE_SCHEMA AND O.TABLE_NAME = T.TABLE_NAME
WHERE (T.TABLE_SCHEMA = '{connpar.Database}') AND 
  (T.TABLE_NAME = ?TABLENAME OR ?TABLENAME IS NULL) AND 
  (T.COLUMN_NAME = ?COLUMNNAME OR ?COLUMNNAME IS NULL)
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
                Length = reader.IsDBNull(5) ? (long?)null : wrapper.GetInt64(reader, 5),
                NumericPrecision = reader.IsDBNull(6) ? (int?)null : wrapper.GetInt32(reader, 6),
                NumericScale = reader.IsDBNull(7) ? (int?)null : wrapper.GetInt32(reader, 7),
                IsNullable = wrapper.GetString(reader, 8) == "YES",
                IsPrimaryKey = wrapper.GetString(reader, 9) == "PRI",
                Default = wrapper.GetString(reader, 10),
                Description = wrapper.GetString(reader, 11),
                Autoincrement = false
            });
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
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
WHERE (T.CONSTRAINT_SCHEMA = '{connpar.Database}') AND 
   (T.TABLE_NAME = ?TABLENAME OR ?TABLENAME IS NULL) AND 
   (T.CONSTRAINT_NAME = ?NAME OR ?NAME IS NULL) AND
   REFERENCED_TABLE_NAME IS NOT NULL";

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

        protected override IEnumerable<Procedure> GetProcedures(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT
  SPECIFIC_NAME,
  ROUTINE_CATALOG,
  ROUTINE_SCHEMA,
  ROUTINE_NAME,
  ROUTINE_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE (ROUTINE_SCHEMA = '{connpar.Database}')
  AND (ROUTINE_NAME = @NAME OR (@NAME IS NULL))
  AND (ROUTINE_TYPE = @TYPE OR (@TYPE IS NULL))
ORDER BY ROUTINE_CATALOG, ROUTINE_SCHEMA, ROUTINE_NAME";

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

            SpecialCommand sql = $@"
SELECT
  SPECIFIC_CATALOG,
  SPECIFIC_SCHEMA,
  SPECIFIC_NAME,
  ORDINAL_POSITION,
  PARAMETER_MODE,
  PARAMETER_NAME,
  DATA_TYPE,
  CHARACTER_MAXIMUM_LENGTH,
  NUMERIC_PRECISION,
  NUMERIC_SCALE
WHERE (SPECIFIC_SCHEMA = '{connpar.Database}')
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
                Name = wrapper.GetString(reader, 5),
                Direction = wrapper.GetString(reader, 4) == "IN" ? ParameterDirection.Input : ParameterDirection.Output,
                NumericPrecision = wrapper.GetInt32(reader, 8),
                NumericScale = wrapper.GetInt32(reader, 9),
                DataType = wrapper.GetString(reader, 6),
                Length = wrapper.GetInt64(reader, 7)
            });
        }
    }
}
