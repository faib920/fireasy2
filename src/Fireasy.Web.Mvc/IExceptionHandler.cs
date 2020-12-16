// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETCOREAPP
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
#else
using System.Web.Mvc;
#endif

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 提供异常返回结果的处理器。
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// 返回结果。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
#if NETCOREAPP
        IActionResult GetResult(ExceptionContext context);
#else
        ActionResult GetResult(ExceptionContext context);
#endif
    }
}
