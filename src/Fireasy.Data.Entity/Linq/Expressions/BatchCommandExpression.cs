// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    public sealed class BatchCommandExpression : DbExpression
    {
        public BatchCommandExpression(Expression input, LambdaExpression operation)
            : base(DbExpressionType.Batch, typeof(IEnumerable<>).MakeGenericType(operation.Body.Type))
        {
            Input = input;
            Operation = operation;
        }

        public Expression Input { get; private set; }

        public LambdaExpression Operation { get; private set; }

        public BatchCommandExpression Update(Expression input, LambdaExpression operation)
        {
            if (input != Input || operation != Operation)
            {
                return new BatchCommandExpression(input, operation);
            }

            return this;
        }
    }
}
