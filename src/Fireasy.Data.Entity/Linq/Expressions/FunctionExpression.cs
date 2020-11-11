// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示使用 SQL 函数的表达式。
    /// </summary>
    public sealed class FunctionExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="FunctionExpression"/> 类的新实例。
        /// </summary>
        /// <param name="type">函数的返回类型。</param>
        /// <param name="name">函数的名称。</param>
        /// <param name="arguments">函数所包含的参数表达式。</param>
        public FunctionExpression(Type type, string name, IEnumerable<Expression> arguments)
            : base(DbExpressionType.Function, type)
        {
            Name = name;
            Arguments = arguments.ToReadOnly();
        }

        /// <summary>
        /// 获取函数的名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 获取函数所包含的参数表达式。
        /// </summary>
        public ReadOnlyCollection<Expression> Arguments { get; }

        /// <summary>
        /// 更新 <see cref="FunctionExpression"/> 对象。
        /// </summary>
        /// <param name="name">函数的名称。</param>
        /// <param name="arguments">函数所包含的参数表达式。</param>
        /// <returns></returns>
        public FunctionExpression Update(string name, IEnumerable<Expression> arguments)
        {
            if (name != Name || arguments != Arguments)
            {
                return new FunctionExpression(Type, name, arguments);
            }
            return this;
        }

        public override string ToString()
        {
            return $"Function({Name})";
        }
    }
}
