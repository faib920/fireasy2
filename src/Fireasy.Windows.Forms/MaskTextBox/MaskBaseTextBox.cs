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
        /// �޸ı�־��
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="addOrRemove"></param>
        public void ModifyFlags(int flags, bool addOrRemove)
        {
            _behavior.ModifyFlags(flags, addOrRemove);
        }

        /// <summary>
        /// ��֤���ݡ�
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            return _behavior.Validate();
        }

        /// <summary>
        /// ��ȡ�Ƿ���Ч��
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return _behavior.IsValid();
        }

        /// <summary>
        /// ��������Ի���
        /// </summary>
        /// <param name="message"></param>
        public void ShowErrorMessageBox(string message)
        {
            _behavior.ShowErrorMessageBox(message);
        }

        /// <summary>
        /// ��ʾ����ͼ�ꡣ
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
