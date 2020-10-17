// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.RecordWrapper;
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
            AddRestriction<IndexColumn>(s => s.TableName, s => s.IndexName, s => s.ColumnName);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);

            AddDataType("boolean", DbType.Boolean, typeof(bool));
            AddDataType("smallint", DbType.Int16, typeof(short));
            AddDataType("integer", DbType.Int32, typeof(int));
            AddDataType("bigint", DbType.Int64, typeof(long));
            AddDataType("float", DbType.Single, typeof(float));
            AddDataType("numeric", DbType.Decimal, typeof(decimal));
            AddDataType("decimal", DbType.Decimal, typeof(decimal));
            AddDataType("double precision", DbType.Double, typeof(double));
            AddDataType("blob", DbType.Binary, typeof(byte[]));
            AddDataType("char", DbType.String, typeof(string));
            AddDataType("varchar", DbType.String, typeof(string));
            AddDataType("timestamp", DbType.DateTime, typeof(DateTime));
            AddDataType("date", DbType.Date, typeof(DateTime));
            AddDataType("time", DbType.Int64, typeof(TimeSpan));
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT
null AS TABLE_CATALOG,
null AS TABLE_SCHEMA,
trim(rdb$relation_name) AS TABLE_NAME,
(case when rdb$system_flag = 1 then 'SYSTEM_TABLE' else 'TABLE' end) AS TABLE_TYPE,
rdb$description AS DESCRIPTION
FROM rdb$relations
WHERE 
  (rdb$relation_name = @NAME OR @NAME IS NULL) AND 
  ((@TABLETYPE = 1 and rdb$system_flag = 1) OR ((@TABLETYPE = 0 or @TABLETYPE IS NULL) and rdb$system_flag = 0)) AND
  rdb$view_blr IS NULL
ORDER BY rdb$system_flag, rdb$owner_name, rdb$relation_name";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(Table.Name))
                .Parameterize(parameters, "TABLETYPE", nameof(Table.Type));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                Type = wrapper.GetString(reader, 3) == "TABLE" ? TableType.BaseTable : TableType.SystemTable,
                Description = wrapper.GetString(reader, 4)
            });
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT
  null AS TABLE_CATALOG,
  null AS TABLE_SCHEMA,
  trim(rfr.rdb$relation_name) AS TABLE_NAME,
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
  (rfr.rdb$field_name = @COLUMNNAME OR (@COLUMNNAME IS NULL)) AND rfr.rdb$system_flag = 0
ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position
";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(Column.TableName))
                .Parameterize(parameters, "COLUMNNAME", nameof(Column.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => SetColumnType(SetDataType(wrapper, reader, new Column
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                TableName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 3),
                Default = wrapper.GetString(reader, 12),
                NumericPrecision = reader.IsDBNull(7) ? (int?)null : wrapper.GetInt32(reader, 7),
                Description = wrapper.GetString(reader, 15),
                IsNullable = wrapper.GetInt32(reader, 13) == 1,
                Length = reader.IsDBNull(9) ? (long?)null : wrapper.GetInt64(reader, 9),
                Position = wrapper.GetInt32(reader, 11),
                IsPrimaryKey = wrapper.GetInt32(reader, 16) == 1
            })));
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
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

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                TableName = wrapper.GetString(reader, 5),
                ColumnName = wrapper.GetString(reader, 6),
                PKTable = wrapper.GetString(reader, 9),
                PKColumn = wrapper.GetString(reader, 10),
            });
        }

        protected override IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT
null AS TABLE_CATALOG,
null AS TABLE_SCHEMA,
trim(rdb$relation_name) AS TABLE_NAME,
rdb$owner_name AS OWNER_NAME,
rdb$description AS DESCRIPTION,
rdb$view_source AS VIEW_SOURCE
FROM rdb$relations
WHERE 
  (rdb$relation_name = @NAME OR (@NAME IS NULL)) AND 
  rdb$system_flag = 0 AND NOT rdb$view_blr IS NULL
