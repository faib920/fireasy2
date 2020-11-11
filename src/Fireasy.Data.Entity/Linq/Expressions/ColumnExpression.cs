// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示数据列的表达式。
    /// </summary>
    public class ColumnExpression : DbExpression, IEquatable<ColumnExpression>
    {
        public ColumnExpression(Type type, TableAlias alias, string name, PropertyMapInfo map)
            : base(DbExpressionType.Column, type)
        {
            Alias = alias;
            Name = name;
            MapInfo = map;
        }

        /// <summary>
        /// 获取所属的表的别名。
        /// </summary>
        public TableAlias Alias { get; }

        /// <summary>
        /// 获取列的名称。
        /// </summary>
        public string Name { get; }

        public PropertyMapInfo MapInfo { get; }

        /// <summary>
        /// 获取哈希码。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Alias.GetHashCode() + Name.GetHashCode();
        }

        /// <summary>
        /// 判断两个对象是否相等。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ColumnExpression);
        }

        /// <summary>
        /// 判断两个对象是否相等。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ColumnExpression other)
        {
            return other != null
                && ((object)this) == (object)other
                 || (Alias == other.Alias && Name == other.Name);
        }


        public override string ToString()
        {
            return $"{Alias}.Column({Name})";
        }
    }

    public class SubqueryColumnExpression : ColumnExpression
    {
        internal SubqueryColumnExpression(Type type, TableAlias alias, string name, string subquery)
            : base(type, alias, name, null)
        {
            Subquery = subquery;
        }

        public string Subquery { get; }
    }
}
