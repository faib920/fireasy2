// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data
{
    /// <summary>
    /// 全局的参数设置。
    /// </summary>
    public sealed class GlobalOptions
    {
        public readonly static GlobalOptions Default = new GlobalOptions();

        /// <summary>
        /// 获取或设置是否附加引号标识符。默认为 false。
        /// </summary>
        public bool AttachQuote { get; set; }
    }
}
