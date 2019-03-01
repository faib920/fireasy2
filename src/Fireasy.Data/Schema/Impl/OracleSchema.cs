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
            AddRestriction<User>(s => s.Name);
            AddRestriction<Procedure>(s => s.Name);
            AddRestriction<ProcedureParameter>(s => s.ProcedureName);
            AddRestriction<Index>(s => s.Name, s => s.TableName);
            AddRestriction<IndexColumn>(s => s.Name, s => s.TableName, s => s.ColumnName);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);
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

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
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

            SqlCommand sql = $@"
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
 WHERE (T.OWNER = '{connpar.UserId.ToUpper()}') AND 
   (T.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL) AND 
   (T.COLUMN_NAME = :COLUMNNAME OR :COLUMNNAME IS NULL)
 ORDER BY T.OWNER, T.TABLE_NAME, T.COLUMN_ID";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
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
            });
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();
            var connpar = GetConnectionParameter(database);

            SqlCommand sql = $@"
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
    }
}
