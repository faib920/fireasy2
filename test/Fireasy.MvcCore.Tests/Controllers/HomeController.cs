using Fireasy.Common.Serialization;
using Fireasy.Data.Entity;
using Fireasy.MvcCore.Tests.Models;
using Fireasy.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;

namespace Fireasy.MvcCore.Tests.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(TestContext context)
        {

        }

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

        [HttpGet]
        public JsonResult TestJsonSerializeOption([FromServices]IOptions<Fireasy.Web.Mvc.MvcOptions> options)
        {
            options.Value.JsonSerializeOption.Converters.Add(new FullDateTimeJsonConverter());
            return Json(45);
        }

        [HttpGet]
        public JsonResult TestFromServices([FromServices]IModel model)
        {
            return Json(45);
        }

        [HttpGet]
        public JsonResult TestEntity(Entity model)
        {
            return Json(45);
        }

        public JsonResult TestOption([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new DateTimeJsonConverter("hh:mm"));
            return Json(new Entity { Date = DateTime.Now });
        }

        public JsonResult TestOption1([FromServices]JsonSerializeOptionHosting hosting)
        {
            return Json(new Entity { Date = DateTime.Now });
        }
    }

    public class Entity : LightEntity<Entity>
    {
        public virtual string Name { get; set; }

        public virtual DateTime Date { get; set; }
    }
}
