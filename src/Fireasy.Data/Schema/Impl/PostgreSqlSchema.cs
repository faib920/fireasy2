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

            AddDataType("bit", DbType.Byte, typeof(byte));
            AddDataType("boolean", DbType.Boolean, typeof(bool));
            AddDataType("smallint", DbType.Int16, typeof(short));
            AddDataType("integer", DbType.Int32, typeof(int));
            AddDataType("serial", DbType.Int32, typeof(int));
            AddDataType("bigint", DbType.Int64, typeof(long));
            AddDataType("bigserial", DbType.Int64, typeof(long));
            AddDataType("real", DbType.Single, typeof(float));
            AddDataType("bytea", DbType.Binary, typeof(byte[]));
            AddDataType("char", DbType.String, typeof(string));
            AddDataType("varchar", DbType.String, typeof(string));
            AddDataType("text", DbType.String, typeof(string));
            AddDataType("xml", DbType.Xml, typeof(string));
            AddDataType("uuid", DbType.Guid, typeof(Guid));
            AddDataType("timestamp", DbType.DateTime, typeof(DateTime));
        }

        protected override IEnumerable<Database> GetDatabases(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
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

            SqlCommand sql = $@"
SELECT
  T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME,
  T.TABLE_TYPE,
  C.TABLE_COMMENT
FROM INFORMATION_SCHEMA.TABLES T
JOIN (
  SELECT RELNAME AS TABLE_NAME,CAST(OBJ_DESCRIPTION(RELFILENODE, 'pg_class') AS VARCHAR) AS TABLE_COMMENT FROM PG_CLASS C 
  WHERE RELKIND = 'r' AND RELNAME NOT LIKE 'pg_%' AND RELNAME NOT LIKE 'sql_%'
) C ON T.TABLE_NAME = C.TABLE_NAME
WHERE (TABLE_CATALOG = '{connpar.Database}')
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

            SqlCommand sql = $@"
SELECT
    COL.TABLE_CATALOG,
    COL.TABLE_SCHEMA,
    COL.TABLE_NAME,
    COL.ORDINAL_POSITION,
    COL.COLUMN_NAME,
    COL.DATA_TYPE,
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
  SELECT RELNAME AS TABLE_NAME,CAST(OBJ_DESCRIPTION(RELFILENODE, 'pg_class') AS VARCHAR) AS TABLE_COMMENT FROM PG_CLASS C 
  WHERE RELKIND = 'r' AND RELNAME NOT LIKE 'pg_%' AND RELNAME NOT LIKE 'sql_%'
) C ON col.TABLE_NAME = C.TABLE_NAME
LEFT JOIN PG_DESCRIPTION DES
    ON (SELECT OID FROM PG_CLASS WHERE RELNAME = COL.TABLE_NAME) = DES.OBJOID
    AND COL.ORDINAL_POSITION = DES.OBJSUBID
LEFT JOIN PG_CONSTRAINT CON
		ON CON.CONRELID = DES.OBJOID
WHERE (COL.TABLE_CATALOG = '{connpar.Database}')
  AND (COL.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL)
  AND (COL.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
ORDER BY
  COL.TABLE_CATALOG, COL.TABLE_SCHEMA, COL.TABLE_NAME, COL.ORDINAL_POSITION";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 4),
                DataType = wrapper.GetString(reader, 5),
                Length = wrapper.GetInt64(reader, 6),
                NumericPrecision = wrapper.GetInt32(reader, 7),
                NumericScale = wrapper.GetInt32(reader, 8),
                IsNullable = wrapper.GetString(reader, 9) == "YES",
                IsPrimaryKey = wrapper.GetString(reader, 12) == "YES",
                Default = wrapper.GetString(reader, 10),
                Description = wrapper.GetString(reader, 11),
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
  KCU.COLUMN_NAME,
  CCU.TABLE_NAME AS FOREIGN_TABLE_NAME,
  CCU.COLUMN_NAME AS FOREIGN_COLUMN_NAME,
  TC.IS_DEFERRABLE,
  TC.INITIALLY_DEFERRED
FROM
  INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU ON TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE AS CCU ON CCU.CONSTRAINT_NAME = TC.CONSTRAINT_NAME
WHERE TC.CONSTRAINT_CATALOG = '{connpar.Database}' AND
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
    }
}
