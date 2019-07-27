using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fireasy.Common.Caching;
using Fireasy.Common.Ioc;
using Fireasy.Data.Entity;
using Fireasy.Data.Entity.Tests.Models;
using Fireasy.MvcCore.Services;
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
                .AddIoc(ContainerUnity.GetContainer())
                .AddEntityContext<TestContext>();

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

    public interface IModel
    {
        string Name { get; set; }
    }

    public class TestModel : IModel
    {
        public string Name { get; set; }
    }

    public class CacheManager : ICacheManager
    {
        public CacheManager(IModel model)
        {
        }

        public T Add<T>(string cacheKey, T value, TimeSpan? expire = null, CacheItemRemovedCallback removeCallback = null)
        {
            throw new NotImplementedException();
        }

        public T Add<T>(string cacheKey, T value, ICacheItemExpiration expiration, CacheItemRemovedCallback removeCallback = null)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public object Get(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public TimeSpan? GetExpirationTime(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetKeys(string pattern)
        {
            throw new NotImplementedException();
        }

        public void Remove(string cacheKey)
        {
            throw new NotImplementedException();
        }

        public void SetExpirationTime(string cacheKey, Func<ICacheItemExpiration> expiration)
        {
            throw new NotImplementedException();
        }

        public T TryGet<T>(string cacheKey, Func<T> factory, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public object TryGet(Type dataType, string cacheKey, Func<object> factory, Func<ICacheItemExpiration> expiration = null)
        {
            throw new NotImplementedException();
        }

        public bool TryGet<T>(string cacheKey, out T value)
        {
            throw new NotImplementedException();
        }
    }
}
