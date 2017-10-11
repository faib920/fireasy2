// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 一个支持文本输入的编辑器。
    /// </summary>
    public class TreeListTextEditor : TreeListWrapEditor<TextBoxBase>
    {
        /// <summary>
        /// 初始化 <see cref="TreeListTextEditor"/> 类的新实例。
        /// </summary>
        /// <param name="textBox">指定真实的编辑器。</param>
        public TreeListTextEditor(TextBoxBase textBox)
            : base(textBox)
        {
        }

        /// <summary>
        /// 初始化 <see cref="TreeListTextEditor"/> 类的新实例。
        /// </summary>
        public TreeListTextEditor()
            : base(new TextBox())
        {
        }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public override void Show(Rectangle rect)
        {
            base.Show(rect);

            var textbox = Inner as TextBox;
            textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textbox.Left = 2;

            textbox.TextAlign = Controller.Cell.Column.TextAlign;
            textbox.Width = Width - 4;
            textbox.Top = (Height - textbox.Height) / 2;

            textbox.Focus();
            textbox.SelectAll();
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            (Inner as TextBoxBase).Text = value == null ? string.Empty : value.ToString();
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            return (Inner as TextBoxBase).Text;
        }
    }
}
