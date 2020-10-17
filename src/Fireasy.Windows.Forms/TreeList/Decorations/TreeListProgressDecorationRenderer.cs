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
using System.Windows.Forms.VisualStyles;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 使 <see cref="TreeListCell"/> 呈现为进度条样式。
    /// </summary>
    public class TreeListProgressDecorationRenderer : TreeListDefaultDecorationRenderer
    {
        /// <summary>
        /// 绘制指定的 <see cref="TreeListCell"/> 对象。
        /// </summary>
        /// <param name="e"></param>
        public override void DrawCell(TreeListCellRenderEventArgs e)
        {
            var rect = e.Bounds;
            rect.Inflate(-1, -1);

            DrawProgressBackground(e.Graphics, e.Cell, rect);

            if (e.Cell.Value == null)
            {
                return;
            }

            rect.Inflate(-1, -1);
            var r = Convert.ToDecimal(e.Cell.Value);
            rect.Width = (int)(rect.Width * r);
            DrawProgressValueBar(e.Graphics, e.Cell, rect);
        }

        /// <summary>
        /// 绘制进度条的背景。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="cell"></param>
        /// <param name="rect"></param>
        private void DrawProgressBackground(Graphics graphics, TreeListCell cell, Rectangle rect)
        {
            if (Application.RenderWithVisualStyles)
            {
                var element = VisualStyleElement.ProgressBar.Bar.Normal;
                if (VisualStyleRenderer.IsElementDefined(element))
                {
                    new VisualStyleRenderer(element).DrawBackground(graphics, rect);
                    return;
                }
            }

            graphics.FillRectangle(SystemBrushes.Window, rect);
            ControlPaint.DrawBorder3D(graphics, rect, Border3DStyle.Flat);
        }

        /// <summary>
        /// 根据 <see cref="TreeListCell"/> 的值绘制进度条。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="cell"></param>
        /// <param name="rect"></param>
        private void DrawProgressValueBar(Graphics graphics, TreeListCell cell, Rectangle rect)
        {
            if (Application.RenderWithVisualStyles)
            {
                var element = VisualStyleElement.ProgressBar.Chunk.Normal;
                if (VisualStyleRenderer.IsElementDefined(element))
                {
                    new VisualStyleRenderer(element).DrawBackground(graphics, rect);
                    return;
                }
            }

            rect.Inflate(-1, -1);
            graphics.FillRectangle(SystemBrushes.Highlight, rect);
        }
    }
}
