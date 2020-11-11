using Fireasy.Common;
using Fireasy.Common.Caching;
using Fireasy.Common.Localization;
using Fireasy.Common.Logging;
using Fireasy.Common.Mapper;
using Fireasy.Common.Serialization;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Tasks;
using Fireasy.Common.Threading;
using Fireasy.Data.Entity;
using Fireasy.MvcCore.Services;
using Fireasy.MvcCore.Tests.Models;
using Fireasy.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
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

        public HomeController(IServiceProvider serviceProvider,
            TestContext dbContext,
            IModel model,
            IService service,
            ITaskScheduler taskScheduler,
            ISerializer serializer,
            ISubscribeManager subscribeMgr,
            ICacheManager cacheMgr,
            IMemoryCacheManager memoryCacheMgr,
            IDistributedCacheManager distributedCacheMgr,
            ILogger<HomeController> logger,
            IDistributedLocker distributedLocker,
            IObjectMapper objMapper,
            IStringLocalizerManager stringLocalizer)
        {
            var tt = serializer.Serialize("dfafdsaf");
            var str = stringLocalizer.GetLocalizer("Test")["Name"];
            this.logger = logger;
            this.service = service;
            serviceProvider.GetRequiredService<ISubscribeManager>().Publish(new TestSub { Name = "dfdfdf" });

            var m1 = new Map1();

            var m2 = objMapper.Map<Map1, Map2>(m1);

            //taskScheduler.StartExecutor< TestExecutor1>(new StartOptions<TestExecutor1>(TimeSpan.Zero, TimeSpan.FromMinutes(1)));
            //taskScheduler.StartExecutorAsync(new StartOptions<TT1>(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5)));
        }

        public class TT : ITaskExecutor
        {
            public void Execute(TaskExecuteContext context)
            {
                Console.WriteLine(DateTime.Now + "tt");
            }
        }

        public class TT1 : IAsyncTaskExecutor
        {
            public Task ExecuteAsync(TaskExecuteContext context)
            {
                Console.WriteLine(DateTime.Now + "tt1");
                return Task.CompletedTask;
            }
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        [HttpPost]
        public IActionResult Test(QueryParameterBase c)
        {
            return Json("ddd");
        }

        public async Task<IActionResult> Index()
        {
            Tracer.Debug("dfdfasfafasdf");

            try
            {
                await service.GetCountAsync();
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
        public JsonResult TestJsonSerializeOption([FromServices] JsonSerializeOptionHosting hosting)
        {
            throw new Exception("");
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return Json(DateTime.Now);
        }

        [HttpGet]
        public JsonResult TestFromServices([FromServices] IModel model)
        {
            return Json(45);
        }

        [HttpGet]
        public DateTime GetTime([FromServices] JsonSerializeOptionHosting hosting)
        {
            hosting.Option.Converters.Add(new FullDateTimeJsonConverter());
            return DateTime.Now;
        }

        [HttpGet]
        public JsonResult GetTime1([FromServices] JsonSerializeOptionHosting hosting)
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

        public object TestOption1([FromServices] JsonSerializeOptionHosting hosting)
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

    public class QueryParameterBase
    {
        public int PageSize { get; set; }

        public int PageNum { get; set; }

        public string Name { get; set; }
    }
}
