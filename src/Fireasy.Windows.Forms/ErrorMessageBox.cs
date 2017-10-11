using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public partial class ErrorMessageBox : Component
    {
        private string caption;
        private Exception exception;
        private IntPtr hMsgBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBox"/> class.
        /// </summary>
        protected ErrorMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBox"/> class with a container.
        /// </summary>
        /// <param name="container">Container of the component.</param>
        protected ErrorMessageBox(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        public static DialogResult Show(IWin32Window owner, string caption, Exception exception, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var dialog = new ErrorMessageBox())
            {
                return dialog.ShowDialog(owner, string.Empty, exception, caption, buttons, icon);
            }
        }

        public static DialogResult Show(string caption, Exception exception)
        {
            return Show(null, caption, exception, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult Show(IWin32Window owner, string caption, Exception exception)
        {
            return Show(owner, caption, exception, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        MessageBoxChildCollection childWindowList;

        /// <summary>
        /// Displays the message box in front of speicified window.
        /// </summary>
        /// <param name="owner">
        /// A implementation of <see cref="System.Windows.Forms.IWin32Window"/> that will own the modal dialog box.
        /// This parameter will be ignored when <see cref="System.Windows.Forms.MessageBoxOptions.ServiceNotification"/>
        /// option specified.
        /// </param>
        /// <returns></returns>
        public DialogResult ShowDialog(IWin32Window owner, string title, Exception exception, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            this.caption = caption;
            this.exception = exception;
            this.timer1.Interval = 10;
            this.timer1.Start();

            return global::System.Windows.Forms.MessageBox.Show(owner, title + exception.Message, caption, buttons, icon);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            hMsgBox = NativeMethods.FindWindow(null, caption);

            if (hMsgBox != IntPtr.Zero)
            {
                timer.Stop();
                DecorateMessageBox(hMsgBox);
            }
            else
            {
                timer.Stop();
            }
        }

        private void lnkShow_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBoxChild bn = this.childWindowList.GetButton(MessageBoxDefaultButton.Button1);
            NativeMethods.SendMessage(hMsgBox, NativeMethods.WM_COMMAND,
                NativeMethods.MakeWParam(bn.Id, NativeMethods.BN_CLICKED),
                bn.Handle);

            new MsgForm(exception).ShowDialog();
        }

        private void DecorateMessageBox(IntPtr hWndMsgBox)
        {
            if (this.childWindowList == null)
                this.childWindowList = new MessageBoxChildCollection();
            else
                this.childWindowList.Clear();

            NativeMethods.EnumChildWindows(hWndMsgBox, EnumChildren, IntPtr.Zero);

            if (this.childWindowList.Count > 0)
            {
                NativeMethods.POINT point = new NativeMethods.POINT();

                if (this.childWindowList.Buttons.Count > 0)
                {
                    var button = this.childWindowList.Buttons[0];
                    point.x = button.Rectangle.left;
                    point.y = button.Rectangle.top;

                    NativeMethods.ScreenToClient(hWndMsgBox, ref point);
                }

                NativeMethods.SetParent(this.lnkShow.Handle, hWndMsgBox);
                this.lnkShow.Location = new Point(20, point.y + 10);
            }
        }

        private bool EnumChildren(IntPtr hWnd, IntPtr param)
        {
            StringBuilder name = new StringBuilder(1024), caption = new StringBuilder(1024);
            NativeMethods.RECT rect = new NativeMethods.RECT();
            int style, id;

            NativeMethods.GetClassName(hWnd, name, 1024);
            NativeMethods.GetWindowText(hWnd, caption, 1024);
            NativeMethods.GetWindowRect(hWnd, ref rect);

            style = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_STYLE);
            id = NativeMethods.GetWindowLong(hWnd, NativeMethods.GWL_ID);

            this.childWindowList.Add(new MessageBoxChild(hWnd, name.ToString(), caption.ToString(), rect, style, id));

            return true;
        }

        #region Helper classes
        class MessageBoxChildCollection : List<MessageBoxChild>
        {
            public MessageBoxChildCollection()
                : base()
            {
                this.buttons = new List<MessageBoxChild>(4);
            }

            public new void Add(MessageBoxChild child)
            {
                switch (child.ClassName.ToUpper())
                {
                    case NativeMethods.CLS_BUTTON:
                        this.buttons.Add(child);
                        break;
                    case NativeMethods.CLS_STATIC:
                        if ((child.Style & NativeMethods.SS_ICON) == NativeMethods.SS_ICON)
                            this.icon = child;
                        else
                            this.text = child;
                        break;
                    default:
                        break;
                }

                base.Add(child);
            }

            public new bool Remove(MessageBoxChild child)
            {
                if (child.Equals(icon)) icon = null;
                else if (child.Equals(text)) text = null;

                buttons.Remove(child);

                return base.Remove(child);
            }

            public new void Clear()
            {
                this.icon = null;
                this.text = null;
                this.buttons.Clear();

                base.Clear();
            }

            private MessageBoxChild icon;
            public MessageBoxChild Icon
            {
                get { return icon; }
            }

            private MessageBoxChild text;
            public MessageBoxChild Text
            {
                get { return text; }
            }

            private List<MessageBoxChild> buttons;
            public List<MessageBoxChild> Buttons
            {
                get { return buttons; }
            }

            public MessageBoxChild GetButton(DialogResult result)
            {
                int id;

                switch (result)
                {
                    case DialogResult.Abort:
                        id = NativeMethods.IDABORT;
                        break;
                    case DialogResult.Cancel:
                        id = NativeMethods.IDCANCEL;
                        break;
                    case DialogResult.Ignore:
                        id = NativeMethods.IDIGNORE;
                        break;
                    case DialogResult.No:
                        id = NativeMethods.IDNO;
                        break;
                    case DialogResult.OK:
                        id = NativeMethods.IDOK;
                        break;
                    case DialogResult.Retry:
                        id = NativeMethods.IDRETRY;
                        break;
                    case DialogResult.Yes:
                        id = NativeMethods.IDYES;
                        break;
                    default:
                        id = 0;
                        break;
                }

                if (id > 0)
                {
                    foreach (MessageBoxChild bn in this.buttons)
                    {
                        if (bn.Id == id)
                            return bn;
                    }
                }

                return null;
            }

            public MessageBoxChild GetButton(MessageBoxDefaultButton button)
            {
                int index = 0;

                if (button == MessageBoxDefaultButton.Button2)
                    index = 1;
                else if (button == MessageBoxDefaultButton.Button3)
                    index = 2;

                if (index >= this.buttons.Count)
                    index = 0;

                return this.buttons[index];
            }

        }

        class MessageBoxChild : IWin32Window
        {

            public MessageBoxChild(IntPtr handle, string className, string caption, NativeMethods.RECT rect, int style, int id)
            {
                this.handle = handle;
                this.className = className;
                this.caption = caption;
                this.rectangle = rect;
                this.style = style;
                this.id = id;
            }

            private int id;
            public int Id
            {
                get { return id; }
            }

            private int style;
            public int Style
            {
                get { return style; }
            }


            private IntPtr handle;
            public IntPtr Handle
            {
                get { return handle; }
            }


            private string className;
            public string ClassName
            {
                get { return className; }
            }

            private string caption;
            public string Caption
            {
                get { return caption; }
            }

            private NativeMethods.RECT rectangle;
            public NativeMethods.RECT Rectangle
            {
                get { return rectangle; }
            }


            // override object.Equals
            public override bool Equals(object obj)
            {

                //       
                // See the full list of guidelines at
                //   http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconequals.asp    
                // and also the guidance for operator== at
                //   http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconimplementingequalsoperator.asp
                //

                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return ((MessageBoxChild)obj).Handle == this.Handle;
            }

            // override object.GetHashCode
            public override int GetHashCode()
            {
                return this.Handle.GetHashCode();
            }

            #region IWin32Window 成员

            IntPtr IWin32Window.Handle
            {
                get { return this.Handle; }
            }

            #endregion
        }

        #endregion

        private class MsgForm : Form
        {
            private TextBox textBox;

            public MsgForm(Exception exp)
            {
                Text = "详细错误信息";
                Width = 600;
                Height = 400;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                ShowIcon = false;
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

                textBox = new TextBox();
                textBox.Multiline = true;
                textBox.ReadOnly = true;
                textBox.ScrollBars = ScrollBars.Both;
                textBox.Parent = this;
                textBox.Dock = DockStyle.Fill;

                WriteErrorMessage(exp);
            }

            private void WriteErrorMessage(Exception exception)
            {
                var sb = new StringBuilder();
                sb.AppendLine(exception.Message);
                sb.AppendLine();

                if (exception != null)
                {
                    var e = exception;
                    sb.AppendLine("--异常--");
                    var ident = 0;
                    while (e != null)
                    {
                        sb.AppendLine(new string(' ', (ident++) * 2) + e.GetType().Name + " => " + e.Message);
                        e = e.InnerException;
                    }

                    if (exception.StackTrace != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine("--堆栈--");
                        sb.AppendLine(exception.StackTrace);
                    }
                }

                textBox.Text = sb.ToString();
            }
        }
    }
}
