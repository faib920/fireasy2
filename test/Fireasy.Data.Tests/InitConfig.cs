using Fireasy.Common.Configuration;
#if NETCOREAPP2_0
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
#if NETCOREAPP2_0
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            config.Initialize(typeof(InitConfig).Assembly);
#endif
        }
    }
}
