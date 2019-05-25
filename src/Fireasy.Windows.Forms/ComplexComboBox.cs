// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class ComplexComboBox : PopupComboBox
    {
        private TreeList tree;

        public ComplexComboBox()
        {
            InitializeComponent();
            DropDownControl = tree;
        }

        /// <summary>
        /// 获取或设置作为下拉内容的 <see cref="TreeList"/> 对象。
        /// </summary>
        public TreeList TreeList
        {
            get { return tree; }
        }

        public new object SelectedItem
        {
            get
            {
                if (TreeList.SelectedItems.Count == 0)
                {
                    return null;
                }

                return TreeList.SelectedItems[0];
            }
        }

        public override string SelectedText
        {
            get
            {
                return string.Empty;
            }
        }

        public override object SelectedValue
        {
            get
            {
                return base.SelectedValue;
            }
            set
            {
                if (TreeList.ShowCheckBoxes)
                {
                    base.SelectedValue = value;

                    if (value != null)
                    {
                        var texts = new List<string>();
                        CheckItem(TreeList.Items, value as object[], texts);
                        SetText(string.Join(",", texts));
                    }
                    else
                    {
                        SetText(string.Empty);
                    }
                }
                else
                {
                    TreeList.SelectedItems.Clear();
                    if (!FindItem(TreeList.Items, value))
                    {
                        SetText(string.Empty);
                    }
                }
            }
        }

        protected override void OnPopupOpened()
        {
            TreeList.Focus();
        }

        /// <summary>
        /// 设置需要下拉显示的 <see cref="TreeList"/> 控件。
        /// </summary>
        /// <param name="treelist"></param>
        public void SetTreeList(TreeList treelist)
        {
            tree = treelist;
            DropDownControl = treelist;
        }

        private bool FindItem(TreeListItemCollection items, object value)
        {
            foreach (var item in items)
            {
                if (object.Equals(GetValue(item), value))
                {
                    item.Selected = true;
                    base.SelectedValue = value;
                    SetText(GetDisplayText(item));
                    ExpendAllParent(item);
                    return true;
                }

                if (FindItem(item.Items, value))
                {
                    return true;
                }
            }

            return false;
        }

        private void ExpendAllParent(TreeListItem item)
        {
            var parent = item.Parent;
            while (parent != null)
            {
                parent.Expended = true;
                parent = parent.Parent;
            }
        }

        private void CheckItem(TreeListItemCollection items, object[] values, List<string> texts)
        {
            foreach (var item in items)
            {
                if (values.Contains(GetValue(item)))
                {
                    item.Checked = true;
                    texts.Add(GetDisplayText(item));
                }

                CheckItem(item.Items, values, texts);
            }
        }

        private void InitializeComponent()
        {
            tree = new TreeList();
            tree.Dock = DockStyle.Fill;
            tree.HotTracking = true;
            tree.ShowHeader = false;
            tree.Columns.Add(new TreeListColumn { Spring = true });
            tree.ItemClick += TreeList_ItemClick;
            tree.AfterItemCheckChange += TreeList_AfterItemCheckChange;
            tree.ShowGridLines = false;
        }

        void TreeList_ItemClick(object sender, TreeListItemEventArgs e)
        {
            if (TreeList.ShowCheckBoxes)
            {
                return;
            }

            SetItem(GetDisplayText(e.Item), GetValue(e.Item));

            HideDropDown();
        }

        void TreeList_AfterItemCheckChange(object sender, TreeListItemEventArgs e)
        {
            if (!TreeList.ShowCheckBoxes)
            {
                return;
            }

            var sb = new StringBuilder();
            var array = new List<object>();

            foreach (var item in TreeList.CheckedItems)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetDisplayText(item));
                array.Add(GetValue(item));
            }

            SetItem(sb.ToString(), array.Count == 0 ? null : array.ToArray());
        }

        private string GetDisplayText(TreeListItem item)
        {
            if (!string.IsNullOrEmpty(DisplayMember))
            {
                return item.Cells[DisplayMember].Text;
            }

            return item.Text;
        }

        private object GetValue(TreeListItem item)
        {
            if (!string.IsNullOrEmpty(ValueMember))
            {
                return item.Cells[ValueMember].Value;
            }

            return item.KeyValue;
        }
    }
}
