// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表达式节点的类型。
    /// </summary>
    public enum DbExpressionType
    {
        /// <summary>
        /// 数据表表达式。
        /// </summary>
        Table = 1000,
        /// <summary>
        /// 客户端连接。
        /// </summary>
        ClientJoin,
        /// <summary>
        /// 数据列表达式。
        /// </summary>
        Column,
        /// <summary>
        /// 选择表达式。
        /// </summary>
        Select,
        /// <summary>
        /// 影射表达式。
        /// </summary>
        Projection,
        /// <summary>
        /// 实体表达式。
        /// </summary>
        Entity,
        /// <summary>
        /// 链接表达式。
        /// </summary>
        Join,
        /// <summary>
        /// 聚合表达式。
        /// </summary>
        Aggregate,
        /// <summary>
        /// 标题表达式。
        /// </summary>
        Scalar,
        /// <summary>
        /// 存在判断表达式。
        /// </summary>
        Exists,
        /// <summary>
        /// 集合包含表达式。
        /// </summary>
        In,
        /// <summary>
        /// 排序表达式。
        /// </summary>
        OrderBy,
        /// <summary>
        /// 分组表达式。
        /// </summary>
        Grouping,
        /// <summary>
        /// 聚合子查询表达式。
        /// </summary>
        AggregateSubquery,
        /// <summary>
        /// 为空表达式。
        /// </summary>
        IsNull,
        /// <summary>
        /// 期间表达式。
        /// </summary>
        Between,
        /// <summary>
        /// 行数表达式。
        /// </summary>
        RowCount,
        /// <summary>
        /// 键值对表达式。
        /// </summary>
        NamedValue,
        /// <summary>
        /// 外链接表达式。
        /// </summary>
        OuterJoined,
        /// <summary>
        /// 行表达式。
        /// </summary>
        Function,
        /// <summary>
        /// 条件表达式。
        /// </summary>
        Condition,
        /// <summary>
        /// 分段表达式。
        /// </summary>
        Segment,
        /// <summary>
        /// 插入表达式。
        /// </summary>
        Insert,
        /// <summary>
        /// 删除表达式。
        /// </summary>
        Delete,
        /// <summary>
        /// 更新表达式。
        /// </summary>
        Update,
        /// <summary>
        /// 批量操作表达式。
        /// </summary>
        Batch,
        /// <summary>
        /// 命令块表达式。
        /// </summary>
        Block,
        Generator,
        Declaration,
        Variable,
        SqlText,
        CaseWhen
    }
}
