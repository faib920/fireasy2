// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
#if NETCOREAPP
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Xml;

namespace Fireasy.Windows.Forms.Configuration
{
    [ConfigurationSectionStorage("fireasy/winThemes")]
    public class ThemeConfigurationSection : ConfigurationSection
    {
        public override void Initialize(XmlNode section)
        {
            var baseRedererTypeName = section.GetAttributeValue("base");
            if (!string.IsNullOrEmpty(baseRedererTypeName))
            {
                BaseRedererType = baseRedererTypeName.ParseType();
            }

            var treeListRedererTypeName = section.GetAttributeValue("treeList");
            if (!string.IsNullOrEmpty(treeListRedererTypeName))
            {
                TreeListRedererType = treeListRedererTypeName.ParseType();
            }

            var tabControlRedererTypeName = section.GetAttributeValue("tabControl");
            if (!string.IsNullOrEmpty(tabControlRedererTypeName))
            {
                TabControlRedererType = tabControlRedererTypeName.ParseType();
            }
        }

#if NETCOREAPP
        public override void Bind(IConfiguration configuration)
        {
            var baseRedererTypeName = configuration.GetSection("base");
            if (baseRedererTypeName.Exists() && !string.IsNullOrEmpty(baseRedererTypeName.Value))
            {
                BaseRedererType = baseRedererTypeName.Value.ParseType();
            }

            var treeListRedererTypeName = configuration.GetSection("treeList");
            if (treeListRedererTypeName.Exists() && !string.IsNullOrEmpty(treeListRedererTypeName.Value))
            {
                TreeListRedererType = treeListRedererTypeName.Value.ParseType();
            }

            var tabControlRedererTypeName = configuration.GetSection("tabControl");
            if (tabControlRedererTypeName.Exists() && !string.IsNullOrEmpty(tabControlRedererTypeName.Value))
            {
                TabControlRedererType = tabControlRedererTypeName.Value.ParseType();
            }
        }
#endif

        public Type BaseRedererType { get; set; }

        public Type TreeListRedererType { get; set; }

        public Type TabControlRedererType { get; set; }
    }
}
