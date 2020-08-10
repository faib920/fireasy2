// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace Fireasy.Windows.Forms
{
    internal class VirtualTreeListItemCollection : List<VirtualTreeListItem>
    {
    }

    internal interface IVirtualItem
    {
        bool Selected { get; set; }
    }

    internal class VirtualTreeListItem
    {
        internal VirtualTreeListItem(IVirtualItem item, int index)
        {
            Item = item;
            Index = index;

            if (item is TreeListGroup)
            {
                ItemType = Forms.ItemType.Group;
            }
            else if (item is TreeListItem)
            {
                ItemType = Forms.ItemType.Item;
            }
        }

        internal IVirtualItem Item { get; set; }

        internal int Index { get; set; }

        internal ItemType ItemType { get; private set; }
    }

    internal enum ItemType
    {
        Group,
        Item
    }
}
