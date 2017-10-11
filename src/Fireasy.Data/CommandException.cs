// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data.Common;
using Fireasy.Data.Extensions;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示执行错误所引发的异常。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class CommandException : DbException
    {
        /// <summary>
        /// 实例化一个 <see cref="CommandException"/> 对象。
        /// </summary>
        /// <param name="command">执行查询的命令。</param>
        /// <param name="innerException">所引发的异常源。</param>
        public CommandException(DbCommand command, Exception innerException)
            : base(SR.GetString(SRKind.FailInExecute, command.Output()), innerException)
        {
            Command = command;
        }

        /// <summary>
        /// 实例化一个 <see cref="CommandException"/> 对象。
        /// </summary>
        /// <param name="command">执行查询的命令。</param>
        public CommandException(DbCommand command)
            : this(command, null)
        {
        }

        /// <summary>
        /// 获取命令对象。
        /// </summary>
        public DbCommand Command { get; private set; }
    }
}
