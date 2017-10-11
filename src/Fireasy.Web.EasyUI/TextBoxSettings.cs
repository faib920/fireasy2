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
    /// TextBox 的参数选项。
    /// </summary>
    public class TextBoxSettings : ValidateBoxSettings
    {
        /// <summary>
        /// 获取或设置文本框类型。可用值有 text 和 password。
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 获取或设置是多行文本框。
        /// </summary>
        public bool? Multiline { get; set; }

        /// <summary>
        /// 获取或设置附加按钮显示的文本内容。
        /// </summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// 获取或设置附加按钮显示的图标。
        /// </summary>
        public string ButtonIcon { get; set; }

        /// <summary>
        /// 获取或设置在输入框显示提示消息。
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// 获取或设置是否可以直接在该字段内输入文字。
        /// </summary>
        public bool? Editable { get; set; }

        /// <summary>
        /// 获取或设置是否禁用该字段。
        /// </summary>
        public bool? Disabled { get; set; }

        /// <summary>
        /// 获取或设置是否将该控件设为只读。
        /// </summary>
        public bool? Readonly { get; set; }

        /// <summary>
        /// 获取或设置组件的宽度。
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// 获取或设置组件的高度。
        /// </summary>
        public int? Height { get; set; }

        /// <summary>
        /// 获取或设置值。
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 获取或设置字段值改变的时候触发的函数。
        /// </summary>
        [EventFunctionAttribute]
        public string OnChange { get; set; }

        /// <summary>
        /// 获取或设置用户点击按钮的时候触发。
        /// </summary>
        [EventFunctionAttribute]
        public string OnClickButton { get; set; }

        /// <summary>
        /// 获取或设置用户点击图标的时候触发。
        /// </summary>
        [EventFunctionAttribute]
        public string OnClickIcon { get; set; }

    }
}
