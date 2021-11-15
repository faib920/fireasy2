// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 用于绑定的 <see cref="IConfigurationSection"/>。
    /// </summary>
    public sealed class BindingConfiguration : Microsoft.Extensions.Configuration.IConfigurationSection
    {
        public BindingConfiguration(IConfiguration root, string path)
        {
            if (root is BindingConfiguration bc)
            {
                Root = bc.Root;
            }
            else
            {
                Root = root;
            }

            Current = root.GetSection(path);
        }

        public BindingConfiguration(IConfiguration root, Microsoft.Extensions.Configuration.IConfigurationSection current)
        {
            if (root is BindingConfiguration bc)
            {
                Root = bc.Root;
            }
            else
            {
                Root = root;
            }

            Current = current;
        }

        public string this[string key] { get => Current[key]; set => Current[key] = value; }

        /// <summary>
        /// 获取根节点配置。
        /// </summary>
        public IConfiguration Root { get; }

        /// <summary>
        /// 获取当前节点配置。
        /// </summary>
        public Microsoft.Extensions.Configuration.IConfigurationSection Current { get; }

        public string Key => Current.Key;

        public string Path => Current.Path;

        public string Value { get => Current.Value; set => Current.Value = value; }

        public IEnumerable<Microsoft.Extensions.Configuration.IConfigurationSection> GetChildren()
        {
            return Current.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return Current.GetReloadToken();
        }

        public Microsoft.Extensions.Configuration.IConfigurationSection GetSection(string key)
        {
            return Current.GetSection(key);
        }
    }
}
#endif
