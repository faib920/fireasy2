// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public delegate void TreeListColumnClickEventHandler(object sender, TreeListColumnClickEventArgs e);

    public delegate void TreeListItemDataBoundEventHandler(object sender, TreeListItemDataBoundEventArgs e);

    public delegate void TreeListCellDataBoundEventHandler(object sender, TreeListCellDataBoundEventArgs e);

    public delegate void TreeListItemClickEventHandler(object sender, TreeListItemEventArgs e);

    public delegate void TreeListItemDoubleClickEventHandler(object sender, TreeListItemEventArgs e);

    public delegate void TreeListItemSelectionChangedEventHandler(object sender, TreeListItemSelectionEventArgs e);

    public delegate void TreeListItemBeforeCollapseEventHandler(object sender, TreeListItemCancelEventArgs e);

    public delegate void TreeListItemBeforeExpandEventHandler(object sender, TreeListItemCancelEventArgs e);

    public delegate void TreeListItemAfterCollapseEventHandler(object sender, TreeListItemEventArgs e);

    public delegate void TreeListItemAfterExpandEventHandler(object sender, TreeListItemEventArgs e);

    public delegate void TreeListDemandLoadEventHandler(object sender, TreeListItemEventArgs e);

    public delegate void TreeListCellClickEventHandler(object sender, TreeListCellEventArgs e);

    public delegate void TreeListBeforeCellEditingEventHandler(object sender, TreeListBeforeCellEditingEventArgs e);

    public delegate void TreeListAfterCellEditedEventHandler(object sender, TreeListAfterCellEditedEventArgs e);

    public delegate void TreeListBeforeCellUpdatingEventHandler(object sender, TreeListBeforeCellUpdatingEventArgs e);

    public delegate void TreeListAfterCellUpdatedEventHandler(object sender, TreeListAfterCellUpdatedEventArgs e);

    public delegate void TreeListItemBeforeCheckedEventHandler(object sender, TreeListItemCancelEventArgs e);

    public delegate void TreeListItemAfterCheckedEventHandler(object sender, TreeListItemEventArgs e);

    public delegate void TreeListCellBeforeCheckedEventHandler(object sender, TreeListCellCancelEventArgs e);

    public delegate void TreeListCellAfterCheckedEventHandler(object sender, TreeListCellEventArgs e);

    public delegate void TreeListItemCheckChangeEventHandler(object sender, TreeListItemEventArgs e);

    public class TreeListItemSelectionEventArgs
    {
    }

    public class TreeListItemEventArgs
    {
        public TreeListItem Item { get; internal set; }
    }

    public class TreeListCellEventArgs
    {
        public TreeListCell Cell { get; internal set; }
    }

    public class TreeListColumnClickEventArgs
    {
        /// <summary>
        /// 获取所单击的列。
        /// </summary>
        public TreeListColumn Column { get; internal set; }

        /// <summary>
        /// 获取或设置是否允许排序。
        /// </summary>
        public bool Sortable { get; set; }

        /// <summary>
        /// 获取排序方式。
        /// </summary>
        public SortOrder Sorting { get; internal set; }
    }

    public class TreeListItemDataBoundEventArgs : TreeListItemEventArgs
    {
        public int Index { get; internal set; }

        public object ItemData { get; internal set; }
    }

    public class TreeListItemCancelEventArgs : TreeListItemEventArgs
    {
        public bool Cancel { get; set; }
    }

    public class TreeListCellCancelEventArgs : TreeListCellEventArgs
    {
        public bool Cancel { get; set; }
    }

    public class TreeListCellDataBoundEventArgs : TreeListCellEventArgs
    {

        public object ItemData { get; internal set; }

        public object Value { get; internal set; }
    }

    public class TreeListBeforeCellEditingEventArgs : TreeListCellEventArgs
    {
        public bool Cancel { get; set; }
    }

    public class TreeListAfterCellEditedEventArgs : TreeListCellEventArgs
    {
        /// <summary>
        /// 获取是否是按下了回车键结束的编辑。
        /// </summary>
        public bool EnterKey { get; internal set; }
    }

    public class TreeListBeforeCellUpdatingEventArgs : TreeListCellEventArgs
    {
        public object OldValue { get; internal set; }

        public object NewValue { get; set; }

        public bool Cancel { get; set; }
    }

    public class TreeListAfterCellUpdatedEventArgs : TreeListCellEventArgs
    {
        public object OldValue { get; internal set; }

        public object NewValue { get; set; }

        public bool EnterKey { get; set; }
    }
}
