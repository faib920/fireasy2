// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 一个抽象类，表示具有表的别名的表达式。
    /// </summary>
    public abstract class AliasedExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="AliasedExpression"/> 类的新实例。
        /// </summary>
        /// <param name="nodeType">表达式的类型。</param>
        /// <param name="type">表达式的数据类型。</param>
        /// <param name="alias">表的别名。</param>
        protected AliasedExpression(DbExpressionType nodeType, Type type, TableAlias alias)
            : base(nodeType, type)
        {
            Alias = alias;
        }

        /// <summary>
        /// 获取表的别名。
        /// </summary>
        public TableAlias Alias { get; }

        public override string ToString()
        {
            return $"Alias({Alias})";
        }
    }
}
