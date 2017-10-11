// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Xml;
using Fireasy.Common;
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Data.Provider;

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 基于路由服务的数据库实例配置。
    /// </summary>
    [Serializable]
    public class RouteInstanceSetting : IInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        public string ProviderName
        {
            get { return GetSetting().ProviderName; }
            set { }
        }

        /// <summary>
        /// 获取实例名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public string ProviderType
        {
            get { return GetSetting().ProviderType; }
            set { }
        }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public Type DatabaseType
        {
            get { return GetSetting().DatabaseType; }
            set { }
        }

        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        public string ConnectionString
        {
            get { return GetSetting().ConnectionString; }
            set { }
        }

        /// <summary>
        /// 获取或设置路由服务对象。
        /// </summary>
        public IInstanceRouteService RouteService { get; set; }

        private IInstanceConfigurationSetting GetSetting()
        {
            Guard.NullReference(RouteService);
            return RouteService.GetSetting();
        }

        internal class SettingParseHandler : IConfigurationSettingParseHandler
        {
            public IConfigurationSettingItem Parse(XmlNode node)
            {
                var setting = new RouteInstanceSetting();
                var type = Type.GetType(node.GetAttributeValue("type"), false, true);
                if (type != null)
                {
                    setting.RouteService = type.New<IInstanceRouteService>();
                }
                return setting;
            }
        }
    }
}
