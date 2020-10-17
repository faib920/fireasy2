// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;

namespace Fireasy.Data.Configuration
{
    internal class InstanceParseHandleFactory
    {
        internal static IConfigurationSettingParseHandler GetParseHandler(string storeType)
        {
            if (string.IsNullOrEmpty(storeType))
            {
                return new StringInstanceSetting.SettingParseHandler();
            }

            return (storeType.ToLower()) switch
            {
                "registry" => new RegistryInstanceSetting.SettingParseHandler(),
                "xml" => new XmlInstanceSetting.SettingParseHandler(),
                "json" => new JsonInstanceSetting.SettingParseHandler(),
                "binary" => new BinaryInstanceSetting.SettingParseHandler(),
                "route" => new RouteInstanceSetting.SettingParseHandler(),
                _ => new StringInstanceSetting.SettingParseHandler(),
            };
        }
    }
}
