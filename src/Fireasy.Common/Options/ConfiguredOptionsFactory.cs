// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Fireasy.Common.Options
{
    /// <summary>
    /// 附加标志的选项工厂。
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public sealed class ConfiguredOptionsFactory<TOptions> : IOptionsFactory<TOptions> where TOptions : class, new()
    {
        private readonly IEnumerable<IConfigureOptions<TOptions>> _setups;
        private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigures;

        public ConfiguredOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> setups, IEnumerable<IPostConfigureOptions<TOptions>> postConfigures)
        {
            _setups = setups;
            _postConfigures = postConfigures;
        }

        TOptions IOptionsFactory<TOptions>.Create(string name)
        {
            var options = new TOptions();

            var isConfigured = false;

            foreach (var setup in _setups)
            {
                if (setup is IConfigureNamedOptions<TOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                    isConfigured = true;
                }
                else if (name == Microsoft.Extensions.Options.Options.DefaultName)
                {
                    setup.Configure(options);
                    isConfigured = true;
                }
            }

            foreach (var post in _postConfigures)
            {
                post.PostConfigure(name, options);
                isConfigured = true;
            }

            if (options is IConfiguredOptions cfg)
            {
                cfg.IsConfigured = isConfigured;
            }

            return options;
        }
    }
}
#endif