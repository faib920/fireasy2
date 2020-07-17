// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Globalization;
using System.Resources;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 缺省的字符串本地化，使用资源管理器进行配置。
    /// </summary>
    public class DefaultStringLocalizer : IStringLocalizer
    {
        private readonly ResourceManager _resourceMgr;
        private readonly ConcurrentDictionary<string, ResourceSet> _resSets = new ConcurrentDictionary<string, ResourceSet>();

        public DefaultStringLocalizer(ResourceManager resourceMgr, CultureInfo cultureInfo)
        {
            _resourceMgr = resourceMgr;
            CultureInfo = cultureInfo;
        }

        public CultureInfo CultureInfo { get; private set; }

        public string this[string name]
        {
            get
            {
                var str = GetResourceString(name);
                if (str == null)
                {
                    return name;
                }

                return str;
            }
        }

        public string this[string name, params object[] args]
        {
            get
            {
                var str = GetResourceString(name);
                if (str == null)
                {
                    return name;
                }

                if (args != null && args.Length > 0)
                {
                    return string.Format(str, args);
                }

                return str;
            }
        }

        private string GetResourceString(string name)
        {
            var set = _resSets.GetOrAdd(CultureInfo.Name, k => _resourceMgr.GetResourceSet(CultureInfo, true, false));
            if (set == null)
            {
                return name;
            }

            return set.GetString(name);
        }
    }
}
