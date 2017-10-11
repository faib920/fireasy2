// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;

namespace Fireasy.Data
{
    /// <summary>
    /// 提供一个用于记录命令执行的跟踪器。
    /// </summary>
    public interface ICommandTracker
    {
        /// <summary>
        /// 记录下当前执行的命令和所用的时间。
        /// </summary>
        /// <param name="command">当前执行的命令。</param>
        /// <param name="period">所耗用的时间。</param>
        void Write(IDbCommand command, TimeSpan period);

        /// <summary>
        /// 记录下执行失败的命令。
        /// </summary>
        /// <param name="command">当前执行的命令。</param>
        /// <param name="exception"></param>
        void Fail(IDbCommand command, Exception exception);
    }
}
