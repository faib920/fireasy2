// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 基于 XML 的字符串本地化管理器。
    /// </summary>
    public class XmlStringLocalizerManager : IStringLocalizerManager
    {
        private static readonly ConcurrentDictionary<string, IStringLocalizer> _localizers = new ConcurrentDictionary<string, IStringLocalizer>();

        /// <summary>
        /// 获取或设置 <see cref="CultureInfo"/> 对象。
        /// </summary>
        public CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// 获取 <paramref name="name"/> 对应的 <see cref="IStringLocalizer"/> 实例。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="assembly">程序集，如果缺省则为 Assembly.GetEntryAssembly() 返回的程序集。</param>
        /// <returns></returns>
        public IStringLocalizer GetLocalizer(string name, Assembly assembly = null)
        {
            return GetStringLocalizer(name, CultureInfo ?? CultureInfo.CurrentCulture, assembly);
        }

        /// <summary>
        /// 使用指定的 <see cref="CultureInfo"/> 来获取 <paramref name="name"/> 对应的 <see cref="IStringLocalizer"/> 实例。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="cultureInfo">区域信息。</param>
        /// <param name="assembly">程序集，如果缺省则为 Assembly.GetEntryAssembly() 返回的程序集。</param>
        /// <returns></returns>
        /// <returns></returns>
        public IStringLocalizer GetLocalizer(string name, CultureInfo cultureInfo, Assembly assembly = null)
        {
            return GetStringLocalizer(name, cultureInfo, assembly);
        }

        private IStringLocalizer GetStringLocalizer(string name, CultureInfo cultureInfo, Assembly assembly)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetEntryAssembly();
            }

            var path = new FileInfo(assembly.Location).Directory.FullName;
            var fileName = Path.Combine(path, cultureInfo.Name, string.Concat(name, ".xml"));
            if (!File.Exists(fileName))
            {
                return NullStringLocalizer.Instance;
            }

            return _localizers.GetOrAdd(fileName, key =>
                {
                    var doc = new XmlDocument();
                    doc.Load(key);
                    return new XmlStringLocalizer(doc, cultureInfo);
                });
        }
    }
}
