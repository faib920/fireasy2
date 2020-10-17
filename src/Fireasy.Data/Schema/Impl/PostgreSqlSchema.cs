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
    /// PostgreSql相关数据库架构信息的获取方法。
    /// </summary>
    public class PostgreSqlSchema : SchemaBase
    {
        public PostgreSqlSchema()
        {
            AddRestriction<Database>(s => s.Name);
            AddRestriction<Table>(s => s.Name, s => s.Type);
            AddRestriction<Column>(s => s.TableName, s => s.Name);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);
            AddRestriction<View>(s => s.Name);
            AddRestriction<ViewColumn>(s => s.ViewName, s => s.Name);

            AddDataType("bit", DbType.Byte, typeof(byte));
            AddDataType("varbit", DbType.Byte, typeof(byte));
            AddDataType("bool", DbType.Boolean, typeof(bool));
            AddDataType("int2", DbType.Int16, typeof(short));
            AddDataType("smallint", DbType.Int16, typeof(short));
            AddDataType("int4", DbType.Int32, typeof(int));
            AddDataType("int", DbType.Int32, typeof(int));
            AddDataType("integer", DbType.Int32, typeof(int));
            AddDataType("int8", DbType.Int64, typeof(long));
            AddDataType("bigint", DbType.Int64, typeof(long));
            AddDataType("serial2", DbType.Int16, typeof(short));
            AddDataType("smallserial", DbType.Int16, typeof(short));
            AddDataType("serial4", DbType.Int32, typeof(int));
            AddDataType("serial", DbType.Int32, typeof(int));
            AddDataType("serial8", DbType.Int64, typeof(long));
            AddDataType("bigserial", DbType.Int64, typeof(long));
            AddDataType("interval", DbType.Int64, typeof(long));
            AddDataType("decimal", DbType.Decimal, typeof(decimal));
            AddDataType("numeric", DbType.Decimal, typeof(decimal));
            AddDataType("money", DbType.Currency, typeof(decimal));
            AddDataType("float4", DbType.Single, typeof(float));
            AddDataType("float8", DbType.Double, typeof(double));
            AddDataType("real", DbType.Double, typeof(double));
            AddDataType("bytea", DbType.Binary, typeof(byte[]));
            AddDataType("char", DbType.String, typeof(string));
            AddDataType("varchar", DbType.String, typeof(string));
            AddDataType("text", DbType.String, typeof(string));
            AddDataType("xml", DbType.Xml, typeof(string));
            AddDataType("uuid", DbType.Guid, typeof(Guid));
            AddDataType("date", DbType.Date, typeof(DateTime));
            AddDataType("time", DbType.Time, typeof(DateTime));
            AddDataType("timez", DbType.Time, typeof(DateTime));
            AddDataType("timestamp", DbType.DateTime, typeof(DateTime));
            AddDataType("timestampz", DbType.DateTime, typeof(DateTime));
        }

        protected override IEnumerable<Database> GetDatabases(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT DATNAME FROM PG_DATABASE WHERE (DATNAME = @NAME OR (@NAME IS NULL))";

            restrictionValues.Parameterize(parameters, "NAME", nameof(Database.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Database
            {
                Name = wrapper.GetString(reader, 0)
            });
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT
  T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME,
  T.TABLE_TYPE,
  C.TABLE_COMMENT
FROM INFORMATION_SCHEMA.TABLES T
JOIN (
  SELECT RELNAME AS TABLE_NAME, CAST(OBJ_DESCRIPTION(RELFILENODE, 'pg_class') AS VARCHAR) AS TABLE_COMMENT FROM PG_CLASS C 
  WHERE RELKIND = 'r'
) C ON T.TABLE_NAME = C.TABLE_NAME
WHERE (T.TABLE_CATALOG = '{connpar.Database}' AND T.TABLE_SCHEMA = ANY (CURRENT_SCHEMAS(false)))
  AND (T.TABLE_NAME = :NAME OR :NAME IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Table.Name));

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

            SpecialCommand sql = $@"
SELECT
    COL.TABLE_CATALOG,
    COL.TABLE_SCHEMA,
    COL.TABLE_NAME,
    COL.ORDINAL_POSITION,
    COL.COLUMN_NAME,
    COL.UDT_NAME,
    COL.CHARACTER_MAXIMUM_LENGTH,
    COL.NUMERIC_PRECISION,
    COL.NUMERIC_SCALE,
    COL.IS_NULLABLE,
    COL.COLUMN_DEFAULT ,
    DES.DESCRIPTION,
	(CASE WHEN COL.ORDINAL_POSITION = CON.CONKEY[1] THEN 'YES' ELSE 'NO' END) IS_KEY
FROM
    INFORMATION_SCHEMA.COLUMNS COL
JOIN (
  SELECT RELNAME AS TABLE_NAME, CAST(OBJ_DESCRIPTION(RELFILENODE, 'pg_class') AS VARCHAR) AS TABLE_COMMENT FROM PG_CLASS C 
  WHERE RELKIND = 'r'
  ) C
    ON col.TABLE_NAME = C.TABLE_NAME
LEFT JOIN PG_CLASS PCL
    ON PCL.RELNAME = COL.TABLE_NAME
LEFT JOIN PG_DESCRIPTION DES
    ON PCL.OID = DES.OBJOID AND COL.ORDINAL_POSITION = DES.OBJSUBID
LEFT JOIN PG_CONSTRAINT CON
	ON CON.CONRELID = PCL.OID
WHERE (COL.TABLE_CATALOG = '{connpar.Database}' AND COL.TABLE_SCHEMA = ANY (CURRENT_SCHEMAS(false)))
  AND (COL.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL)
  AND (COL.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
  AND CON.CONTYPE = 'p'
ORDER BY
  COL.TABLE_CATALOG, COL.TABLE_SCHEMA, COL.TABLE_NAME, COL.ORDINAL_POSITION";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => SetColumnType(new Column
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 4),
                DataType = wrapper.GetString(reader, 5),
                Length = reader.IsDBNull(6) ? (long?)null : wrapper.GetInt64(reader, 6),
                NumericPrecision = reader.IsDBNull(7) ? (int?)null : wrapper.GetInt32(reader, 7),
                NumericScale = reader.IsDBNull(8) ? (int?)null : wrapper.GetInt32(reader, 8),
                IsNullable = wrapper.GetString(reader, 9) == "YES",
                IsPrimaryKey = wrapper.GetString(reader, 12) == "YES",
                Default = wrapper.GetString(reader, 10),
                Description = wrapper.GetString(reader, 11),
            }));
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT
  TC.CONSTRAINT_CATALOG,
  TC.CONSTRAINT_SCHEMA,
  TC.CONSTRAINT_NAME,
  TC.TABLE_NAME,
  KCU.COLUMN_NAME,
  CCU.TABLE_NAME AS FOREIGN_TABLE_NAME,
  CCU.COLUMN_NAME AS FOREIGN_COLUMN_NAME,
  TC.IS_DEFERRABLE,
  TC.INITIALLY_DEFERRED
