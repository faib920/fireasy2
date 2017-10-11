// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.RecordWrapper;
using System;
using System.Data;
using System.Linq.Expressions;

namespace Fireasy.Data
{
    public abstract class ExpressionRowMapper<T> : IDataRowMapper<T>
    {
        private Func<IDataReader, T> funcDataRecd;
        private Func<DataRow, T> funcDataRow;

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public T Map(IDataReader reader)
        {
            if (funcDataRecd == null)
            {
                funcDataRecd = BuildExpressionForDataReader().Compile();
            }

            var result = funcDataRecd(reader);
            if (Initializer != null)
            {
                Initializer(result);
            }

            return result;
        }

        /// <summary>
        /// 将一个 <see cref="DataRow"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="row">一个 <see cref="DataRow"/> 对象。</param>
        /// <returns>由 <see cref="DataRow"/> 中数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public T Map(DataRow row)
        {
            if (funcDataRow == null)
            {
                funcDataRow = BuildExpressionForDataRow().Compile();
            }

            var result = funcDataRow(row);
            if (Initializer != null)
            {
                Initializer(result);
            }

            return result;
        }

        /// <summary>
        /// 获取或设置 <see cref="IRecordWrapper"/>。
        /// </summary>
        public IRecordWrapper RecordWrapper { get; set; }

        /// <summary>
        /// 获取或设置对象的初始化器。
        /// </summary>
        public Action<object> Initializer { get; set; }

        object IDataRowMapper.Map(IDataReader reader)
        {
            return Map(reader);
        }

        object IDataRowMapper.Map(DataRow row)
        {
            return Map(row);
        }

        protected abstract Expression<Func<IDataReader, T>> BuildExpressionForDataReader();

        protected abstract Expression<Func<DataRow, T>> BuildExpressionForDataRow();
    }
}
