using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    internal class ToolTipWrapper
    {
        private ToolTip inner;
        private TreeList treelist;
        private Size size = Size.Empty;

        /// <summary>
        /// 获取或设置 ToolTip 是否显示。
        /// </summary>
        internal bool IsShow { get; set; }

        internal ToolTipWrapper(TreeList treelist)
        {
            this.treelist = treelist;
        }

        internal void Show(TreeListCell cell, Rectangle rect)
        {
            Initialize(cell);

            inner.Show(cell.Text, treelist, rect.X - 2, rect.Y + (rect.Height - size.Height - 2) / 2 + 1);
            IsShow = true;
        }

        internal void Hide()
        {
            inner.Hide(treelist);
            IsShow = false;
        }

        private void Initialize(TreeListCell cell)
        {
            if (inner != null)
            {
                inner.Dispose();
            }

            var font = cell.Column.Font ?? cell.Column.TreeList.Font;
            size = TextRenderer.MeasureText(cell.Text, font);
            size.Height += 8;
            size.Width += 6;

            inner = new ToolTip();
            inner.ToolTipTitle = "dfdsafafd";
            inner.OwnerDraw = true;
            inner.Popup += (o, e) =>
                {
                    e.ToolTipSize = size;
                };

            inner.Draw += (o, e) =>
                {
                    var flags = TextFormatFlags.VerticalCenter;
                    var r = e.Bounds;
                    r.Offset(3, 0);
                    using (var br = new LinearGradientBrush(e.Bounds, Color.White, Color.LightGray, 90))
                    {
                        e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
                    }

                    e.DrawBorder();
                    TextRenderer.DrawText(e.Graphics, cell.Text, font, r, Color.Black, flags);
                };
        }
    }
}
