// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Query
{
    internal sealed class BatchExecuteHelper
    {
        internal static bool TryAddCommand(string command, Func<IEnumerable<NamedValueExpression>> namedValueFunc)
        {
            if (BatchExecuteScope.Current != null)
            {
                BatchExecuteScope.Current.AddCommand(command);

                foreach (var nv in namedValueFunc())
                {
                    if (nv.Value is ConstantExpression constExp)
                    {
                        BatchExecuteScope.Current.AddParameter(nv.Name, constExp.Value);
                    }
                }

                return true;
            }

            return false;
        }

        internal static string TryAddParameterName(Func<string> nameFunc)
        {
            if (BatchExecuteScope.Current != null)
            {
                return BatchExecuteScope.Current.NewParameterName();
            }

            return nameFunc();
        }
    }
}
