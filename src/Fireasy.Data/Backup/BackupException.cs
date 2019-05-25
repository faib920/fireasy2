// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Runtime.Serialization;

namespace Fireasy.Data.Backup
{
    /// <summary>
    /// 数据库在备份或恢复过程中发生异常。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class BackupException : Exception
    {
        /// <summary>
        /// 初始化 <see cref="BackupException"/> 类的新实例。
        /// </summary>
        public BackupException()
        {
        }

        /// <summary>
        /// 初始化 <see cref="BackupException"/> 类的新实例。
        /// </summary>
        /// <param name="exception">导致当前异常的异常。</param>
        public BackupException(Exception exception)
            : base (SR.GetString(SRKind.FailInBackup, exception.Message), exception)
        {
        }

        /// <summary>
        /// 初始化 <see cref="BackupException"/> 类的新实例。
        /// </summary>
        /// <param name="message">解释异常原因的错误消息。</param>
        /// <param name="exception">导致当前异常的异常</param>
        public BackupException(string message, Exception exception)
            : base (message, exception)
        {
        }
    }
}
