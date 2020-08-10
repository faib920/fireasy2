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
    /// <summary>
    /// <see cref="TreeListCell"/> 的默认修饰呈现类。
    /// </summary>
    public class TreeListDefaultDecorationRenderer : TreeListDecorationRenderer
    {
        /// <summary>
        /// 绘制指定的 <see cref="TreeListCell"/> 对象。
        /// </summary>
        /// <param name="e"></param>
        public override void DrawCell(TreeListCellRenderEventArgs e)
        {
            var cl = e.Cell.Item.TreeList.GetColumnBound(e.Cell.Column);

            switch (e.Cell.BoxType)
            {
                case BoxType.RadioButton:
                    ThemeManager.BaseRenderer.DrawRadioButton(e.Graphics, e.Bounds, e.Cell.Checked, true);
                    break;
                case BoxType.CheckBox:
                    ThemeManager.BaseRenderer.DrawCheckbox(e.Graphics, e.Bounds, e.Cell.Checked, false, true);
                    break;
            }

            DrawCellText(e.Graphics, e.Cell, e.Bounds, e.DrawState);

            if (!e.Cell.IsValid)
            {
                DrawInvalidate(e.Graphics, e.Cell, e.Bounds);
            }
        }

        /// <summary>
        /// 绘制子项的文本。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="cell"></param>
        /// <param name="rect"></param>
        /// <param name="state"></param>
        protected virtual void DrawCellText(Graphics graphics, TreeListCell cell, Rectangle rect, DrawState state)
        {
            var item = cell.Item;
            var column = cell.Column;
            var treeList = item.TreeList;
            var foreColor = state == DrawState.Selected && treeList.Focused ? SystemColors.Window :
                (!column.CellForeColor.IsEmpty ? column.CellForeColor : (!column.ForeColor.IsEmpty ? column.ForeColor :
                (!item.ForeColor.IsEmpty ? item.ForeColor : treeList.ForeColor)));
            var font = column.CellFont ?? column.Font ?? item.Font ?? treeList.Font;

            var text = GetDrawingText(cell, out bool isNullText);
            if (isNullText && !(state == DrawState.Selected && treeList.Focused))
            {
                foreColor = Color.DarkGray;
            }

            if (item.Highlight)
            {
                foreColor = Color.White;
            }
            else if (state == DrawState.Hot)
            {
                foreColor = SystemColors.HotTrack;
            }

            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping;

            if (cell.Column.TreeList.ColumnHorizontalCenter || cell.Column.TextAlign == HorizontalAlignment.Center)
            {
                flags |= TextFormatFlags.HorizontalCenter;
            }
            else if (cell.Column.TextAlign == HorizontalAlignment.Right)
            {
                flags |= TextFormatFlags.Right;
            }

            //graphics.DrawString(text, font, new SolidBrush(foreColor), rect, sf);
            TextRenderer.DrawText(graphics, text, font, rect, foreColor, flags);
        }

        private string GetDrawingText(TreeListCell cell, out bool isNullText)
        {
            var cellText = cell.Text;
            if (string.IsNullOrEmpty(cellText) && !string.IsNullOrEmpty(cell.NullText))
            {
                isNullText = true;
                return cell.NullText;
            }

            isNullText = false;
            return cellText;
        }

        protected Rectangle GetItemImageRect(Image image, Rectangle rect)
        {
            var bound = Rectangle.Empty;
            var h = (rect.Height - image.Height) / 2;
            return new Rectangle(rect.X, rect.Y + h, image.Width, image.Height);
        }
    }
}
