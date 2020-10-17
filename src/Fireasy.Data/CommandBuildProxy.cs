// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Data.Extensions;
using Fireasy.Data.Provider;
using Fireasy.Data.Syntax;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Fireasy.Data
{
    /// <summary>
    /// 一个抽象类，用于命令生成的代理。
    /// </summary>
    internal abstract class CommandBuildProxy
    {
        internal DbCommand SelectCommand { get; set; }

        internal DbCommand InsertCommand { get; set; }

        internal DbCommand UpdateCommand { get; set; }

        internal DbCommand DeleteCommand { get; set; }

        internal abstract void BuildCommands(IProvider provider, DataTable table, DbConnection connection, DbTransaction transaction);

        protected internal DbParameter CreateCommandParameter(IProvider provider, string prefix, DataColumn column)
        {
            var parameter = provider.DbProviderFactory.CreateParameter();
            Guard.NullReference(parameter);

            parameter.ParameterName = prefix + (column.Ordinal + 1);
            parameter.SourceColumn = column.ColumnName;
            parameter.DbType = column.DataType.GetDbType();
            return parameter;
        }

        protected internal void Initializate(IProvider provider, DbConnection connection, DbTransaction transaction)
        {
            SelectCommand = provider.DbProviderFactory.CreateCommand();
            InsertCommand = provider.DbProviderFactory.CreateCommand();
            UpdateCommand = provider.DbProviderFactory.CreateCommand();
            DeleteCommand = provider.DbProviderFactory.CreateCommand();

            if (SelectCommand == null || InsertCommand == null || UpdateCommand == null || DeleteCommand == null)
            {
                throw new NullReferenceException(SR.GetString(SRKind.UnableInitCommand));
            }

            SelectCommand.Connection =
                InsertCommand.Connection =
                UpdateCommand.Connection =
                DeleteCommand.Connection = connection;

            if (transaction != null)
            {
                SelectCommand.Transaction =
                    InsertCommand.Transaction =
                    UpdateCommand.Transaction =
                    DeleteCommand.Transaction = transaction;
            }
        }
    }

    /// <summary>
    /// 使用带有主键的 <see cref="DataTable"/> 生成命令。
    /// </summary>
    internal class CommandBuildProxyWithPrimaryKey : CommandBuildProxy
    {
        [SuppressMessage("Security", "CA2100")]
        internal override void BuildCommands(IProvider provider, DataTable table, DbConnection connection, DbTransaction transaction)
        {
            var syntax = provider.GetService<ISyntaxProvider>();
            Initializate(provider, connection, transaction);

            var tableName = syntax.DelimitTable(table.TableName);
            var sbSelect = new StringBuilder($"SELECT {{0}} FROM {tableName}");
            var sbInsert = new StringBuilder($"INSERT INTO {tableName}({{0}}) VALUES({{1}})");
            var sbUpdate = new StringBuilder($"UPDATE {tableName} SET {{0}} WHERE");
            var sbDelete = new StringBuilder($"DELETE FROM {tableName} WHERE");
            var flag = new AssertFlag();

            foreach (var column in table.PrimaryKey)
            {
                if (!flag.AssertTrue())
                {
                    sbUpdate.Append(" AND");
                    sbDelete.Append(" AND");
                }

                var set = $" {syntax.DelimitColumn(column.ColumnName)} = {syntax.ParameterPrefix}p_{column.Ordinal + 1}";
                sbUpdate.Append(set);
                sbDelete.Append(set);
                UpdateCommand.Parameters.Add(CreateCommandParameter(provider, "p_", column));
                DeleteCommand.Parameters.Add(CreateCommandParameter(provider, "p_", column));
            }

            var sbSelectFields = new StringBuilder();
            var sbInsertFields = new StringBuilder();
            var sbInsertValues = new StringBuilder();
            var sbUpdateSets = new StringBuilder();

            foreach (DataColumn column in table.Columns)
            {
                if (column.AutoIncrement)
                {
                    continue;
                }

                var columnName = syntax.DelimitColumn(column.ColumnName);
                if (sbSelectFields.Length > 0)
                {
                    sbSelectFields.Append(", ");
                }

                sbSelectFields.Append(columnName);
                if (sbInsertFields.Length > 0)
                {
                    sbInsertFields.Append(", ");
                    sbInsertValues.Append(", ");
                    sbUpdateSets.Append(", ");
                }

                sbInsertFields.Append(columnName);
                sbInsertValues.AppendFormat("{0}p{1}", syntax.ParameterPrefix, column.Ordinal + 1);
                sbUpdateSets.AppendFormat("{0} = {1}p{2}", columnName, syntax.ParameterPrefix, column.Ordinal + 1);

                InsertCommand.Parameters.Add(CreateCommandParameter(provider, "p", column));
                UpdateCommand.Parameters.Add(CreateCommandParameter(provider, "p", column));
            }

            SelectCommand.CommandText = string.Format(sbSelect.ToString(), sbSelectFields);
            InsertCommand.CommandText = string.Format(sbInsert.ToString(), sbInsertFields, sbInsertValues);
            UpdateCommand.CommandText = string.Format(sbUpdate.ToString(), sbUpdateSets);
            DeleteCommand.CommandText = sbDelete.ToString();
        }
    }

    /// <summary>
    /// 使用没有任何主键的 <see cref="DataTable"/> 生成命令。
    /// </summary>
    internal class CommandBuildProxyWithoutPrimaryKey : CommandBuildProxy
    {
        [SuppressMessage("Security", "CA2100")]
        internal override void BuildCommands(IProvider provider, DataTable table, DbConnection connection, DbTransaction transaction)
        {
            var syntax = provider.GetService<ISyntaxProvider>();
            Initializate(provider, connection, transaction);

            var tableName = syntax.DelimitTable(table.TableName);
            var sbSelect = new StringBuilder($"SELECT {{0}} FROM {tableName}");
            var sbInsert = new StringBuilder($"INSERT INTO {tableName}({{0}}) VALUES({{1}})");
            var sbUpdate = new StringBuilder($"UPDATE {tableName} SET {{0}} WHERE {{1}}");
            var sbDelete = new StringBuilder($"DELETE FROM {tableName} WHERE {{0}}");

            var sbFields = new StringBuilder();
            var sbInsertValues = new StringBuilder();
            var sbUpdateSets = new StringBuilder();
            var sbWhere = new StringBuilder();

            foreach (DataColumn column in table.Columns)
            {
                var columnName = syntax.DelimitColumn(column.ColumnName);
                if (sbFields.Length > 0)
                {
                    sbFields.Append(", ");
                    sbInsertValues.Append(", ");
                    sbUpdateSets.Append(", ");
                    sbWhere.Append(" AND ");
                }

                sbFields.Append(columnName);
                sbInsertValues.AppendFormat("{0}p{1}", syntax.ParameterPrefix, column.Ordinal + 1);
                sbUpdateSets.AppendFormat("{0} = {1}p{2}", columnName, syntax.ParameterPrefix, column.Ordinal + 1);
                sbWhere.AppendFormat("{0} = {1}p{2}", columnName, syntax.ParameterPrefix, column.Ordinal + 1);

                InsertCommand.Parameters.Add(CreateCommandParameter(provider, "p", column));
                UpdateCommand.Parameters.Add(CreateCommandParameter(provider, "p", column));
                DeleteCommand.Parameters.Add(CreateCommandParameter(provider, "p", column));
            }

            SelectCommand.CommandText = string.Format(sbSelect.ToString(), sbFields);
            InsertCommand.CommandText = string.Format(sbInsert.ToString(), sbFields, sbInsertValues);
            UpdateCommand.CommandText = string.Format(sbUpdate.ToString(), sbUpdateSets, sbWhere);
            DeleteCommand.CommandText = string.Format(sbDelete.ToString(), sbWhere);
        }
    }
}
