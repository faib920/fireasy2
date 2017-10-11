using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Fireasy.Common.Extensions;


namespace Fireasy.Windows.Forms
{
    public class TreeListComplexComboBoxEditor : TreeListWrapEditor<ComplexComboBox>
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        private const Int32 CB_SETITEMHEIGHT = 0x153;

        /// <summary>
        /// 初始化 <see cref="TreeListComplexComboBoxEditor"/> 类的新实例。
        /// </summary>
        /// <param name="combobox">指定真实的编辑器。</param>
        public TreeListComplexComboBoxEditor(ComplexComboBox combobox)
            : base(combobox)
        {
        }

        public TreeListComplexComboBoxEditor()
            : base(new ComplexComboBox())
        {
        }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public override void Show(Rectangle rect)
        {
            base.Show(rect);

            var picker = (ComboBox)Inner;
            picker.Left = -1;

            picker.Width = Width + 2;
            picker.Top = -1;

            SendMessage(picker.Handle, CB_SETITEMHEIGHT, -1, rect.Height + 2);

            picker.DroppedDown = true;
            picker.Focus();
        }

        public override void Hide()
        {
            Inner.HideDropDown();
            base.Hide();
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            var picker = (ComplexComboBox)Inner;
            picker.SelectedValue = value;
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            var picker = (ComplexComboBox)Inner;
            return picker.SelectedValue;
        }
    }
}
