// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public partial class TreeList
    {
        [Description("当列头单击时触发此事件。")]
        public event TreeListColumnClickEventHandler ColumnClick;
        [Description("当行数据绑定时触发此事件。")]
        public event TreeListItemDataBoundEventHandler ItemDataBound;
        [Description("当单元格数据绑定时触发此事件。")]
        public event TreeListCellDataBoundEventHandler CellDataBound;
        [Description("当行单击时触发此事件。")]
        public event TreeListItemClickEventHandler ItemClick;
        [Description("当单击格单击时触发此事件。")]
        public event TreeListCellClickEventHandler CellClick;
        [Description("当行双击时触发此事件。")]
        public event TreeListItemDoubleClickEventHandler ItemDoubleClick;
        [Description("当行选择改变时触发此事件。")]
        public event TreeListItemSelectionChangedEventHandler ItemSelectionChanged;
        [Description("当行节点收缩之前触发此事件。可通过 Cancel 属性取消后续操作。")]
        public event TreeListItemBeforeCollapseEventHandler BeforeItemCollapse;
        [Description("当行节点收缩之后触发此事件。")]
        public event TreeListItemAfterCollapseEventHandler AfterItemCollapse;
        [Description("当行节点展开之前触发此事件。可通过 Cancel 属性取消后续操作。")]
        public event TreeListItemBeforeExpandEventHandler BeforeItemExpand;
        [Description("当行节点展开之后触发此事件。")]
        public event TreeListItemAfterExpandEventHandler AfterItemExpand;
        [Description("当行需要加载子节点时触发事件。")]
        public event TreeListDemandLoadEventHandler DemandLoad;
        public event TreeListBeforeCellEditingEventHandler BeforeCellEditing;
        public event TreeListAfterCellEditedEventHandler AfterCellEdited;
        public event TreeListBeforeCellUpdatingEventHandler BeforeCellUpdating;
        public event TreeListAfterCellUpdatedEventHandler AfterCellUpdated;
        [Description("当行的 Checked 值改变之前触发此事件。可通过 Cancel 属性取消后续操作。")]
        public event TreeListItemBeforeCheckedEventHandler BeforeItemCheckChange;
        [Description("当行的 Checked 值改变之后触发此事件。")]
        public event TreeListItemAfterCheckedEventHandler AfterItemCheckChange;
        [Description("当单元格的 Checked 值改变之前触发此事件。可通过 Cancel 属性取消后续操作。")]
        public event TreeListCellBeforeCheckedEventHandler BeforeCellCheckChange;
        [Description("当单元格的 Checked 值改变之后触发此事件。")]
        public event TreeListCellAfterCheckedEventHandler AfterCellCheckChange;
        [Description("当行的 Checked 值改变后触发此事件。")]
        public event TreeListItemCheckChangeEventHandler ItemCheckChanged;
        [Description("当滚动条拖到底时触发此事件按需加载数据。")]
        public event EventHandler DemandLoadWhenScrollEnd;

        #region 触发事件
        protected internal bool RaiseColumnClickEvent(TreeListColumn column, SortOrder order)
        {
            if (ColumnClick == null)
            {
                return true;
            }

            var e = new TreeListColumnClickEventArgs { Column = column, Sorting = order, Sortable = false };
            OnColumnClick(e);
            return e.Sortable;
        }

        protected internal void RaiseItemDataBoundEvent(TreeListItem item, object data, int index)
        {
            var e = new TreeListItemDataBoundEventArgs { Item = item, ItemData = data, Index = index };
            OnItemDataBound(e);
        }

        protected internal void RaiseCellDataBoundEvent(TreeListCell cell, object data, object value)
        {
            var e = new TreeListCellDataBoundEventArgs { Cell = cell, ItemData = data, Value = value };
            OnCellDataBound(e);
        }

        protected internal void RaiseItemClickEvent(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnItemClick(e);
        }

        protected internal void RaiseCellClickEvent(TreeListCell cell)
        {
            var e = new TreeListCellEventArgs { Cell = cell };
            OnCellClick(e);
        }

        protected internal void RaiseItemDoubleClickEvent(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnItemDoubleClick(e);
        }

        protected internal void RaiseItemSelectionChangedEvent()
        {
            var e = new TreeListItemSelectionEventArgs();
            OnItemSelectionChanged(e);
        }

        protected internal bool RaiseBeforeItemCollapseEvent(TreeListItem item)
        {
            var e = new TreeListItemCancelEventArgs { Item = item };
            OnBeforeItemCollapse(e);
            return e.Cancel;
        }

        protected internal bool RaiseBeforeItemExpandEvent(TreeListItem item)
        {
            var e = new TreeListItemCancelEventArgs { Item = item };
            OnBeforeItemExpand(e);
            return e.Cancel;
        }

        protected internal void RaiseAfterItemCollapseEvent(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnAfterItemCollapse(e);
        }

        protected internal void RaiseAfterItemExpandEvent(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnAfterItemExpand(e);
        }

        protected internal void RaiseDamanLoadEvent(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnDemandLoad(e);
        }

        protected internal bool RaiseBeforeCellEditingEvent(TreeListCell cell)
        {
            var e = new TreeListBeforeCellEditingEventArgs { Cell = cell };
            OnBeforeCellEditing(e);
            return e.Cancel;
        }

        protected internal void RaiseAfterCellEditedEvent(TreeListCell cell, bool enterKey)
        {
            var e = new TreeListAfterCellEditedEventArgs { Cell = cell, EnterKey = enterKey };
            OnAfterCellEdited(e);
        }

        protected internal bool RaiseBeforeCellUpdatingEvent(TreeListCell cell, object oldValue, ref object newValue)
        {
            var e = new TreeListBeforeCellUpdatingEventArgs { Cell = cell, OldValue = oldValue, NewValue = newValue };
            OnBeforeCellUpdating(e);
            newValue = e.NewValue;
            return e.Cancel;
        }

        protected internal bool RaiseAfterCellUpdatedEvent(TreeListCell cell, object oldValue, object newValue, bool enterKey)
        {
            var e = new TreeListAfterCellUpdatedEventArgs { Cell = cell, OldValue = oldValue, NewValue = newValue, EnterKey = enterKey };
            OnAfterCellUpdated(e);
            return e.EnterKey;
        }

        protected internal bool RaiseBeforeItemCheckChangeEvent(TreeListItem item)
        {
            var e = new TreeListItemCancelEventArgs { Item = item };
            OnBeforeItemCheckChange(e);
            return e.Cancel;
        }

        protected internal void RaiseAfterItemCheckChangeEvent(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnAfterItemCheckChange(e);
        }

        protected internal bool RaiseBeforeCellCheckChangeEvent(TreeListCell cell)
        {
            var e = new TreeListCellCancelEventArgs { Cell = cell };
            OnBeforeCellCheckChange(e);
            return e.Cancel;
        }

        protected internal void RaiseAfterCellCheckChangeEvent(TreeListCell cell)
        {
            var e = new TreeListCellEventArgs { Cell = cell };
            OnAfterCellCheckChange(e);
        }

        protected internal void RaiseItemCheckChanged(TreeListItem item)
        {
            var e = new TreeListItemEventArgs { Item = item };
            OnItemCheckChanged(e);
        }
        #endregion

        #region 事件虚方法
        protected virtual void OnColumnClick(TreeListColumnClickEventArgs e)
        {
            ColumnClick?.Invoke(this, e);
        }

        protected virtual void OnItemDataBound(TreeListItemDataBoundEventArgs e)
        {
            ItemDataBound?.Invoke(this, e);
        }

        protected virtual void OnCellDataBound(TreeListCellDataBoundEventArgs e)
        {
            CellDataBound?.Invoke(this, e);
        }

        protected virtual void OnItemClick(TreeListItemEventArgs e)
        {
            ItemClick?.Invoke(this, e);
        }

        protected virtual void OnCellClick(TreeListCellEventArgs e)
        {
            CellClick?.Invoke(this, e);
        }

        protected virtual void OnItemDoubleClick(TreeListItemEventArgs e)
        {
            ItemDoubleClick?.Invoke(this, e);
        }

        protected virtual void OnItemSelectionChanged(TreeListItemSelectionEventArgs e)
        {
            ItemSelectionChanged?.Invoke(this, e);
        }

        protected virtual void OnBeforeItemExpand(TreeListItemCancelEventArgs e)
        {
            BeforeItemExpand?.Invoke(this, e);
        }

        protected virtual void OnBeforeItemCollapse(TreeListItemCancelEventArgs e)
        {
            BeforeItemCollapse?.Invoke(this, e);
        }

        protected virtual void OnAfterItemExpand(TreeListItemEventArgs e)
        {
            AfterItemExpand?.Invoke(this, e);
        }

        protected virtual void OnAfterItemCollapse(TreeListItemEventArgs e)
        {
            AfterItemCollapse?.Invoke(this, e);
        }

        protected virtual void OnDemandLoad(TreeListItemEventArgs e)
        {
            DemandLoad?.Invoke(this, e);
        }

        protected virtual void OnBeforeCellEditing(TreeListBeforeCellEditingEventArgs e)
        {
            BeforeCellEditing?.Invoke(this, e);
        }

        protected virtual void OnAfterCellEdited(TreeListAfterCellEditedEventArgs e)
        {
            AfterCellEdited?.Invoke(this, e);
        }

        protected virtual void OnBeforeCellUpdating(TreeListBeforeCellUpdatingEventArgs e)
        {
            BeforeCellUpdating?.Invoke(this, e);
        }

        protected virtual void OnAfterCellUpdated(TreeListAfterCellUpdatedEventArgs e)
        {
            AfterCellUpdated?.Invoke(this, e);
        }

        protected virtual void OnBeforeItemCheckChange(TreeListItemCancelEventArgs e)
        {
            BeforeItemCheckChange?.Invoke(this, e);
        }

        protected virtual void OnAfterItemCheckChange(TreeListItemEventArgs e)
        {
            AfterItemCheckChange?.Invoke(this, e);
        }

        protected virtual void OnBeforeCellCheckChange(TreeListCellCancelEventArgs e)
        {
            BeforeCellCheckChange?.Invoke(this, e);
        }

        protected virtual void OnAfterCellCheckChange(TreeListCellEventArgs e)
        {
            AfterCellCheckChange?.Invoke(this, e);
        }

        protected virtual void OnItemCheckChanged(TreeListItemEventArgs e)
        {
            ItemCheckChanged?.Invoke(this, e);
        }
        #endregion
    }
}
