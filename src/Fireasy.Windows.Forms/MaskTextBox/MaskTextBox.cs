using System;
using System.ComponentModel;
using System.Collections;
using System.Text;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the Masked behavior.")]
    public class MaskTextBox : MaskBaseTextBox
    {
        public MaskTextBox()
        {
            m_behavior = new MaskBehavior(this);
        }

        public MaskTextBox(string mask)
        {
            m_behavior = new MaskBehavior(this, mask);
        }

        public MaskTextBox(MaskBehavior behavior)
            :
            base(behavior)
        {
        }

        [Browsable(false)]
        public MaskBehavior Behavior
        {
            get
            {
                return (MaskBehavior)m_behavior;
            }
        }

        [Category("Behavior")]
        [Description("The string used for formatting the characters entered into the textbox. (# = digit)")]
        public string Mask
        {
            get
            {
                return Behavior.Mask;
            }
            set
            {
                Behavior.Mask = value;
            }
        }

        [Browsable(false)]
        public ArrayList Symbols
        {
            get
            {
                return Behavior.Symbols;
            }
        }

        [Browsable(false)]
        public string NumericText
        {
            get
            {
                return Behavior.NumericText;
            }
        }
    }
}
