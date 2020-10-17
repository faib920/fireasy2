using Fireasy.Common;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.ObjectModel;

namespace Fireasy.Windows.Forms
{
    public class TreeListCellCollection : ObservableCollection<TreeListCell>
    {
        private readonly TreeListItem _listitem;

        internal TreeListCellCollection(TreeListItem item)
        {
            _listitem = item;
        }

        public TreeListCell this[string key]
        {
            get
            {
                Guard.ArgumentNull(key, nameof(key));

                if (_listitem != null)
                {
                    foreach (var column in _listitem.TreeList.Columns)
                    {
                        if (key.Equals(column.DataKey))
                        {
                            return Items[column.Index];
                        }
                    }
                }

                return null;
            }
        }

        public TreeListCell this[TreeListColumn column]
        {
            get
            {
                Guard.ArgumentNull(column, nameof(column));

                if (_listitem != null)
                {
                    return Items[column.Index];
                }

                return null;
            }
        }

        public TreeListCell Add(string text)
        {
            var item = new TreeListCell { Value = text };
            Add(item);

            return item;
        }

        protected override void InsertItem(int index, TreeListCell item)
        {
            if (_listitem != null)
            {
                item.Update(_listitem, index);
            }

            base.InsertItem(index, item);
        }
    }
}
