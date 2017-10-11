// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示表的别名。
    /// </summary>
    public class TableAlias
    {
        public override string ToString()
        {
            return "A:" + GetHashCode();
        }
    }
}
