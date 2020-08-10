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
        private readonly TreeList _treelist;
        private ITreeListEditor _editor = null;

        internal TreeListEditController(TreeList treelist)
        {
            _treelist = treelist;
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

            _editor = cell.Column.Editor;
            Cell = cell;
            var control = (Control)_editor;
            _treelist.Controls.Add(control);

            IsEditing = true;
            _editor.Controller = this;
            _editor.SetValue(cell.Value);
            _editor.Show(rect);
        }

        /// <summary>
        /// 结束单元格的编辑。
        /// </summary>
        /// <param name="enterKey"></param>
        public void AcceptEdit(bool enterKey = false)
        {
            if (_editor == null || !IsEditing)
            {
                return;
            }

            if (!_editor.IsValid())
            {
                RemoveEditor();
                return;
            }

            var newValue = _editor.GetValue();

            if ((Cell.Value == null && newValue == null) ||
                (Cell.Value != null && Cell.Value.Equals(newValue)))
            {
                RemoveEditor();
                RaiseEditedEvent(enterKey);
                return;
            }


            if (!_treelist.RaiseBeforeCellUpdatingEvent(Cell, Cell.Value, ref newValue))
            {
                Cell.Value = newValue;
                if (!_treelist.RaiseAfterCellUpdatedEvent(Cell, Cell.Value, newValue, enterKey))
                {
                    enterKey = false;
                }

                RemoveEditor();
                RaiseEditedEvent(enterKey);
            }

            if (enterKey)
            {
                var index = Cell.Column.Index + 1;
                if (index <= _treelist.Columns.Count - 1)
                {
                    _treelist.BeginEdit(Cell.Item.Cells[index]);
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
            if (_editor != null)
            {
                _editor.Hide();
                _treelist.Focus();
                _treelist.Controls.Remove((Control)_editor);
            }
            _editor = null;
        }

        /// <summary>
        /// 引发编辑完成事件。
        /// </summary>
        /// <param name="enterKey">是否按下回车键。</param>
        private void RaiseEditedEvent(bool enterKey)
        {
            _treelist.RaiseAfterCellEditedEvent(Cell, enterKey);
        }
    }
}
