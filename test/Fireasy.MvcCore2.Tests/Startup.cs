using AutoMapper;
using Fireasy.Common.Ioc;
using Fireasy.Common.Subscribes;
using Fireasy.Common.Tasks;
using Fireasy.Data.Entity.Subscribes;
using Fireasy.MvcCore.Services;
using Fireasy.MvcCore.Tests.Controllers;
using Fireasy.Web.Sockets;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Fireasy.MvcCore.Tests
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddScoped<IModel, TestModel>();

            services.Configure<EntityOption>(s =>
            {
                s.Name = "test";
            });

            services
                .AddFireasy(Configuration)
                .AddIoc()
                .AddEntityContextPool<TestContext>()
                .AddEntityContextPool<TestContext1>()
                .AddNLogger()
                //.AddLog4netLogger()
                .AddNewtonsoftSerializer(s => { s.DateFormatString = "yyyy-MM"; });

            services.AddSubscribers();

            services.AddAutoMapper(s => s.AddProfile<AutoProfile>());

            //services.AddRedisCaching(s => s.DefaultDb = 22);

            services.AddMvc(s =>
            {
                s.ModelBinderProviders.Insert(0, new QueryParameterBinderProvider(s.InputFormatters));
            });
            //.ConfigureFireasyMvc(s => { s.JsonSerializeOption.Converters.Add(new Fireasy.Data.Entity.LightEntityJsonConverter());  })
            //.AddXmlSerializerFormatters();

            //services.AddRedisCaching().AddRedisDistributedLocker();


            services.AddPersistentSubscriber<dd>();

            services.AddHangfire(o => o.UseStorage(
                new MySqlStorage("Data Source=localhost;database=northwind;User Id=root;password=faib;pooling=true;charset=utf8",
                new MySqlStorageOptions
                {
                    TablesPrefix = "hangfire_",
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    JobExpirationCheckInterval = TimeSpan.FromHours(1),
                    CountersAggregateInterval = TimeSpan.FromMinutes(5),
                    PrepareSchemaIfNecessary = true,
                    DashboardJobListLimit = 1000,
                    TransactionTimeout = TimeSpan.FromMinutes(1)
                })));

            services.AddHangfireScheduler();
        }

        private class dd : PersistentSubscriber
        {
            public dd(IServiceProvider p)
            {

            }
        }

        private class QueryParameterBinderProvider : IModelBinderProvider
        {
            private readonly IList<IInputFormatter> _formatters;

            public QueryParameterBinderProvider(IList<IInputFormatter> formatters)
            {
                _formatters = formatters;
            }

            public IModelBinder GetBinder(ModelBinderProviderContext context)
            {
                if (context.BindingInfo.BindingSource != null &&
                    context.BindingInfo.BindingSource.CanAcceptDataFrom(BindingSource.Body))
                {
                    return new QueryParameterBinder(_formatters);
                }

                return null;
            }
        }

        private class QueryParameterBinder : IModelBinder
        {
            private readonly IList<IInputFormatter> _formatters;

            public QueryParameterBinder(IList<IInputFormatter> formatters)
            {
                _formatters = formatters;
            }

            public async Task BindModelAsync(ModelBindingContext bindingContext)
            {
                var httpContext = bindingContext.HttpContext;
                Func<Stream, Encoding, TextReader> readerFactory = httpContext.RequestServices.GetService<IHttpRequestStreamReaderFactory>().CreateReader;

                string modelBindingKey;
                if (bindingContext.IsTopLevelObject)
                {
                    modelBindingKey = bindingContext.BinderModelName ?? string.Empty;
                }
                else
                {
                    modelBindingKey = bindingContext.ModelName;
                }

                var formatterContext = new InputFormatterContext(
                    httpContext,
                    modelBindingKey,
                    bindingContext.ModelState,
                    bindingContext.ModelMetadata,
                    readerFactory,
                    true);

                var formatter = (IInputFormatter)null;
                for (var i = 0; i < _formatters.Count; i++)
                {
                    if (_formatters[i].CanRead(formatterContext))
                    {
                        formatter = _formatters[i];
                        break;
                    }
                }

                if (formatter == null)
                {
                    return;
                }

                try
                {
                    var result = await formatter.ReadAsync(formatterContext);

                    if (result.HasError)
                    {
                        return;
                    }

                    if (result.IsModelSet)
                    {
                        if (result.Model is QueryParameterBase c)
                        {
                            if (httpContext.Request.Query["pageSize"].Count > 0)
                            {
                                c.PageSize = Convert.ToInt32(httpContext.Request.Query["pageSize"]);
                            }

                            if (httpContext.Request.Query["pageNum"].Count > 0)
                            {
                                c.PageNum = Convert.ToInt32(httpContext.Request.Query["pageNum"]);
                            }
                        }

                        bindingContext.Result = ModelBindingResult.Success(result.Model);
                    }
                    else
                    {
                        var message = bindingContext
                            .ModelMetadata
                            .ModelBindingMessageProvider
                            .MissingRequestBodyRequiredValueAccessor();
                        bindingContext.ModelState.AddModelError(modelBindingKey, message);
                    }
                }
                catch (Exception exp)
                {
                    bindingContext.ModelState.AddModelError(modelBindingKey, exp, bindingContext.ModelMetadata);
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                WorkerCount = 1,
                ServerName = "dev",
                Queues = new[] { "dev" },
                ShutdownTimeout = TimeSpan.FromMinutes(5)
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public interface IModel
    {
        string Name { get; set; }
    }

    public class TestModel : IModel
    {
        public TestModel(IServiceProvider serviceProvider)
        {

        }

        public string Name { get; set; }
    }

    public class TestExecutor : IAsyncTaskExecutor
    {
        public TestExecutor(IServiceProvider service)
        {

        }

        public async Task ExecuteAsync(TaskExecuteContext context)
        {
            var cc = context.ServiceProvider.GetService<TestContext>();
            Console.WriteLine(DateTime.Now + " bbb");

            var wsClient = new WebSocketClient();
            await wsClient.StartAsync("ws://localhost:7015/wsNotify");
            await wsClient.SendAsync("test", "hello world");
            await wsClient.CloseAsync();

        }
    }
    public class TestExecutor1 : ITaskExecutor
    {
        public TestExecutor1()
        {

        }

        public void Execute(TaskExecuteContext context)
        {
            var cc = context.ServiceProvider.GetService<TestContext>();
            Console.WriteLine(DateTime.Now.ToString() + cc.GetType().Name);
        }
    }

public class TestSubHandler : ISubscribeHandler, IScopedService
{
    public TestSubHandler(IServiceProvider service)
    {

    }

    public void Handle(TestSub sub)
    {
        Console.WriteLine(sub.Name);
    }
}

public class TestSubscriber : ISubscriber<TestSub>, IScopedService
{
    public TestSubscriber(IServiceProvider service)
    {

    }

    public void Accept(TestSub subject)
    {
        Console.WriteLine(subject.Name);
    }
}

    public class TestSub
    {
        public string Name { get; set; }
    }

    public class AutoProfile : Profile
    {
        public AutoProfile()
        {
            CreateMap<Map1, Map2>();
        }
    }

    public class Map1
    {

    }

    public class Map2
    {

    }
}
