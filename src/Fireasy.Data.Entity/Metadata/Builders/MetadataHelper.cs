// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Data.Entity.Metadata.Builders
{
    public sealed class MetadataHelper
    {
        public static PropertyInfo FindProperty(LambdaExpression exp)
        {
            if (exp.Body is MemberExpression mbrExp)
            {
                return mbrExp.Member as PropertyInfo;
            }

            throw new ArgumentException();
        }
    }
}
