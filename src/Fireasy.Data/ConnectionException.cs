// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using System.Data.Common;

namespace Fireasy.Data
{
    /// <summary>
    /// 表示由数据库连接引发的异常。
    /// </summary>
    public sealed class ConnectionException : DbException
    {
        /// <summary>
        /// 实例化一个 <see cref="ConnectionException"/> 对象。
        /// </summary>
        /// <param name="message">异常的提示信息。</param>
        /// <param name="innerException">所引发的异常源。</param>
        public ConnectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal static ConnectionException Throw(ConnectionState state, Exception innerException)
        {
            switch (state)
            {
                case ConnectionState.Open:
                    return new ConnectionException(SR.GetString(SRKind.UnableOpenConnection), innerException);
                case ConnectionState.Closed:
                    return new ConnectionException(SR.GetString(SRKind.UnableCloseConnection), innerException);
            }

            return new ConnectionException(SR.GetString(SRKind.ConnectionError), innerException);
        }
    }
}
