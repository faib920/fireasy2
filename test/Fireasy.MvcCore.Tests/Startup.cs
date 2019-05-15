using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fireasy.Data.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddTransient<IModel, TestModel>();

            services
                .AddFireasy(Configuration)
                .AddEntityContext<TestContext>(o => o.AutoCreateTables = true)
                .UseSQLite("Data source=|appdir|../../../../../documents/db/northwind.db3");

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .ConfigureFireasyMvc(s => { s.JsonSerializeOption.Converters.Add(new Fireasy.Data.Entity.LightEntityJsonConverter());  });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

    public class TestContext : EntityContext
    {
        public TestContext(EntityContextOptions options)
            : base(options)
        {
        }
    }

    public interface IModel
    {
        string Name { get; set; }
    }

    public class TestModel : IModel
    {
        public string Name { get; set; }
    }
}
