// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data
{
    /// <summary>
    /// 当数据库不支持分段查询数据时，抛出该异常。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class SegmentNotSupportedException : Exception
    {
    }
}
