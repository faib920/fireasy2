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
using System.Linq;

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

        protected override IEnumerable<T> GetSchemas<T>(IDatabase database, SchemaCategory category, string[] restrictionValues)
        {
            switch (category)
            {
                //case SchemaCategory.Table:
                //    return GetTables(database, restrictionValues).Cast<T>();
                case SchemaCategory.Column:
                    return GetColumns(database, restrictionValues).Cast<T>();
                //case SchemaCategory.ForeignKey:
                //    return GetForeignKeys(database, restrictionValues).Cast<T>();
            }

            return base.GetSchemas<T>(database, category, restrictionValues);
        }

        /// <summary>
        /// 获取架构的名称。
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        protected override string GetSchemaCategoryName(SchemaCategory category)
        {
            switch (category)
            {
                case SchemaCategory.Column:
                    return "Columns";
                case SchemaCategory.DataType:
                    return "DataTypes";
                case SchemaCategory.MetadataCollection:
                    return "MetaDataCollections";
                case SchemaCategory.ReservedWord:
                    return "ReservedWords";
                case SchemaCategory.Restriction:
                    return "Restrictions";
                case SchemaCategory.Table:
                    return "Tables";
                case SchemaCategory.User:
                    return "Users";
                case SchemaCategory.View:
                    return "Views";
                default:
                    return base.GetSchemaCategoryName(category);
            }
        }

        private IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
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

            using (var reader = database.ExecuteReader(sql, parameters: parameters))
            {
                var wrapper = database.Provider.GetService<IRecordWrapper>();
                while (reader.Read())
                {
                    yield return new Column
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
                    };
                }
            }
        }

    }
}