FROM
  INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU ON TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS CCU ON CCU.CONSTRAINT_NAME = TC.CONSTRAINT_NAME
WHERE (TC.CONSTRAINT_CATALOG = '{connpar.Database}' AND TC.TABLE_SCHEMA = ANY (CURRENT_SCHEMAS(false))) AND
  CONSTRAINT_TYPE = 'FOREIGN KEY' AND
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

        protected override IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SpecialCommand sql = $@"
SELECT
  T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME,
  C.TABLE_COMMENT
FROM INFORMATION_SCHEMA.VIEWS T
JOIN (
  SELECT RELNAME AS TABLE_NAME, CAST(OBJ_DESCRIPTION(RELFILENODE, 'pg_class') AS VARCHAR) AS TABLE_COMMENT FROM PG_CLASS C 
  WHERE RELKIND = 'v'
  ) C
    ON t.TABLE_NAME = C.TABLE_NAME
WHERE (TABLE_CATALOG = '{connpar.Database}' AND TABLE_SCHEMA = ANY (CURRENT_SCHEMAS(false)))
  AND (T.TABLE_NAME = :NAME OR :NAME IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(View.Name));

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

            SpecialCommand sql = $@"
SELECT
    COL.TABLE_CATALOG,
    COL.TABLE_SCHEMA,
    COL.TABLE_NAME,
    COL.ORDINAL_POSITION,
    COL.COLUMN_NAME,
    COL.UDT_NAME,
    COL.CHARACTER_MAXIMUM_LENGTH,
    COL.NUMERIC_PRECISION,
    COL.NUMERIC_SCALE
FROM
    INFORMATION_SCHEMA.COLUMNS COL
JOIN (
  SELECT RELNAME AS TABLE_NAME, CAST(OBJ_DESCRIPTION(RELFILENODE, 'pg_class') AS VARCHAR) AS TABLE_COMMENT FROM PG_CLASS C 
  WHERE RELKIND = 'v' AND RELNAME NOT LIKE 'pg_%' AND RELNAME NOT LIKE 'sql_%'
  ) C
    ON col.TABLE_NAME = C.TABLE_NAME
WHERE (COL.TABLE_CATALOG = '{connpar.Database}' AND COL.TABLE_SCHEMA = ANY (CURRENT_SCHEMAS(false)))
  AND (COL.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL)
  AND (COL.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
ORDER BY
  COL.TABLE_CATALOG, COL.TABLE_SCHEMA, COL.TABLE_NAME, COL.ORDINAL_POSITION";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(ViewColumn.ViewName))
                .Parameterize(parameters, "COLUMNNAME", nameof(ViewColumn.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ViewColumn
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                ViewName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 4),
                DataType = wrapper.GetString(reader, 5),
                Length = reader.IsDBNull(6) ? (long?)null : wrapper.GetInt64(reader, 6),
                NumericPrecision = reader.IsDBNull(7) ? (int?)null : wrapper.GetInt32(reader, 7),
                NumericScale = reader.IsDBNull(8) ? (int?)null : wrapper.GetInt32(reader, 8)
            });
        }

    }
}
