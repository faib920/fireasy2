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
        private readonly DbConnection connection;
        private readonly IDataReader reader;
        private readonly bool canCloseConnection;
        private readonly ReaderNestedlocked locker;

        public InternalDataReader(IDbConnection connection, IDataReader reader, bool canCloseConnection, ReaderNestedlocked locker)
        {
            this.connection = (DbConnection)connection;
            this.reader = reader;
            this.canCloseConnection = canCloseConnection;
            this.locker = locker;
            locker.Increment();
        }

        public object this[int i] => reader[i];

        public object this[string name] => reader[name];

        public int Depth => reader.Depth;

        public bool IsClosed => reader.IsClosed;

        public int RecordsAffected => reader.RecordsAffected;

        public int FieldCount => reader.FieldCount;

        public void Close()
        {
            reader.Close();
        }

        void IDisposable.Dispose()
        {
            reader.Dispose();

            if (locker.Decrement() == 0 && canCloseConnection)
            {
                connection.TryClose();
            }
        }

        public bool GetBoolean(int i)
        {
            return reader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return reader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return reader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public IDataReader GetData(int i)
        {
            return reader.GetData(i);
        }

        public string GetDataTypeName(int i)
        {
            return reader.GetDataTypeName(i);
        }

        public DateTime GetDateTime(int i)
        {
            return reader.GetDateTime(i);
        }

        public decimal GetDecimal(int i)
        {
            return reader.GetDecimal(i);
        }

        public double GetDouble(int i)
        {
            return reader.GetDouble(i);
        }

        public Type GetFieldType(int i)
        {
            return reader.GetFieldType(i);
        }

        public float GetFloat(int i)
        {
            return reader.GetFloat(i);
        }

        public Guid GetGuid(int i)
        {
            return reader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return reader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return reader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return reader.GetInt64(i);
        }

        public string GetName(int i)
        {
            return reader.GetName(i);
        }

        public int GetOrdinal(string name)
        {
            return reader.GetOrdinal(name);
        }

        public DataTable GetSchemaTable()
        {
            return reader.GetSchemaTable();
        }

        public string GetString(int i)
        {
            return reader.GetString(i);
        }

        public object GetValue(int i)
        {
            return reader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return reader.GetValues(values);
        }

        public bool IsDBNull(int i)
        {
            return reader.IsDBNull(i);
        }

        public bool NextResult()
        {
            return reader.NextResult();
        }

        public bool Read()
        {
            return reader.Read();
        }

        public async Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return await ((DbDataReader)reader).ReadAsync(cancellationToken);
        }
    }
}
