// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// HttpRequst 详情信息的异常。
    /// </summary>
    public class RequestDetailException : Exception
    {
        public RequestDetailException(string message, Exception innerExp)
            : base (message, innerExp)
        {
        }
    }
}