ORDER BY rdb$system_flag, rdb$owner_name, rdb$relation_name";

            restrictionValues
                .Parameterize(parameters, "NAME", nameof(View.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new View
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                Name = wrapper.GetString(reader, 2),
                Description = wrapper.GetString(reader, 4)
            });
        }

        protected override IEnumerable<ViewColumn> GetViewColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            SpecialCommand sql = @"
SELECT
  null AS TABLE_CATALOG,
  null AS TABLE_SCHEMA,
  trim(rfr.rdb$relation_name) AS TABLE_NAME,
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
JOIN rdb$relations rel ON rel.rdb$relation_name = rfr.rdb$relation_name
LEFT JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name
LEFT JOIN rdb$character_sets cs ON cs.rdb$character_set_id = fld.rdb$character_set_id
LEFT JOIN rdb$collations coll ON (coll.rdb$collation_id = fld.rdb$collation_id AND coll.rdb$character_set_id = fld.rdb$character_set_id)
WHERE (rfr.rdb$relation_name = @TABLENAME OR (@TABLENAME IS NULL)) AND 
  (rfr.rdb$field_name = @COLUMNNAME OR (@COLUMNNAME IS NULL)) AND rfr.rdb$system_flag = 0 AND NOT rel.rdb$view_blr IS NULL
ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position
";

            restrictionValues
                .Parameterize(parameters, "TABLENAME", nameof(ViewColumn.ViewName))
                .Parameterize(parameters, "COLUMNNAME", nameof(ViewColumn.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => SetDataType(wrapper, reader, new ViewColumn
            {
                Catalog = wrapper.GetString(reader, 0),
                Schema = wrapper.GetString(reader, 1),
                ViewName = wrapper.GetString(reader, 2),
                Name = wrapper.GetString(reader, 3),
                Default = wrapper.GetString(reader, 12),
                NumericPrecision = reader.IsDBNull(7) ? (int?)null : wrapper.GetInt32(reader, 7),
                Description = wrapper.GetString(reader, 15),
                IsNullable = wrapper.GetInt32(reader, 13) == 1,
                Length = reader.IsDBNull(9) ? (long?)null : wrapper.GetInt64(reader, 9),
                Position = wrapper.GetInt32(reader, 11),
                IsPrimaryKey = wrapper.GetInt32(reader, 16) == 1
            }));
        }

        private string GetDbDataType(int blrType, int subType, int? scale)
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

        private Tuple<int, int, int?> GetNumericTuple(IRecordWrapper wrapper, IDataReader reader)
        {
            var subtype = 0;
            int? scale = null;
            if (!reader.IsDBNull(reader.GetOrdinal("COLUMN_SUB_TYPE")))
            {
                subtype = wrapper.GetInt32(reader, "COLUMN_SUB_TYPE");
            }

            if (!reader.IsDBNull(reader.GetOrdinal("NUMERIC_SCALE")))
            {
                scale = wrapper.GetInt32(reader, "NUMERIC_SCALE");
            }

            var ftype = wrapper.GetInt32(reader, "FIELD_TYPE");

            return Tuple.Create(ftype, subtype, scale);
        }

        private Column SetDataType(IRecordWrapper wrapper, IDataReader reader, Column column)
        {
            var tuple = GetNumericTuple(wrapper, reader);

            column.NumericScale = tuple.Item3;
            column.DataType = GetDbDataType(tuple.Item1, tuple.Item2, tuple.Item3);
            return column;
        }

        private ViewColumn SetDataType(IRecordWrapper wrapper, IDataReader reader, ViewColumn column)
        {
            var tuple = GetNumericTuple(wrapper, reader);

            column.NumericScale = tuple.Item3;
            column.DataType = GetDbDataType(tuple.Item1, tuple.Item2, tuple.Item3);
            return column;
        }
    }
}
