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
using System.Linq;
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
            var columns = new List<Column>();

            if (restrictionValues.Length >= 2)
            {
                SqlCommand sql = $@"
PRAGMA {catalog}.TABLE_INFO('{restrictionValues[2]}')";

                columns.AddRange(ParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
                    {
                        TableName = restrictionValues[2],
                        Name = wrapper.GetString(reader, 1),
                        IsNullable = wrapper.GetInt32(reader, 3) == 1,
                        IsPrimaryKey = wrapper.GetInt32(reader, 4) == 1
                    }));
            }
            else
            {
                foreach (var tb in GetTables(database, new string[0]))
                {
                    SqlCommand sql = $@"
PRAGMA {catalog}.TABLE_INFO('{tb.Name}')";

                    columns.AddRange(ParseMetadata(database, sql, parameters, (wrapper, reader) => new Column
                        {
                            TableName = restrictionValues[2],
                            Name = wrapper.GetString(reader, 1),
                            IsNullable = wrapper.GetInt32(reader, 3) == 1,
                            IsPrimaryKey = wrapper.GetInt32(reader, 4) == 1
                        }));
                }
            }

            if (restrictionValues.Length >= 3)
            {
                columns = columns.Where(s => s.Name == restrictionValues[3]).ToList();
            }

            return columns;
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, string[] restrictionValues)
        {
            var parameters = new ParameterCollection();

            var catalog = restrictionValues.Length == 0 || string.IsNullOrEmpty(restrictionValues[0]) ? "main" : restrictionValues[0];
            var table = (string.Compare(catalog, "temp", StringComparison.OrdinalIgnoreCase) == 0) ? "sqlite_temp_master" : "sqlite_master";

            SqlCommand sql = $@"
PRAGMA {catalog}.FOREIGN_KEY_LIST('{restrictionValues[2]}')";

            return ParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
                {
                    TableName = restrictionValues[2],
                    ColumnName = wrapper.GetString(reader, 4),
                    PKTable = wrapper.GetString(reader, 2),
                    PKColumn = wrapper.GetString(reader, 3)
                });
        }
    }
}
