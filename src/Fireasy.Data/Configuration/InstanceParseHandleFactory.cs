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

            switch (storeType.ToLower())
            {
                case "registry":
                    return new RegistryInstanceSetting.SettingParseHandler();
                case "xml":
                    return new XmlInstanceSetting.SettingParseHandler();
                case "json":
                    return new JsonInstanceSetting.SettingParseHandler();
                case "binary":
                    return new BinaryInstanceSetting.SettingParseHandler();
                case "route":
                    return new RouteInstanceSetting.SettingParseHandler();
                default:
                    return new StringInstanceSetting.SettingParseHandler();
            }
        }
    }
}
