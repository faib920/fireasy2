// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System;
using System.Xml;

namespace Fireasy.Windows.Forms.Configuration
{
    [ConfigurationSectionStorage("fireasy/winThemes")]
    public class ThemeConfigurationSection : ConfigurationSection
    {
        public override void Initialize(XmlNode section)
        {
            var baseRedererTypeName = section.GetAttributeValue("treeList");
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

        public Type BaseRedererType { get; set; }

        public Type TreeListRedererType { get; set; }

        public Type TabControlRedererType { get; set; }
    }
}
