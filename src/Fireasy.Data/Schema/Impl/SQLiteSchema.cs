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
            AddRestrictionIndex<Table>(s => s.Catalog, null, s => s.Name, s => s.Type);
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

        /// <summary>
        /// 获取 <see cref="Column"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<Column> GetColumns(DataTable table, Action<Column, DataRow> action)
        {
            return base.GetColumns(table, (t, r) =>
                {
                    t.Autoincrement = r["AUTOINCREMENT"] != DBNull.Value && r["AUTOINCREMENT"].ToString() == "True";
                    t.IsPrimaryKey = r["PRIMARY_KEY"] != DBNull.Value && r["PRIMARY_KEY"].ToString() == "True";
                });
        }

        /// <summary>
        /// 获取 <see cref="ForeignKey"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<ForeignKey> GetForeignKeys(DataTable table, Action<ForeignKey, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ForeignKey
                {
                    Catalog = row["CONSTRAINT_CATALOG"].ToString(),
                    Schema = row["CONSTRAINT_SCHEMA"].ToString(),
                    Name = row["CONSTRAINT_NAME"].ToString(),
                    TableName = row["TABLE_NAME"].ToString().Replace("\"", ""),
                    PKTable = row["FKEY_TO_TABLE"].ToString().Replace("\"", ""),
                    PKColumn = row["FKEY_TO_COLUMN"].ToString().Replace("\"", ""),
                    ColumnName = row["FKEY_FROM_COLUMN"].ToString().Replace("\"", "")
                };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="ViewColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<ViewColumn> GetViewColumns(DataTable table, Action<ViewColumn, DataRow> action)
        {
            return base.GetViewColumns(table, (t, r) =>
                 {
                    t.Default = r["COLUMN_DEFAULT"].ToString();
                    t.DataType = r["DATA_TYPE"].ToString();
                    t.NumericPrecision = r["NUMERIC_PRECISION"].To<int>();
                    t.NumericScale = r["NUMERIC_SCALE"].To<int>();
                    t.IsNullable = r["IS_NULLABLE"] == DBNull.Value ? false : r["IS_NULLABLE"].ToString() == "True";
                    t.Size = r["CHARACTER_MAXIMUM_LENGTH"].To<long>();
                    t.Position = r["ORDINAL_POSITION"].To<int>();
                });
        }
        
        /// <summary>
        /// 获取 <see cref="Index"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<Index> GetIndexs(DataTable table, Action<Index, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Index
                    {
                        Catalog = row["INDEX_CATALOG"].ToString(),
                        Schema = row["INDEX_SCHEMA"].ToString(),
                        Name = row["INDEX_NAME"].ToString(),
                        TableName = row["TABLE_NAME"].ToString().Replace("\"", ""),
                        IsPrimaryKey = row["PRIMARY_KEY"].ToString() == "True",
                        IsUnique = row["UNIQUE"].ToString() == "True"
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="IndexColumn"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<IndexColumn> GetIndexColumns(DataTable table, Action<IndexColumn, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new IndexColumn
                    {
                        Catalog = row["CONSTRAINT_CATALOG"].ToString(),
                        Schema = row["CONSTRAINT_SCHEMA"].ToString(),
                        Name = row["CONSTRAINT_NAME"].ToString(),
                        TableName = row["TABLE_NAME"].ToString().Replace("\"", ""),
                        ColumnName = row["COLUMN_NAME"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }
    }
}
