// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD

using Fireasy.App.Licence;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ConfigurationBinder
    {
        internal static void Bind(IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                services.AddSingleton<ILicenseManager, DefaultLicenseManager>();
                services.AddSingleton<ILicenseDataStore, DefaultLicenseDataStore>();
            }
        }
    }
}
#endif