// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 使 <see cref="TreeListCell"/> 呈现为对与错的布尔样式。
    /// </summary>
    public class TreeListBooleanDecorationRenderer : TreeListDefaultDecorationRenderer
    {
        /// <summary>
        /// 绘制指定的 <see cref="TreeListCell"/> 对象。
        /// </summary>
        /// <param name="e"></param>
        public override void DrawCell(TreeListCellRenderEventArgs e)
        {
            if (e.Cell.Value == null)
            {
                return;
            }

            var value = (bool)e.Cell.Value;
            var rect = e.Bounds.Middle(16, 16);
            e.Graphics.DrawImage(value ? Resource._checked : Resource._unchecked, rect);
        }
    }
}
