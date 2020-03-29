// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#if NETSTANDARD
namespace Fireasy.Common.Options
{
    /// <summary>
    /// 附加标志的选项。
    /// </summary>
    public interface IConfiguredOptions
    {
        /// <summary>
        /// 获取或设置是否配置过。
        /// </summary>
        bool IsConfigured { get; set; }
    }
}
#endif