﻿// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Data.Configuration;
using Fireasy.Data.Converter.Configuration;
using Fireasy.Data.Provider.Configuration;
using Microsoft.Extensions.Configuration;
using System;

[assembly: ConfigurationBinder(typeof(Microsoft.Extensions.DependencyInjection.ConfigurationBinder))]

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                ConfigurationUnity.Bind<GlobalConfigurationSection>(configuration);
                ConfigurationUnity.Bind<ProviderConfigurationSection>(configuration);
                ConfigurationUnity.Bind<ConverterConfigurationSection>(configuration);
                ConfigurationUnity.Bind<InstanceConfigurationSection>(configuration);
            }
            catch (Exception exp)
            {
                Tracer.Error($"{typeof(ConfigurationBinder).FullName} throw exception when binding:{exp.Output()}");
            }
        }
    }
}
#endif