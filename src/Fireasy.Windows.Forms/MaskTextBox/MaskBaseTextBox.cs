using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(false),
    DesignTimeVisible(false),
    DefaultProperty("Text")]
    public abstract class MaskBaseTextBox : TextBox
    {
        protected Behavior m_behavior = null;

        protected MaskBaseTextBox()
        {
        }

        internal MaskBaseTextBox(Behavior behavior)
        {
            m_behavior = behavior;
        }

        public bool UpdateText()
        {
            return m_behavior.UpdateText();
        }

        [Category("Behavior")]
        [Description("")]
        public int Flags
        {
            get { return m_behavior.Flags; }
            set { m_behavior.Flags = value; }
        }

        /// <summary>
        /// 修改标志。
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="addOrRemove"></param>
        public void ModifyFlags(int flags, bool addOrRemove)
        {
            m_behavior.ModifyFlags(flags, addOrRemove);
        }

        /// <summary>
        /// 验证数据。
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return m_behavior.Validate();
        }

        /// <summary>
        /// 获取是否有效。
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return m_behavior.IsValid();
        }

        /// <summary>
        /// 弹出错误对话框。
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessageBox(string message)
        {
            m_behavior.ShowErrorMessageBox(message);
        }

        /// <summary>
        /// 显示错误图标。
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorIcon(string message)
        {
            m_behavior.ShowErrorIcon(message);
        }

        [Browsable(false)]
        public string ErrorMessage
        {
            get { return m_behavior.ErrorMessage; }
        }

        internal class Designer : ControlDesigner
        {
        }
    }
}
