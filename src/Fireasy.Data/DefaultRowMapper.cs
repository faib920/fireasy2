using Fireasy.Common.Extensions;
using Fireasy.Data.Extensions;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data
{
    /// <summary>
    /// 一个缺省的数据行映射器。无法继承此类。
    /// </summary>
    /// <typeparam name="T">要转换的类型。</typeparam>
    public sealed class DefaultRowMapper<T> : FieldRowMapperBase<T>
    {
        private Func<IDataReader, T> funcDataRecd;
        private Func<DataRow, T> funcDataRow;
        private static MethodInfo isNullMethod = typeof(IDataRecord).GetMethod(nameof(IDataReader.IsDBNull), new[] { typeof(int) });
        private static MethodInfo convertMethod = typeof(Extensions.DataExtension).GetMethod(nameof(Extensions.DataExtension.ToTypeEx), BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public override T Map(IDatabase database, IDataReader reader)
        {
            if (funcDataRecd == null)
            {
                CompileFunction(reader);
            }

            var result = funcDataRecd(reader);
            Initializer?.Invoke(result);

            return result;
        }

        /// <summary>
        /// 将一个 <see cref="DataRow"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="row">一个 <see cref="DataRow"/> 对象。</param>
        /// <returns>由 <see cref="DataRow"/> 中数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public override T Map(IDatabase database, DataRow row)
        {
            if (funcDataRecd == null)
            {
                CompileFunction(row);
            }

            var result = funcDataRow(row);
            Initializer?.Invoke(result);

            return result;
        }

        private IEnumerable<PropertyInfo> GetProperties()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(s => DataExtension.IsDbTypeSupported(s.PropertyType));
        }

        private IEnumerable<PropertyMapping> GetMapping(string[] fields)
        {
            return from s in GetProperties()
                          let index = IndexOf(fields, s.Name)
                          where s.CanWrite && index != -1 && s.GetIndexParameters().Length == 0
                          select new PropertyMapping { Info = s, Index = index };
        }

        private void CompileFunction(IDataReader reader)
        {
            var newExp = Expression.New(typeof(T));
            var mapping = GetMapping(GetDataReaderFields(reader));

            var rowMapExp = Expression.Constant(this.RecordWrapper);
            var parExp = Expression.Parameter(typeof(IDataRecord), "s");

            var bindings =
                mapping.Select(s => {
                    var dbType = reader.GetFieldType(s.Index);
                    var getValueMethod = Fireasy.Data.RecordWrapper.RecordWrapHelper.GetMethodByOrdinal(dbType.GetDbType());
                    //ToTypeEx<TS, TC>()
                    var convertMT = convertMethod.MakeGenericMethod(dbType, s.Info.PropertyType);

                    var expression = (Expression)Expression.Call(rowMapExp, getValueMethod, new Expression[] { parExp, Expression.Constant(s.Index) });
                    var convertExp = Expression.Call(convertMT, new Expression[] { expression });

                    if (s.Info.PropertyType.IsNullableType())
                    {
                        expression = Expression.Condition(
                            Expression.Call(parExp, isNullMethod, Expression.Constant(s.Index, typeof(int))),
                            Expression.Convert(Expression.Constant(null), s.Info.PropertyType),
                        convertExp);
                    }
                    else if (dbType != s.Info.PropertyType)
                    {
                        expression = convertExp;
                    }

                    return Expression.Bind(s.Info, expression);
                });

            var expr =
                Expression.Lambda<Func<IDataReader, T>>(
                    Expression.MemberInit(
                        newExp,
                        bindings.ToArray()),
                    parExp);

            funcDataRecd = expr.Compile();
        }

        private void CompileFunction(DataRow row)
        {
            var newExp = Expression.New(typeof(T));
            var mapping = GetMapping(GetDataRowFields(row));

            var parExp = Expression.Parameter(typeof(DataRow), "s");
            var convertMethod = typeof(Extensions.DataExtension).GetMethod("ToTypeEx", BindingFlags.NonPublic | BindingFlags.Static);
            var itemProperty = typeof(DataRow).GetProperty("Item", new[] { typeof(int) });
            var bindings =
                mapping.Select(s => (MemberBinding)
                    Expression.Bind(
                        s.Info,
                        Expression.Convert(
                            Expression.Call(convertMethod, new Expression[] 
                                { 
                                    Expression.MakeIndex(parExp, itemProperty, new List<Expression> { Expression.Constant(s.Index) }),
                                    Expression.Constant(s.Info.PropertyType),
                                    Expression.Constant(null)
                                }
                            ), s.Info.PropertyType)));

            var expr =
                Expression.Lambda<Func<DataRow, T>>(
                    Expression.MemberInit(
                        newExp,
                        bindings),
                    parExp);

            funcDataRow = expr.Compile();
        }
    }
}
