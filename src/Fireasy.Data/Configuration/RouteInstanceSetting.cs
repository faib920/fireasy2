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
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Data.Configuration
{
    /// <summary>
    /// 基于路由服务的数据库实例配置。
    /// </summary>
    [Serializable]
    public class RouteInstanceSetting : DefaultInstanceConfigurationSetting
    {
        /// <summary>
        /// 返回提供者配置名称。
        /// </summary>
        public override string ProviderName
        {
            get { return GetSetting().ProviderName; }
            set { }
        }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public override string ProviderType
        {
            get { return GetSetting().ProviderType; }
            set { }
        }

        /// <summary>
        /// 返回数据库类型。
        /// </summary>
        public override Type DatabaseType
        {
            get { return GetSetting().DatabaseType; }
            set { }
        }

        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        public override string ConnectionString
        {
            get { return GetSetting().ConnectionString; }
            set { }
        }

        /// <summary>
        /// 获取或设置路由服务对象。
        /// </summary>
        public IInstanceRouteService RouteService { get; private set; }

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

#if NETSTANDARD
            public IConfigurationSettingItem Parse(IConfiguration configuration)
            {
                var setting = new RouteInstanceSetting();
                var type = Type.GetType(configuration.GetSection("type").Value, false, true);
                if (type != null)
                {
                    setting.RouteService = type.New<IInstanceRouteService>();
                }
                return setting;
            }
#endif

        }
    }
}
