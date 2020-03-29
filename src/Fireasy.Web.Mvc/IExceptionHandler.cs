using System;
#if !NETCOREAPP
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc;
#endif

namespace Fireasy.Web.Mvc
{
    public interface IExceptionHandler
    {
        ActionResult GetResult(Exception exception);
    }
}
