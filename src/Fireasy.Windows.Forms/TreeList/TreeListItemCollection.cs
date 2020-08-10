using System;
using System.Collections.Generic;
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
    /// <summary>
    /// 表示 <see cref="TreeListItem"/> 的集合。
    /// </summary>
    public class TreeListItemCollection : ObservableCollection<TreeListItem>
    {
        private readonly TreeList _treelist;
        private readonly TreeListItem _parent;
        private readonly int _level;
        private int _sortVersion;

        /// <summary>
        /// 初始化 <see cref="TreeListItemCollection"/> 类的新实例。
        /// </summary>
        public TreeListItemCollection()
        {
        }

        internal TreeListItemCollection(TreeList treelist, TreeListItem parent, int level)
        {
            _treelist = treelist;
            _parent = parent;
            _level = level;
        }

        public TreeListItem Add(string text)
        {
            var item = new TreeListItem(text);
            Add(item);
            return item;
        }

        /// <summary>
        /// 添加一行以及单元格的数据。
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public TreeListItem AddCells(params object[] values)
        {
            var item = new TreeListItem();
            Add(item);

            if (values.Length > item.Cells.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (var i = 0; i < values.Length; i++)
            {
                item.Cells[i].Value = values[i];
            }

            return item;
        }

        /// <summary>
        /// 在集合中添加一组项。
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<TreeListItem> items)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// 将一项插入集合中到指定索引处。
        /// </summary>
        /// <param name="index">插入的位置。</param>
        /// <param name="item">要插入的对象。</param>
        protected override void InsertItem(int index, TreeListItem item)
        {
            item.Index = Count;
            item.Owner = this;
            base.InsertItem(index, item);

            ResortIndex();

            if (_treelist != null)
            {
                item.Level = _level;
                item.Update(_treelist, _parent, _level + 1);
                _treelist.UpdateItems();
            }
        }

        /// <summary>
        /// 移除集合中指定索引处的项。
        /// </summary>
        /// <param name="index">移除的索引位置。</param>
        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);

            ResortIndex();

            if (_treelist != null)
            {
                _treelist.RemoveValidateFlags(item);
                _treelist.UpdateItems(index);
            }
        }

        /// <summary>
        /// 从集合中清除所有项。
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();

            if (_treelist != null)
            {
                _treelist.RemoveValidateFlags();
                _treelist.UpdateItems();
            }
        }

        /// <summary>
        /// 对集合进行排序。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="order"></param>
        public void Sort(TreeListColumn column, SortOrder order)
        {
            Sort(_sortVersion, column, order);
        }

        /// <summary>
        /// 对集合进行排序。
        /// </summary>
        /// <param name="sortVersion"></param>
        /// <param name="column"></param>
        /// <param name="order"></param>
        internal void Sort(int sortVersion, TreeListColumn column, SortOrder order)
        {
            if (_sortVersion == sortVersion)
            {
                return;
            }

            _sortVersion = sortVersion;

            ((List<TreeListItem>)Items).Sort(new TreeListItemComparer(column, order));

            //子节点排序，重新编排索引值
            var index = 0;
            foreach (var item in Items)
            {
                if (item.Expended)
                {
                    item.Items.Sort(sortVersion, column, order);
                }

                item.Index = index++;
            }
        }

        /// <summary>
        /// 重新排序索引值。
        /// </summary>
        private void ResortIndex()
        {
            for (var i = 0; i < Items.Count; i++)
            {
                Items[i].Index = i;
            }
        }
    }
}