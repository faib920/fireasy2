// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920?126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace Fireasy.Data.Schema
{
    public class OleDbSchema : SchemaBase
    {
        public OleDbSchema()
        {
            AddRestriction<Table>(s => s.Name, s => s.Type);
            AddRestriction<View>(s => s.Name);
            AddRestriction<Column>(s => s.TableName, s => s.Name);
            AddRestriction<ForeignKey>(s => s.TableName, s => s.Name);

            AddDataType("byte", DbType.Byte, typeof(byte));
            AddDataType("short", DbType.Int16, typeof(short));
            AddDataType("long", DbType.Int64, typeof(long));
            AddDataType("single", DbType.Single, typeof(float));
            AddDataType("double", DbType.Double, typeof(double));
            AddDataType("bit", DbType.Boolean, typeof(bool));
            AddDataType("decimal", DbType.Decimal, typeof(decimal));
            AddDataType("currency", DbType.Currency, typeof(decimal));
            AddDataType("varbinary", DbType.Binary, typeof(byte[]));
            AddDataType("longbinary", DbType.Binary, typeof(byte[]));
            AddDataType("bigbinary", DbType.Binary, typeof(byte[]));
            AddDataType("guid", DbType.Guid, typeof(Guid));
            AddDataType("varchar", DbType.String, typeof(string));
            AddDataType("longtext", DbType.Binary, typeof(string));
            AddDataType("datetime", DbType.DateTime, typeof(DateTime));
        }

        protected override IEnumerable<Table> GetTables(IDatabase database, RestrictionDictionary restrictionValues)
        {
            string tableType = null;
            switch (restrictionValues.GetValue(nameof(Table.Type)))
            {
                case null:
                case "0":
                    tableType = "TABLE";
                    break;
                case "1":
                    tableType = "SYSTEM TABLE";
                    break;
            }

            var restrictions = new[] { null, null, restrictionValues.GetValue(nameof(Table.Name)), tableType };
            var conn = (OleDbConnection)database.Connection.TryOpen();

            foreach (DataRow row in conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrictions).Rows)
            {
                yield return new Table
                {
                    Catalog = row["TABLE_CATALOG"].ToString(),
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    Name = row["TABLE_NAME"].ToString(),
                    Type = row["TABLE_TYPE"].ToString() == "TABLE" ? TableType.BaseTable : TableType.SystemTable,
                    Description = ""
                };
            }
        }

        protected override IEnumerable<Column> GetColumns(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var restrictions = new[] { null, null, restrictionValues.GetValue(nameof(Column.TableName)), restrictionValues.GetValue(nameof(Column.Name)) };
            var conn = (OleDbConnection)database.Connection.TryOpen();

            var tbPrimary = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new[] { null, null, restrictionValues.GetValue(nameof(Column.TableName)) });

            foreach (DataRow row in conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, restrictions).Rows)
            {
                yield return new Column
                {
                    Catalog = row["TABLE_CATALOG"].ToString(),
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    TableName = row["TABLE_NAME"].ToString(),
                    Name = row["COLUMN_NAME"].ToString(),
                    Default = row["COLUMN_DEFAULT"].ToString(),
                    DataType = Enum.Parse(typeof(OleDbType), row["DATA_TYPE"].ToString()).ToString(),
                    NumericPrecision = row["NUMERIC_PRECISION"] == DBNull.Value ? (int?)null : row["NUMERIC_PRECISION"].To<int>(),
                    NumericScale = row["NUMERIC_SCALE"] == DBNull.Value ? (int?)null : row["NUMERIC_SCALE"].To<int>(),
                    IsNullable = row["IS_NULLABLE"] != DBNull.Value && row["IS_NULLABLE"].To<bool>(),
                    Length = row["CHARACTER_MAXIMUM_LENGTH"] == DBNull.Value ? (long?)null : row["CHARACTER_MAXIMUM_LENGTH"].To<long>(),
                    Position = row["ORDINAL_POSITION"].To<int>(),
                    IsPrimaryKey = tbPrimary.Select($"TABLE_NAME = '{row["TABLE_NAME"].ToString().Replace("\"", "").Replace("'", "")}' AND COLUMN_NAME='{row["COLUMN_NAME"].ToString()}'").Length > 0
                };
            }
        }

        protected override IEnumerable<View> GetViews(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var restrictions = new[] { null, null, restrictionValues.GetValue(nameof(View.Name)) };
            var conn = (OleDbConnection)database.Connection.TryOpen();

            foreach (DataRow row in conn.GetOleDbSchemaTable(OleDbSchemaGuid.Views, restrictions).Rows)
            {
                yield return new View
                {
                    Catalog = row["TABLE_CATALOG"].ToString(),
                    Schema = row["TABLE_SCHEMA"].ToString(),
                    Name = row["TABLE_NAME"].ToString(),
                    Description = ""
                };
            }
        }

        protected override IEnumerable<ForeignKey> GetForeignKeys(IDatabase database, RestrictionDictionary restrictionValues)
        {
            var restrictions = new[] { null, null, restrictionValues.GetValue(nameof(ForeignKey.TableName)), restrictionValues.GetValue(nameof(ForeignKey.Name)) };
            var conn = (OleDbConnection)database.Connection.TryOpen();

            foreach (DataRow row in conn.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, restrictions).Rows)
            {
                yield return new ForeignKey
                {
                    Catalog = row["FK_TABLE_CATALOG"].ToString(),
                    Schema = row["FK_TABLE_SCHEMA"].ToString(),
                    Name = row["FK_NAME"].ToString(),
                    PKTable = row["PK_TABLE_NAME"].ToString(),
                    PKColumn = row["PK_COLUMN_NAME"].ToString(),
                    TableName = row["FK_TABLE_NAME"].ToString(),
                    ColumnName = row["FK_COLUMN_NAME"].ToString()
                };
            }
        }
    }
}