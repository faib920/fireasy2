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

namespace Fireasy.Windows.Forms
{
    public class TreeListComboBoxEditor : TreeListWrapEditor<ComboBox>
    {
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, Int32 wParam, Int32 lParam);
        private const Int32 CB_SETITEMHEIGHT = 0x153;

        /// <summary>
        /// 初始化 <see cref="TreeListTextEditor"/> 类的新实例。
        /// </summary>
        /// <param name="combobox">指定真实的编辑器。</param>
        public TreeListComboBoxEditor(ComboBox combobox)
            : base(combobox)
        {
        }

        public TreeListComboBoxEditor()
            : base(new CustomComboBox())
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

            SendMessage(picker.Handle, CB_SETITEMHEIGHT, -1, rect.Height - 3);

            picker.DroppedDown = true;
            picker.Focus();
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            var picker = (ComboBox)Inner;
            picker.SelectedItem = value;
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            var picker = (ComboBox)Inner;
            return picker.SelectedItem;
        }

        private class CustomComboBox : ComboBox
        {
            public CustomComboBox()
            {
                DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
                ItemHeight = 20;
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (!e.State.HasFlag(DrawItemState.ComboBoxEdit))
                {
                    e.DrawBackground();
                }

                if (e.Index == -1)
                {
                    return;
                }

                var brush = e.State.HasFlag(DrawItemState.Selected) && !e.State.HasFlag(DrawItemState.ComboBoxEdit) ?
                    SystemBrushes.HighlightText : SystemBrushes.WindowText;
                var sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                e.Graphics.DrawString(Items[e.Index].ToString(), Font, brush, e.Bounds, sf);
            }
        }
    }
}
