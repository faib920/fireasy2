// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System.Xml;

namespace Fireasy.Web.Mvc.Configuration
{
    [ConfigurationSectionStorage("fireasy/mvc/bundles")]
    public class BundleGroupConfigurationSection : ConfigurationSection<BundleGroupConfigurationSetting>
    {
        /// <summary>
        /// 根据 <paramref name="section"/> 来初始化配置节。
        /// </summary>
        /// <param name="section"></param>
        public override void Initialize(XmlNode section)
        {
            base.InitializeNode(section, "bundle", null, node =>
                {
                    var setting = new BundleGroupConfigurationSetting();
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        switch (child.Name)
                        {
                            case "script":
                                setting.Resources.Add(new ResourcePath { Type = ResourceType.Script, Path = child.InnerText });
                                break;
                            case "style":
                                setting.Resources.Add(new ResourcePath { Type = ResourceType.Style, Path = child.InnerText });
                                break;
                        }
                    }

                    return setting;
                });

            EnableOptimization = section.GetAttributeValue("enableOptimization", true);
        }

        /// <summary>
        /// 获取或设置是否优化脚本（压缩）。
        /// </summary>
        public bool EnableOptimization { get; set; }
    }
}
#endif