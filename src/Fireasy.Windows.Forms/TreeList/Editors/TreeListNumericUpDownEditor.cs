using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class TreeListNumericUpDownEditor : TreeListWrapEditor<NumericUpDown>
    {
        //[DllImport("user32.dll", SetLastError = true)]
        //static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        //[DllImport("user32.dll")]
        //static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong); 

        public TreeListNumericUpDownEditor()
            : base(new NumericUpDown())
        {
            //var field = typeof(NumericUpDown).GetField("upDownButtons", BindingFlags.Instance | BindingFlags.NonPublic);
            //if (field != null)
            //{
            //    var upDownButton = field.GetValue(Inner) as Control;
            //    var method = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            //    method.Invoke(upDownButton, new object[] { ControlStyles.Selectable, false });
            //}
        }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public override void Show(Rectangle rect)
        {
            base.Show(rect);

            var textbox = Inner as NumericUpDown;
            textbox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textbox.Left = 2;

            textbox.TextAlign = Controller.Cell.Column.TextAlign;
            textbox.Width = Width - 4;
            textbox.Top = (Height - textbox.Height) / 2;

            textbox.Focus();
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public override void SetValue(object value)
        {
            var picker = (NumericUpDown)Inner;
            picker.Value = Convert.ToDecimal(value);
        }

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public override object GetValue()
        {
            var picker = (NumericUpDown)Inner;
            return picker.Value;
        }
    }
}
