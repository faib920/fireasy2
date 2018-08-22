// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>

// -----------------------------------------------------------------------
namespace Fireasy.Common.Configuration
{
    internal class ExtendConfigurationSetting : IConfigurationSettingItem
    {
        public IConfigurationSettingItem Base { get; set; }

        public IConfigurationSettingItem Extend { get; set; }
    }
}
