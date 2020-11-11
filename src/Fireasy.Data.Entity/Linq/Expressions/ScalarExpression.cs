// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    public class ScalarExpression : SubqueryExpression
    {
        public ScalarExpression(Type type, SelectExpression select)
            : base(DbExpressionType.Scalar, type, select)
        {
        }

        public ScalarExpression Update(SelectExpression select)
        {
            return select != Select ? new ScalarExpression(Type, select) : this;
        }

        public override string ToString()
        {
            return $"Scalar({Select})";
        }
    }
}
