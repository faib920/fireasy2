using Fireasy.Common.Ioc;
using Fireasy.Common.Tasks;
using Fireasy.MvcCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newton = Newtonsoft.Json;

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
                .AddEntityContext<TestContext1>()
                //.AddNLogger()
                .AddLog4netLogger()
                .AddNewtonsoftSerializer(s => { s.DateFormatString = "yyyy-MM"; });

            //services.AddRedisCaching(s => s.DefaultDb = 22);

            services.AddMvc()
                .ConfigureFireasyMvc(s => { s.JsonSerializeOption.Converters.Add(new Fireasy.Data.Entity.LightEntityJsonConverter());  })
                .AddXmlSerializerFormatters();


            //services.AddQuartzScheduler(options =>
            //    options.Add(TimeSpan.Zero, TimeSpan.FromSeconds(5), (p, c) =>
            //        {
            //            Console.WriteLine(DateTime.Now + " quartz coming...");
            //        }));
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
        public TestExecutor(Fireasy.Common.Logging.ILogger logger)
        {

        }

        public async Task ExecuteAsync(TaskExecuteContext context)
        {
            Console.WriteLine(DateTime.Now + " bbb");
        }
    }
    public class TestExecutor1 : ITaskExecutor
    {
        public TestExecutor1(Fireasy.Common.Logging.ILogger logger)
        {

        }

        public void Execute(TaskExecuteContext context)
        {
            Console.WriteLine(DateTime.Now + " ccc");
        }
    }
}
