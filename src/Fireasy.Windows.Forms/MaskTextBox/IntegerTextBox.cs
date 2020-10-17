using System.ComponentModel;

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
            _behavior = new IntegerBehavior(this);
        }

        public IntegerTextBox(int maxWholeDigits)
            :
            base(null)
        {
            _behavior = new IntegerBehavior(this, maxWholeDigits);
        }

        public IntegerTextBox(IntegerBehavior behavior)
            :
            base(behavior)
        {
        }
    }


}
