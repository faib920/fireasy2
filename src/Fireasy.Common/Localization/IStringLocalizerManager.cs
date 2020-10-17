// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Globalization;
using System.Reflection;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 字符串本地化管理器。
    /// </summary>
    public interface IStringLocalizerManager
    {
        /// <summary>
        /// 获取或设置 <see cref="CultureInfo"/> 对象。
        /// </summary>
        CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// 获取 <paramref name="name"/> 对应的 <see cref="IStringLocalizer"/> 实例。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="assembly">程序集，如果缺省则为 Assembly.GetEntryAssembly() 返回的程序集。</param>
        /// <returns></returns>
        IStringLocalizer GetLocalizer(string name, Assembly assembly = null);

        /// <summary>
        /// 使用指定的 <see cref="CultureInfo"/> 来获取 <paramref name="name"/> 对应的 <see cref="IStringLocalizer"/> 实例。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="cultureInfo">区域信息。</param>
        /// <param name="assembly">程序集，如果缺省则为 Assembly.GetEntryAssembly() 返回的程序集。</param>
        /// <returns></returns>
        IStringLocalizer GetLocalizer(string name, CultureInfo cultureInfo, Assembly assembly = null);
    }
}
