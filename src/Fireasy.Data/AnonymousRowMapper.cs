// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common;
using Fireasy.Common.Extensions;
using Fireasy.Data.RecordWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data
{
    /// <summary>
    /// 一个用于将数据行转换为匿名类型对象的映射器。无法继承此类。
    /// </summary>
    /// <typeparam name="T">要构造的匿名类型。</typeparam>
    public class AnonymousRowMapper<T> : IDataRowMapper<T>
    {
        private Func<IDataReader, T> funcDataRecd;
        private Func<DataRow, T> funcDataRow;

        private class MethodCache
        {
            internal static readonly MethodInfo ToType = typeof(GenericExtension).GetMethod(nameof(GenericExtension.ToType));
            internal static readonly MethodInfo GetValue = typeof(IRecordWrapper).GetMethod(nameof(IRecordWrapper.GetValue), new[] { typeof(IDataReader), typeof(string) });
            internal static readonly PropertyInfo DataRowIndex = typeof(DataRow).GetProperty("Item", new[] { typeof(string) });
        }

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public T Map(IDatabase database, IDataReader reader)
        {
            if (funcDataRecd == null)
            {
                funcDataRecd = BuildExpressionForDataReader().Compile();
            }

            return funcDataRecd(reader);
        }

        /// <summary>
        /// 将一个 <see cref="DataRow"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="row">一个 <see cref="DataRow"/> 对象。</param>
        /// <returns>由 <see cref="DataRow"/> 中数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public T Map(IDatabase database, DataRow row)
        {
            if (funcDataRow == null)
            {
                funcDataRow = BuildExpressionForDataRow().Compile();
            }

            return funcDataRow(row);
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

        private IEnumerable<ParameterInfo> GetParameters(ConstructorInfo conInfo)
        {
            return conInfo.GetParameters().Where(s => Extensions.DataExtension.IsDbTypeSupported(s.ParameterType));
        }

        protected virtual Expression<Func<IDataReader, T>> BuildExpressionForDataReader()
        {
            var conInfo = typeof(T).GetConstructors().FirstOrDefault();
            Guard.NullReference(conInfo);
            var parExp = Expression.Parameter(typeof(IDataReader), "s");
            var parameters =
                GetParameters(conInfo).Select(s => (Expression)Expression.Convert(
                            Expression.Call(MethodCache.ToType, new Expression[] 
                                    { 
                                        Expression.Call(Expression.Constant(RecordWrapper), MethodCache.GetValue, new Expression[] { parExp, Expression.Constant(s.Name) }),
                                        Expression.Constant(s.ParameterType),
                                        Expression.Constant(null)
                                    }
                            ), s.ParameterType));

            var newExp = Expression.New(conInfo, parameters);

            return Expression.Lambda<Func<IDataReader, T>>(
                    Expression.MemberInit(newExp), parExp);
        }

        protected virtual Expression<Func<DataRow, T>> BuildExpressionForDataRow()
        {
            var conInfo = typeof(T).GetConstructors().FirstOrDefault();
            Guard.NullReference(conInfo);
            var parExp = Expression.Parameter(typeof(DataRow), "s");
            var parameters =
                GetParameters(conInfo).Select(s => (Expression)Expression.Convert(
                            Expression.Call(MethodCache.ToType, new Expression[] 
                                    { 
                                        Expression.MakeIndex(parExp, MethodCache.DataRowIndex, new List<Expression> { Expression.Constant(s.Name) }),
                                        Expression.Constant(s.ParameterType),
                                        Expression.Constant(null)
                                    }
                            ), s.ParameterType));

            var newExp = Expression.New(conInfo, parameters);

            return Expression.Lambda<Func<DataRow, T>>(
                    Expression.MemberInit(newExp), parExp);
        }
    }
}
