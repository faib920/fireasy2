using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(false),
    DesignTimeVisible(false),
    DefaultProperty("Text")]
    public abstract class MaskBaseTextBox : TextBox
    {
        protected Behavior _behavior = null;

        protected MaskBaseTextBox()
        {
        }

        internal MaskBaseTextBox(Behavior behavior)
        {
            _behavior = behavior;
        }

        public bool UpdateText()
        {
            return _behavior.UpdateText();
        }

        [Category("Behavior")]
        [Description("")]
        public int Flags
        {
            get { return _behavior.Flags; }
            set { _behavior.Flags = value; }
        }

        /// <summary>
        /// 修改标志。
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="addOrRemove"></param>
        public void ModifyFlags(int flags, bool addOrRemove)
        {
            _behavior.ModifyFlags(flags, addOrRemove);
        }

        /// <summary>
        /// 验证数据。
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return _behavior.Validate();
        }

        /// <summary>
        /// 获取是否有效。
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return _behavior.IsValid();
        }

        /// <summary>
        /// 弹出错误对话框。
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessageBox(string message)
        {
            _behavior.ShowErrorMessageBox(message);
        }

        /// <summary>
        /// 显示错误图标。
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorIcon(string message)
        {
            _behavior.ShowErrorIcon(message);
        }

        [Browsable(false)]
        public string ErrorMessage
        {
            get { return _behavior.ErrorMessage; }
        }

        internal class Designer : ControlDesigner
        {
        }
    }
}
