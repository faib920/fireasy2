// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Extensions;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Internal
{
    internal class InternalDbCommand : IDbCommand
    {
        private readonly IDbCommand _command;
        private readonly ReaderNestedlocked _locker;

        public InternalDbCommand(IDbCommand command, ReaderNestedlocked locker)
        {
            _command = command;
            _locker = locker;
        }

        public string CommandText
        {
            get { return _command.CommandText; }
            set { _command.CommandText = value; }
        }

        public int CommandTimeout
        {
            get { return _command.CommandTimeout; }
            set { _command.CommandTimeout = value; }
        }

        public CommandType CommandType
        {
            get { return _command.CommandType; }
            set { _command.CommandType = value; }
        }

        public IDbConnection Connection
        {
            get { return _command.Connection; }
            set { _command.Connection = value; }
        }

        public IDataParameterCollection Parameters
        {
            get { return _command.Parameters; }
        }

        public IDbTransaction Transaction
        {
            get { return _command.Transaction; }
            set { _command.Transaction = value; }
        }

        public UpdateRowSource UpdatedRowSource
        {
            get { return _command.UpdatedRowSource; }
            set { _command.UpdatedRowSource = value; }
        }

        public void Cancel()
        {
            _command.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return _command.CreateParameter();
        }

        void IDisposable.Dispose()
        {
            _command.Dispose();
        }

        public int ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            ((DbConnection)_command.Connection).TryOpen();
            return new InternalDataReader(_command, _command.ExecuteReader(), _locker);
        }

        public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ((DbConnection)_command.Connection).TryOpenAsync();
            var reader = await ((DbCommand)_command).ExecuteReaderAsync(cancellationToken);
            return new InternalDataReader(_command, reader, _locker);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            ((DbConnection)_command.Connection).TryOpen();
            return new InternalDataReader(_command, _command.ExecuteReader(behavior), _locker);
        }

        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ((DbConnection)_command.Connection).TryOpenAsync();
            var reader = await ((DbCommand)_command).ExecuteReaderAsync(behavior, cancellationToken);
            return new InternalDataReader(_command, reader, _locker);
        }

        public object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }

        void IDbCommand.Prepare()
        {
            _command.Prepare();
        }
    }
}
