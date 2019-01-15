// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// PostgreSql相关数据库架构信息的获取方法。
    /// </summary>
    public class PostgreSqlSchema : SchemaBase
    {
        public PostgreSqlSchema()
        {
            AddRestrictionIndex<Table>(s => s.Catalog, s => s.Schema, s => s.Name, s => s.Type);
            AddRestrictionIndex<Column>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<User>(s => s.Name);
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            SqlCommand sql = @"
   SELECT
    col.table_schema ,
    col.table_name ,
    col.ordinal_position,
    col.column_name ,
    col.data_type ,
    col.character_maximum_length,
    col.numeric_precision,
    col.numeric_scale,
    col.is_nullable,
    col.column_default ,
    des.description,
		(case when col.ordinal_position = con.conkey[1] then 'YES' else 'NO' end) is_key
FROM
    information_schema.columns col
LEFT JOIN pg_description des
    ON col.table_name::regclass = des.objoid
    AND col.ordinal_position = des.objsubid
LEFT JOIN pg_constraint con
		ON con.conrelid = des.objoid
WHERE col.table_name = 'orders'
ORDER BY
    ordinal_position";

            ParameteRestrition(parameters, "OWNER", 0, restrictionValues);
            ParameteRestrition(parameters, "TABLENAME", 1, restrictionValues);
            ParameteRestrition(parameters, "COLUMNNAME", 2, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
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
    }
}
