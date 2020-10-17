// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示子查询的表达式。
    /// </summary>
    public abstract class SubqueryExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="SubqueryExpression"/> 类的新实例。
        /// </summary>
        /// <param name="nodeType"></param>
        /// <param name="type"></param>
        /// <param name="select"></param>
        public SubqueryExpression(DbExpressionType nodeType, Type type, SelectExpression select)
            : base(nodeType, type)
        {
            Select = select;
        }

        /// <summary>
        /// 获取查询表达式。
        /// </summary>
        public SelectExpression Select { get; }
    }
}
