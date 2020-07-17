// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 用于访问对内嵌字符串资源中的字符串。
    /// </summary>
    public sealed class StringResource
    {
        private ResourceManager _manager;
        private CultureInfo _cultureInfo;

        internal StringResource()
        {
        }

        /// <summary>
        /// 从资源中获取指定名称的字符串资源。
        /// </summary>
        /// <param name="name">资源的名称。</param>
        /// <param name="args">一个对象数组，其中包含零个或多个要设置格式的对象。</param>
        /// <returns></returns>
        public string GetString(string name, params object[] args)
        {
            var res = _manager.GetString(name, _cultureInfo);
            if (res == null)
            {
                return name;
            }

            if (args != null && args.Length > 0)
            {
                return string.Format(res, args);
            }

            return res;
        }

        /// <summary>
        /// 创建一个 <see cref="StringResource"/> 实例。
        /// </summary>
        /// <param name="resourceName">资源的名称。该名称不包含程序集名称前缀。</param>
        /// <param name="assembly">资源文件所属的程序集。</param>
        /// <param name="cultureInfo">指定区域信息。</param>
        /// <returns></returns>
        public static StringResource Create(string resourceName, Assembly assembly = null, CultureInfo cultureInfo = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetCallingAssembly();
            }

            var instance = new StringResource
            {
                _manager = new ResourceManager(assembly.GetName().Name + "." + resourceName, assembly),
                _cultureInfo = cultureInfo ?? CultureInfo.CurrentCulture
            };

            return instance;
        }
    }
}
