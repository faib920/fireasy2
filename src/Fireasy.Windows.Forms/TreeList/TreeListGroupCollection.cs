// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class TreeListGroupCollection : ObservableCollection<TreeListGroup>
    {
        private TreeList treelist;
        private int sortVersion;

        public TreeListGroupCollection()
        {
        }

        internal TreeListGroupCollection(TreeList treelist)
        {
            this.treelist = treelist;
        }

        public void AddRange(params TreeListGroup[] groups)
        {
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    Add(group);
                }
            }
        }

        protected override void InsertItem(int index, TreeListGroup group)
        {
            base.InsertItem(index, group);

            if (treelist != null)
            {
                group.Update(treelist);
                treelist.Invalidate();
            }
        }

        /// <summary>
        /// 对集合进行排序。
        /// </summary>
        /// <param name="sortVersion"></param>
        /// <param name="column"></param>
        /// <param name="order"></param>
        internal void Sort(int sortVersion, TreeListColumn column, SortOrder order)
        {
            if (this.sortVersion == sortVersion)
            {
                return;
            }

            this.sortVersion = sortVersion;

            foreach (var item in Items)
            {
                if (item.Expended)
                {
                    item.Items.Sort(sortVersion, column, order);
                }
            }
        }

    }
}
