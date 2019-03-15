using Fireasy.Common.Serialization;
using Fireasy.MvcCore.Tests.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Fireasy.MvcCore.Tests.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public JsonResult TestJsonSerializeOption([FromServices]IOptions<Fireasy.Web.Mvc.MvcOptions> options)
        {
            options.Value.JsonSerializeOption.Converters.Add(new FullDateTimeJsonConverter());
            return Json(45);
        }

        public JsonResult TestFromServices([FromServices]IModel model)
        {
            return Json(45);
        }

    }
}
