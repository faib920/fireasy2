using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Fireasy.Windows.Forms
{
    public class TreeListIdentityDecorationRenderer : TreeListDefaultDecorationRenderer
    {
        public override void DrawCell(TreeListCellRenderEventArgs e)
        {
            var item = e.Cell.Item;
            var treeList = item.TreeList;
            var rect = e.Bounds;
            var plusRect = Rectangle.Empty;
            var imageRect = Rectangle.Empty;
            var checkRect = Rectangle.Empty;
            Image image = null;

            rect = rect.ReduceRight(item.Level * treeList.Indent);

            if (treeList.ShowPlusMinus)
            {
                plusRect = new Rectangle(rect.X, rect.Top, 16, rect.Height);
                rect = rect.ReduceRight(16);
            }

            //图标绘制区
            if (item.Items.Count > 0 || item.ShowExpanded)
            {
                if (item.Expended)
                {
                    image = treeList.UseDefaultNodeImage ? Resource.tree_folder_open : treeList.DefaultExpandNodeImage;
                }
                else if (!item.Expended || item.ShowExpanded)
                {
                    image = treeList.UseDefaultNodeImage ? Resource.tree_folder : treeList.DefaultCollapseNodeImage;
                }
            }

            image = image ?? e.Cell.Item.GetImage() ?? (treeList.UseDefaultNodeImage ? Resource.tree_file : treeList.DefaultImage);

            //画连接线段
            if (!plusRect.IsEmpty && treeList.ShowPlusMinusLines)
            {
                DrawItemPlusMinusLines(e.Graphics, plusRect, e.Cell.Item);
            }

            //画展开/收缩小图标
            if (!plusRect.IsEmpty && (item.Items.Count > 0 || item.ShowExpanded))
            {
                DrawItemPlusMinus(e.Graphics, plusRect, item.Expended);
            }

            if (treeList.ShowCheckBoxes && item.ShowBox)
            {
                var h = (rect.Height - 15) / 2;
                checkRect = new Rectangle(rect.X, rect.Top + h, 15, 15);
                rect = rect.ReduceRight(17);
                ThemeManager.BaseRenderer.DrawCheckbox(e.Graphics, checkRect, item.Checked, item.Mixed, item.Enabled);
            }

            imageRect = image == null ? Rectangle.Empty : GetItemImageRect(image, rect);

            if (!imageRect.IsEmpty && image != null)
            {
                e.Graphics.DrawImage(image, imageRect.Middle(16, 16));
                rect = rect.ReduceRight(image.Width);
            }

            DrawCellText(e.Graphics, e.Cell, rect, e.DrawState);

            if (!e.Cell.IsValid)
            {
                DrawInvalidate(e.Graphics, e.Cell, e.Bounds);
            }
        }

        protected virtual void DrawItemPlusMinusLines(Graphics graphics, Rectangle rect, TreeListItem item)
        {
            if (item.Owner == null)
            {
                return;
            }

            using (var pen = new Pen(Color.Gray))
            {
                pen.DashStyle = DashStyle.Dot;

                var x = rect.X + 7;
                int half = rect.Height / 2;
                int y0 = item.Parent == null && item.Index == 0 ? rect.Top + half : rect.Top;
                int y1 = item.Index == item.Owner.Count - 1 ? rect.Top + half : rect.Bottom;

                graphics.DrawLine(pen, x, y0, x, y1);
                graphics.DrawLine(pen, x, rect.Top + half, x + 8, rect.Top + half);

                var parent = item;
                for (int i = 1; i <= item.Level; i++)
                {
                    parent = parent.Parent;
                    if (parent.Index != parent.Owner.Count - 1)
                    {
                        int ox = x - i * item.TreeList.Indent;
                        graphics.DrawLine(pen, ox, rect.Top, ox, rect.Bottom);
                    }
                }
            }
        }

        protected virtual void DrawItemPlusMinus(Graphics graphics, Rectangle rect, bool expended)
        {
            if (Application.RenderWithVisualStyles)
            {
                var element = expended ? VisualStyleElement.TreeView.Glyph.Opened : VisualStyleElement.TreeView.Glyph.Closed;
                if (VisualStyleRenderer.IsElementDefined(element))
                {
                    new VisualStyleRenderer(element).DrawBackground(graphics, rect);
                    return;
                }
            }

            var prect = rect.Middle(8, 8);
            graphics.FillRectangle(Brushes.White, prect);
            graphics.DrawRectangle(Pens.Gray, prect);
            graphics.DrawLine(Pens.Gray, prect.X + 2, prect.Y + 4, prect.Right - 2, prect.Y + 4);

            if (!expended)
            {
                graphics.DrawLine(Pens.Gray, prect.X + 4, prect.Y + 2, prect.X + 4, prect.Bottom - 2);
            }
        }
    }
}
