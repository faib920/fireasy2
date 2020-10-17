using Fireasy.Common.Extensions;
using Fireasy.Data.Converter;
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
        private Func<IDataReader, T> _funcDataRecd;

        private class MethodCache
        {
            internal protected static readonly MethodInfo IsDBNull = typeof(IDataRecord).GetMethod(nameof(IDataReader.IsDBNull), new[] { typeof(int) });
            internal protected static readonly MethodInfo ConvertFrom = typeof(IValueConverter).GetMethod(nameof(IValueConverter.ConvertFrom));
        }

        /// <summary>
        /// 将一个 <see cref="IDataReader"/> 转换为一个 <typeparamref name="T"/> 的对象。
        /// </summary>
        /// <param name="database">当前的 <see cref="IDatabase"/> 对象。</param>
        /// <param name="reader">一个 <see cref="IDataReader"/> 对象。</param>
        /// <returns>由当前 <see cref="IDataReader"/> 对象中的数据转换成的 <typeparamref name="T"/> 对象实例。</returns>
        public override T Map(IDatabase database, IDataReader reader)
        {
            if (_funcDataRecd == null)
            {
                CompileFunction(reader);
            }

            return _funcDataRecd(reader);
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

            var rowMapExp = Expression.Constant(RecordWrapper);
            var parExp = Expression.Parameter(typeof(IDataRecord), "s");

            var bindings =
                mapping.Select(s =>
                {
                    var dbType = reader.GetFieldType(s.Index);
                    var getValueMethod = Data.RecordWrapper.RecordWrapHelper.GetMethodByOrdinal(dbType.GetDbType());

                    var expression = (Expression)Expression.Call(rowMapExp, getValueMethod, new Expression[] { parExp, Expression.Constant(s.Index) });

                    if (ConvertManager.CanConvert(s.Info.PropertyType))
                    {
                        var converter = Expression.Call(typeof(ConvertManager), nameof(ConvertManager.GetConverter), null, Expression.Constant(s.Info.PropertyType));
                        expression = Expression.Call(converter, MethodCache.ConvertFrom, Expression.Convert(expression, typeof(object)), Expression.Constant(dbType.GetDbType()));
                        expression = Expression.Convert(expression, s.Info.PropertyType);
                    }
                    else if (s.Info.PropertyType.IsNullableType())
                    {
                        expression = Expression.Condition(
                            Expression.Call(parExp, MethodCache.IsDBNull, Expression.Constant(s.Index, typeof(int))),
                                Expression.Convert(Expression.Constant(null), s.Info.PropertyType),
                            Expression.Convert(expression, s.Info.PropertyType));
                    }
                    else if (dbType != s.Info.PropertyType)
                    {
                        expression = Expression.Convert(expression, s.Info.PropertyType);
                    }

                    return Expression.Bind(s.Info, expression);
                });

            var expr =
                Expression.Lambda<Func<IDataReader, T>>(
                    Expression.MemberInit(
                        newExp,
                        bindings.ToArray()),
                    parExp);

            _funcDataRecd = expr.Compile();
        }
    }
}
