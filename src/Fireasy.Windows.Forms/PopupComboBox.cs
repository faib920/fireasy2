// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class PopupComboBox : ComboBox
    {
        private Popup dropDown;
        private Control dropDownControl;
        private bool isFirstDrop = true;
        private bool isOpened = false;
        private bool isDroped = false;
        private object selectedValue;

        public new event EventHandler DropDown;
        public new event EventHandler DropDownClosed;

        /// <summary>
        /// 初始化 <see cref="PopupComboBox"/> 类的新实例。
        /// </summary>
        public PopupComboBox()
        {
            Resizable = true;
        }

        [DefaultValue(true)]
        public bool FocusOnOpen { get; set; }

        [DefaultValue(false)]
        public bool UseFadeEffect { get; set; }

        [DefaultValue(true)]
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

                dropDown = new Popup(value);
                dropDown.FocusOnOpen = FocusOnOpen;
                dropDown.UseFadeEffect = UseFadeEffect;
                dropDown.Resizable = Resizable;
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
                if (DropDown != null)
                {
                    DropDown(this, EventArgs.Empty);
                }

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
                if (DropDownClosed != null)
                {
                    DropDownClosed(this, EventArgs.Empty);
                }
            }
        }

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

        public virtual new object SelectedValue
        {
            get { return selectedValue; }
            set { selectedValue = value; }
        }

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
