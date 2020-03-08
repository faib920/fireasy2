// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Data.Provider;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Fireasy.Data.Extensions
{
    /// <summary>
    /// 数据库提供者的扩展方法。
    /// </summary>
    public static class ProviderExtension
    {
        /// <summary>
        /// 创建一个新的 <see cref="DbConnection"/> 对象。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <returns></returns>
        public static DbConnection CreateConnection(this IProvider provider, string connectionString)
        {
            Guard.ArgumentNull(provider, nameof(provider));
            Guard.NullReference(provider.DbProviderFactory);

            var connection = provider.DbProviderFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            return connection;
        }

        /// <summary>
        /// 创建一个新的 <see cref="DbCommand"/> 对象。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandText"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [SuppressMessage("Sercurity", "CA2100")]
        public static DbCommand CreateCommand(this IProvider provider, DbConnection connection, DbTransaction transaction, string commandText, CommandType commandType = CommandType.Text, IEnumerable<Parameter> parameters = null)
        {
            Guard.ArgumentNull(provider, nameof(provider));
            Guard.NullReference(provider.DbProviderFactory);

            var command = provider.DbProviderFactory.CreateCommand();
            command.Connection = connection;
            command.CommandType = commandType;
            command.CommandText = commandText;
            command.Transaction = transaction;

            if (parameters != null)
            {
                command.PrepareParameters(provider, parameters);
            }
            return command;
        }

        /// <summary>
        /// 创建一个新的 <see cref="DbParameter"/> 对象。
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DbParameter CreateParameter(this IProvider provider, string parameterName, object value)
        {
            Guard.ArgumentNull(provider, nameof(provider));
            Guard.NullReference(provider.DbProviderFactory);

            var parameter = provider.DbProviderFactory.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value;
            return parameter;
        }
    }
}