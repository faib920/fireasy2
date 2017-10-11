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
    /// combotree 的参数选项。
    /// </summary>
    public class ComboTreeSettings : ComboBoxSettings
    {
        /// <summary>
        /// 获取或设置节点在展开或折叠的时候是否显示动画效果。
        /// </summary>
        public bool? Animate { get; set; }

        /// <summary>
        /// 获取或设置是否层叠选中状态。
        /// </summary>
        public bool? CascadeCheck { get; set; }

        /// <summary>
        /// 获取或设置是否只在末级节点之前显示复选框。
        /// </summary>
        public bool? OnlyLeafCheck { get; set; }

        /// <summary>
        /// 获取或设置是否在每一个借点之前都显示复选框。
        /// </summary>
        public bool? Checkbox { get; set; }

        /// <summary>
        /// 获取或设置是否显示树控件上的虚线。
        /// </summary>
        public bool? Lines { get; set; }
    }
}
