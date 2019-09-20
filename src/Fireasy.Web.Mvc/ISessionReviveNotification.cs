// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP

using Microsoft.AspNetCore.Http;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// Session 复活后的通知。
    /// </summary>
    public interface ISessionReviveNotification
    {
        /// <summary>
        /// 调用事件通知。
        /// </summary>
        /// <param name="context"></param>
        void Invoke(HttpContext context);
    }
}
#endif
