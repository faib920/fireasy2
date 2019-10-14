// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 影射表达式。
    /// </summary>
    public sealed class ProjectionExpression : DbExpression
    {
        public ProjectionExpression(SelectExpression source, Expression projector, bool isAsync)
            : this(source, projector, null, isAsync)
        {
        }

        public ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator, bool isAsync)
            : base(DbExpressionType.Projection, 
                  aggregator != null ? 
                    (isAsync && aggregator.Body.Type.IsGenericType ? 
                        aggregator.Body.Type.GetGenericArguments()[0] : aggregator.Body.Type) :
                        typeof(IEnumerable<>).MakeGenericType(projector.Type))
        {
            Select = source;
            Projector = projector;
            Aggregator = aggregator;
            IsAsync = isAsync;
        }

        /// <summary>
        /// 获取查询表达式。
        /// </summary>
        public SelectExpression Select { get; private set; }

        /// <summary>
        /// 获取 Linq 表达式。
        /// </summary>
        public Expression Projector { get; private set; }

        /// <summary>
        /// 获取聚合表达式。
        /// </summary>
        public LambdaExpression Aggregator { get; private set; }

        /// <summary>
        /// 获取是否返回单列。
        /// </summary>
        public bool IsSingleton
        {
            get
            {
                return Aggregator != null && 
                    (IsAsync && Aggregator.Body.Type.IsGenericType ?
                        Aggregator.Body.Type.GetGenericArguments()[0] == Projector.Type : 
                        Aggregator.Body.Type == Projector.Type);
            }
        }

        public bool IsAsync { get; private set; }

        public ProjectionExpression Update(SelectExpression select, Expression projector, LambdaExpression aggregator)
        {
            if (select != Select || projector != Projector || aggregator != Aggregator)
            {
                return new ProjectionExpression(select, projector, aggregator, IsAsync);
            }
            return this;
        }
    }
}
