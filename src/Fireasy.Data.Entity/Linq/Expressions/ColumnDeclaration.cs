// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 列达式的映射定义。
    /// </summary>
    public sealed class ColumnDeclaration
    {
        public ColumnDeclaration(string name, Expression expression)
        {
            Name = name;
            Expression = expression;
        }

        /// <summary>
        /// 返回列的名称。
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 返回列表达式。
        /// </summary>
        public Expression Expression { get; }

        public override string ToString()
        {
            return $"Declarate({Name}:{Expression})";
        }
    }
}
