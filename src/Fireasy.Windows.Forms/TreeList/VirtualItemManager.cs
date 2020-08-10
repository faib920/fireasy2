// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Windows.Forms
{
    internal class VirtualItemManager
    {
        private readonly VirtualTreeListItemCollection _virtualList = new VirtualTreeListItemCollection();
        private readonly TreeList _treeList;
        private int _rowNumberIndex;

        public VirtualItemManager(TreeList treeList)
        {
            _treeList = treeList;
        }

        public VirtualTreeListItemCollection Items
        {
            get
            {
                return _virtualList;
            }
        }

        public void Recalc()
        {
            _rowNumberIndex = 0;
            _virtualList.Clear();
            if (_treeList.Groups.Count == 0)
            {
                GenerateVirtualListItems(_treeList.Items);
            }
            else
            {
                foreach (var g in _treeList.Groups)
                {

                    var vitem = new VirtualTreeListItem(g, _virtualList.Count);
                    _virtualList.Add(vitem);

                    if (g.Expended)
                    {
                        GenerateVirtualListItems(g.Items);
                    }
                }
            }
        }

        private void GenerateVirtualListItems(TreeListItemCollection items)
        {
            for (var i = 0; i < items.Count; i++)
            {
                var vitem = new VirtualTreeListItem(items[i], _virtualList.Count);
                items[i].DataIndex = (++_rowNumberIndex);
                _virtualList.Add(vitem);

                if (items[i].Selected)
                {
                    _treeList.SelectedItems.Add(items[i]);
                }

                if (items[i].Expended)
                {
                    GenerateVirtualListItems(items[i].Items);
                }
            }
        }

    }
}
