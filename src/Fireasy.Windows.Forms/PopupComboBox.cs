// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Security.Permissions;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 自定义弹出框的 <see cref="ComboBox"/>。
    /// </summary>
    public class PopupComboBox : ComboBox
    {
        private Popup _dropDown;
        private Control _dropDownControl;
        private bool _isFirstDrop = true;
        private bool _isOpened = false;
        private bool _isDroped = false;
        private object _selectedValue;
        private object _selectedItem;

        public new event EventHandler DropDown;
        public new event EventHandler DropDownClosed;

        /// <summary>
        /// 初始化 <see cref="PopupComboBox"/> 类的新实例。
        /// </summary>
        public PopupComboBox()
        {
            Resizable = true;
        }

        /// <summary>
        /// 获取或设置是否在打开时获取焦点。
        /// </summary>
        [DefaultValue(true)]
        [Description("获取或设置是否在打开时获取焦点。")]
        public bool FocusOnOpen { get; set; }

        /// <summary>
        /// 获取或设置是否使用效果。
        /// </summary>
        [DefaultValue(false)]
        [Description("获取或设置是否使用效果。")]
        public bool UseFadeEffect { get; set; }

        /// <summary>
        /// 获取或设置是否可调整大小。
        /// </summary>
        [DefaultValue(true)]
        [Description("获取或设置是否可调整大小。")]
        public bool Resizable { get; set; }

        /// <summary>
        /// 获取或设置下拉显示的控件。
        /// </summary>
        public Control DropDownControl
        {
            get
            {
                return _dropDownControl;
            }
            set
            {
                if (_dropDownControl == value)
                {
                    return;
                }

                _dropDownControl = value;

                if (_dropDown != null)
                {
                    _dropDown.Dispose();
                }

                _dropDown = new Popup(value)
                {
                    FocusOnOpen = FocusOnOpen,
                    UseFadeEffect = UseFadeEffect,
                    Resizable = Resizable
                };

                _dropDown.Opening += (o, e) => _isOpened = true;
                _dropDown.Opened += (o, e) => { _isDroped = true; OnPopupOpened(); };
                _dropDown.Closed += (o, e) => _isDroped = false;
            }
        }

        /// <summary>
        /// 获取或设置是否显示下拉部份。
        /// </summary>
        public new bool DroppedDown
        {
            get
            {
                return _dropDown.Visible;
            }
            set
            {
                if (DroppedDown)
                {
                    HideDropDown();
                }
                else
                {
                    ShowDropDown();
                }
            }
        }

        /// <summary>
        /// 显示下拉控件。
        /// </summary>
        public void ShowDropDown()
        {
            if (_dropDown != null)
            {
                DropDown?.Invoke(this, EventArgs.Empty);

                if (_isFirstDrop)
                {
                    _dropDown.Width = DropDownWidth;
                    _dropDown.Height = DropDownHeight;
                    DropDownControl.LostFocus += (o, e) =>
                    {
                        _isOpened = false;
                    };
                    _isFirstDrop = false;
                }

                _dropDown.Show(this);
                OnDropDown(EventArgs.Empty);
            }
        }

        /// <summary>
        /// 隐藏下拉控件。
        /// </summary>
        public void HideDropDown()
        {
            if (_dropDown != null)
            {
                _dropDown.Hide();
                DropDownClosed?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 设置 <see cref="ComboBox"/> 的显示文本。
        /// </summary>
        /// <param name="text"></param>
        public void SetText(string text)
        {
            if (Items.Count == 0)
            {
                Items.Add(string.Empty);
            }

            if (Items[0].Equals(text))
            {
                return;
            }

            Items[0] = text;
            SelectedIndex = 0;
        }

        /// <summary>
        /// 设置当前选中项。
        /// </summary>
        /// <param name="text">控件显示的文本。</param>
        /// <param name="value">当前的值。</param>
        public void SetItem(string text, object value)
        {
            SelectedValue = value;
            SetText(text);
        }

        protected override void OnResize(EventArgs e)
        {
            _dropDown.Width = DropDownWidth;
            base.OnResize(e);
        }

        /// <summary>
        /// 获取或设置选中项的值。
        /// </summary>
        public virtual new object SelectedValue
        {
            get { return _selectedValue; }
            set { _selectedValue = value; }
        }

        /// <summary>
        /// 获取或设置选中项的项。
        /// </summary>
        public virtual new object SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; }
        }

        /// <summary>
        /// 获取或设置选中项的文本。
        /// </summary>
        public virtual new string SelectedText
        {
            get { return string.Empty; }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Down)
            {
                ShowDropDown();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLostFocus(EventArgs e)
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (NativeMethods.W_COMMAND + NativeMethods.W_REFLECT) &&
                NativeMethods.HIWORD((int)m.WParam) == NativeMethods.CBN_DROPDOWN)
            {
                if (_isDroped)
                {
                    HideDropDown();
                }
                else
                {
                    BeginInvoke(new MethodInvoker(ShowDropDown));
                }

                return;
            }

            base.WndProc(ref m);
        }

        protected virtual void OnPopupOpened()
        {
        }

        #region " Unused Properties "

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        internal new ObjectCollection Items
        {
            get { return base.Items; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        internal new int ItemHeight
        {
            get { return base.ItemHeight; }
            set { base.ItemHeight = value; }
        }

        #endregion
    }
}
