// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示SQL里的聚合运算的表达式。
    /// </summary>
    public sealed class AggregateExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="AggregateExpression"/> 类的新实例。
        /// </summary>
        /// <param name="type">函数的返回类型。</param>
        /// <param name="aggType">聚合函数的类型。</param>
        /// <param name="argument">聚合函数所包含的参数表达式。</param>
        /// <param name="isDistinct">是否使用 DISTINCT 关键字。</param>
        public AggregateExpression(Type type, AggregateType aggType, Expression argument, bool isDistinct)
            : base(DbExpressionType.Aggregate, type)
        {
            AggregateType = aggType;
            Argument = argument;
            IsDistinct = isDistinct;
        }

        /// <summary>
        /// 获取聚合函数的类型。
        /// </summary>
        public AggregateType AggregateType { get; }

        /// <summary>
        /// 获取聚合函数所包含的参数表达式。
        /// </summary>
        public Expression Argument { get; }

        /// <summary>
        /// 获取是否使用 DISTINCT 关键字。
        /// </summary>
        public bool IsDistinct { get; }

        /// <summary>
        /// 更新 <see cref="AggregateExpression"/> 对象。
        /// </summary>
        /// <param name="type">函数的返回类型。</param>
        /// <param name="aggType">聚合函数的类型。</param>
        /// <param name="argument">聚合函数所包含的参数表达式。</param>
        /// <param name="isDistinct">是否使用 DISTINCT 关键字。</param>
        /// <returns></returns>
        public AggregateExpression Update(Type type, AggregateType aggType, Expression argument, bool isDistinct)
        {
            if (type != Type ||
                aggType != AggregateType ||
                argument != Argument ||
                isDistinct != IsDistinct)
            {
                return new AggregateExpression(type, aggType, argument, isDistinct);
            }
            return this;
        }

        public override string ToString()
        {
            return $"Aggregate({AggregateType}({Argument}))";
        }
    }
}
