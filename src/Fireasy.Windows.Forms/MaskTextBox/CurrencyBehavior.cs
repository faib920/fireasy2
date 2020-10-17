using System;
using System.Globalization;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    #region CurrencyBehavior
    public class CurrencyBehavior : NumericBehavior
    {
        /// <summary>
        ///   Initializes a new instance of the CurrencyBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets <see cref="NumericBehavior.MaxWholeDigits" /> = 9, <see cref="NumericBehavior.AllowNegative" /> = true, 
        ///   and the rest of the properties according to user's system. If the system has the 
        ///   currency symbol configured to be placed in front of the value, then it is assigned to the <see cref="NumericBehavior.Prefix" />.
        ///   Also, the number is automatically padded with zeros after the <see cref="NumericBehavior.DecimalPoint" /> when the textbox
        ///   loses focus. </remarks>
        public CurrencyBehavior(TextBoxBase textBox)
            :
            base(textBox)
        {
            _flags |= ((int)LostFocusFlag.RemoveExtraLeadingZeros |
                        (int)LostFocusFlag.PadWithZerosAfterDecimal |
                        (int)LostFocusFlag.DontPadWithZerosIfEmpty |
                        (int)LostFocusFlag.CallHandlerWhenTextPropertyIsSet);

            // Get the system's current settings
            DigitsInGroup = NumberFormatInfo.CurrentInfo.CurrencyGroupSizes[0];
            MaxDecimalPlaces = NumberFormatInfo.CurrentInfo.CurrencyDecimalDigits;
            DecimalPoint = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator[0];
            GroupSeparator = NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator[0];

            // Determine how the currency symbol should be shown for the prefix
            switch (NumberFormatInfo.CurrentInfo.CurrencyPositivePattern)
            {
                case 0:		// Prefix, no separation
                    Prefix = NumberFormatInfo.CurrentInfo.CurrencySymbol;
                    break;
                case 2:		// Prefix, one space separation
                    Prefix = NumberFormatInfo.CurrentInfo.CurrencySymbol + ' ';
                    break;

                    // The rest are suffixes, so no prefix
            }

            AdjustDecimalAndGroupSeparators();
        }

        /// <summary>
        ///   Initializes a new instance of the CurrencyBehavior class by copying it from 
        ///   another CurrencyBehavior object. </summary>
        /// <param name="behavior">
        ///   The CurrencyBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public CurrencyBehavior(CurrencyBehavior behavior)
            :
            base(behavior)
        {
        }
    }
    #endregion

}
