// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    internal class ToolTipWrapper
    {
        private ToolTip _inner;
        private readonly TreeList _treelist;
        private Size _size = Size.Empty;

        /// <summary>
        /// 获取或设置 ToolTip 是否显示。
        /// </summary>
        internal bool IsShow { get; set; }

        internal ToolTipWrapper(TreeList treelist)
        {
            _treelist = treelist;
        }

        internal void Show(TreeListCell cell, Rectangle rect)
        {
            Initialize(cell);

            _inner.Show(cell.Text, _treelist, rect.X - 2, rect.Y + (rect.Height - _size.Height - 2) / 2 + 1);
            IsShow = true;
        }

        internal void Hide()
        {
            _inner.Hide(_treelist);
            IsShow = false;
        }

        private void Initialize(TreeListCell cell)
        {
            if (_inner != null)
            {
                _inner.Dispose();
            }

            var font = cell.Column.Font ?? cell.Column.TreeList.Font;
            _size = TextRenderer.MeasureText(cell.Text, font);
            _size.Height += 8;
            _size.Width += 6;

            _inner = new ToolTip();
            _inner.ToolTipTitle = "dfdsafafd";
            _inner.OwnerDraw = true;
            _inner.Popup += (o, e) =>
                {
                    e.ToolTipSize = _size;
                };

            _inner.Draw += (o, e) =>
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
