// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class TreeListDateTimeEditor : TreeListWrapEditor<DateTimePicker>
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        private const Int32 WM_LBUTTONDOWN = 0x201;
        private const Int32 MK_LBUTTON = 0x0001;

        /// <summary>
        /// 初始化 <see cref="TreeListDateTimeEditor"/> 类的新实例。
        /// </summary>
        public TreeListDateTimeEditor()
            : base(new DateTimePicker())
        {
        }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public override void Show(Rectangle rect)
        {
            base.Show(rect);

            var picker = (DateTimePicker)Inner;
            picker.Left = -1;

            picker.Width = Width + 2;
            picker.Top = -1;

            picker.Focus();
            SendMessage(picker.Handle, WM_LBUTTONDOWN, MK_LBUTTON, picker.Width - 5);
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            var picker = (DateTimePicker)Inner;
            if (value == null)
            {
                picker.ShowCheckBox = true;
                picker.Checked = false;
            }
            else
            {
                picker.Value = (DateTime)value;
            }
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            var picker = (DateTimePicker)Inner;
            if (picker.ShowCheckBox && !picker.Checked)
            {
                return null;
            }
            else
            {
                return picker.Value;
            }
        }
    }
}
