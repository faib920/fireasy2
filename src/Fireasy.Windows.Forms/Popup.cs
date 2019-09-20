using Fireasy.Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(false)]
    public partial class Popup : ToolStripDropDown
    {
        #region " Fields & Properties "

        private ToolStripControlHost host;
        private Control opener;
        private Popup ownerPopup;
        private Popup childPopup;
        private bool resizableTop;
        private bool resizableLeft;

        private bool isChildPopupOpened;
        private bool resizable = true;

        /// <summary>
        /// 获取下拉显示的控件。
        /// </summary>
        public Control Content { get; private set; }

        /// <summary>
        /// 获取或设置在下拉显示时，下拉内容是否获得焦点。
        /// </summary>
        public bool FocusOnOpen { get; set; }

        /// <summary>
        /// 获取或设置是否使用渐显效果。
        /// </summary>
        public bool UseFadeEffect { get; set; }

        /// <summary>
        /// 获取或设置下拉状态下是否接受 Alt 按键。
        /// </summary>
        public bool AcceptAlt { get; set; }

        /// <summary>
        /// 获取或设置是否可以调整下拉内容的大小。
        /// </summary>
        public bool Resizable
        {
            get { return resizable && !isChildPopupOpened; }
            set { resizable = value; }
        }

        /// <summary>
        /// 获取或设置下拉内容可调整的最小尺寸。
        /// </summary>
        public new Size MinimumSize { get; set; }

        /// <summary>
        /// 获取或设置下拉内容可调整的最大尺寸。
        /// </summary>
        public new Size MaximumSize { get; set; }

        /// <summary>
        /// 获取窗口的参数。
        /// </summary>
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_NOACTIVATE;
                return cp;
            }
        }

        #endregion

        #region " Constructors "

        /// <summary>
        /// 初始化 <see cref="Popup"/> 类的新实例。
        /// </summary>
        /// <param name="content">下拉控件。</param>
        public Popup(Control content)
        {
            Guard.ArgumentNull(content, "content");

            Content = content;
            FocusOnOpen = true;
            AcceptAlt = true;
            Resizable = true;
            AutoSize = false;
            DoubleBuffered = true;
            ResizeRedraw = true;
            host = new ToolStripControlHost(content);
            Padding = Margin = host.Padding = host.Margin = Padding.Empty;
            MinimumSize = content.MinimumSize;
            content.MinimumSize = content.Size;
            MaximumSize = content.MaximumSize;
            content.MaximumSize = content.Size;
            Size = content.Size;
            TabStop = content.TabStop = true;
            Items.Add(host);
            content.Disposed += (sender, e) =>
            {
                content = null;
                Dispose(true);
            };
            content.RegionChanged += (sender, e) => UpdateRegion();
            content.Paint += (sender, e) => PaintSizeGrip(e);
            UpdateRegion();

            //如果控件已经放在一个容器中，则从容器中移除
            var container = content.Parent as ContainerControl;
            if (container != null)
            {
                container.Controls.Remove(content);
            }
        }

        #endregion

        #region " Methods "
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (AcceptAlt && ((keyData & Keys.Alt) == Keys.Alt))
            {
                if ((keyData & Keys.F4) != Keys.F4)
                {
                    return false;
                }
                else
                {
                    Close();
                }
            }

            bool processed = base.ProcessDialogKey(keyData);
            if (!processed && (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift)))
            {
                bool backward = (keyData & Keys.Shift) == Keys.Shift;
                Content.SelectNextControl(null, !backward, true, true, true);
            }

            return processed;
        }

        protected void UpdateRegion()
        {
            if (Region != null)
            {
                Region.Dispose();
                Region = null;
            }

            if (Content.Region != null)
            {
                Region = Content.Region.Clone();
            }
        }

        /// <summary>
        /// 将下拉内容显示到指定的控件上。
        /// </summary>
        /// <param name="control"></param>
        public void Show(Control control)
        {
            Guard.ArgumentNull(control, "control");

            Show(control, control.ClientRectangle);
        }

        /// <summary>
        /// 将下拉内容显示到指定的区域内。
        /// </summary>
        /// <param name="area"></param>
        public void Show(Rectangle area)
        {
            resizableTop = resizableLeft = false;
            Point location = new Point(area.Left, area.Top + area.Height);
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            if (location.X + Size.Width > (screen.Left + screen.Width))
            {
                resizableLeft = true;
                location.X = (screen.Left + screen.Width) - Size.Width;
            }

            if (location.Y + Size.Height > (screen.Top + screen.Height))
            {
                resizableTop = true;
                location.Y -= Size.Height + area.Height;
            }

            Show(location, ToolStripDropDownDirection.AboveLeft);
        }

        public void Show(Control control, Rectangle area)
        {
            Guard.ArgumentNull(control, "control");

            SetOwnerItem(control);

            resizableTop = resizableLeft = false;
            Point location = control.PointToScreen(new Point(area.Left, area.Top + area.Height - 3));
            Rectangle screen = Screen.FromControl(control).WorkingArea;
            if (location.X + Size.Width > (screen.Left + screen.Width))
            {
                resizableLeft = true;
                location.X = (screen.Left + screen.Width) - Size.Width;
            }

            if (location.Y + Size.Height > (screen.Top + screen.Height))
            {
                resizableTop = true;
                location.Y -= Size.Height + area.Height;
            }

            location = control.PointToClient(location);
            Show(control, location, ToolStripDropDownDirection.BelowRight);
        }

        private void SetOwnerItem(Control control)
        {
            if (control == null)
            {
                return;
            }

            if (control is Popup)
            {
                Popup popupControl = control as Popup;
                ownerPopup = popupControl;
                ownerPopup.childPopup = this;
                OwnerItem = popupControl.Items[0];
                return;
            }
            else if (opener == null)
            {
                opener = control;
            }

            if (control.Parent != null)
            {
                SetOwnerItem(control.Parent);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (Content != null)
            {
                Content.MinimumSize = Size;
                Content.MaximumSize = Size;
                Content.Size = Size;
                Content.Location = Point.Empty;
            }

            base.OnSizeChanged(e);
        }

        protected override void OnOpening(CancelEventArgs e)
        {
            if (Content.IsDisposed || Content.Disposing)
            {
                e.Cancel = true;
                return;
            }

            UpdateRegion();
            base.OnOpening(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            if (ownerPopup != null)
            {
                ownerPopup.isChildPopupOpened = true;
            }

            if (FocusOnOpen)
            {
                Content.Focus();
            }

            base.OnOpened(e);
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            opener = null;
            if (ownerPopup != null)
            {
                ownerPopup.isChildPopupOpened = false;
            }

            base.OnClosed(e);
        }

        protected override void SetVisibleCore(bool visible)
        {
            var frames = 5;
            var totalduration = 200;
            var frameduration = totalduration / frames;
            var opacity = Opacity;
            if (visible && FocusOnOpen && UseFadeEffect)
            {
                Opacity = 0;
            }

            base.SetVisibleCore(visible);

            if (!visible || !FocusOnOpen || !UseFadeEffect)
            {
                return;
            }

            for (int i = 1; i <= frames; i++)
            {
                if (i > 1)
                {
                    System.Threading.Thread.Sleep(frameduration);//漸顯
                }
                Opacity = opacity * (double)i / (double)frames;
            }
            Opacity = opacity;
        }

        #endregion

        #region " Resizing Support "
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_PRINT && !Visible)
            {
                Visible = true;
            }

            if (InternalProcessResizing(ref m, false))
            {
                return;
            }

            base.WndProc(ref m);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool ProcessResizing(ref Message m)
        {
            return InternalProcessResizing(ref m, true);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private bool InternalProcessResizing(ref Message m, bool contentControl)
        {
            if (m.Msg == NativeMethods.WM_NCACTIVATE &&
                m.WParam != IntPtr.Zero &&
                childPopup != null &&
                childPopup.Visible)
            {
                childPopup.Hide();
            }

            if (!Resizable)
            {
                return false;
            }

            if (m.Msg == NativeMethods.WM_NCHITTEST)
            {
                return OnNcHitTest(ref m, contentControl);
            }
            else if (m.Msg == NativeMethods.WM_GETMINMAXINFO)
            {
                return OnGetMinMaxInfo(ref m);
            }

            return false;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private bool OnGetMinMaxInfo(ref Message m)
        {
            var minmax = (NativeMethods.MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.MINMAXINFO));
            if (!MaximumSize.IsEmpty)
            {
                minmax.maxTrackSize = MaximumSize;
            }

            minmax.minTrackSize = MinimumSize;
            Marshal.StructureToPtr(minmax, m.LParam, false);
            return true;
        }

        private bool OnNcHitTest(ref Message m, bool contentControl)
        {
            var x = NativeMethods.LOWORD(m.LParam);
            var y = NativeMethods.HIWORD(m.LParam);
            var clientLocation = PointToClient(new Point(x, y));

            var gripBouns = new GripBounds(contentControl ? Content.ClientRectangle : ClientRectangle);
            var transparent = new IntPtr(NativeMethods.HTTRANSPARENT);

            if (resizableTop)
            {
                if (resizableLeft && gripBouns.TopLeft.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTTOPLEFT;
                    return true;
                }

                if (!resizableLeft && gripBouns.TopRight.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTTOPRIGHT;
                    return true;
                }

                if (gripBouns.Top.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTTOP;
                    return true;
                }
            }
            else
            {
                if (resizableLeft && gripBouns.BottomLeft.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTBOTTOMLEFT;
                    return true;
                }

                if (!resizableLeft && gripBouns.BottomRight.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTBOTTOMRIGHT;
                    return true;
                }

                if (gripBouns.Bottom.Contains(clientLocation))
                {
                    m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTBOTTOM;
                    return true;
                }
            }

            if (resizableLeft && gripBouns.Left.Contains(clientLocation))
            {
                m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTLEFT;
                return true;
            }

            if (!resizableLeft && gripBouns.Right.Contains(clientLocation))
            {
                m.Result = contentControl ? transparent : (IntPtr)NativeMethods.HTRIGHT;
                return true;
            }

            return false;
        }

        private VisualStyleRenderer _sizeGripRenderer;
        /// <summary>
        /// Paints the sizing grip.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs" /> instance containing the event data.</param>
        public void PaintSizeGrip(PaintEventArgs e)
        {
            if (e == null || e.Graphics == null || !resizable)
            {
                return;
            }

            var clientSize = Content.ClientSize;
            using (var gripImage = new Bitmap(0x10, 0x10))
            {
                using (Graphics g = Graphics.FromImage(gripImage))
                {
                    if (Application.RenderWithVisualStyles)
                    {
                        if (_sizeGripRenderer == null)
                        {
                            _sizeGripRenderer = new VisualStyleRenderer(VisualStyleElement.Status.Gripper.Normal);
                        }
                        _sizeGripRenderer.DrawBackground(g, new Rectangle(0, 0, 0x10, 0x10));
                    }
                    else
                    {
                        ControlPaint.DrawSizeGrip(g, Content.BackColor, 0, 0, 0x10, 0x10);
                    }
                }

                var gs = e.Graphics.Save();
                e.Graphics.ResetTransform();
                if (resizableTop)
                {
                    if (resizableLeft)
                    {
                        e.Graphics.RotateTransform(180);
                        e.Graphics.TranslateTransform(-clientSize.Width, -clientSize.Height);
                    }
                    else
                    {
                        e.Graphics.ScaleTransform(1, -1);
                        e.Graphics.TranslateTransform(0, -clientSize.Height);
                    }
                }
                else if (resizableLeft)
                {
                    e.Graphics.ScaleTransform(-1, 1);
                    e.Graphics.TranslateTransform(-clientSize.Width, 0);
                }

                e.Graphics.DrawImage(gripImage, clientSize.Width - 0x10, clientSize.Height - 0x10 + 1, 0x10, 0x10);
                e.Graphics.Restore(gs);
            }
        }

        #endregion
    }
}
