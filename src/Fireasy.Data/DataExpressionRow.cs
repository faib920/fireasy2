// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Data
{
    /// <summary>
    /// 一个包含赋值委托的数据行。
    /// </summary>
    public class DataExpressionRow
    {
        /// <summary>
        /// 获取赋值委托。
        /// </summary>
        public Delegate Setter { get; private set; }

        /// <summary>
        /// 获取数据。
        /// </summary>
        /// <param name="database"></param>
        /// <returns></returns>
        public object GetValue(IDatabase database)
        {
            return Setter.DynamicInvoke(database);
        }

        /// <summary>
        /// 创建一个 <see cref="DataExpressionRow"/> 类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type CreateType(Type type)
        {
            return typeof(GenericDataExpressionRow<>).MakeGenericType(type);
        }

        /// <summary>
        /// 创建一个 <see cref="DataExpressionRow"/> 实例。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valueCreator">赋值委托。</param>
        /// <returns></returns>
        public static DataExpressionRow Create(Type type, Delegate valueCreator)
        {
            var par = typeof(GenericDataExpressionRow<>).MakeGenericType(type).New<DataExpressionRow>();
            par.Setter = valueCreator;
            return par;
        }

        /// <summary>
        /// 获取参数类型。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetParameterType(Type type)
        {
            if (typeof(DataExpressionRow).IsAssignableFrom(type) && type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        /// <summary>
        /// 一个泛型的 <see cref="DataExpressionRow"/>。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class GenericDataExpressionRow<T> : DataExpressionRow
        {
        }
    }
}