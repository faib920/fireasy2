// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    public class OuterJoinedExpression : DbExpression
    {
        public OuterJoinedExpression(Expression test, Expression expression)
            : base(DbExpressionType.OuterJoined, expression.Type)
        {
            Test = test;
            Expression = expression;
        }

        public Expression Test { get; private set; }

        public Expression Expression { get; private set; }

        public OuterJoinedExpression Update(Expression test, Expression expression)
        {
            if (test != Test || expression != Expression)
            {
                return new OuterJoinedExpression(test, expression);
            }
            return this;
        }

    }
}
