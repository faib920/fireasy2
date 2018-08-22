// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;

namespace Fireasy.Web.EasyUI
{
    public class TagBoxSettings : ComboBoxSettings
    {
        /// <summary>
        /// 获取或设置值。
        /// </summary>
        [TextSerializeElement("value")]
        public string[] Values { get; set; }

        /// <summary>
        /// 获取或设置点击标签框的时候触发的事件。
        /// </summary>
        [EventFunction]
        public string OnClickTag { get; set; }

        /// <summary>
        /// 获取或设置在移除标签框之前触发的事件。
        /// </summary>
        [EventFunction]
        public string OnBeforeRemoveTag { get; set; }

        /// <summary>
        /// 获取或设置在移除标签框时触发的事件。
        /// </summary>
        [EventFunction]
        public string OnRemoveTag { get; set; }
    }
}
