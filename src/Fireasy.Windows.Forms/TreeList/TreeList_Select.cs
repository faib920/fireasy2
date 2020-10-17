// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public partial class TreeList
    {
        private bool _controlPressed;
        private bool _shiftPressed;
        private int _lastRowIndex = -1;
        private int _shiftRowIndex = -1;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (MultiSelect) //允许多选
            {
                //按下Control
                if (e.KeyCode == Keys.ControlKey && e.Modifiers == Keys.Control && !_controlPressed)
                {
                    _controlPressed = true;
                }
                //按下Shift
                else if (e.KeyCode == Keys.ShiftKey && e.Modifiers == Keys.Shift && !_shiftPressed)
                {
                    _shiftPressed = true;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            //全选
            if (MultiSelect && e.Modifiers == Keys.Control && e.KeyCode == Keys.A)
            {
                SelectAll();
            }
            //取消Control键
            else if (_controlPressed)
            {
                _controlPressed = false;
            }
            //取消Shift键
            else if (_shiftPressed)
            {
                _shiftPressed = false;
                _shiftRowIndex = -1;
            }
            else if (ShowCheckBoxes && e.Modifiers == Keys.None && e.KeyCode == Keys.Space)
            {
                var item = SelectedItems.FirstOrDefault();
                if (item != null)
                {
                    item.Checked = !item.Checked;
                }
            }
        }

        /// <summary>
        /// 处理键盘消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override bool PreProcessMessage(ref Message msg)
        {
            if (SelectedItems.Count == 0)
            {
                return base.PreProcessMessage(ref msg);
            }

            var item = SelectedItems[0];
            var vitem = _virMgr.Items.FirstOrDefault(s => s.Item == item);
            var index = vitem.Index;

            if (msg.Msg == 0x100) //避免移动方向键时跳出控件
            {
                switch (msg.WParam.ToInt32())
                {
                    case 40: //下移
                        if (index++ >= _virMgr.Items.Count - 1)
                        {
                            return base.PreProcessMessage(ref msg);
                        }

                        if (_virMgr.Items[index].Item is TreeListGroup)
                        {
                            if (index++ >= _virMgr.Items.Count - 1)
                            {
                                return base.PreProcessMessage(ref msg);
                            }
                        }

                        _virMgr.Items[index].Item.Selected = true;
                        break;
                    case 38: //上移
                        if (index-- <= 0)
                        {
                            return base.PreProcessMessage(ref msg);
                        }

                        if (_virMgr.Items[index].Item is TreeListGroup)
                        {
                            if (index-- <= 0)
                            {
                                return base.PreProcessMessage(ref msg);
                            }
                        }

                        _virMgr.Items[index].Item.Selected = true;
                        break;
                }

                return false;
            }

            return base.PreProcessMessage(ref msg);
        }

        /// <summary>
        /// 全选所有行。
        /// </summary>
        private void SelectAll()
        {
            foreach (var item in _virMgr.Items)
            {
                if (item.Item is TreeListItem)
                {
                    SelectItem(((TreeListItem)item.Item), true, false);
                }
            }

            Invalidate();
        }
    }
}
