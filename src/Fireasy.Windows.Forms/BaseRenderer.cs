// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Lord. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 基础的控件呈现类。
    /// </summary>
    public class BaseRenderer
    {
        /// <summary>
        /// 绘制控件的背景。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawBackground(BackgroundRenderEventArgs e)
        {
            var control = e.Control as Control;
            if (control == null)
            {
                return;
            }

            e.Graphics.Clear(control.BackColor);

            if (control.BackgroundImage != null)
            {
                var imgRect = new Rectangle(0, 0, control.BackgroundImage.Width, control.BackgroundImage.Height);
                var offsetRect = e.Bounds;
                offsetRect.Offset(0, -e.Bounds.Y);

                switch (control.BackgroundImageLayout)
                {
                    case ImageLayout.None:
                        imgRect.Offset(0, e.Bounds.Y);
                        imgRect = GetImageRectByAlignment(e.Bounds, imgRect, e.Control.BackgroundImageAligment);
                        e.Graphics.DrawImage(control.BackgroundImage, imgRect);
                        break;
                    case ImageLayout.Tile:
                        for (var i = e.Bounds.X; i < e.Bounds.Width; i += imgRect.Width)
                        {
                            for (var j = e.Bounds.Y; j < e.Bounds.Height; j += imgRect.Height)
                            {
                                e.Graphics.DrawImage(control.BackgroundImage, new Rectangle(i, j, imgRect.Width, imgRect.Height));
                            }
                        }

                        break;
                    case ImageLayout.Stretch:
                        e.Graphics.DrawImage(control.BackgroundImage, e.Bounds);
                        break;
                }
            }
        }

        /// <summary>
        /// 根据 <see cref="System.Drawing.ContentAlignment"/> 获取图像的绘制位置。
        /// </summary>
        /// <param name="clientRect">图像绘制的范围。</param>
        /// <param name="imgRect">图像的大小范围。</param>
        /// <param name="align">图像对齐方式。</param>
        /// <returns></returns>
        protected virtual Rectangle GetImageRectByAlignment(Rectangle clientRect, Rectangle imgRect, System.Drawing.ContentAlignment align)
        {
            switch (align)
            {
                case System.Drawing.ContentAlignment.TopCenter:
                    imgRect.Offset((clientRect.Width - imgRect.Width) / 2, 0);
                    break;
                case System.Drawing.ContentAlignment.TopRight:
                    imgRect.Offset(clientRect.Width - imgRect.Width, 0);
                    break;
                case System.Drawing.ContentAlignment.BottomLeft:
                    imgRect.Offset(0, clientRect.Height - imgRect.Height);
                    break;
                case System.Drawing.ContentAlignment.BottomCenter:
                    imgRect.Offset((clientRect.Width - imgRect.Width) / 2, clientRect.Height - imgRect.Height);
                    break;
                case System.Drawing.ContentAlignment.BottomRight:
                    imgRect.Offset(clientRect.Width - imgRect.Width, clientRect.Height - imgRect.Height);
                    break;
                case System.Drawing.ContentAlignment.MiddleLeft:
                    imgRect.Offset(0, (clientRect.Height - imgRect.Height) / 2);
                    break;
                case System.Drawing.ContentAlignment.MiddleCenter:
                    imgRect.Offset((clientRect.Width - imgRect.Width) / 2, (clientRect.Height - imgRect.Height) / 2);
                    break;
                case System.Drawing.ContentAlignment.MiddleRight:
                    imgRect.Offset(clientRect.Width - imgRect.Width, (clientRect.Height - imgRect.Height) / 2);
                    break;
            }

            return imgRect;
        }

        /// <summary>
        /// 绘制边框。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawBorder(BorderRenderEventArgs e)
        {
            if (e.Control == null)
            {
                return;
            }

            //启用了VisualStyles
            if (Application.RenderWithVisualStyles && e.Control.BorderStyle != BorderStyle.None)
            {
                ControlPaint.DrawBorder(e.Graphics, e.Bounds, VisualStyleInformation.TextControlBorder, ButtonBorderStyle.Solid);
            }
            else if (e.Control.BorderStyle == BorderStyle.Fixed3D)
            {
                ControlPaint.DrawBorder3D(e.Graphics, e.Bounds, Border3DStyle.Sunken);
            }
            else if (e.Control.BorderStyle == BorderStyle.FixedSingle)
            {
                ControlPaint.DrawBorder3D(e.Graphics, e.Bounds, Border3DStyle.Flat);
            }
        }

        /// <summary>
        /// 绘制 CheckBox。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="rect"></param>
        /// <param name="checked"></param>
        /// <param name="mixed"></param>
        /// <param name="enabled"></param>
        public virtual void DrawCheckbox(Graphics graphics, Rectangle rect, bool @checked, bool mixed, bool enabled)
        {
            if (Application.RenderWithVisualStyles)
            {
                var element = GetCheckVisualStyleElement(@checked, mixed, enabled);
                if (element != null && VisualStyleRenderer.IsElementDefined(element))
                {
                    new VisualStyleRenderer(element).DrawBackground(graphics, rect);
                    return;
                }
            }

            var bstate = ButtonState.Flat;
            if (@checked)
            {
                bstate |= ButtonState.Checked;
            }

            if (!enabled)
            {
                bstate |= ButtonState.Inactive;
            }

            ControlPaint.DrawCheckBox(graphics, rect, bstate);

            if (mixed)
            {
                var r = rect;
                r.Inflate(-4, -4);
                graphics.FillRectangle(Brushes.LightGray, r);
            }
        }

        /// <summary>
        /// 绘制 RadioButton。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="rect"></param>
        /// <param name="checked"></param>
        /// <param name="enabled"></param>
        public virtual void DrawRadioButton(Graphics graphics, Rectangle rect, bool @checked, bool enabled)
        {
            if (Application.RenderWithVisualStyles)
            {
                var element = GetCheckVisualStyleElement(@checked, enabled);
                if (element != null && VisualStyleRenderer.IsElementDefined(element))
                {
                    new VisualStyleRenderer(element).DrawBackground(graphics, rect);
                    return;
                }
            }

            var bstate = ButtonState.Flat;
            if (@checked)
            {
                bstate |= ButtonState.Checked;
            }

            if (!enabled)
            {
                bstate |= ButtonState.Inactive;
            }

            ControlPaint.DrawRadioButton(graphics, rect, bstate);
        }

        private VisualStyleElement GetCheckVisualStyleElement(bool @checked, bool mixed, bool enabled)
        {
            if (mixed)
            {
                return enabled ? VisualStyleElement.Button.CheckBox.MixedNormal :
                    VisualStyleElement.Button.CheckBox.MixedDisabled;
            }
            else if (@checked)
            {
                return enabled ? VisualStyleElement.Button.CheckBox.CheckedNormal :
                    VisualStyleElement.Button.CheckBox.CheckedDisabled;
            }

            return enabled ? VisualStyleElement.Button.CheckBox.UncheckedNormal :
                VisualStyleElement.Button.CheckBox.UncheckedDisabled;
        }

        private VisualStyleElement GetCheckVisualStyleElement(bool @checked, bool enabled)
        {
            if (@checked)
            {
                return enabled ? VisualStyleElement.Button.RadioButton.CheckedNormal :
                    VisualStyleElement.Button.RadioButton.CheckedDisabled;
            }

            return enabled ? VisualStyleElement.Button.RadioButton.UncheckedNormal :
                VisualStyleElement.Button.RadioButton.UncheckedDisabled;
        }
    }
}
