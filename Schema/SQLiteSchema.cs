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
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// SQLite相关数据库架构信息的获取方法。
    /// </summary>
    public class SQLiteSchema : SchemaBase
    {
        public SQLiteSchema()
        {
            AddRestrictionIndex<Table>(s => s.Catalog, s => s.Name);
            AddRestrictionIndex<Column>(s => s.Catalog, null, s => s.TableName, s => s.Name);
            AddRestrictionIndex<View>(s => s.Catalog, s => null, s => s.Name);
            AddRestrictionIndex<ViewColumn>(s => s.Catalog, null, s => s.ViewName, s => s.Name);
            AddRestrictionIndex<User>(s => s.Name);
            AddRestrictionIndex<Procedure>(s => s.Catalog, null, s => s.Name, s => s.Type);
            AddRestrictionIndex<ProcedureParameter>(s => s.Catalog, null, s => s.ProcedureName, s => s.Name);
            AddRestrictionIndex<Index>(s => s.Catalog, null, s => s.TableName, s => s.Name);
            AddRestrictionIndex<IndexColumn>(s => s.Catalog, null, s => s.TableName, s => s.Name, s => s.ColumnName);
            AddRestrictionIndex<ForeignKey>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            var catalog = restrictionValues.Length == 0 || string.IsNullOrEmpty(restrictionValues[0]) ? "main" : restrictionValues[0];
            var table = (string.Compare(catalog, "temp", StringComparison.OrdinalIgnoreCase) == 0) ? "sqlite_temp_master" : "sqlite_master";

            SqlCommand sql = $@"
SELECT name, type FROM {catalog}.{table}
WHERE type LIKE 'table'
AND (name = @NAME OR @NAME IS NULL)";

            ParameteRestrition(parameters, "NAME", 1, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
                {
                    Name = wrapper.GetString(reader, 0),
                    Type = wrapper.GetString(reader, 1)
                });
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            var catalog = restrictionValues.Length == 0 || string.IsNullOrEmpty(restrictionValues[0]) ? "main" : restrictionValues[0];
            var table = (string.Compare(catalog, "temp", StringComparison.OrdinalIgnoreCase) == 0) ? "sqlite_temp_master" : "sqlite_master";

            SqlCommand sql = $@"
SELECT name, type FROM {catalog}.{table}
WHERE type LIKE 'table'
AND (name = @NAME OR @NAME IS NULL)";

            ParameteRestrition(parameters, "NAME", 1, restrictionValues);

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
            {
                Name = wrapper.GetString(reader, 0),
                Type = wrapper.GetString(reader, 1)
            });
        }
    }
}
