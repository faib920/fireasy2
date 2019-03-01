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

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// SQLite相关数据库架构信息的获取方法。
    /// </summary>
    public class SQLiteSchema : SchemaBase
    {
        public SQLiteSchema()
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

            SqlCommand sql = $@"
SELECT name, type FROM main.sqlite_master
WHERE type LIKE 'table'
AND (name = @NAME OR @NAME IS NULL)";

            restrictionValues.Parameterize(parameters, "NAME", nameof(Table.Name));

            return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new Table
            {
                Catalog = "main",
                Name = wrapper.GetString(reader, 0),
                Type = TableType.BaseTable
            });
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            var columns = new List<Column>();

            //如果指定使用表名查询
            if (restrictionValues.TryGetValue(nameof(Column.TableName), out string tableName))
            {
                SqlCommand sql = $@"
PRAGMA main.TABLE_INFO('{tableName}')";

                columns.AddRange(GetColumns(database, tableName));
            }
            else
            {
                //循环所有表，对每个表进行查询
                foreach (var tb in GetTables(database, RestrictionDictionary.Empty))
                {
                    SqlCommand sql = $@"
PRAGMA main.TABLE_INFO('{tb.Name}')";

                    columns.AddRange(GetColumns(database, tb.Name));
                }
            }

            //如果使用列名进行查询
            if (restrictionValues.TryGetValue(nameof(Column.Name), out string columnName))
            {
                columns = columns.Where(s => s.Name == columnName).ToList();
            }

            return columns;
        }

        private List<Column> GetColumns(IDatabase database, string tableName)
        {
            var columns = ExecuteAndParseMetadata(database, $"PRAGMA main.TABLE_INFO('{tableName}')", null, (wrapper, reader) => new Column
            {
                Catalog = "main",
                TableName = tableName,
                Name = wrapper.GetString(reader, 1),
                DataType = wrapper.GetString(reader, 2),
                IsNullable = wrapper.GetInt32(reader, 3) == 1,
                Default = wrapper.GetString(reader, 4),
                IsPrimaryKey = wrapper.GetInt32(reader, 5) == 1
            }).ToList();

            var sql = $"select * from main.[{tableName}]";

            if (database.Provider.DbProviderFactory.GetType().Assembly.GetName().Name != "System.Data.SQLite")
            {
                return columns;
            }

            using (var command = database.Provider.DbProviderFactory.CreateCommand())
            {
                if (database.Connection.State != ConnectionState.Open)
                {
                    database.Connection.Open();
                }

                command.CommandText = sql;
                command.Connection = database.Connection;
                using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
                {
                    var table = reader.GetSchemaTable();
                    foreach (DataRow row in table.Rows)
                    {
                        var column = columns.FirstOrDefault(s => s.Name == row["ColumnName"].ToString());
                        if (column == null)
                        {
                            continue;
                        }

                        column.Autoincrement = (bool)row["IsAutoincrement"];
                        column.NumericPrecision = row["NumericPrecision"] == DBNull.Value ? 0 : (int)row["NumericPrecision"];
                        column.NumericScale = row["NumericScale"] == DBNull.Value ? 0 : (int)row["NumericScale"];
                        column.DataType = row["DataTypeName"].ToString().ToLower();
                        column.Length = row["ColumnSize"] == DBNull.Value ? 0 : (int)row["ColumnSize"];
                    }
                }
            }

            return columns;
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var parameters = new ParameterCollection();

            //如果指定使用表名查询
            if (restrictionValues.TryGetValue(nameof(ForeignKey.TableName), out string tbName))
            {
                SqlCommand sql = $@"
PRAGMA main.FOREIGN_KEY_LIST('{tbName}')";

                return ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
                {
                    TableName = tbName,
                    ColumnName = wrapper.GetString(reader, 4),
                    PKTable = wrapper.GetString(reader, 2),
                    PKColumn = wrapper.GetString(reader, 3)
                });
            }
            else
            {
                var columns = new List<ForeignKey>();

                //循环所有表，对每个表进行查询
                foreach (var tb in GetTables(database, RestrictionDictionary.Empty))
                {
                    SqlCommand sql = $@"
PRAGMA main.FOREIGN_KEY_LIST('{tb.Name}')";

                    columns.AddRange(ExecuteAndParseMetadata(database, sql, parameters, (wrapper, reader) => new ForeignKey
                    {
                        TableName = tb.Name,
                        ColumnName = wrapper.GetString(reader, 4),
                        PKTable = wrapper.GetString(reader, 2),
                        PKColumn = wrapper.GetString(reader, 3)
                    }));
                }

                return columns;
            }
        }
    }
}
