// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    public class CaseWhenExpression : DbExpression
    {
        public CaseWhenExpression(List<CaseWhenParameter> parameters, Type type)
            : base (DbExpressionType.CaseWhen, type)
        {
            Parameters = parameters.ToReadOnly();
        }

        public ReadOnlyCollection<CaseWhenParameter> Parameters { get; private set; }
    }

    public class CaseWhenParameter
    {
        public Expression When { get; set; }

        public Expression Then { get; set; }
    }
}
