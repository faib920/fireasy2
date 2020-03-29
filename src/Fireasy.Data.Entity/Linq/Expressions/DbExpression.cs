// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 一个抽象类，表示 ELinq 自定义表达式。
    /// </summary>
    [DebuggerDisplay("NodeType={NodeType},Type={Type}")]
    public abstract class DbExpression : Expression
    {
        private readonly Type type;
        private readonly DbExpressionType nodeType;

        /// <summary>
        /// 初始化 <see cref="DbExpression"/> 类的新实例。
        /// </summary>
        /// <param name="nodeType">节点的类型。</param>
        public DbExpression(DbExpressionType nodeType)
            : this (nodeType, typeof(void))
        {
        }

        /// <summary>
        /// 初始化 <see cref="DbExpression"/> 类的新实例。
        /// </summary>
        /// <param name="nodeType">节点的类型。</param>
        /// <param name="type">表达式的静态类型。</param>
        public DbExpression(DbExpressionType nodeType, Type type)
        {
            this.nodeType = nodeType;
            this.type = type;
        }

        public override Type Type => type;

        public override ExpressionType NodeType => (ExpressionType)nodeType;
    }
}
