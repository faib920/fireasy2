// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Extensions;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Fireasy.Data.Internal
{
    internal class InternalDbCommand : DisposableBase, IDbCommand
    {
        private IDbCommand _command;

        public InternalDbCommand(IDbCommand command)
        {
            _command = command;
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

        protected override bool Dispose(bool disposing)
        {
            if (disposing)
            {
                _command.Dispose();
                _command = null;
            }

            return true;
        }

        public int ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            ((DbConnection)_command.Connection).TryOpen();
            return new InternalDataReader(this, _command.ExecuteReader());
        }

        public async Task<IDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ((DbConnection)_command.Connection).TryOpenAsync();
            var reader = await ((DbCommand)_command).ExecuteReaderAsync(cancellationToken);
            return new InternalDataReader(this, reader);
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            ((DbConnection)_command.Connection).TryOpen();
            return new InternalDataReader(this, _command.ExecuteReader(behavior));
        }

        public async Task<IDataReader> ExecuteReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ((DbConnection)_command.Connection).TryOpenAsync();
            var reader = await ((DbCommand)_command).ExecuteReaderAsync(behavior, cancellationToken);
            return new InternalDataReader(this, reader);
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
