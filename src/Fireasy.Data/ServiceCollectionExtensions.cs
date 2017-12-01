// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD2_0
using Fireasy.Common.Configuration;
using Fireasy.Data.Configuration;
using Fireasy.Data.Converter.Configuration;
using Fireasy.Data.Provider.Configuration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurationUnity.Bind<GlobalConfigurationSection>(configuration);
            ConfigurationUnity.Bind<ProviderConfigurationSection>(configuration);
            ConfigurationUnity.Bind<ConverterConfigurationSection>(configuration);
            ConfigurationUnity.Bind<InstanceConfigurationSection>(configuration);
        }
    }
}
#endif