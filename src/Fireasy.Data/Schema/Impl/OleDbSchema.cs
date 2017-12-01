// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using Fireasy.Common.Extensions;

namespace Fireasy.Data.Schema
{
    /// <summary>
    /// OleDb相关数据库架构信息的获取方法。
    /// </summary>
    public sealed class OleDbSchema : SchemaBase
    {
        private static DataTable tbPrimary;

        public OleDbSchema()
        {
            AddRestrictionIndex<Table>(s => s.Catalog, s => s.Schema, s => s.Name);
            AddRestrictionIndex<Column>(s => s.Catalog, s => s.Schema, s => s.TableName, s => s.Name);
            AddRestrictionIndex<User>(s => s.Name);
            AddRestrictionIndex<Procedure>(s => s.Catalog, s => s.Schema, s => s.Name, s => s.Type);
            AddRestrictionIndex<ProcedureParameter>(s => s.Catalog, s => s.Schema, s => s.ProcedureName, s => s.Name);
            AddRestrictionIndex<Index>(s => s.Catalog, s => s.Schema, s => s.Name, null, s => s.TableName);
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
                    TableName = row["TABLE_NAME"].ToString().Replace("\"", "").Replace("'", "")
                };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
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
                    if (tbPrimary != null)
                    {
                        var tableName = t.TableName.Replace("\"", "").Replace("'", "");
                        var rows = tbPrimary.Select("TABLE_NAME = '" + tableName + "' AND COLUMN_NAME='" + t.Name + "'");
                        if (rows.Length > 0)
                        {
                            t.IsPrimaryKey = true;
                        }
                    }
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
                        Catalog = row["FK_TABLE_CATALOG"].ToString(),
                        Schema = row["FK_TABLE_SCHEMA"].ToString(),
                        Name = row["FK_NAME"].ToString(),
                        PKTable = row["PK_TABLE_NAME"].ToString(),
                        //FKTable = row["FK_TABLE_NAME"].ToString(),
                        PKColumn = row["PK_COLUMN_NAME"].ToString(),
                        ColumnName = row["FK_COLUMN_NAME"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="Procedure"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<Procedure> GetProcedures(DataTable table, Action<Procedure, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new Procedure
                    {
                        Catalog = row["PROCEDURE_CATALOG"].ToString(),
                        Schema = row["PROCEDURE_SCHEMA"].ToString(),
                        Name = row["PROCEDURE_NAME"].ToString(),
                        Type = row["PROCEDURE_TYPE"].ToString()
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 获取 <see cref="ProcedureParameter"/> 元数据序列。
        /// </summary>
        /// <param name="table">架构信息的表。</param>
        /// <param name="action">用于填充元数据的方法。</param>
        /// <returns></returns>
        protected override IEnumerable<ProcedureParameter> GetProcedureParameters(DataTable table, Action<ProcedureParameter, DataRow> action)
        {
            foreach (DataRow row in table.Rows)
            {
                var item = new ProcedureParameter
                    {
                        Catalog = row["PROCEDURE_CATALOG"].ToString(),
                        Schema = row["PROCEDURE_SCHEMA"].ToString(),
                        ProcedureName = row["PROCEDURE_NAME"].ToString(),
                        Direction = row["PARAMETER_TYPE"].ToString() == "4" ? ParameterDirection.Output : ParameterDirection.Input,
                        Name = row["PARAMETER_NAME"].ToString(),
                        DataType = row["TYPE_NAME"].ToString(),
                        Length = row["CHARACTER_MAXIMUM_LENGTH"].To<long>(),
                        NumericScale = row["NUMERIC_SCALE"].To<int>(),
                        NumericPrecision = row["NUMERIC_PRECISION"].To<int>(),
                    };
                if (action != null)
                {
                    action(item, row);
                }
                yield return item;
            }
        }

        /// <summary>
        /// 自定义获取架构的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection">当前的 <see cref="DbConnection"/> 对象。</param>
        /// <param name="collectionName">指定要返回的架构的名称。</param>
        /// <param name="restrictionValues">限制的数组。</param>
        /// <returns></returns>
        protected override DataTable DoGetSchemas<T>(DbConnection connection, string collectionName, string[] restrictionValues)
        {
            var oledbConn = connection as OleDbConnection;
            if (oledbConn == null)
            {
                return base.DoGetSchemas<T>(connection, collectionName, restrictionValues);
            }

            switch (typeof(T).Name)
            {
                case "ForeignKey":
                    return oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, restrictionValues);
                case "Table":
                    return oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, restrictionValues); ;
                case "View":
                    return oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Views, restrictionValues); ;
            }
            return base.DoGetSchemas<T>(connection, collectionName, restrictionValues);
        }

        /// <summary>
        /// 在枚举构架元素之前进行一些初始化的操作。
        /// </summary>
        /// <typeparam name="T">架构的类型。</typeparam>
        /// <param name="connection">数据库链接对象。</param>
        /// <param name="restrictionValues">限制数组。</param>
        protected override void BeforeReturnSchemaElements<T>(DbConnection connection, string[] restrictionValues)
        {
            if (typeof(T) == typeof(Column))
            {
                var res = GetTableRestrictions(restrictionValues);

                connection.As<OleDbConnection>(conn =>
                    {
                        tbPrimary = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, res);
                    });
            }

            base.BeforeReturnSchemaElements<T>(connection, restrictionValues);
        }

        private string[] GetTableRestrictions(string[] restrictionValues)
        {
            var res = new string[Math.Min(3, restrictionValues == null ? 0 : restrictionValues.Length)];
            for (var i = 0; i < res.Length; i++)
            {
                res[i] = restrictionValues[i];
            }
            return res;
        }
    }
}
#endif