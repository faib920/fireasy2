// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Web.EasyUI
{
    public class LinkButtonSettings : SettingsBase
    {
        /// <summary>
        /// 获取或设置是否禁用。
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// 获取或设置是否为四边形，为 false 时为圆角外形。
        /// </summary>
        public bool? Plain { get; set; }

        /// <summary>
        /// 获取或设置图标样式。
        /// </summary>
        public string IconCls { get; set; }

        /// <summary>
        /// 获取或设置图标对齐。
        /// </summary>
        public string IconAlign { get; set; }
    }
}
