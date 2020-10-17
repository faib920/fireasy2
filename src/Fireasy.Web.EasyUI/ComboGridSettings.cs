// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// ComboGrid 的参数选项。
    /// </summary>
    public class ComboGridSettings : ComboSettings
    {
        /// <summary>
        /// 获取或设置绑定到 ID 的字段名称。
        /// </summary>
        public string IdField { get; set; }

        /// <summary>
        /// 获取或设置绑定到 Text 的字段名称。
        /// </summary>
        public string TextField { get; set; }

        /// <summary>
        /// 获取或设置取数的模式。本地 local 或远程 remote。
        /// </summary>
        public string Mode { get; set; }
    }
}
