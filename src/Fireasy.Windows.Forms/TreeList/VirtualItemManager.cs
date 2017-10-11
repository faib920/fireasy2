
namespace Fireasy.Windows.Forms
{
    internal class VirtualItemManager
    {
        private VirtualTreeListItemCollection virtualList = new VirtualTreeListItemCollection();
        private TreeList treeList;
        private int rowNumberIndex;

        public VirtualItemManager(TreeList treeList)
        {
            this.treeList = treeList;
        }

        public VirtualTreeListItemCollection Items
        {
            get
            {
                return virtualList;
            }
        }

        public void Recalc()
        {
            rowNumberIndex = 0;
            virtualList.Clear();
            if (treeList.Groups.Count == 0)
            {
                GenerateVirtualListItems(treeList.Items);
            }
            else
            {
                foreach (var g in treeList.Groups)
                {

                    var vitem = new VirtualTreeListItem(g, virtualList.Count);
                    virtualList.Add(vitem);

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
                var vitem = new VirtualTreeListItem(items[i], virtualList.Count);
                items[i].DataIndex = (++rowNumberIndex);
                virtualList.Add(vitem);

                if (items[i].Selected)
                {
                    treeList.SelectedItems.Add(items[i]);
                }

                if (items[i].Expended)
                {
                    GenerateVirtualListItems(items[i].Items);
                }
            }
        }

    }
}
