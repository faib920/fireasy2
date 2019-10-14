using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Fireasy.MvcCore3.Tests.Models;
using Fireasy.Data.Entity;
using Fireasy.Web.Mvc;
using Fireasy.Common.Serialization;
using Fireasy.Common;
using Fireasy.Common.Extensions;

namespace Fireasy.MvcCore3.Tests.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public DateTime GetTime([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return DateTime.Now;
        }

        [HttpGet]
        public IActionResult GetTime1([FromServices]JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return Json(DateTime.Now);
        }

        [HttpGet]
        public JsonResult TestEntity(Entity model)
        {
            return Json(model);
        }
    }

    public class Entity : LightEntity<Entity>
    {
        public virtual string Name { get; set; }

        public virtual DateTime Date { get; set; }
    }

}
