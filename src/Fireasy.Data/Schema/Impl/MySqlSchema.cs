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
            AddRestrictionIndex<Database>(s => s.Name);
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
        }

        protected override IEnumerable<Database> GetDatabases(IDatabase database, string[] restrictionValues)
        {
            var sql = "SHOW DATABASES";

            if (restrictionValues != null && restrictionValues.Length == 1)
            {
                sql += $" LIKE '{restrictionValues[0]}'";
            }

            return ParseMetadata(database, sql, null, (wrapper, reader) => new Database
                {
                    Name = wrapper.GetString(reader, 0),
                    CreateDate = wrapper.GetDateTime(reader, 1)
                });
        }
        
        protected override IEnumerable<User> GetUsers(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = "SELECT HOST, USER FROM MYSQL.USER WHERE (NAME = ?NAME OR ?NAME IS NULL)";

            ParameteRestrition(parameters, "NAME", 0, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new User
                {
                    Name = wrapper.GetString(reader, 1)
                });
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT
  TABLE_CATALOG,
  TABLE_SCHEMA,
  TABLE_NAME,
  TABLE_TYPE,
  TABLE_COMMENT
FROM INFORMATION_SCHEMA.TABLES T
WHERE (TABLE_CATALOG = ?CATALOG OR ?CATALOG IS NULL)
  AND (T.TABLE_SCHEMA = ?SCHEMA OR ?SCHEMA IS NULL)
  AND (T.TABLE_NAME = ?NAME OR ?NAME IS NULL)
  AND (T.TABLE_TYPE = ?TABLETYPE OR ?TABLETYPE IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "SCHEMA", 1, restrictionValues);
            ParameteRestrition(parameters, "NAME", 2, restrictionValues);
            ParameteRestrition(parameters, "TABLETYPE", 3, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
                {
                    Catalog = wrapper.GetString(reader, 0),
                    Schema = wrapper.GetString(reader, 1),
                    Name = wrapper.GetString(reader, 2),
                    Type = wrapper.GetString(reader, 3),
                    Description = wrapper.GetString(reader, 4)
                });
        }
        
        protected override IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
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
JOIN INFORMATION_SCHEMA.TABLES O
  ON O.TABLE_SCHEMA = T.TABLE_SCHEMA AND O.TABLE_NAME = T.TABLE_NAME
WHERE (T.TABLE_CATALOG = ?CATALOG OR ?CATALOG IS NULL) AND 
  (T.TABLE_SCHEMA = ?SCHEMA OR ?SCHEMA IS NULL) AND 
  (T.TABLE_NAME = ?TABLENAME OR ?TABLENAME IS NULL) AND 
  (T.COLUMN_NAME = ?COLUMNNAME OR ?COLUMNNAME IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "SCHEMA", 1, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 2, restrictionValues);
            ParameteRestrition(parameters, "COLUMNNAME", 3, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
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
                    IsPrimaryKey = wrapper.GetString(reader, 9) == "PRI",
                    Default = wrapper.GetString(reader, 10),
                    Description = wrapper.GetString(reader, 11),
                    Autoincrement = !wrapper.IsDbNull(reader, 12) && wrapper.GetString(reader, 12).IndexOf("auto_increment") != -1
                });
        }

        protected override IEnumerable<View> GetViews(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.TABLE_CATALOG,
  T.TABLE_SCHEMA,
  T.TABLE_NAME
FROM 
  INFORMATION_SCHEMA.VIEWS T
WHERE AND (T.TABLE_CATALOG = ?CATALOG OR ?CATALOG IS NULL)
  AND (T.TABLE_SCHEMA = ?SCHEMA OR ?SCHEMA IS NULL)
  AND (T.TABLE_NAME = ?NAME OR ?NAME IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "SCHEMA", 1, restrictionValues);
            ParameteRestrition(parameters, "NAME", 2, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new View
                {
                    Catalog = wrapper.GetString(reader, 0),
                    Schema = wrapper.GetString(reader, 1),
                    Name = wrapper.GetString(reader, 2)
                });
        }

        protected override IEnumerable<ViewColumn> GetViewColumns(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT T.TABLE_CATALOG,
       T.TABLE_SCHEMA,
       T.TABLE_NAME,
       T.COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS T
WHERE (T.TABLE_CATALOG = ?CATALOG OR ?CATALOG IS NULL) AND 
  (T.TABLE_SCHEMA = ?SCHEMA OR ?SCHEMA IS NULL) AND 
  (T.TABLE_NAME = ?TABLENAME OR ?TABLENAME IS NULL) AND 
  (T.COLUMN_NAME = ?COLUMNNAME OR ?COLUMNNAME IS NULL)
 ORDER BY T.TABLE_CATALOG, T.TABLE_SCHEMA, T.TABLE_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "SCHEMA", 1, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 2, restrictionValues);
            ParameteRestrition(parameters, "COLUMNNAME", 3, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new ViewColumn
                {
                    Catalog = wrapper.GetString(reader, 0),
                    Schema = wrapper.GetString(reader, 1),
                    ViewName = wrapper.GetString(reader, 2),
                    Name = wrapper.GetString(reader, 3)
                });
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, string[] restrictionValues)
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

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
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

        protected override IEnumerable<Procedure> GetProcedures(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT
  SPECIFIC_NAME,
  ROUTINE_CATALOG,
  ROUTINE_SCHEMA,
  ROUTINE_NAME,
  ROUTINE_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE (SPECIFIC_CATALOG = @CATALOG OR (@CATALOG IS NULL))
  AND (SPECIFIC_SCHEMA = @OWNER OR (@OWNER IS NULL))
  AND (SPECIFIC_NAME = @NAME OR (@NAME IS NULL))
  AND (ROUTINE_TYPE = @TYPE OR (@TYPE IS NULL))
ORDER BY SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "OWNER", 1, restrictionValues);
            ParameteRestrition(parameters, "NAME", 2, restrictionValues);
            ParameteRestrition(parameters, "TYPE", 3, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new Procedure
                {
                    Catalog = wrapper.GetString(reader, 0),
                    Schema = wrapper.GetString(reader, 1),
                    Name = wrapper.GetString(reader, 2),
                    Type = wrapper.GetString(reader, 6)
                });
        }
        
        protected override IEnumerable<ProcedureParameter> GetProcedureParameters(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
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
WHERE (SPECIFIC_CATALOG = @CATALOG OR (@CATALOG IS NULL))
  AND (SPECIFIC_SCHEMA = @OWNER OR (@OWNER IS NULL))
  AND (SPECIFIC_NAME = @NAME OR (@NAME IS NULL))
  AND (PARAMETER_NAME = @PARAMETER OR (@PARAMETER IS NULL))
ORDER BY SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, PARAMETER_NAME";

            ParameteRestrition(parameters, "CATALOG", 0, restrictionValues);
            ParameteRestrition(parameters, "OWNER", 1, restrictionValues);
            ParameteRestrition(parameters, "NAME", 2, restrictionValues);
            ParameteRestrition(parameters, "PARAMETER", 3, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new ProcedureParameter
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
