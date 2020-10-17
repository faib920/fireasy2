// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 序列链接的类型。
    /// </summary>
    public enum JoinType
    {
        /// <summary>
        /// 交叉链接。
        /// </summary>
        CrossJoin,
        /// <summary>
        /// 内链接。
        /// </summary>
        InnerJoin,
        /// <summary>
        /// 
        /// </summary>
        CrossApply,
        /// <summary>
        /// 
        /// </summary>
        OuterApply,
        /// <summary>
        /// 左链接。
        /// </summary>
        LeftOuter,
        /// <summary>
        /// 右链接。
        /// </summary>
        RightOuter,
        SingletonLeftOuter
    }
}
