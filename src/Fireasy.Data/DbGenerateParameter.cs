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
    public class DataExpressionColumn
    {
        public Delegate Setter { get; set; }

        public static Type CreateType(Type type)
        {
            return typeof(InnerDataExpressionColumn<>).MakeGenericType(type);
        }

        public static DataExpressionColumn Create(Type type, Delegate factory)
        {
            var par = typeof(InnerDataExpressionColumn<>).MakeGenericType(type).New<DataExpressionColumn>();
            par.Setter = factory;
            return par;
        }

        public static Type GetParameterType(Type type)
        {
            if (typeof(DataExpressionColumn).IsAssignableFrom(type) && type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return null;
        }

        private class InnerDataExpressionColumn<T> : DataExpressionColumn
        {
        }
    }
}