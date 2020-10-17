// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class ComplexTextBox : TextBox, IBorderStylization
    {
        private string _waterMarkText;
        private Color _waterMarkTextColor = Color.DarkGray;
        private readonly Behavior _behavior;

        /// <summary>
        /// 指定水印文字。
        /// </summary>
        [DefaultValue((string)null)]
        [Description("指定水印文字。")]
        public string WaterMarkText
        {
            get { return _waterMarkText; }
            set
            {
                _waterMarkText = value;
                base.Invalidate();
            }
        }

        /// <summary>
        /// 指定水印文字颜色。
        /// </summary>
        [DefaultValue(typeof(Color), "DarkGray")]
        [Description("指定水印文字颜色。")]
        public Color WaterMarkTextColor
        {
            get { return _waterMarkTextColor; }
            set
            {
                _waterMarkTextColor = value;
                base.Invalidate();
            }
        }


        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == NativeMethods.W_PAINT || m.Msg == NativeMethods.W_NCPAINT)
            {
                WmPaint(ref m);
            }
        }

        private void WmPaint(ref Message m)
        {
            using (Graphics graphics = Graphics.FromHwnd(base.Handle))
            {
                if (Text.Length == 0 && !string.IsNullOrEmpty(_waterMarkText) && !Focused)
                {
                    TextFormatFlags format = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;

                    if (RightToLeft == RightToLeft.Yes)
                    {
                        format |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                    }

                    TextRenderer.DrawText(graphics, _waterMarkText, Font, base.ClientRectangle, _waterMarkTextColor, format);
                }
            }
        }
    }
}
