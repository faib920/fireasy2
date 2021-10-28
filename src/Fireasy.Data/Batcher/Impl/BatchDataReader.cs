// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Data.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Fireasy.Data.Batcher
{
    /// <summary>
    /// <see cref="DataTable"/> 的读取器。
    /// </summary>
    public class DataTableBatchReader : DbDataReader
    {
        private readonly DataTable _table;
        private DataRow _current;
        private int _index;

        /// <summary>
        /// 初始化 <see cref="DataTableBatchReader"/> 类的新实例。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="initializer"></param>
        public DataTableBatchReader(DataTable table, Action<int, string> initializer)
        {
            _table = table;

            for (int i = 0, n = table.Columns.Count; i < n; i++)
            {
                initializer(i, table.Columns[i].ColumnName);
            }
        }

        public override object this[int i] => throw new NotSupportedException();

        public override object this[string name] => throw new NotSupportedException();

        public override int Depth => 1;

        public override bool IsClosed => false;

        public override int RecordsAffected => 0;

        public override int FieldCount => _table.Columns.Count;

        public override bool HasRows => _table.Rows.Count > 0;

        public override void Close()
        {
        }

        public override bool GetBoolean(int i)
        {
            throw new NotSupportedException();
        }

        public override byte GetByte(int i)
        {
            throw new NotSupportedException();
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public override char GetChar(int i)
        {
            throw new NotSupportedException();
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int i)
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int i)
        {
            throw new NotSupportedException();
        }

        public override decimal GetDecimal(int i)
        {
            throw new NotSupportedException();
        }

        public override double GetDouble(int i)
        {
            throw new NotSupportedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public override Type GetFieldType(int i)
        {
            return _table.Columns[i].DataType;
        }

        public override float GetFloat(int i)
        {
            throw new NotSupportedException();
        }

        public override Guid GetGuid(int i)
        {
            throw new NotSupportedException();
        }

        public override short GetInt16(int i)
        {
            throw new NotSupportedException();
        }

        public override int GetInt32(int i)
        {
            throw new NotSupportedException();
        }

        public override long GetInt64(int i)
        {
            throw new NotSupportedException();
        }

        public override string GetName(int i)
        {
            throw new NotSupportedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotSupportedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override string GetString(int i)
        {
            throw new NotSupportedException();
        }

        public override object GetValue(int i)
        {
            return _current[i];
        }

        public override int GetValues(object[] values)
        {
            throw new NotSupportedException();
        }

        public override bool IsDBNull(int i)
        {
            return _current[i] == DBNull.Value;
        }

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read()
        {
            if (_index >= _table.Rows.Count)
            {
                return false;
            }

            _current = _table.Rows[_index++];

            return true;
        }
    }

    /// <summary>
    /// <see cref="IEnumerable"/> 的读取器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerableBatchReader<T> : DbDataReader
    {
        private IEnumerator<T> _enumerator;
        private T _current;
        private bool _hasRows;
        private readonly List<Func<object, object>> _values = new List<Func<object, object>>();

        /// <summary>
        /// 初始化 <see cref="EnumerableBatchReader"/> 类的新实例。
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="initializer"></param>
        public EnumerableBatchReader(IEnumerable<T> enumerable, Action<int, string> initializer)
        {
            InitAccessories(enumerable, initializer);
            _enumerator = enumerable.GetEnumerator();
        }

        public override object this[int i] => throw new NotSupportedException();

        public override object this[string name] => throw new NotSupportedException();

        public override int Depth => 1;

        public override bool IsClosed => false;

        public override int RecordsAffected => 0;

        public override int FieldCount => _values.Count;

        public override bool HasRows => _hasRows;

        public override void Close()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_enumerator != null)
                {
                    _enumerator.Dispose();
                    _enumerator = null;
                }
            }
        }

        public override bool GetBoolean(int i)
        {
            throw new NotSupportedException();
        }

        public override byte GetByte(int i)
        {
            throw new NotSupportedException();
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public override char GetChar(int i)
        {
            throw new NotSupportedException();
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int i)
        {
            throw new NotSupportedException();
        }

        public override DateTime GetDateTime(int i)
        {
            throw new NotSupportedException();
        }

        public override decimal GetDecimal(int i)
        {
            throw new NotSupportedException();
        }

        public override double GetDouble(int i)
        {
            throw new NotSupportedException();
        }

        public override IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public override Type GetFieldType(int i)
        {
            throw new NotSupportedException();
        }

        public override float GetFloat(int i)
        {
            throw new NotSupportedException();
        }

        public override Guid GetGuid(int i)
        {
            throw new NotSupportedException();
        }

        public override short GetInt16(int i)
        {
            throw new NotSupportedException();
        }

        public override int GetInt32(int i)
        {
            throw new NotSupportedException();
        }

        public override long GetInt64(int i)
        {
            throw new NotSupportedException();
        }

        public override string GetName(int i)
        {
            throw new NotSupportedException();
        }

        public override int GetOrdinal(string name)
        {
            throw new NotSupportedException();
        }

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }

        public override string GetString(int i)
        {
            throw new NotSupportedException();
        }

        public override object GetValue(int i)
        {
            return _values[i](_current);
        }

        public override int GetValues(object[] values)
        {
            throw new NotSupportedException();
        }

        public override bool IsDBNull(int i)
        {
            return GetValue(i) == DBNull.Value;
        }

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read()
        {
            if (_enumerator.MoveNext())
            {
                _current = _enumerator.Current;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 初始化对象访问器。
        /// </summary>
        /// <param name="list"></param>
        private void InitAccessories(IEnumerable<T> list, Action<int, string> initializer)
        {
            var e = list.GetEnumerator();
            if (!e.MoveNext())
            {
                return;
            }

            _hasRows = true;

            if (e.Current is IPropertyFieldMappingResolver resolver)
            {
                foreach (var map in resolver.GetDbMapping())
                {
                    initializer(_values.Count, map.FieldName);
                    _values.Add(map.ValueFunc);

                }
            }
            else
            {
                TypeDescriptorUtility.AddDefaultDynamicProvider();
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(e.Current))
                {
                    if (property.PropertyType.IsDbTypeSupported())
                    {
                        initializer(_values.Count, property.Name);
                        _values.Add(o => property.GetValue(o));
                    }
                }
            }
        }
    }
}
