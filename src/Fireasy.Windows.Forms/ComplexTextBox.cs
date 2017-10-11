
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class ComplexTextBox : TextBox, IBorderStylization
    {
        private string waterMarkText;
        private Color waterMarkTextColor = Color.DarkGray;
        private Behavior behavior;

        /// <summary>
        /// 指定水印文字。
        /// </summary>
        [DefaultValue((string)null)]
        [Description("指定水印文字。")]
        public string WaterMarkText
        {
            get { return waterMarkText; }
            set
            {
                waterMarkText = value;
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
            get { return waterMarkTextColor; }
            set
            {
                waterMarkTextColor = value;
                base.Invalidate();
            }
        }


        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == NativeMethods.WM_PAINT || m.Msg == NativeMethods.WM_NCPAINT)
            {
                WmPaint(ref m);
            }
        }

        private void WmPaint(ref Message m)
        {
            using (Graphics graphics = Graphics.FromHwnd(base.Handle))
            {
                if (Text.Length == 0 && !string.IsNullOrEmpty(waterMarkText) && !Focused)
                {
                    TextFormatFlags format = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;

                    if (RightToLeft == RightToLeft.Yes)
                    {
                        format |= TextFormatFlags.RightToLeft | TextFormatFlags.Right;
                    }

                    TextRenderer.DrawText(graphics, waterMarkText, Font, base.ClientRectangle, waterMarkTextColor, format);
                }
            }
        }
    }
}
