using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public abstract class Behavior : IDisposable
    {
        protected TextBoxBase _textBox;
        protected int _flags;
        protected bool _noTextChanged;
        protected Selection _selection;
        protected ErrorProvider _errorProvider;
        private static string _errorCaption;

        protected Behavior(TextBoxBase textBox, bool addEventHandlers)
        {
            if (textBox == null)
            {
                throw new ArgumentNullException("没有指定TextBox");
            }

            _textBox = textBox;
            _selection = new Selection(_textBox);
            _selection.TextChanging += new EventHandler(HandleTextChangingBySelection);

            if (addEventHandlers)
                AddEventHandlers();
        }

        protected Behavior(Behavior behavior)
        {
            if (behavior == null)
                throw new ArgumentNullException("没有指定Behavior");

            TextBox = behavior.TextBox;
            _flags = behavior._flags;

            behavior.Dispose();
        }

        private void HandleTextChangingBySelection(object sender, EventArgs e)
        {
            _noTextChanged = true;
        }

        protected virtual string GetValidText()
        {
            return _textBox.Text;
        }

        public virtual bool UpdateText()
        {
            string validText = GetValidText();
            if (validText != _textBox.Text)
            {
                _textBox.Text = validText;
                return true;
            }
            return false;
        }

        public TextBoxBase TextBox
        {
            get { return _textBox; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("没有指定Value");

                RemoveEventHandlers();

                _textBox = value;
                _selection = new Selection(_textBox);
                _selection.TextChanging += new EventHandler(HandleTextChangingBySelection);

                AddEventHandlers();
            }
        }

        /// <summary>
        /// 转换到整数。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected int ToInt(String text)
        {
            try
            {
                for (int i = 0, length = text.Length; i < length; i++)
                {
                    if (!Char.IsDigit(text[i]))
                        return Convert.ToInt32(text.Substring(0, i));
                }

                return Convert.ToInt32(text);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 转换到双精度。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected double ToDouble(String text)
        {
            double.TryParse(text, out double result);
            return result;
        }

        public virtual int Flags
        {
            get { return _flags; }
            set
            {
                if (_flags == value)
                    return;

                _flags = value;
                UpdateText();
            }
        }

        public void ModifyFlags(int flags, bool addOrRemove)
        {
            if (addOrRemove)
                Flags = _flags | flags;
            else
                Flags = _flags & ~flags;
        }

        public bool HasFlag(int flag)
        {
            return (_flags & flag) != 0;
        }

        /// <summary>
        /// 显示错误提示信息。
        /// </summary>
        /// <param name="message"></param>
        public virtual void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(_textBox, message, ErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// 显示错误提示信息。
        /// </summary>
        /// <param name="message"></param>
        public virtual void ShowErrorIcon(string message)
        {
            if (_errorProvider == null)
            {
                if (message == "")
                    return;
                _errorProvider = new ErrorProvider();
            }
            _errorProvider.SetError(_textBox, message);
        }

        /// <summary>
        /// 定义错误信息。
        /// </summary>
        public virtual string ErrorMessage
        {
            get
            {
                return "请输入有效的值";
            }
        }

        /// <summary>
        /// 错误信息的标题。
        /// </summary>
        public static string ErrorCaption
        {
            get
            {
                if (_errorCaption == null)
                    return Application.ProductName;
                return _errorCaption;
            }
            set { _errorCaption = value; }
        }

        [Conditional("TRACE_AMS")]
        public void TraceLine(string message)
        {
            Trace.WriteLine(message);
        }

        public bool Validate()
        {
            return Validate(Flags, false);
        }

        public virtual bool Validate(int flags, bool setFocusIfNotValid)
        {
            ShowErrorIcon("");

            if ((flags & (int)ValidatingFlag.Max) == 0)
                return true;

            if ((flags & (int)ValidatingFlag.Max_IfEmpty) != 0 && _textBox.Text.Length == 0)
            {
                if ((flags & (int)ValidatingFlag.Beep_IfEmpty) != 0)
                    NativeMethods.MessageBeep(MessageBoxIcon.Exclamation);

                if ((flags & (int)ValidatingFlag.SetValid_IfEmpty) != 0)
                {
                    UpdateText();
                    return true;
                }

                if ((flags & (int)ValidatingFlag.ShowIcon_IfEmpty) != 0)
                    ShowErrorIcon(ErrorMessage);

                if ((flags & (int)ValidatingFlag.ShowMessage_IfEmpty) != 0)
                    ShowErrorMessageBox(ErrorMessage);

                if (setFocusIfNotValid)
                    _textBox.Focus();

                return false;
            }

            if ((flags & (int)ValidatingFlag.Max_IfInvalid) != 0 && _textBox.Text.Length != 0 && !IsValid())
            {
                if ((flags & (int)ValidatingFlag.Beep_IfInvalid) != 0)
                    NativeMethods.MessageBeep(MessageBoxIcon.Exclamation);

                if ((flags & (int)ValidatingFlag.SetValid_IfInvalid) != 0)
                {
                    UpdateText();
                    return true;
                }

                if ((flags & (int)ValidatingFlag.ShowIcon_IfInvalid) != 0)
                    ShowErrorIcon(ErrorMessage);

                if ((flags & (int)ValidatingFlag.ShowMessage_IfInvalid) != 0)
                    ShowErrorMessageBox(ErrorMessage);

                if (setFocusIfNotValid)
                    _textBox.Focus();

                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取是否有效。
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return true;
        }

        /// <summary>
        /// 添加事件委托。
        /// </summary>
        protected virtual void AddEventHandlers()
        {
            _textBox.KeyDown += new KeyEventHandler(HandleKeyDown);
            _textBox.KeyPress += new KeyPressEventHandler(HandleKeyPress);
            _textBox.TextChanged += new EventHandler(HandleTextChanged);
            _textBox.Validating += new CancelEventHandler(HandleValidating);
            _textBox.LostFocus += new EventHandler(HandleLostFocus);
            _textBox.DataBindings.CollectionChanged += new CollectionChangeEventHandler(HandleBindingChanges);
        }

        /// <summary>
        /// 移除事件委托。
        /// </summary>
        protected virtual void RemoveEventHandlers()
        {
            if (_textBox == null)
                return;

            _textBox.KeyDown -= new KeyEventHandler(HandleKeyDown);
            _textBox.KeyPress -= new KeyPressEventHandler(HandleKeyPress);
            _textBox.TextChanged -= new EventHandler(HandleTextChanged);
            _textBox.Validating -= new CancelEventHandler(HandleValidating);
            _textBox.LostFocus -= new EventHandler(HandleLostFocus);
            _textBox.DataBindings.CollectionChanged -= new CollectionChangeEventHandler(HandleBindingChanges);
        }

        /// <summary>
        /// 销毁对象。
        /// </summary>
        public virtual void Dispose()
        {
            RemoveEventHandlers();
            _textBox = null;
        }

        protected virtual void HandleKeyDown(object sender, KeyEventArgs e)
        {
            TraceLine("Behavior.HandleKeyDown " + e.KeyCode);

            e.Handled = false;
        }

        protected virtual void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            TraceLine("Behavior.HandleKeyPress " + e.KeyChar);

            e.Handled = false;
        }

        protected virtual void HandleTextChanged(object sender, EventArgs e)
        {
            TraceLine("Behavior.HandleTextChanged " + _noTextChanged);

            if (!_noTextChanged)
                UpdateText();

            _noTextChanged = false;
        }

        protected virtual void HandleValidating(object sender, CancelEventArgs e)
        {
            TraceLine("Behavior.HandleValidating");

            e.Cancel = !Validate();
        }

        protected virtual void HandleLostFocus(object sender, EventArgs e)
        {
            TraceLine("Behavior.HandleLostFocus");
        }

        protected virtual void HandleBindingChanges(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Add)
            {
                Binding binding = (Binding)e.Element;
                binding.Format += new ConvertEventHandler(HandleBindingFormat);
                binding.Parse += new ConvertEventHandler(HandleBindingParse);
            }
        }

        protected virtual void HandleBindingFormat(object sender, ConvertEventArgs e)
        {
        }

        protected virtual void HandleBindingParse(object sender, ConvertEventArgs e)
        {
            if (e.Value.ToString() == "")
                e.Value = DBNull.Value;
        }
    }

    [Flags]
    public enum ValidatingFlag
    {
        /// <summary> 如果值无效则发出声响。 </summary>
        Beep_IfInvalid = 0x00000001,

        /// <summary> 如果值为空则发出声响。 </summary>
        Beep_IfEmpty = 0x00000002,

        /// <summary> 发出声响。 </summary>
        Beep = Beep_IfInvalid | Beep_IfEmpty,

        /// <summary> If the value is not valid, change its value to something valid. </summary>
        SetValid_IfInvalid = 0x00000004,

        /// <summary> If the value is empty, change its value to something valid. </summary>
        SetValid_IfEmpty = 0x00000008,

        /// <summary> If the value is empty or not valid, change its value to something valid. </summary>
        SetValid = SetValid_IfInvalid | SetValid_IfEmpty,

        /// <summary> If the value is not valid, show an error message box. </summary>
        ShowMessage_IfInvalid = 0x00000010,

        /// <summary> If the value is empty, show an error message box. </summary>
        ShowMessage_IfEmpty = 0x00000020,

        /// <summary> If the value is empty or not valid, show an error message box. </summary>
        ShowMessage = ShowMessage_IfInvalid | ShowMessage_IfEmpty,

        /// <summary> If the value is not valid, show a blinking icon next to it. </summary>
        ShowIcon_IfInvalid = 0x00000040,

        /// <summary> If the value is empty, show a blinking icon next to it. </summary>
        ShowIcon_IfEmpty = 0x00000080,

        /// <summary> If the value is empty or not valid, show a blinking icon next to it. </summary>
        ShowIcon = ShowIcon_IfInvalid | ShowIcon_IfEmpty,

        /// <summary> Combination of all IfInvalid flags (above); used internally by the program. </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Max_IfInvalid = Beep_IfInvalid | SetValid_IfInvalid | ShowMessage_IfInvalid | ShowIcon_IfInvalid,

        /// <summary> Combination of all IfEmpty flags (above); used internally by the program. </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Max_IfEmpty = Beep_IfEmpty | SetValid_IfEmpty | ShowMessage_IfEmpty | ShowIcon_IfEmpty,

        /// <summary> Combination of all flags; used internally by the program. </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Max = Max_IfInvalid + Max_IfEmpty
    };

}
