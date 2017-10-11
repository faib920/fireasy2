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
using System.Windows.Forms.VisualStyles;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 提供控件的自定义呈现方法。
    /// </summary>
    public class ControlRenderer
    {
        /// <summary>
        /// 绘制控件的边框。该控件需要实现 <see cref="IBorderStylization"/> 接口。
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
                        for (var i = e.Bounds.X; i < e.Bounds.Width; i+= imgRect.Width)
                        {
                            for (var j = e.Bounds.Y; j < e.Bounds.Height; j+= imgRect.Height)
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
    }

    public class RenderEventArgs
    {
        internal RenderEventArgs(Graphics graphics, Rectangle rect)
        {
            Bounds = rect;
            Graphics = graphics;
        }

        public Rectangle Bounds { get; private set; }

        public Graphics Graphics { get; private set; }
    }

    public class BorderRenderEventArgs : RenderEventArgs
    {
        internal BorderRenderEventArgs(IBorderStylization control, Graphics graphics, Rectangle rect)
            : base (graphics, rect)
        {
            Control = control;
        }

        public IBorderStylization Control { get; private set; }
    }

    public class BackgroundRenderEventArgs : RenderEventArgs
    {
        internal BackgroundRenderEventArgs(IBackgroundAligning control, Graphics graphics, Rectangle rect)
            : base (graphics, rect)
        {
            Control = control;
        }

        public IBackgroundAligning Control { get; private set; }
    }
}
