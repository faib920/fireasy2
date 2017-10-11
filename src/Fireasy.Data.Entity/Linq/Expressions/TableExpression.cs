// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示数据表的表达式。
    /// </summary>
    public class TableExpression : AliasedExpression
    {
        public TableExpression(TableAlias alias, string name, Type entityType)
            : base(DbExpressionType.Table, entityType, alias)
        {
            Name = name;
        }

        /// <summary>
        /// 获取数据表的名称。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 输出表的名称。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "T(" + Name + ")";
        }
    }
}
