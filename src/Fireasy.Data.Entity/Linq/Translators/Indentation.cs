// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)


namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 语法的缩进样式。
    /// </summary>
    public enum Indentation
    {
        /// <summary>
        /// 保持相同的级别。
        /// </summary>
        Same,
        /// <summary>
        /// 向内缩进。
        /// </summary>
        Inner,
        /// <summary>
        /// 向外缩进。
        /// </summary>
        Outer
    }
}
