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
        private Func<IDatabase, IDataReader, T> funcDataRecd;
        private Func<IDatabase, DataRow, T> funcDataRow;

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public virtual T Map(IDatabase database, IDataReader reader)
        {
            if (funcDataRecd == null)
            {
                funcDataRecd = BuildExpressionForDataReader().Compile();
            }

            var result = funcDataRecd(database, reader);
            Initializer?.Invoke(result);

            return result;
        }

        /// <summary>
        /// 将一个 <see cref="DataRow"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="row">一个 <see cref="DataRow"/> 对象。</param>
        /// <returns>由 <see cref="DataRow"/> 中数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public virtual T Map(IDatabase database, DataRow row)
        {
            if (funcDataRow == null)
            {
                funcDataRow = BuildExpressionForDataRow().Compile();
            }

            var result = funcDataRow(database, row);
            Initializer?.Invoke(result);

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

        object IDataRowMapper.Map(IDatabase database, IDataReader reader)
        {
            return Map(database, reader);
        }

        object IDataRowMapper.Map(IDatabase database, DataRow row)
        {
            return Map(database, row);
        }

        /// <summary>
        /// 为 <see cref="IDataReader"/> 构造表达式。
        /// </summary>
        /// <returns></returns>
        protected abstract Expression<Func<IDatabase, IDataReader, T>> BuildExpressionForDataReader();

        /// <summary>
        /// 为 <see cref="DataReader"/> 构造表达式。
        /// </summary>
        /// <returns></returns>
        protected abstract Expression<Func<IDatabase, DataRow, T>> BuildExpressionForDataRow();
    }
}
