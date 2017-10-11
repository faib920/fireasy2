using System;
using System.ComponentModel;
using System.Text;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the Integer behavior.")]
    public class IntegerTextBox : NumericTextBox
    {
        public IntegerTextBox()
            :
            base(null)
        {
            m_behavior = new IntegerBehavior(this);
        }

        public IntegerTextBox(int maxWholeDigits)
            :
            base(null)
        {
            m_behavior = new IntegerBehavior(this, maxWholeDigits);
        }

        public IntegerTextBox(IntegerBehavior behavior)
            :
            base(behavior)
        {
        }
    }


}
