// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Globalization;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 提供字符串本地化的方法。
    /// </summary>
    public interface IStringLocalizer
    {
        /// <summary>
        /// 获取当前的 <see cref="CultureInfo"/> 对象。
        /// </summary>
        CultureInfo CultureInfo { get; }

        /// <summary>
        /// 获取 <paramref name="name"/> 配置的字符串。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string this[string name] { get; }

        /// <summary>
        /// 获取 <paramref name="name"/> 配置的字符串，并使用参数进行格式化。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        string this[string name, params object[] args] { get; }
    }
}
