// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using System.Reflection;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// Combo 的参数选项。
    /// </summary>
    public class ComboSettings : TextBoxSettings
    {
        /// <summary>
        /// 获取或设置下拉框的宽度。
        /// </summary>
        public int? PanelWidth { get; set; }

        /// <summary>
        /// 获取或设置下拉框的宽度。为 0 时表示自动设定高度。
        /// </summary>
        public int? PanelHeight { get; set; }

        /// <summary>
        /// 获取或设置下拉框最小宽度。
        /// </summary>
        public int? PanelMinWidth { get; set; }

        /// <summary>
        /// 获取或设置下拉框最大宽度。
        /// </summary>
        public int? PanelMaxWidth { get; set; }

        /// <summary>
        /// 获取或设置下拉框最小高度。
        /// </summary>
        public int? PanelMinHeight { get; set; }

        /// <summary>
        /// 获取或设置下拉框最大高度。
        /// </summary>
        public int? PanelMaxHeight { get; set; }

        /// <summary>
        /// 获取或设置下拉框的对齐方式。
        /// </summary>
        public string PanelAlign { get; set; }

        /// <summary>
        /// 获取或设置是否可多选。
        /// </summary>
        public bool? Multiple { get; set; }

        /// <summary>
        /// 获取或设置支持多值提交。
        /// </summary>
        public bool? Multivalue { get; set; }

        /// <summary>
        /// 获取或设置在失去焦点的时候是否恢复原始值。
        /// </summary>
        public bool Reversed { get; set; }

        /// <summary>
        /// 获取或设置是否显示下拉箭头。
        /// </summary>
        public bool? HasDownArrow { get; set; }

        /// <summary>
        /// 重写序列化属性值的方法。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        protected override string SerializePropertyValue(PropertyInfo property, object value, ITextSerializer serializer)
        {
            //PanelHeight 为 0 时输出 auto
            if (property.Name == nameof(PanelHeight) && value.To<int?>() == 0)
            {
                return "'auto'";
            }

            return base.SerializePropertyValue(property, value, serializer);
        }
    }
}
