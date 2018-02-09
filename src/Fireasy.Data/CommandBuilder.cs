// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Data;
using System.Data.Common;
using Fireasy.Common;
using Fireasy.Data.Provider;

namespace Fireasy.Data
{
    /// <summary>
    /// 自动生成单表命令，用于将对 <see cref="DataTable"/> 所做的更改与数据库的更改相协调。无法继承此类。
    /// </summary>
    public sealed class CommandBuilder
    {
        /// <summary>
        /// 使用关联的 <see cref="IDatabase"/> 对象初始化一个 <see cref="CommandBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="provider">当前数据环境中的提供者对象。</param>
        /// <param name="table">数据表对象。</param>
        /// <param name="connection">数据库连接对象。</param>
        /// <param name="transaction">数据库事务对象。</param>
        public CommandBuilder(IProvider provider, DataTable table, DbConnection connection, DbTransaction transaction)
        {
            Guard.ArgumentNull(provider, nameof(provider));
            Guard.ArgumentNull(table, nameof(table));
            Guard.ArgumentNull(connection, nameof(connection));

            var proxy = table.PrimaryKey.Length > 0
                        ? (CommandBuildProxy) new CommandBuildProxyWithPrimaryKey()
                        : new CommandBuildProxyWithoutPrimaryKey();

            proxy.BuildCommands(provider, table, connection, transaction);

            SelectCommand = proxy.SelectCommand;
            DeleteCommand = proxy.DeleteCommand;
            InsertCommand = proxy.InsertCommand;
            DeleteCommand = proxy.DeleteCommand;
            UpdateCommand = proxy.UpdateCommand;
        }

        /// <summary>
        /// 获取自动生成的、对数据库执行选择操作所需的 <see cref="DbCommand"/> 对象。
        /// </summary>
        public DbCommand SelectCommand { get; private set; }

        /// <summary>
        /// 获取自动生成的、对数据库执行插入操作所需的 <see cref="DbCommand"/> 对象。
        /// </summary>
        public DbCommand InsertCommand { get; private set; }

        /// <summary>
        /// 获取自动生成的、对数据库执行更新操作所需的 <see cref="DbCommand"/> 对象。
        /// </summary>
        public DbCommand UpdateCommand { get; private set; }

        /// <summary>
        /// 获取自动生成的、对数据库执行删除操作所需的 <see cref="DbCommand"/> 对象。
        /// </summary>
        public DbCommand DeleteCommand { get; private set; }

        /// <summary>
        /// 将生成的各命令填充到 <see cref="DbDataAdapter"/>。
        /// </summary>
        /// <param name="adapter">数据适配器。</param>
        public void FillAdapter(DbDataAdapter adapter)
        {
            adapter.SelectCommand = SelectCommand;
            adapter.UpdateCommand = UpdateCommand;
            adapter.InsertCommand = InsertCommand;
            adapter.DeleteCommand = DeleteCommand;
        }
    }
}
