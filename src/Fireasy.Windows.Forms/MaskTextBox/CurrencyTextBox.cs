using System.Collections;
using System.ComponentModel;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the Currency behavior.")]
    [Designer(typeof(CurrencyTextBox.Designer))]
    public class CurrencyTextBox : NumericTextBox
    {
        public CurrencyTextBox()
            :
            base(null)
        {
            _behavior = new CurrencyBehavior(this);
            TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        }

        internal CurrencyTextBox(CurrencyBehavior behavior)
            :
            base(behavior)
        {
            TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        }

        internal new class Designer : NumericTextBox.Designer
        {
            protected override void PostFilterProperties(IDictionary properties)
            {
                properties.Remove("DigitsInGroup");
                properties.Remove("Prefix");
                properties.Remove("MaxDecimalPlaces");

                base.PostFilterProperties(properties);
            }
        }
    }
}
