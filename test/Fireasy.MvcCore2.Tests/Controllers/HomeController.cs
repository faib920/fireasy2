using Fireasy.Common.Caching;
using Fireasy.Common.Serialization;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.MvcCore.Services;
using Fireasy.MvcCore.Tests.Models;
using Fireasy.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Linq;

namespace Fireasy.MvcCore.Tests.Controllers
{
    public class HomeController : Controller
    {
        private IService service;

        public HomeController(IService context, ICacheManager cacheMgr)
        {
            this.service = context;
        }

        public IActionResult Index()
        {
            //try
            //{
            //    service.Update();
            //}
            //catch (Exception exp)
            //{
            //    Console.WriteLine(exp.Message);
            //}
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
        public JsonResult TestJsonSerializeOption([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return Json(DateTime.Now);
        }

        [HttpGet]
        public JsonResult TestFromServices([FromServices]IModel model)
        {
            return Json(45);
        }

        [HttpGet]
        public DateTime GetTime([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return DateTime.Now;
        }

        [HttpGet]
        public JsonResult GetTime1([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return Json(DateTime.Now);
        }

        [HttpGet]
        public JsonResult TestEntity(Entity model)
        {
            return Json(model);
        }

        public JsonResult TestOption()
        {
            return this.Json(new Entity { Date = DateTime.Now }, new DateTimeJsonConverter("hh:mm"));
        }

        public object TestOption1([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new DateTimeJsonConverter("hh:mm"));
            return new Entity { Date = DateTime.Now };
        }
    }

    public class Entity : LightEntity<Entity>
    {
        public virtual string Name { get; set; }

        public virtual DateTime Date { get; set; }
    }
}
