using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Localization;
using Fireasy.Common.Logging;
using Fireasy.Common.Serialization;
using Fireasy.Common.Tasks;
using Fireasy.Data.Entity;
using Fireasy.MvcCore.Services;
using Fireasy.MvcCore.Tests.Models;
using Fireasy.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Fireasy.MvcCore.Tests.Controllers
{
    public class HomeController : Controller
    {
        private IService service;
        private ILogger<HomeController> logger;

        public HomeController(IServiceProvider sp, ITaskScheduler sc, ISerializer ser, IModel model, IService context, ICacheManager cacheMgr, ILogger<HomeController> logger, IStringLocalizerManager localizer)
        {
            var tt = ser.Serialize("dfafdsaf");
            var str = localizer.GetLocalizer("Test")["Name"];
            this.logger = logger;
            this.service = context;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        public IActionResult Index()
        {
            Tracer.Debug("dfdfasfafasdf");

            try
            {
                service.Update();
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
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
            throw new Exception("");
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

        public async Task<List<string>> TestList()
        {
            return await Task.Run(() => new List<string> { "df", "gg" });
        }
    }

    public class Entity : LightEntity<Entity>
    {
        public virtual string Name { get; set; }

        public virtual DateTime Date { get; set; }
    }
}
