// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 一个抽象类，表示 ELinq 自定义表达式。
    /// </summary>
    public abstract class DbExpression : Expression
    {
        private readonly Type _type;
        private readonly DbExpressionType _nodeType;

        /// <summary>
        /// 初始化 <see cref="DbExpression"/> 类的新实例。
        /// </summary>
        /// <param name="nodeType">节点的类型。</param>
        public DbExpression(DbExpressionType nodeType)
            : this(nodeType, typeof(void))
        {
        }

        /// <summary>
        /// 初始化 <see cref="DbExpression"/> 类的新实例。
        /// </summary>
        /// <param name="nodeType">节点的类型。</param>
        /// <param name="type">表达式的静态类型。</param>
        public DbExpression(DbExpressionType nodeType, Type type)
        {
            _nodeType = nodeType;
            _type = type;
        }

        public override Type Type => _type;

        public override ExpressionType NodeType => (ExpressionType)_nodeType;
    }
}
