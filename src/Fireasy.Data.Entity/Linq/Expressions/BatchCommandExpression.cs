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
        public BatchCommandExpression(Expression input, LambdaExpression operation, bool isAsync, List<string> arguments)
            : base(DbExpressionType.Batch, typeof(IEnumerable<>).MakeGenericType(operation.Body.Type))
        {
            Input = input;
            Operation = operation;
            Arguments = arguments;
            IsAsync = isAsync;
        }

        public Expression Input { get; private set; }

        public LambdaExpression Operation { get; private set; }

        public List<string> Arguments { get; private set; }

        public bool IsAsync { get; set; }

        public BatchCommandExpression Update(Expression input, LambdaExpression operation, List<string> arguments)
        {
            if (input != Input || operation != Operation || arguments != Arguments)
            {
                return new BatchCommandExpression(input, operation, IsAsync, arguments);
            }

            return this;
        }
    }
}
