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
        private Func<IDatabase, IDataReader, T> _funcDataRecd;

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public virtual T Map(IDatabase database, IDataReader reader)
        {
            if (_funcDataRecd == null)
            {
                _funcDataRecd = BuildExpressionForDataReader().Compile();
            }

            var result = _funcDataRecd(database, reader);
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

        /// <summary>
        /// 为 <see cref="IDataReader"/> 构造表达式。
        /// </summary>
        /// <returns></returns>
        protected abstract Expression<Func<IDatabase, IDataReader, T>> BuildExpressionForDataReader();
    }
}
