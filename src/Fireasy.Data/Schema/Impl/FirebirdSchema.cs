// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Data;

namespace Fireasy.Data.Schema
{
    public class FirebirdSchema : SchemaBase
    {
        public FirebirdSchema()
        {
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
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT
null AS TABLE_CATALOG,
null AS TABLE_SCHEMA,
trim(rdb$relation_name) AS TABLE_NAME,
(case when rdb$system_flag = 1 then 'SYSTEM_TABLE' else 'TABLE' end) AS TABLE_TYPE,
rdb$owner_name AS OWNER_NAME,
rdb$description AS DESCRIPTION,
rdb$view_source AS VIEW_SOURCE
FROM rdb$relations
WHERE 
  (rdb$relation_name = @NAME OR (@NAME IS NULL)) AND 
  ((@TABLETYPE = 1 and rdb$system_flag = 1) OR ((@TABLETYPE = 0 or @TABLETYPE IS NULL) and rdb$system_flag = 0))
ORDER BY rdb$system_flag, rdb$owner_name, rdb$relation_name";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Table.Name))
                .Parameterize(parameters, "TABLETYPE", nameof(Table.Type));

            var table = database.ExecuteDataTable(sql, parameters: parameters);
            foreach (DataRow row in table.Rows)
            {
                yield return new Table
                {
                    Catalog = row["TABLE_CATALOG"].ToString(),
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    Name = row["TABLE_NAME"].ToString(),
                    Description = row["DESCRIPTION"].ToStringSafely(),
                    Type = row["TABLE_TYPE"].ToString() == "TABLE" ? TableType.BaseTable : TableType.SystemTable
                };
            }
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT
  null AS TABLE_CATALOG,
  null AS TABLE_SCHEMA,
  rfr.rdb$relation_name AS TABLE_NAME,
  trim(rfr.rdb$field_name) AS COLUMN_NAME,
  null AS COLUMN_DATA_TYPE,
  fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
  CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
  CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
  CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
  CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
  CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
  rfr.rdb$field_position AS ORDINAL_POSITION,
  rfr.rdb$default_source AS COLUMN_DEFAULT,
  coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS COLUMN_NULLABLE,
  fld.rdb$field_type AS FIELD_TYPE,
  rfr.rdb$description AS DESCRIPTION,
  (select count(1)
    FROM RDB$RELATION_CONSTRAINTS RC
    LEFT JOIN RDB$INDICES I ON 
      (I.RDB$INDEX_NAME = RC.RDB$INDEX_NAME)
    LEFT JOIN RDB$INDEX_SEGMENTS S 
    ON 
      (S.RDB$INDEX_NAME = I.RDB$INDEX_NAME)
    WHERE (RC.RDB$CONSTRAINT_TYPE = 'PRIMARY KEY') and 
    rc.RDB$RELATION_NAME = rfr.rdb$relation_name and RDB$FIELD_NAME = rfr.rdb$field_name) as COLUMN_IS_PK
FROM rdb$relation_fields rfr
LEFT JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name
LEFT JOIN rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id
LEFT JOIN rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
WHERE (rfr.rdb$relation_name = @TABLENAME OR (@TABLENAME IS NULL)) AND 
  (rfr.rdb$field_name = @COLUMNNAME OR (@COLUMNNAME IS NULL))
ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position
";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            var table = database.ExecuteDataTable(sql, parameters: parameters);
            foreach (DataRow row in table.Rows)
            {
                var subtype = 0;
                int scale = 0;
                if (row["COLUMN_SUB_TYPE"] != DBNull.Value)
                {
                    subtype = row["COLUMN_SUB_TYPE"].To<int>();
                }

                if (row["NUMERIC_SCALE"] != DBNull.Value)
                {
                    scale = row["NUMERIC_SCALE"].To<int>();
                }

                var ftype = row["FIELD_TYPE"].To<int>();
                yield return new Column
                {
                    Catalog = row["TABLE_CATALOG"].ToString(),
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    TableName = row["TABLE_NAME"].ToString(),
                    Name = row["COLUMN_NAME"].ToString(),
                    Default = row["COLUMN_DEFAULT"].ToString(),
                    DataType = GetDbDataType(ftype, subtype, scale),
                    NumericPrecision = row["NUMERIC_PRECISION"].To<int>(),
                    NumericScale = scale,
                    Description = row["DESCRIPTION"].ToString(),
                    IsNullable = row["COLUMN_NULLABLE"].To<bool>(),
                    Length = row["COLUMN_SIZE"].To<long>(),
                    Position = row["ORDINAL_POSITION"].To<int>(),
                    IsPrimaryKey = row["COLUMN_IS_PK"].To<bool>()
                };
            }
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
SELECT
  null AS CONSTRAINT_CATALOG,
  null AS CONSTRAINT_SCHEMA,
  trim(co.rdb$constraint_name) AS CONSTRAINT_NAME,
  null AS TABLE_CATALOG,
  null AS TABLE_SCHEMA,
  trim(co.rdb$relation_name) AS TABLE_NAME,
  trim(coidxseg.rdb$field_name) AS COLUMN_NAME,
  null as REFERENCED_TABLE_CATALOG,
  null as REFERENCED_TABLE_SCHEMA,
  trim(refidx.rdb$relation_name) as REFERENCED_TABLE_NAME,
  trim(refidxseg.rdb$field_name) AS REFERENCED_COLUMN_NAME,
  ref.rdb$update_rule AS UPDATE_RULE,
  ref.rdb$delete_rule AS DELETE_RULE
FROM rdb$relation_constraints co
INNER JOIN rdb$ref_constraints ref ON co.rdb$constraint_name = ref.rdb$constraint_name
INNER JOIN rdb$indices tempidx ON co.rdb$index_name = tempidx.rdb$index_name
INNER JOIN rdb$index_segments coidxseg ON co.rdb$index_name = coidxseg.rdb$index_name
INNER JOIN rdb$indices refidx ON refidx.rdb$index_name = tempidx.rdb$foreign_key
INNER JOIN rdb$index_segments refidxseg ON refidxseg.rdb$index_name = refidx.rdb$index_name AND refidxseg.rdb$field_position = coidxseg.rdb$field_position
where co.rdb$constraint_type = 'FOREIGN KEY' AND
  (co.rdb$relation_name = @TABLENAME OR (@TABLENAME IS NULL)) AND 
  (co.rdb$constraint_name = @NAME OR (@NAME IS NULL))";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(ForeignKey.TableName))
                .Parameterize(parameters, "NAME", nameof(ForeignKey.Name));

            var table = database.ExecuteDataTable(sql, parameters: parameters);
            foreach (DataRow row in table.Rows)
            {
                yield return new ForeignKey
                {
                    Schema = row["CONSTRAINT_SCHEMA"].ToString(),
                    Name = row["CONSTRAINT_NAME"].ToString(),
                    TableName = row["TABLE_NAME"].ToString().Replace("\"", ""),
                    PKTable = row["REFERENCED_TABLE_NAME"].ToString(),
                    ColumnName = row["COLUMN_NAME"].ToString(),
                    PKColumn = row["REFERENCED_COLUMN_NAME"].ToString(),
                };
            }
        }

