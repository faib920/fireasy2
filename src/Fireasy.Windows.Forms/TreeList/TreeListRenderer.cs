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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 提供 <see cref="TreeList"/> 的自定义呈现方法。
    /// </summary>
    public class TreeListRenderer : ControlRenderer
    {
        /// <summary>
        /// 绘制 <see cref="TreeList"/> 的列头。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawColumnHeader(TreeListColumnRenderEventArgs e)
        {
            //排序箭头的绘制区
            var sortArrayRect = GetColumnHeaderSortArrayBound(e.Column, e.Bounds);

            //图标绘制区
            var image = e.Column == null ? null : e.Column.GetImage();
            var imageRect = image == null ? Rectangle.Empty : GetColumnHeaderImageRect(image, e.Column.ImageAlign, e.Bounds, sortArrayRect.Width);

            //文本绘制区
            var textBound = e.Column == null ? Rectangle.Empty : GetColumnHeaderTextRect(e.Bounds, sortArrayRect.Width, imageRect.Width, e.Column.ImageAlign);

            DrawColumnHeaderBackground(e.Graphics, e.Bounds, e.DrawState);

            if (!sortArrayRect.IsEmpty && sortArrayRect.Left > e.Bounds.Left)
            {
                var order = e.Column.TreeList.GetSortOrder(e.Column);
                DrawColumnSortArray(e.Graphics, sortArrayRect, order);
            }

            if (!imageRect.IsEmpty)
            {
                e.Graphics.DrawImage(image, imageRect);
            }

            if (!textBound.IsEmpty)
            {
                DrawColumnHeaderText(e.Graphics, e.Column, textBound);
            }
        }

        /// <summary>
        /// 绘制行号列头。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawRowNumberColumn(TreeListColumnRenderEventArgs e)
        {
            DrawColumnHeaderBackground(e.Graphics, e.Bounds, DrawState.Normal);
        }

        /// <summary>
        /// 绘制行号。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawRowNumber(TreeListRowNumberRenderEventArgs e)
        {
            using (var brush = new SolidBrush(GetRowNumberBackgroundColor(e)))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.HorizontalCenter;

            var color = e.DrawState == DrawState.Selected && e.TreeList.Focused ? SystemColors.Window : SystemColors.WindowText;
            TextRenderer.DrawText(e.Graphics, e.Index.ToString(), e.TreeList.Font, e.Bounds, color, flags);
        }

        private Color GetRowNumberBackgroundColor(TreeListRowNumberRenderEventArgs e)
        {
            if (e.DrawState == DrawState.Selected)
            {
                return e.TreeList.Focused ? SystemColors.Highlight : SystemColors.ButtonFace;
            }
            else
            {
                return Color.FromArgb(252, 252, 252);
            }
        }

        /// <summary>
        /// 绘制加载状态。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawLoading(TreeListRenderEventArgs e)
        {
            if (string.IsNullOrEmpty(e.TreeList.LoadingText))
            {
                return;
            }

            var color = Color.FromArgb(30, 220, 220, 220);
            using (var brush = new SolidBrush(color))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            var size = TextRenderer.MeasureText(e.TreeList.LoadingText, e.TreeList.Font);
            var rect = e.Bounds.Middle(size.Width + 80, size.Height + 40);

            e.Graphics.FillRectangle(SystemBrushes.Window, rect);
            e.Graphics.DrawRectangle(Pens.LightGray, rect);

            var sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(e.TreeList.LoadingText, e.TreeList.Font, Brushes.Gray, rect, sf);
        }

        /// <summary>
        /// 绘制分组。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawGroup(TreeListGroupRenderEventArgs e)
        {
            var sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;

            var trect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 10, e.Bounds.Height);
            using (var brush = new SolidBrush(e.Group.TreeList.GroupForeColor))
            {
                e.Graphics.DrawString(e.Group.Text, e.Group.TreeList.GroupFont, brush, trect, sf);
            }

            var tsize = e.Graphics.MeasureString(e.Group.Text, e.Group.TreeList.GroupFont);
            var x = trect.X + (int)tsize.Width + 10;
            var y = e.Bounds.Y + e.Bounds.Height / 2;
            var lrect = new Rectangle(x, y, e.Bounds.Width - x - 10, 1);
            using (var brush = new SolidBrush(Color.FromArgb(242, 242, 248)))
            {
                e.Graphics.FillRectangle(brush, lrect);
            }
        }

        /// <summary>
        /// 绘制 <see cref="TreeList"/> 中的行。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawItem(TreeListItemRenderEventArgs e)
        {
            DrawItemBackground(e);
        }

        /// <summary>
        /// 绘制没有数据时背景。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawNoneItem(TreeListRenderEventArgs e)
        {
            var sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

            var rect = e.Bounds;

            e.Graphics.DrawString(e.TreeList.NoneItemText, e.TreeList.Font, Brushes.LightGray, rect, sf);
        }

        /// <summary>
        /// 绘制页脚。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawFooterBackground(TreeListItemRenderEventArgs e)
        {
            using (var br = new LinearGradientBrush(e.Bounds, Color.FromArgb(250, 250, 250), Color.FromArgb(240, 240, 240), 90))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
            }

            e.Graphics.DrawLine(Pens.LightGray, e.Bounds.X, e.Bounds.Y, e.Bounds.Right, e.Bounds.Y);
        }

        /// <summary>
        /// 绘制 <see cref="TreeList"/> 的子项。
        /// </summary>
        /// <param name="e"></param>
        public virtual void DrawCell(TreeListCellRenderEventArgs e)
        {
            var drenderer = CreateDecorationRenderer(e.Cell);
            if (drenderer != null)
            {
                drenderer.DrawCell(e);
            }
        }

        protected virtual TreeListDecorationRenderer CreateDecorationRenderer(TreeListCell cell)
        {
            if (cell.Column.Index == 0)
            {
                return new TreeListIdentityDecorationRenderer();
            }
            else if (cell.Column.DataType == TreeListCellDataType.Boolean)
            {
                return new TreeListBooleanDecorationRenderer();
            }
            else
            {
                return new TreeListDefaultDecorationRenderer();
            }
        }

        /// <summary>
        /// 绘制排序箭头。
        /// </summary>
        /// <param name="graphics">GDI+ 对象。</param>
        /// <param name="rect">箭头绘制的范围。</param>
        /// <param name="sortOrder">排序方式。</param>
        protected virtual void DrawColumnSortArray(Graphics graphics, Rectangle rect, SortOrder sortOrder)
        {
            if (Application.RenderWithVisualStyles)
            {
                var sortElement = GetColumnSortVisualStyleElement(sortOrder);
                if (sortElement != null && VisualStyleRenderer.IsElementDefined(sortElement))
                {
                    new VisualStyleRenderer(sortElement).DrawBackground(graphics, rect);
                    return;
                }
            }

            using (var font = new Font("Marlett", 9))
            {
                var form = new StringFormat();
                form.Alignment = StringAlignment.Center;
                form.LineAlignment = StringAlignment.Center;

                graphics.DrawString(sortOrder == SortOrder.Ascending ? "5" : "6", font, SystemBrushes.ControlDark, rect, form);
            }
        }

        /// <summary>
        /// 绘制列头背景。
        /// </summary>
        /// <param name="graphics">GDI+ 对象。</param>
        /// <param name="rect">绘制的范围。</param>
        /// <param name="state">绘制状态。</param>
        protected virtual void DrawColumnHeaderBackground(Graphics graphics, Rectangle rect, DrawState state)
        {
            if (Application.RenderWithVisualStyles)
            {
                var bkElement = GetColumnHeaderVisualStyleElement(state);
                if (VisualStyleRenderer.IsElementDefined(bkElement))
                {
                    new VisualStyleRenderer(bkElement).DrawBackground(graphics, rect);
                    return;
                }
            }

            graphics.FillRectangle(SystemBrushes.ButtonFace, rect);

            //正常时凸面立体效果，按下时平面效果
            ControlPaint.DrawBorder3D(graphics, rect, state == DrawState.Pressed ? Border3DStyle.Flat : Border3DStyle.RaisedInner);
        }

        /// <summary>
        /// 绘制 <see cref="TreeListItem"/> 的背景。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void DrawItemBackground(TreeListItemRenderEventArgs e)
        {
            //绘制背景图像
            if (e.Item.TreeList.BackgroundImage != null)
            {
                e.Graphics.KeepClip(e.Bounds, () =>
                {
                    var be = new BackgroundRenderEventArgs(e.Item.TreeList, e.Graphics, e.Item.TreeList.GetBoundSet().ItemBound);
                    ThemeManager.BaseRenderer.DrawBackground(be);
                });
            }

            var backColor = GetItemBackgroundColor(e);

            if (!backColor.IsEmpty)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }
        }

        /// <summary>
        /// 绘制列头的文本。
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="column"></param>
        /// <param name="rect"></param>
        protected virtual void DrawColumnHeaderText(Graphics graphics, TreeListColumn column, Rectangle rect)
        {
            var foreColor = column.ForeColor.IsEmpty ? column.TreeList.ForeColor : column.ForeColor;
            var font = column.Font ?? column.TreeList.Font;

            var flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;

            if (column.TreeList.ColumnHorizontalCenter || column.TextAlign == HorizontalAlignment.Center)
            {
                flags |= TextFormatFlags.HorizontalCenter;
            }
            else if (column.TextAlign == HorizontalAlignment.Right)
            {
                flags |= TextFormatFlags.Right;
            }

            TextRenderer.DrawText(graphics, column.Text, font, rect, foreColor, flags);
        }

        public virtual void DrawCellGridLines(Graphics graphics, Rectangle bounds)
        {
            using (var pen = new Pen(Color.FromArgb(230, 230, 230)))
            {
                graphics.DrawLine(pen, bounds.X, bounds.Bottom, bounds.Right, bounds.Bottom);
                graphics.DrawLine(pen, bounds.Right - 1, bounds.Top, bounds.Right - 1, bounds.Bottom);
            }
        }

        protected virtual Color GetItemBackgroundColor(TreeListItemRenderEventArgs e)
        {
            if (e.DrawState == DrawState.Selected)
            {
                return e.Item.TreeList.Focused ? SystemColors.Highlight : SystemColors.ButtonFace;
            }
            else if (e.Item.Highlight)
            {
                return Color.Red;
            }
            else if (e.Item.BackgroundColor != Color.Empty)
            {
                return e.Item.BackgroundColor;
            }
            else if (e.Alternate && e.Item.TreeList.AlternateBackColor != Color.Empty)
            {
                return e.Item.TreeList.AlternateBackColor;
            }

            return e.Item.TreeList.BackColor;
        }

        /// <summary>
        /// 获取列头的绘制样式。
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private VisualStyleElement GetColumnHeaderVisualStyleElement(DrawState state)
        {
            switch (state)
            {
                case DrawState.Hot:
                    return VisualStyleElement.Header.Item.Hot;
                case DrawState.Pressed:
                    return VisualStyleElement.Header.Item.Pressed;
                default:
                    return VisualStyleElement.Header.Item.Normal;
            }
        }

        private VisualStyleElement GetColumnSortVisualStyleElement(SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case SortOrder.Ascending:
                    return VisualStyleElement.Header.SortArrow.SortedUp;
                case SortOrder.Descending:
                    return VisualStyleElement.Header.SortArrow.SortedDown;
                default:
                    return null;
            }
        }

        private Rectangle GetColumnHeaderSortArrayBound(TreeListColumn column, Rectangle rect)
        {
            //排序箭头的绘制区
            var bound = column != null && column.Sortable && column.TreeList.GetSortOrder(column) != SortOrder.None ?
                new Rectangle(rect.Right - 16, rect.Top, 16, rect.Height) : Rectangle.Empty;
            return bound.Left > rect.Left ? bound : Rectangle.Empty;
        }

        private Rectangle GetColumnHeaderImageRect(Image image, HorizontalAlignment align, Rectangle rect, int sortArrayWidth)
        {
            var bound = Rectangle.Empty;
            var h = (rect.Height - image.Height) / 2;
            if (align == HorizontalAlignment.Left)
            {
                bound = new Rectangle(rect.X + 2, rect.Y + h, image.Width, image.Height);
            }
            else if (align == HorizontalAlignment.Right)
            {
                bound = new Rectangle(rect.Right - image.Width - sortArrayWidth - 2, rect.Y + h, image.Width, image.Height);
            }

            return bound.Left > rect.Left ? bound : Rectangle.Empty;
        }

        private Rectangle GetColumnHeaderTextRect(Rectangle rect, int sortArrayWidth, int imageWidth, HorizontalAlignment align)
        {
            var textBound = rect;
            textBound.Width -= sortArrayWidth;

            if (imageWidth != 0)
            {
                if (align == HorizontalAlignment.Left)
                {
                    textBound.X += imageWidth;
                    textBound.Width -= imageWidth;
                }
                else if (align == HorizontalAlignment.Right)
                {
                    textBound.Width -= imageWidth;
                }
            }

            textBound.Inflate(-2, 0);

            return textBound;
        }
    }

    public class TreeListRenderEventArgs : RenderEventArgs
    {
        internal TreeListRenderEventArgs(TreeList treelist, Graphics graphics, Rectangle rect)
            : base(graphics, rect)
        {
            TreeList = treelist;
        }

        public TreeList TreeList { get; private set; }
    }

    public class TreeListColumnRenderEventArgs : RenderEventArgs
    {
        internal TreeListColumnRenderEventArgs(TreeListColumn column, Graphics graphics, Rectangle rect)
            : base(graphics, rect)
        {
            Column = column;
        }

        public TreeListColumn Column { get; private set; }

        public DrawState DrawState { get; set; }
    }

    public class TreeListGroupRenderEventArgs : RenderEventArgs
    {
        internal TreeListGroupRenderEventArgs(TreeListGroup group, Graphics graphics, Rectangle rect)
            : base(graphics, rect)
        {
            Group = group;
        }

        public TreeListGroup Group { get; private set; }

        public DrawState DrawState { get; set; }
    }

    public class TreeListItemRenderEventArgs : RenderEventArgs
    {
        internal TreeListItemRenderEventArgs(TreeListItem item, Graphics graphics, Rectangle rect)
            : base(graphics, rect)
        {
            Item = item;
        }

        public TreeListItem Item { get; private set; }

        public DrawState DrawState { get; set; }

        public bool Alternate { get; set; }
    }

    public class TreeListRowNumberRenderEventArgs : RenderEventArgs
    {
        internal TreeListRowNumberRenderEventArgs(TreeList treeList, int index, Graphics graphics, Rectangle rect)
            : base(graphics, rect)
        {
            TreeList = treeList;
            Index = index;
        }

        public TreeList TreeList { get; private set; }

        public int Index { get; private set; }

        public DrawState DrawState { get; set; }
    }

    public class TreeListCellRenderEventArgs : RenderEventArgs
    {
        internal TreeListCellRenderEventArgs(TreeListCell item, Graphics graphics, Rectangle rect)
            : base(graphics, rect)
        {
            Cell = item;
        }

        public TreeListCell Cell { get; private set; }

        public DrawState DrawState { get; set; }
    }
}
