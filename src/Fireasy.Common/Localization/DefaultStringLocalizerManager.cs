// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 缺省的字符串本地化管理器，使用资源管理器进行配置。
    /// </summary>
    public class DefaultStringLocalizerManager : IStringLocalizerManager
    {
        private static readonly ConcurrentDictionary<string, IStringLocalizer> _localizers = new ConcurrentDictionary<string, IStringLocalizer>();

        /// <summary>
        /// <see cref="DefaultStringLocalizerManager"/> 的默认实例。
        /// </summary>
        public static DefaultStringLocalizerManager Instance = new DefaultStringLocalizerManager();

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

            var baseName = string.Concat(assembly.GetName().Name, ".", name);
            var cacheKey = string.Concat(baseName, ".", cultureInfo.Name);
            return _localizers.GetOrAdd(cacheKey, k => new DefaultStringLocalizer(new ResourceManager(baseName, assembly), cultureInfo));
        }
    }
}
