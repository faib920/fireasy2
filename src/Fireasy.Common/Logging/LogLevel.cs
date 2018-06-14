// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Logging
{
    /// <summary>
    /// 日志级别。
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        Default = 0,
        Info = 1,
        Debug = 2,
        Warn = 4,
        Error = 8,
        Fatal = 16
    }
}
