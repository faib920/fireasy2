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
    public class DataExpressionRow
    {
        public Delegate Setter { get; private set; }

        public object GetValue(IDatabase database)
        {
            return Setter.DynamicInvoke(database);
        }

        public static Type CreateType(Type type)
        {
            return typeof(GenericDataExpressionRow<>).MakeGenericType(type);
        }

        public static DataExpressionRow Create(Type type, Delegate factory)
        {
            var par = typeof(GenericDataExpressionRow<>).MakeGenericType(type).New<DataExpressionRow>();
            par.Setter = factory;
            return par;
        }

        public static Type GetParameterType(Type type)
        {
            if (typeof(DataExpressionRow).IsAssignableFrom(type) && type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        private class GenericDataExpressionRow<T> : DataExpressionRow
        {
        }
    }
}