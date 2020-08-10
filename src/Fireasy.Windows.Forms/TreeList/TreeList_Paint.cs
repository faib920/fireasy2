// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public partial class TreeList
    {
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            ThemeManager.BaseRenderer.DrawBackground(new BackgroundRenderEventArgs(this, e.Graphics, _bound.ItemBound));
            ThemeManager.BaseRenderer.DrawBorder(new BorderRenderEventArgs(this, e.Graphics, ClientRectangle));

            DrawColumns(e.Graphics);
            DrawRowNumberColumn(e.Graphics);
            DrawItems(e.Graphics);
            DrawFooter(e.Graphics);
        }

        private void DrawFooter(Graphics graphics)
        {
            if (!ShowFooter)
            {
                return;
            }

            var rect = _bound.FooterBound;
            rect = rect.ReduceLeft(GetOffsetLeft());
            var e = new TreeListItemRenderEventArgs(Footer, graphics, rect)
            {
                DrawState = DrawState.Normal
            };

            Renderer.DrawFooterBackground(e);

            if (Footer == null)
            {
                return;
            }

            e.Graphics.KeepClip(_bound.FooterBound, () =>
            {
                DrawCells(e.Graphics, e.Item.Cells, e.Bounds, e.DrawState);
            });
        }

        private void DrawItem(TreeListItemRenderEventArgs e, bool setClip = true)
        {
            e.Graphics.KeepClip(_bound.ItemBound, () =>
            {
                Renderer.DrawItem(e);
                DrawCells(e.Graphics, e.Item.Cells, e.Bounds, e.DrawState);
            }, setClip);

            if (ShowRowNumber)
            {
                var e3 = new TreeListRowNumberRenderEventArgs(this, RowNumberIndex + e.Item.DataIndex, e.Graphics, new Rectangle(_bound.WorkBound.X, e.Bounds.Y, RowNumberWidth, e.Bounds.Height));

                e.Graphics.KeepClip(_bound.RowNumberBound, () =>
                {
                    e3.DrawState = e.DrawState;
                    DrawRowNumber(e3);
                });
            }
        }

        private void DrawRowNumber(TreeListRowNumberRenderEventArgs e, bool setClip = true)
        {
            e.Graphics.KeepClip(_bound.WorkBound, () =>
            {
                Renderer.DrawRowNumber(e);

                if (e.TreeList.ShowGridLines)
                {
                    Renderer.DrawCellGridLines(e.Graphics, e.Bounds);
                }
            }, setClip);
        }

        private void DrawGroup(TreeListGroupRenderEventArgs e, bool setClip = true)
        {
            e.Graphics.KeepClip(_bound.ItemBound, () =>
            {
                Renderer.DrawGroup(e);
            }, setClip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="cells">要绘制的子项。</param>
        /// <param name="bound"><see cref="TreeListItem"/> 的绘制范围。</param>
        /// <param name="drawState">绘制状态。</param>
        private void DrawCells(Graphics g, TreeListCellCollection cells, Rectangle bound, DrawState drawState)
        {
            var x = bound.X;
            var isDrawing = false;

            foreach (var cell in cells)
            {
                if (cell.Column.Hidden)
                {
                    continue;
                }

                var rect = new Rectangle(x, bound.Top, cell.Column.Width, bound.Height);
                if (!bound.IntersectsWith(rect) && isDrawing)
                {
                    break;
                }

                rect.Inflate(-2, 0);
                var e = new TreeListCellRenderEventArgs(cell, g, rect)
                {
                    DrawState = drawState
                };

                g.KeepClip(rect, () =>
                {
                    Renderer.DrawCell(e);
                });

                if (e.Cell.Item.TreeList.ShowGridLines)
                {
                    rect.Inflate(2, 0);
                    Renderer.DrawCellGridLines(e.Graphics, rect);
                }

                x += cell.Column.Width;
                isDrawing = true;
            }
        }

        private void ProcessSpringColumnWidth()
        {
            var width = 0;
            TreeListColumn springColumn = null;
            var assert = new AssertFlag();

            foreach (var column in Columns)
            {
                if (!column.Spring || !assert.AssertTrue())
                {
                    width += column.Width;
                }
                else
                {
                    springColumn = column;
                }
            }

            if (springColumn != null)
            {
                springColumn.Width = Math.Max(0, Width - width - _bound.WorkBound.X * 2 - (ShowVerScrollBar ? _vbar.Width : 0));
            }
        }

        /// <summary>
        /// 在调整列头宽度时，绘制一条直接，这条直线是可逆线。
        /// </summary>
        /// <param name="pos">X 轴坐标。</param>
        private void DrawDragLine(int pos)
        {
            if (pos == -1)
            {
                return;
            }

            var start = PointToScreen(new Point(pos, 1));
            var end = PointToScreen(new Point(pos, Height - 1));
            ControlPaint.DrawReversibleLine(start, end, Color.Black);
        }

        private void RedrawSelectedItems()
        {
            using (var graphics = CreateGraphics())
            {
                foreach (var vitem in _virMgr.Items)
                {
                    if (vitem.Item.Selected)
                    {
                        var e = GetListItemRenderEventArgs(graphics, vitem, DrawState.Selected);
                        if (e != null)
                        {
                            DrawItem(e);
                        }
                    }
                }
            }
        }
    }
}
