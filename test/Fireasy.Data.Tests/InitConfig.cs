using Fireasy.Common.Configuration;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

            ConfigurationUnity.Bind(typeof(InitConfig).Assembly, config);
#endif
        }
    }
}
