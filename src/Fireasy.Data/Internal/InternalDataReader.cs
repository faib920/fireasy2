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
    internal class InternalDataReader : IDataReader
    {
        private readonly DbConnection _connection;
        private readonly IDataReader _reader;
        private readonly bool _canCloseConnection;
        private readonly ReaderNestedlocked _locker;

        public InternalDataReader(IDbConnection connection, IDataReader reader, bool canCloseConnection, ReaderNestedlocked locker)
        {
            _connection = (DbConnection)connection;
            _reader = reader;
            _canCloseConnection = canCloseConnection;
            _locker = locker;

            locker.Increment();
        }

        public object this[int i] => _reader[i];

        public object this[string name] => _reader[name];

        public int Depth => _reader.Depth;

        public bool IsClosed => _reader.IsClosed;

        public int RecordsAffected => _reader.RecordsAffected;

        public int FieldCount => _reader.FieldCount;

        public void Close()
        {
            _reader.Close();
        }

        void IDisposable.Dispose()
        {
            _reader.Dispose();

            if (_locker.Decrement() == 0 && _canCloseConnection)
            {
                _connection.TryClose();
            }
        }

        public bool GetBoolean(int i)
        {
            return _reader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return _reader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return _reader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return _reader.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return _reader.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return _reader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return _reader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return _reader.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return _reader.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return _reader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return _reader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return _reader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return _reader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return _reader.GetInt64(i);
        }

        public string GetName(int i)
        {
            return _reader.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return _reader.GetOrdinal(name);
        }

        public DataTable GetSchemaTable()
        {
            return _reader.GetSchemaTable();
        }

        public string GetString(int i)
        {
            return _reader.GetString(i);
        }

        public object GetValue(int i)
        {
            return _reader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return _reader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return _reader.IsDBNull(i);
        }

        public bool NextResult()
        {
            return _reader.NextResult();
        }

        public bool Read()
        {
            return _reader.Read();
        }

        public async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return await ((DbDataReader)_reader).ReadAsync(cancellationToken);
        }
    }
}
