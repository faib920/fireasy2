using Fireasy.Common;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public sealed class TreeListEditController
    {
        private TreeList treelist;
        private ITreeListEditor editor = null;

        internal TreeListEditController(TreeList treelist)
        {
            this.treelist = treelist;
        }

        /// <summary>
        /// 获取或设置是否处于编辑状态。
        /// </summary>
        public bool IsEditing { get; set; }

        /// <summary>
        /// 获取当前处于编辑的单元格对象。
        /// </summary>
        public TreeListCell Cell { get; private set; }

        /// <summary>
        /// 将 <paramref name="cell"/> 置于编辑状态。
        /// </summary>
        /// <param name="cell">要编辑的单元格。</param>
        /// <param name="rect">编辑器放置的位置。</param>
        public void BeginEdit(TreeListCell cell, Rectangle rect)
        {
            Guard.ArgumentNull(cell, nameof(cell));

            if (IsEditing && cell == Cell)
            {
                return;
            }

            rect.Inflate(-1, -1);

            AcceptEdit();

            if (!cell.Column.Editable || cell.Column.Editor == null)
            {
                return;
            }

            editor = cell.Column.Editor;
            Cell = cell;
            var control = (Control)editor;
            treelist.Controls.Add(control);

            IsEditing = true;
            editor.Controller = this;
            editor.SetValue(cell.Value);
            editor.Show(rect);
        }

        /// <summary>
        /// 结束单元格的编辑。
        /// </summary>
        /// <param name="enterKey"></param>
        public void AcceptEdit(bool enterKey = false)
        {
            if (editor == null || !IsEditing)
            {
                return;
            }

            if (!editor.IsValid())
            {
                RemoveEditor();
                return;
            }

            var newValue = editor.GetValue();

            if ((Cell.Value == null && newValue == null) ||
                (Cell.Value != null && Cell.Value.Equals(newValue)))
            {
                RemoveEditor();
                RaiseEditedEvent(enterKey);
                return;
            }


            if (!treelist.RaiseBeforeCellUpdatingEvent(Cell, Cell.Value, ref newValue))
            {
                Cell.Value = newValue;
                if (!treelist.RaiseAfterCellUpdatedEvent(Cell, Cell.Value, newValue, enterKey))
                {
                    enterKey = false;
                }

                RemoveEditor();
                RaiseEditedEvent(enterKey);
            }

            if (enterKey)
            {
                var index = Cell.Column.Index + 1;
                if (index <= treelist.Columns.Count - 1)
                {
                    treelist.BeginEdit(Cell.Item.Cells[index]);
                }
            }
        }

        /// <summary>
        /// 取消编辑。
        /// </summary>
        public void CancelEdit()
        {
            RemoveEditor();
        }

        /// <summary>
        /// 移除单元格编辑器。
        /// </summary>
        private void RemoveEditor()
        {
            IsEditing = false;
            if (editor != null)
            {
                editor.Hide();
                treelist.Focus();
                treelist.Controls.Remove((Control)editor);
            }
            editor = null;
        }

        /// <summary>
        /// 引发编辑完成事件。
        /// </summary>
        /// <param name="enterKey">是否按下回车键。</param>
        private void RaiseEditedEvent(bool enterKey)
        {
            treelist.RaiseAfterCellEditedEvent(Cell, enterKey);
        }
    }
}
