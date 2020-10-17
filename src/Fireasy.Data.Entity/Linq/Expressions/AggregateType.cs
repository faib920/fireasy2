// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 聚合函数的类型。
    /// </summary>
    public enum AggregateType
    {
        /// <summary>
        /// 计算序列的个数。
        /// </summary>
        Count,
        /// <summary>
        /// 求序列中的最小值。
        /// </summary>
        Min,
        /// <summary>
        /// 求序列中的最大值。
        /// </summary>
        Max,
        /// <summary>
        /// 对序列进行求和。
        /// </summary>
        Sum,
        /// <summary>
        /// 计算序列的平均值。
        /// </summary>
        Average
    }

}
