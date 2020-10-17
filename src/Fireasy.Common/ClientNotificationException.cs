// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common
{
    /// <summary>
    /// 用于客户端通知的异常。
    /// </summary>
    public class ClientNotificationException : Exception
    {
        /// <summary>
        /// 使用通知信息初始化 <see cref="ClientNotificationException"/> 类的新实例。
        /// </summary>
        /// <param name="message">通知信息。</param>
        public ClientNotificationException(string message)
            : base(message)
        {
        }
    }
}