        private string GetDbDataType(int blrType, int subType, int scale)
        {
            switch (blrType)
            {
                case 7:
                    if (subType != 2)
                    {
                        if (subType == 1)
                        {
                            return "NUMERIC";
                        }
                        if (scale < 0)
                        {
                            return "DECIMAL";
                        }
                        return "SMALLINT";
                    }
                    return "DECIMAL";

                case 8:
                    if (subType != 2)
                    {
                        if (subType == 1)
                        {
                            return "NUMERIC";
                        }
                        if (scale < 0)
                        {
                            return "DECIMAL";
                        }
                        return "INTEGER";
                    }
                    return "DECIMAL";

                case 9:
                case 0x10:
                case 0x2d:
                    if (subType == 2)
                    {
                        return "DECIMAL";
                    }
                    if (subType == 1)
                    {
                        return "NUMERIC";
                    }
                    if (scale < 0)
                    {
                        return "DECIMAL";
                    }
                    return "BIGINT";

                case 10:
                    return "FLOAT";

                case 11:
                case 0x1b:
                    return "DOUBLE PRECISION";

                case 12:
                    return "DATE";

                case 13:
                    return "TIME";

                case 14:
                case 15:
                    return "CHAR";

                case 0x23:
                    return "TIMESTAMP";

                case 0x25:
                case 0x26:
                    return "VARCHAR";

                case 40:
                case 0x29:
                    return "BLOB SUB_TYPE 1";

                case 0x105:
                    if (subType == 1)
                    {
                        return "BLOB SUB_TYPE 1";
                    }
                    return "BLOB";
            }

            throw new ArgumentException("Invalid data type");
        }
    }
}
