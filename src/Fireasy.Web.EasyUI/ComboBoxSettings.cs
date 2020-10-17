// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// ComboBox 的参数选项。
    /// </summary>
    public class ComboBoxSettings : ComboSettings
    {
        /// <summary>
        /// 获取或设置绑定到 Value 的字段名称。
        /// </summary>
        public string ValueField { get; set; }

        /// <summary>
        /// 获取或设置绑定到 Text 的字段名称。
        /// </summary>
        public string TextField { get; set; }

        /// <summary>
        /// 获取或设置分组的字段名称。
        /// </summary>
        public string GroupField { get; set; }

        /// <summary>
        /// 获取或设置取数的服务地址。
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 获取或设置HTTP方法。
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// 获取或设置取数的模式。本地 local 或远程 remote。
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// 获取或设置输入的值是否只能是列表框中的内容。
        /// </summary>
        public bool? LimitToList { get; set; }

        /// <summary>
        /// 获取或设置定位分组选项。
        /// </summary>
        public bool? ShowItemIcon { get; set; }

        /// <summary>
        /// 获取或设置是否显示选中项的图标。可用值有: static 和 sticky。
        /// </summary>
        public string GroupPosition { get; set; }

        /// <summary>
        /// 获取或设置加载远程数据成功的时候触发的函数。
        /// </summary>
        [EventFunction]
        public string OnLoadSuccess { get; set; }

        /// <summary>
        /// 获取或设置用户选择列表项的时候触发的函数。
        /// </summary>
        [EventFunction]
        public string OnSelect { get; set; }

        /// <summary>
        /// 获取或设置加载的数据列表。 
        /// </summary>
        public IEnumerable Data { get; set; }
    }
}
