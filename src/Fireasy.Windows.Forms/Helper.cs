// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using Fireasy.Common;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public static class Helper
    {
        /// <summary>
        /// 使用新的 <see cref="Region"/> 限定绘画区域，结束后恢复原来的 <see cref="Region"/>。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="region">新的限定绘画的区域。</param>
        /// <param name="action">要使用新 <see cref="Region"/> 限定绘制的方法。</param>
        /// <param name="set"></param>
        public static void KeepClip(this Graphics graphics, Region region, Action action, bool set = true)
        {
            Guard.ArgumentNull(graphics, "graphics");
            Guard.ArgumentNull(region, "region");
            Guard.ArgumentNull(action, "action");

            Region saved = null;
            if (set)
            {
                saved = graphics.Clip == null ? null : graphics.Clip.Clone();
                graphics.SetClip(region, CombineMode.Intersect);
            }

            action();

            if (saved != null)
            {
                graphics.SetClip(saved, CombineMode.Replace);
            }
        }

        public static void KeepClip(this Graphics graphics, Rectangle rect, Action action, bool set = true)
        {
            graphics.KeepClip(new Region(rect), action, set);
        }

        public static Rectangle ReduceRight(this Rectangle rect, int width)
        {
            rect.X += width;
            rect.Width -= width;
            return rect;
        }

        public static Rectangle ReduceLeft(this Rectangle rect, int width)
        {
            rect.X -= width;
            rect.Width += width;
            return rect;
        }

        public static Rectangle Middle(this Rectangle rect, int width, int height)
        {
            return new Rectangle(rect.X + (rect.Width - width) / 2, rect.Y + (rect.Height - height) / 2, width, height);
        }

        public static Image GetImage(this IImageDefinition definition)
        {
            if (definition.Image != null)
            {
                return definition.Image;
            }

            if (definition.ImageList == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(definition.ImageKey) &&
                definition.ImageList.Images.ContainsKey(definition.ImageKey))
            {
                return definition.ImageList.Images[definition.ImageKey];
            }

            if (definition.ImageIndex >= 0 && definition.ImageIndex <= definition.ImageList.Images.Count - 1)
            {
                return definition.ImageList.Images[definition.ImageIndex];
            }

            return null;
        }
    }
}
