// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.RabbitMQ
{
    public class ServerConnectException : Exception
    {
        public ServerConnectException(string message, Exception innerExp = null)
            : base (message, innerExp)
        {
        }
    }
}
