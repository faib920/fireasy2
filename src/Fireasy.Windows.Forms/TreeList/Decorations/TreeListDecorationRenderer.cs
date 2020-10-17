// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 一个抽象类，用于绘制 <see cref="TreeListCell"/> 时进行修饰。
    /// </summary>
    public abstract class TreeListDecorationRenderer : ControlRenderer
    {
        /// <summary>
        /// 绘制指定的 <see cref="TreeListCell"/> 对象。
        /// </summary>
        /// <param name="e"></param>
        public abstract void DrawCell(TreeListCellRenderEventArgs e);

        /// <summary>
        /// 绘制验证失败的标识。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="cell"></param>
        /// <param name="rect"></param>
        protected virtual void DrawInvalidate(Graphics graphics, TreeListCell cell, Rectangle rect)
        {
            var img = Resource.error;
            graphics.DrawImage(img, rect.Left + rect.Width - img.Width, rect.Top + (rect.Height - img.Height) / 2);
        }
    }
}
