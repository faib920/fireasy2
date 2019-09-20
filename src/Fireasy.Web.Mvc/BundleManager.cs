// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if !NETCOREAPP
using Fireasy.Common.Configuration;
using System.Text;
using System.Web;
using System.Web.Optimization;
using Fireasy.Web.Mvc.Configuration;

namespace Fireasy.Web.Mvc
{
    /// <summary>
    /// 脚本和样式的捆绑管理器。
    /// </summary>
    public class BundleManager
    {
        /// <summary>
        /// 通过配置文件来配置 <see cref="BundleCollection"/>。
        /// </summary>
        /// <param name="containMin">是否允许包含 min 文件。</param>
        public static void Config(bool containMin = true)
        {
            if (containMin)
            {
                BundleTable.Bundles.IgnoreList.Clear();
                BundleTable.Bundles.IgnoreList.Ignore("*.intellisense.js");
                BundleTable.Bundles.IgnoreList.Ignore("*-vsdoc.js");
                BundleTable.Bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            }

            var section = ConfigurationUnity.GetSection<BundleGroupConfigurationSection>();
            if (section == null)
            {
                return;
            }

            BundleTable.EnableOptimizations = section.EnableOptimization;

            foreach (var setting in section.Settings)
            {
                var index = 0;
                foreach (var res in setting.Value.Resources)
                {
                    switch (res.Type)
                    {
                        case ResourceType.Script:
                            GetBundle("~/js/" + setting.Key + "/" + index).Include(res.Path);
                            break;
                        case ResourceType.Style:
                            GetBundle("~/css/" + setting.Key + "/" + index).Include(res.Path);
                            break;
                    }

                    index++;
                }
            }
        }

        /// <summary>
        /// 呈现指定路径的脚本和样式表。
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static IHtmlString Render(params string[] paths)
        {
            var sb = new StringBuilder();

            foreach (var path in paths)
            {
                foreach (var bundle in BundleTable.Bundles)
                {
                    if (bundle.Path.StartsWith("~/js/" + path))
                    {
                        sb.Append(Scripts.Render(bundle.Path).ToHtmlString());
                    }
                    else if (bundle.Path.StartsWith("~/css/" + path))
                    {
                        sb.Append(Styles.Render(bundle.Path).ToHtmlString());
                    }
                }
            }

            return new HtmlString(sb.ToString());
        }

        private static Bundle GetBundle(string key)
        {
            var bundle = BundleTable.Bundles.GetBundleFor(key);
            if (bundle == null)
            {
                bundle = new Bundle(key);
                BundleTable.Bundles.Add(bundle);
            }

            return bundle;
        }
    }

    /// <summary>
    /// 资源的类型。
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// JavaScript 脚本。
        /// </summary>
        Script,
        /// <summary>
        /// 样式表。
        /// </summary>
        Style
    }
}
#endif