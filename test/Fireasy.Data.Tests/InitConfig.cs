using Fireasy.Common.Configuration;
#if NETCOREAPP
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#endif
using System.IO;

namespace Fireasy.Data.Tests
{
    public class InitConfig
    {
        public static void Init()
        {
#if NETCOREAPP
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            config.Initialize(typeof(InitConfig).Assembly);
#endif
        }
    }
}
