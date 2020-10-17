﻿using Fireasy.Common;
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
    public class TreeListColumnCollection : ObservableCollection<TreeListColumn>
    {
        private readonly TreeList _treelist;

        public TreeListColumnCollection()
        {
        }

        internal TreeListColumnCollection(TreeList treelist)
        {
            _treelist = treelist;
        }

        public TreeListColumn this[string key]
        {
            get
            {
                Guard.ArgumentNull(key, nameof(key));

                foreach (var column in Items)
                {
                    if (key.Equals(column.DataKey))
                    {
                        return column;
                    }
                }

                return null;
            }
        }

        public TreeListColumn Add(string text)
        {
            var column = new TreeListColumn { Text = text };
            Add(column);
            return column;
        }

        public TreeListColumn Add(string text, int width, string dataKey)
        {
            var column = new TreeListColumn { Text = text, Width = width, DataKey = dataKey };
            Add(column);
            return column;
        }

        public TreeListColumn Add(string text, int width)
        {
            var column = new TreeListColumn { Text = text, Width = width };
            Add(column);
            return column;
        }

        public void AddRange(params TreeListColumn[] columns)
        {
            if (columns != null)
            {
                foreach (var column in columns)
                {
                    Add(column);
                }
            }
        }

        protected override void InsertItem(int index, TreeListColumn column)
        {
            column.Index = index;
            base.InsertItem(index, column);

            if (_treelist != null)
            {
                column.Update(_treelist);
                _treelist.Invalidate();
            }
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);

            for (var i = 0; i < Items.Count; i++)
            {
                Items[i].Index = i;
            }
        }

    }
}
