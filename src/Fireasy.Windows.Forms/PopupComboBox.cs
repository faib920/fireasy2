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
        private Popup dropDown;
        private Control dropDownControl;
        private bool isFirstDrop = true;
        private bool isOpened = false;
        private bool isDroped = false;
        private object selectedValue;
        private object selectedItem;

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
                return dropDownControl;
            }
            set
            {
                if (dropDownControl == value)
                {
                    return;
                }

                dropDownControl = value;

                if (dropDown != null)
                {
                    dropDown.Dispose();
                }

                dropDown = new Popup(value)
                {
                    FocusOnOpen = FocusOnOpen,
                    UseFadeEffect = UseFadeEffect,
                    Resizable = Resizable
                };

                dropDown.Opening += (o, e) => isOpened = true;
                dropDown.Opened += (o, e) => { isDroped = true; OnPopupOpened(); };
                dropDown.Closed += (o, e) => isDroped = false;
            }
        }

        /// <summary>
        /// 获取或设置是否显示下拉部份。
        /// </summary>
        public new bool DroppedDown
        {
            get
            {
                return dropDown.Visible;
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
            if (dropDown != null)
            {
                DropDown?.Invoke(this, EventArgs.Empty);

                if (isFirstDrop)
                {
                    dropDown.Width = DropDownWidth;
                    dropDown.Height = DropDownHeight;
                    DropDownControl.LostFocus += (o, e) =>
                    {
                        isOpened = false;
                    };
                    isFirstDrop = false;
                }

                dropDown.Show(this);
                OnDropDown(EventArgs.Empty);
            }
        }

        /// <summary>
        /// 隐藏下拉控件。
        /// </summary>
        public void HideDropDown()
        {
            if (dropDown != null)
            {
                dropDown.Hide();
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
            dropDown.Width = DropDownWidth;
            base.OnResize(e);
        }

        /// <summary>
        /// 获取或设置选中项的值。
        /// </summary>
        public virtual new object SelectedValue
        {
            get { return selectedValue; }
            set { selectedValue = value; }
        }

        /// <summary>
        /// 获取或设置选中项的项。
        /// </summary>
        public virtual new object SelectedItem
        {
            get { return selectedItem; }
            set { selectedItem = value; }
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
            if (m.Msg == (NativeMethods.WM_COMMAND + NativeMethods.WM_REFLECT) &&
                NativeMethods.HIWORD((int)m.WParam) == NativeMethods.CBN_DROPDOWN)
            {
                if (isDroped)
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
