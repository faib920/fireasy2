// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Internal
{
    internal class InternalDataCommand : IDbCommand
    {
        private readonly IDbCommand command;
        private readonly ReaderNestedlocked locker;

        public InternalDataCommand(IDbCommand command, ReaderNestedlocked locker)
        {
            this.command = command;
            this.locker = locker;
        }

        public string CommandText
        {
            get { return command.CommandText; }
            set { command.CommandText = value; }
        }

        public int CommandTimeout
        {
            get { return command.CommandTimeout; }
            set { command.CommandTimeout = value; }
        }

        public CommandType CommandType
        {
            get { return command.CommandType; }
            set { command.CommandType = value; }
        }

        public IDbConnection Connection
        {
            get { return command.Connection; }
            set { command.Connection = value; }
        }

        public IDataParameterCollection Parameters
        {
            get { return command.Parameters; }
        }

        public IDbTransaction Transaction
        {
            get { return command.Transaction; }
            set { command.Transaction = value; }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get { return command.UpdatedRowSource; }
            set { command.UpdatedRowSource = value; }
        }

        public void Cancel()
        {
            command.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return command.CreateParameter();
        }

        void IDisposable.Dispose()
        {
            command.Dispose();
        }

        public int ExecuteNonQuery()
        {
            return command.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            return new InternalDataReader(command.Connection, command.ExecuteReader(), false, locker);
        }

        public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            var reader = await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
            return new InternalDataReader(command.Connection, reader, false, locker);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return new InternalDataReader(command.Connection, command.ExecuteReader(), behavior == CommandBehavior.CloseConnection, locker);
        }

        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var reader = await ((DbCommand)command).ExecuteReaderAsync(cancellationToken);
            return new InternalDataReader(command.Connection, reader, behavior == CommandBehavior.CloseConnection, locker);
        }

        public object ExecuteScalar()
        {
            return command.ExecuteScalar();
        }

        void IDbCommand.Prepare()
        {
            command.Prepare();
        }
    }
}
