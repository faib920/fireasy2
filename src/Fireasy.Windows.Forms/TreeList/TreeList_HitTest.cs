using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public partial class TreeList
    {
        /// <summary>
        /// 测试鼠标经过的位置的对象。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        protected TreeListHitTestInfo HitTest(int x, int y, TreeListHitTestEventType eventType)
        {
            if (bound.ColumnBound.Contains(x, y))
            {
                return HitTestColumn(x, y);
            }

            if (bound.AvlieBound.Contains(x, y))
            {
                return HitTestItem(x, y);
            }

            return null;
        }

        /// <summary>
        /// 检测鼠标经过的地方的 <see cref="TreeListColumn"/>，如果是两个 <see cref="TreeListColumn"/> 的中间经，则为宽度调整线。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private TreeListHitTestInfo HitTestColumn(int x, int y)
        {
            const int SIZE_WIDTH = 4;
            var workRect = bound.ColumnBound;
            var x1 = workRect.X - GetOffsetLeft();
            foreach (var column in Columns)
            {
                if (column.Hidden)
                {
                    continue;
                }

                var rect = new Rectangle(x1, workRect.Top, column.Width, HeaderHeight);

                //如果列的宽度可调，则矩形框的宽度需要缩小4个像素，以便检测调整线
                if (!column.Fixed)
                {
                    rect.Inflate(-SIZE_WIDTH, 0);
                }

                if (rect.Contains(x, y))
                {
                    //恢复矩形框的宽度
                    if (!column.Fixed)
                    {
                        rect.Inflate(SIZE_WIDTH, 0);
                    }

                    return new TreeListHitTestInfo(TreeListHitTestType.Column, column, rect);
                }

                //检测是不是两列中间的宽度调整线
                var sizeRect = new Rectangle(rect.Right - SIZE_WIDTH, workRect.Top, SIZE_WIDTH * 2, HeaderHeight);
                if (sizeRect.Contains(x, y) && !column.Fixed)
                {
                    return new TreeListHitTestInfo(TreeListHitTestType.ColumnSize, column, rect);
                }

                x1 += column.Width;
            }

            return new TreeListHitTestInfo(TreeListHitTestType.Column);
        }

        private TreeListHitTestInfo HitTestGroup(int x, int y)
        {
            return null;
        }

        /// <summary>
        /// 提供座标 x 和 y 处的 <see cref="TreeListItem"/> 信息。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private TreeListHitTestInfo HitTestItem(int x, int y)
        {
            var itemHeight = GetAdjustItemHeight();
            var totalWidth = GetColumnTotalWidth();
            if (ShowRowNumber)
            {
                totalWidth += RowNumberWidth;
            }

            var columnHeight = ShowHeader ? HeaderHeight : 0;

            //取得当前行的索引号
            var index = (y - columnHeight + GetOffsetTop()) / itemHeight;

            //判断索引是否有效，以及是否超出右边
            if (index < 0 || index > virMgr.Items.Count - 1 || x > totalWidth - GetOffsetLeft())
            {
                return new TreeListHitTestInfo(TreeListHitTestType.Item);
            }

            var item = virMgr.Items[index];
            if (item.ItemType == ItemType.Group)
            {
                return HitTestGroup(x, y);
            }

            var x1 = bound.ItemBound.X - GetOffsetLeft();

            //修正y座标
            var y1 = bound.ItemBound.Y + index * itemHeight - GetOffsetTop();
            var tw = GetColumnTotalWidth();

            var rect = new Rectangle(x1, y1, tw, ItemHeight);

            if (rect.Contains(x, y))
            {
                //测试是否是 +/-
                var info = HitTestItemPlusMinus(item, rect, x, y);
                if (info != null)
                {
                    return info;
                }

                //测试是否为复选框
                info = HitTestItemCheckbox(item, rect, x, y);
                if (info != null)
                {
                    return info;
                }

                //测试是否是Cell
                info = HitTestCell(item, rect, x, y);
                if (info != null)
                {
                    return info;
                }

                return new TreeListHitTestInfo(TreeListHitTestType.Item, item, rect);
            }

            if (ShowRowNumber && new Rectangle(bound.RowNumberBound.X, y1, RowNumberWidth, ItemHeight).Contains(x, y))
            {
                return new TreeListHitTestInfo(TreeListHitTestType.Item, item, rect);
            }

            return null;
        }

        /// <summary>
        /// 提供座标 x 和 y 处的 +/- 信息。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rect"><see cref="TreeListItem"/> 的区域。</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private TreeListHitTestInfo HitTestItemPlusMinus(VirtualTreeListItem item, Rectangle rect, int x, int y)
        {
            var sitem = (TreeListItem)item.Item;

            //是否可以显示 +/-
            if (ShowPlusMinus && (sitem.ShowExpanded || sitem.Items.Count > 0))
            {
                var x1 = rect.Left + sitem.Level * Indent;

                //+/-符号的区域
                var mrect = new Rectangle(x1, rect.Top, 16, ItemHeight).Middle(12, 12);

                //获取第一列的区域
                var firstColumn = GetFirstColumn();
                var crect = GetColumnBound(firstColumn);

                //获取当前Cell的区域
                var srect = new Rectangle(crect.X, rect.Top, crect.Width, ItemHeight);

                if (mrect.Contains(x, y) && srect.IntersectsWith(mrect))
                {
                    return new TreeListHitTestInfo(TreeListHitTestType.PlusMinus, item, mrect);
                }
            }

            return null;
        }

        /// <summary>
        /// 提供座标 x 和 y 处的复选框信息。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="rect"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private TreeListHitTestInfo HitTestItemCheckbox(VirtualTreeListItem item, Rectangle rect, int x, int y)
        {
            if (ShowCheckBoxes)
            {
                var sitem = (TreeListItem)item.Item;

                //如果项不可用，则单选按钮不可用
                if (!sitem.Enabled || !sitem.ShowBox)
                {
                    return null;
                }

                var x1 = rect.Left + sitem.Level * Indent;

                //复选框区域
                var mrect = new Rectangle(x1, rect.Top, 15, ItemHeight).Middle(15, 15);
                if (ShowPlusMinus)
                {
                    mrect.Offset(16, 0);
                }

                //获取第一列的区域
                var firstColumn = GetFirstColumn();
                var crect = GetColumnBound(firstColumn);

                //获取当前Cell的区域
                var srect = new Rectangle(crect.X, rect.Top, crect.Width, ItemHeight);

                if (mrect.Contains(x, y) && srect.IntersectsWith(mrect))
                {
                    return new TreeListHitTestInfo(TreeListHitTestType.Checkbox, item, mrect);
                }
            }

            return null;
        }

        private TreeListHitTestInfo HitTestCell(VirtualTreeListItem vitem, Rectangle rect, int x, int y)
        {
            var workRect = bound.ItemBound;
            var x1 = workRect.X - GetOffsetLeft();
            var item = (TreeListItem)vitem.Item;
            foreach (var column in Columns)
            {
                if (column.Index > item.Cells.Count)
                {
                    return null;
                }

                if (column.Hidden)
                {
                    continue;
                }

                var rect1 = new Rectangle(x1, rect.Y, column.Width, ItemHeight);

                if (rect1.Contains(x, y))
                {
                    var owner = new TreeListHitTestInfo(TreeListHitTestType.Item, vitem, rect);
                    if (column.Index <= item.Cells.Count - 1)
                    {
                        return new TreeListHitTestInfo(TreeListHitTestType.Cell, item.Cells[column.Index], rect1) { Owner = owner };
                    }
                    else
                    {
                        return owner;
                    }
                }

                x1 += column.Width;
            }

            return null;
        }

        /// <summary>
        /// 处理当前鼠标经过的对象。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="drawState"></param>
        private void ProcessHitTestInfo(TreeListHitTestInfo info, DrawState drawState)
        {
            if (info.Bounds.IsEmpty)
            {
                return;
            }

            switch (info.HitTestType)
            {
                case TreeListHitTestType.Column:
                    var column = (TreeListColumn)info.Element;
                    using (var graphics = CreateGraphics())
                    {
                        var drawArgs = new TreeListColumnRenderEventArgs(column, graphics, info.Bounds);
                        drawArgs.DrawState = drawState;
                        graphics.KeepClip(bound.ColumnBound, () => Renderer.DrawColumnHeader(drawArgs));
                    }

                    break;
                case TreeListHitTestType.ColumnSize:
                    Cursor = Cursors.VSplit;
                    break;
                case TreeListHitTestType.Cell:
                    if (info.Owner != null)
                    {
                        ProcessHitTestInfo(info.Owner, drawState);
                    }

                    var cell = (TreeListCell)info.Element;
                    var rect = GetCellTextRectangle(cell, info.Bounds);
                    ShowToolTip(cell, rect);
                    break;
                case TreeListHitTestType.Item:
                    if (HandCursor)
                    {
                        Cursor = Cursors.Hand;
                    }

                    if (drawState == DrawState.Pressed || !HotTracking)
                    {
                        return;
                    }

                    var vitem = (VirtualTreeListItem)info.Element;
                    var item = (TreeListItem)vitem.Item;
                    if (item.Selected)
                    {
                        return;
                    }

                    using (var graphics = CreateGraphics())
                    {
                        var drawArgs = new TreeListItemRenderEventArgs(item, graphics, info.Bounds);
                        drawArgs.DrawState = drawState;
                        drawArgs.Alternate = vitem.Index % 2 != 0;
                        DrawItem(drawArgs);
                    }

                    break;
            }
        }

        /// <summary>
        /// 处理鼠标单击时 <see cref="TreeListHitTestInfo"/> 的处理。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="eventType"></param>
        private void ProcessHitTestInfoClick(TreeListHitTestInfo info, TreeListHitTestEventType eventType)
        {
            if (info.Bounds.IsEmpty)
            {
                return;
            }

            if (eventType == TreeListHitTestEventType.MouseUp &&
                info.HitTestType == TreeListHitTestType.Column)
            {
                ProcessColumnClick(info);
            }
            else if (eventType == TreeListHitTestEventType.MouseDown)
            {
                switch (info.HitTestType)
                {
                    case TreeListHitTestType.Cell:
                        ProcessCellClick(info);
                        break;
                    case TreeListHitTestType.Item:
                        ProcessItemClick(info);
                        break;
                    case TreeListHitTestType.PlusMinus:
                        ProcessPlusMinusClick(info);
                        break;
                    case TreeListHitTestType.Checkbox:
                        ProcessItemCheckedChange(info);
                        break;
                }
            }
        }

        /// <summary>
        /// 处理列头单击。
        /// </summary>
        /// <param name="info"></param>
        private void ProcessColumnClick(TreeListHitTestInfo info)
        {
            if (lastHoverHitInfo != null &&
                lastHoverHitInfo.HitTestType == TreeListHitTestType.Column &&
                lastHoverHitInfo.Element != null)
            {
                var column = (TreeListColumn)info.Element;

                HideEditor();

                if (Sortable)
                {
                    if (sortedColumn != column)
                    {
                        sortedOrder = SortOrder.Ascending;
                    }
                    else
                    {
                        if (sortedOrder == SortOrder.Ascending)
                        {
                            sortedOrder = SortOrder.Descending;
                        }
                        else
                        {
                            sortedOrder = SortOrder.Ascending;
                        }
                    }

                    sortedColumn = column;

                    if (RaiseColumnClickEvent(column, sortedOrder))
                    {
                        if (Groups.Count == 0)
                        {
                            Items.Sort(++sortVersion, column, sortedOrder);
                        }
                        else
                        {
                            Groups.Sort(++sortVersion, column, sortedOrder);
                        }
                        UpdateItems();
                    }
                }
            }
        }

        /// <summary>
        /// 处理行单击。
        /// </summary>
        /// <param name="info"></param>
        private void ProcessItemClick(TreeListHitTestInfo info)
        {
            var item = (TreeListItem)((VirtualTreeListItem)info.Element).Item;

            //按着ctrol切换选中状态
            if (controlPressed)
            {
                SelectItem(item, !item.Selected, false);
            }
            else if (shiftPressed && lastRowIndex != -1)
            {
                if (lastRowIndex > virMgr.Items.Count - 1)
                {
                    return;
                }

                for (var i = SelectedItems.Count - 1; i >= 0; i--)
                {
                    SelectedItems[i].SetSelected(false);
                }

                SelectedItems.InternalClear();
                Invalidate(bound.ItemBound);

                var start = lastRowIndex;
                var end = item.Index;
                if (start > end)
                {
                    start = end;
                    end = lastRowIndex;
                }

                for (var i = start; i <= end; i++)
                {
                    var t = virMgr.Items[i].Item as TreeListItem;
                    if (t == null)
                    {
                        continue;
                    }
                    t.SetSelected(true);
                    SelectedItems.InternalAdd(t);
                }
                Invalidate();
                RaiseItemSelectionChangedEvent();
            }
            //选中且不是多选时，只触发单击事件
            else if (item.Selected && SelectedItems.Count == 1)
            {
                RaiseItemClickEvent(item);
                return;
            }
            //选中当前行
            else
            {
                SelectItem(item, true);
                HideEditor();
                lastRowIndex = item.Index;
            }

            RaiseItemClickEvent(item);
        }

        /// <summary>
        /// 调整数据行的座标。当选中的行显示在顶处或底处，但是只显示一部份时，调整座标以显示整行的内容。
        /// </summary>
        /// <param name="info"></param>
        private void AdjustItemPosistion(TreeListHitTestInfo info)
        {
            var y = info.Bounds.Y - bound.ItemBound.Y;
            if (y < 0 || (y = info.Bounds.Bottom - bound.ItemBound.Bottom) > 0)
            {
                vbar.Value += y;
                var r = info.Bounds;
                r.Offset(0, -y);
                info.Bounds = r;

                if (info.Owner != null)
                {
                    r = info.Owner.Bounds;
                    r.Offset(0, -y);
                    info.Owner.Bounds = r;
                }
            }
        }

        /// <summary>
        /// 处理单元格单击。
        /// </summary>
        /// <param name="info"></param>
        private void ProcessCellClick(TreeListHitTestInfo info)
        {
            AdjustItemPosistion(info);

            if (info.Owner != null)
            {
                ProcessItemClick(info.Owner);
            }

            var cell = (TreeListCell)info.Element;
            RaiseCellClickEvent(cell);

            switch (cell.BoxType)
            {
                case BoxType.CheckBox:
                    if (!RaiseBeforeCellCheckChangeEvent(cell))
                    {
                        cell.Checked = !cell.Checked;
                        RaiseAfterCellCheckChangeEvent(cell);
                    }
                    break;
                case BoxType.RadioButton:
                    var items = cell.Item.Parent == null ? cell.Item.TreeList.Items : cell.Item.Parent.Items;
                    foreach (var item in items)
                    {
                        if (item == cell.Item)
                        {
                            continue;
                        }

                        var c1 = item.Cells[cell.Column];
                        if (!RaiseBeforeCellCheckChangeEvent(c1))
                        {
                            c1.Checked = false;
                            RaiseAfterCellCheckChangeEvent(c1);
                        }
                    }

                    if (!RaiseBeforeCellCheckChangeEvent(cell))
                    {
                        cell.Checked = true;
                        RaiseAfterCellCheckChangeEvent(cell);
                    }
                    break;
            }

            cell.Item.InvalidateItem();

            if (cell.Item.Enabled && !RaiseBeforeCellEditingEvent(cell))
            {
                var rect = GetCellTextRectangle(cell, info.Bounds);
                editor.BeginEdit(cell, rect);
            }
        }

        /// <summary>
        /// 处理 +/- 符号单击。
        /// </summary>
        /// <param name="info"></param>
        private void ProcessPlusMinusClick(TreeListHitTestInfo info)
        {
            var item = (TreeListItem)((VirtualTreeListItem)info.Element).Item;
            var expended = item.Expended;

            var cancel = expended ? RaiseBeforeItemCollapseEvent(item) : RaiseBeforeItemExpandEvent(item);
            if (cancel)
            {
                return;
            }

            HideEditor();

            item.Expended = !item.Expended;
        }

        private void ProcessItemCheckedChange(TreeListHitTestInfo info)
        {
            var vitem = (VirtualTreeListItem)info.Element;
            var item = (TreeListItem)vitem.Item;

            if (RaiseBeforeItemCheckChangeEvent(item))
            {
                return;
            }

            item.Checked = !item.Checked;
            if (item.Mixed)
            {
                item.Mixed = false;
            }

            RaiseAfterItemCheckChangeEvent(item);

            InvalidateItem(vitem);
        }

        internal void ProcessItemExpand(TreeListItem item)
        {
            if (item.Expended)
            {
                RaiseAfterItemExpandEvent(item);

                if (sortedColumn != null)
                {
                    item.Items.Sort(sortVersion, sortedColumn, sortedOrder);
                }
            }
            else
            {
                RaiseAfterItemCollapseEvent(item);
            }

            UpdateItems();
        }

        internal void ProcessItemCheck(TreeListItem item)
        {
            RaiseAfterItemCheckChangeEvent(item);

            InvalidateItem(item);
        }

        /// <summary>
        /// 处理鼠标双击时 <see cref="TreeListHitTestInfo"/> 的处理。
        /// </summary>
        /// <param name="info"></param>
        private void ProcessHitTestInfoDbClick(TreeListHitTestInfo info)
        {
            HideEditor();
            switch (info.HitTestType)
            {
                case TreeListHitTestType.Cell:
                    if (info.Owner != null)
                    {
                        ProcessHitTestInfoDbClick(info.Owner);
                    }

                    break;
                case TreeListHitTestType.Item:
                    if (info.Element != null)
                    {
                        var item = (TreeListItem)((VirtualTreeListItem)info.Element).Item;
                        RaiseItemDoubleClickEvent(item);
                    }

                    break;
            }
        }
    }
}
