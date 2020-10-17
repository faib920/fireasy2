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
    public class TreeListCheckedItemCollection : ObservableCollection<TreeListItem>
    {
        private bool _isForbid;
        private readonly TreeList _treelist;

        /// <summary>
        /// 初始化 <see cref="TreeListCheckedItemCollection"/> 类的新实例。
        /// </summary>
        /// <param name="treelist"></param>
        internal TreeListCheckedItemCollection(TreeList treelist)
        {
            _treelist = treelist;
        }

        protected override void InsertItem(int index, TreeListItem item)
        {
            if (_isForbid)
            {
                base.InsertItem(index, item);
                return;
            }

            _treelist.CheckItem(item, true);
        }

        protected override void RemoveItem(int index)
        {
            if (_isForbid)
            {
                base.RemoveItem(index);
                return;
            }

            _treelist.CheckItem(this[index], false);
        }

        /// <summary>
        /// 从集合中清除所有项。
        /// </summary>
        protected override void ClearItems()
        {
            if (!_isForbid)
            {
                foreach (var item in Items)
                {
                    item.SetChecked(false);
                }

                if (_treelist != null)
                {
                    _treelist.UpdateItems();
                }
            }

            base.ClearItems();
        }

        /// <summary>
        /// 内部清理，防止递归调用。
        /// </summary>
        internal void InternalClear()
        {
            _isForbid = true;
            Clear();
            _isForbid = false;
        }

        internal void InternalAdd(TreeListItem item)
        {
            _isForbid = true;
            Add(item);
            _isForbid = false;
        }

        internal void InternalRemove(TreeListItem item)
        {
            _isForbid = true;
            Remove(item);
            _isForbid = false;
        }
    }
}
