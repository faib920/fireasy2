using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Globalization;
using System.Diagnostics;
using System.Text;

namespace Fireasy.Windows.Forms
{

    public class NumericBehavior : Behavior
    {
        // Fields
        private int m_maxWholeDigits = 9;
        private int m_maxDecimalPlaces = 4;
        private int m_digitsInGroup = 0;
        private char m_negativeSign = NumberFormatInfo.CurrentInfo.NegativeSign[0];
        private char m_decimalPoint = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator[0];
        private char m_groupSeparator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator[0];
        private string m_prefix = "";
        private double m_rangeMin = Double.MinValue;
        private double m_rangeMax = Double.MaxValue;
        private double m_defaultValue = 0;

        private int m_previousSeparatorCount = -1;
        private bool m_textChangedByKeystroke = false;

        /// <summary>
        ///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
        ///   properties of this class. </summary>
        [Flags]
        protected enum Flag
        {
            /// <summary> 不允许输入负号。 </summary>
            CannotBeNegative = 0x00010000,

            /// <summary> If the user enters a digit after the <see cref="MaxWholeDigits" /> have been entered, a <see cref="DecimalPoint" /> is inserted and then the digit. </summary>
            AddDecimalAfterMaxWholeDigits = 0x00020000
        };

        /// <summary>
        ///   Values that may be added/removed to the <see cref="Behavior.Flags" /> property related to 
        ///   what occurs when the textbox loses focus. </summary>
        /// <seealso cref="Behavior.ModifyFlags" />
        /// <seealso cref="Behavior.HasFlag" />
        [Flags]
        public enum LostFocusFlag
        {
            /// <summary> When the textbox loses focus, pad the value with up to <see cref="MaxWholeDigits" /> zeros left of the decimal symbol. </summary>
            PadWithZerosBeforeDecimal = 0x00000100,

            /// <summary> When the textbox loses focus, pad the value with up to <see cref="MaxDecimalPlaces" /> zeros right of the decimal symbol. </summary>
            PadWithZerosAfterDecimal = 0x00000200,

            /// <summary> When combined with the <see cref="PadWithZerosBeforeDecimal" /> or <see cref="PadWithZerosAfterDecimal" />, the padding is only done if the textbox is not empty. </summary>
            DontPadWithZerosIfEmpty = 0x00000400,

            /// <summary> Insignificant zeros are removed from the numeric value left of the decimal point, unless the number itself is 0. </summary>
            RemoveExtraLeadingZeros = 0x00000800,

            /// <summary> Combination of all the above flags; used internally by the program. </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            Max = 0x00000F00,

            /// <summary> If the Text property is set, the LostFocus handler is called. </summary>
            CallHandlerWhenTextPropertyIsSet = 0x00001000,

            /// <summary> If the text changes, the LostFocus handler is called. </summary>
            CallHandlerWhenTextChanges = 0x00002000
        };

        /// <summary>
        ///   Initializes a new instance of the NumericBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets <see cref="MaxWholeDigits" /> = 9, <see cref="MaxDecimalPlaces" /> = 4, 
        ///   <see cref="DigitsInGroup" /> = 0, <see cref="Prefix" /> = "", <see cref="AllowNegative" /> = true, 
        ///   and the rest of the properties according to user's system. </remarks>
        public NumericBehavior(TextBoxBase textBox)
            :
            base(textBox, true)
        {
            AdjustDecimalAndGroupSeparators();
        }

        /// <summary>
        ///   Initializes a new instance of the NumericBehavior class by associating it with a TextBoxBase derived object
        ///   and setting the maximum number of digits allowed left and right of the decimal point. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <param name="maxWholeDigits">
        ///   The maximum number of digits allowed left of the decimal point.
        ///   If it is less than 1, it is set to 1. </param>
        /// <param name="maxDecimalPlaces">
        ///   The maximum number of digits allowed right of the decimal point.
        ///   If it is less than 0, it is set to 0. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets <see cref="DigitsInGroup" /> = 0, <see cref="Prefix" /> = "", <see cref="AllowNegative" /> = true, 
        ///   and the rest of the properties according to user's system. </remarks>
        /// <seealso cref="MaxWholeDigits" />
        /// <seealso cref="MaxDecimalPlaces" />
        public NumericBehavior(TextBoxBase textBox, int maxWholeDigits, int maxDecimalPlaces)
            :
            this(textBox)
        {
            m_maxWholeDigits = maxWholeDigits;
            m_maxDecimalPlaces = maxDecimalPlaces;

            if (m_maxWholeDigits < 1)
                m_maxWholeDigits = 1;
            if (m_maxDecimalPlaces < 0)
                m_maxDecimalPlaces = 0;
        }

        /// <summary>
        ///   Initializes a new instance of the NumericBehavior class by associating it with a TextBoxBase derived object
        ///   and assiging its attributes from a mask string. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <param name="mask">
        ///   The string used to set several of the object's properties. 
        ///   See <see cref="Mask" /> for more information. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets <see cref="AllowNegative" /> = true
        ///   and the rest of the properties using the mask. </remarks>
        /// <seealso cref="Mask" />
        public NumericBehavior(TextBoxBase textBox, string mask)
            :
            base(textBox, true)
        {
            Mask = mask;
        }

        /// <summary>
        ///   Initializes a new instance of the NumericBehavior class by copying it from 
        ///   another NumericBehavior object. </summary>
        /// <param name="behavior">
        ///   The NumericBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public NumericBehavior(NumericBehavior behavior)
            :
            base(behavior)
        {
            m_maxWholeDigits = behavior.m_maxWholeDigits;
            m_maxDecimalPlaces = behavior.m_maxDecimalPlaces;
            m_digitsInGroup = behavior.m_digitsInGroup;
            m_negativeSign = behavior.m_negativeSign;
            m_decimalPoint = behavior.m_decimalPoint;
            m_groupSeparator = behavior.m_groupSeparator;
            m_prefix = behavior.m_prefix;
            m_rangeMin = behavior.m_rangeMin;
            m_rangeMax = behavior.m_rangeMax;
        }

        /// <summary>
        ///   Gets or sets the maximum number of digits allowed left of the decimal point. </summary>
        /// <remarks>
        ///   If this property is set to a number less than 1, it is set to 1. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="MaxDecimalPlaces" />
        public int MaxWholeDigits
        {
            get
            {
                return m_maxWholeDigits;
            }
            set
            {
                if (m_maxWholeDigits == value)
                    return;

                m_maxWholeDigits = value;
                if (m_maxWholeDigits < 1)
                    m_maxWholeDigits = 1;

                UpdateText();
            }
        }

        public double DefaultValue
        {
            get { return m_defaultValue; }
            set { m_defaultValue = value; }
        }

        /// <summary>
        ///   Gets or sets the maximum number of digits allowed right of the decimal point. </summary>
        /// <remarks>
        ///   If this property is set to a number less than 0, it is set to 0. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="MaxWholeDigits" />
        public int MaxDecimalPlaces
        {
            get
            {
                return m_maxDecimalPlaces;
            }
            set
            {
                if (m_maxDecimalPlaces == value)
                    return;

                m_maxDecimalPlaces = value;
                if (m_maxDecimalPlaces < 0)
                    m_maxDecimalPlaces = 0;

                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets whether the value is allowed to be negative or not. </summary>
        /// <remarks>
        ///   By default, this property is set to true, meaning that negative values are allowed. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="NegativeSign" />
        public bool AllowNegative
        {
            get
            {
                return !HasFlag((int)Flag.CannotBeNegative);
            }
            set
            {
                ModifyFlags((int)Flag.CannotBeNegative, !value);
            }
        }

        /// <summary>
        ///   Gets or sets whether a <see cref="DecimalPoint" /> is automatically inserted if the user enters a digit 
        ///   after the <see cref="MaxWholeDigits" /> have been entered </summary>
        /// <remarks>
        ///   By default, this property is set to false, meaning that when the <see cref="MaxWholeDigits" /> have
        ///   been entered, a <see cref="DecimalPoint" /> is not automaticall inserted -- the user has to do it manually. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="NegativeSign" />
        public bool AddDecimalAfterMaxWholeDigits
        {
            get
            {
                return HasFlag((int)Flag.AddDecimalAfterMaxWholeDigits);
            }
            set
            {
                ModifyFlags((int)Flag.AddDecimalAfterMaxWholeDigits, value);
            }
        }

        /// <summary>
        ///   Gets or sets the number of digits to place in each group to the left of the decimal point. </summary>
        /// <remarks>
        ///   By default, this property is set to 0. It may be set to 3 to group thousands using the <see cref="GroupSeparator">group separator</see>. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="GroupSeparator" />
        public int DigitsInGroup
        {
            get
            {
                return m_digitsInGroup;
            }
            set
            {
                if (m_digitsInGroup == value)
                    return;

                m_digitsInGroup = value;
                if (m_digitsInGroup < 0)
                    m_digitsInGroup = 0;

                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets the character to use for the decimal point. </summary>
        /// <remarks>
        ///   By default, this property is set based on the user's system settings. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="GroupSeparator" />
        public char DecimalPoint
        {
            get
            {
                return m_decimalPoint;
            }
            set
            {
                if (m_decimalPoint == value)
                    return;

                m_decimalPoint = value;
                AdjustDecimalAndGroupSeparators();
                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets the character to use for the group separator. </summary>
        /// <remarks>
        ///   By default, this property is set based on the user's system settings. 
        ///   In the US, this is typically a comma and it is used to separate the thousands. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="GroupSeparator" />
        public char GroupSeparator
        {
            get
            {
                return m_groupSeparator;
            }
            set
            {
                if (m_groupSeparator == value)
                    return;

                m_groupSeparator = value;
                AdjustDecimalAndGroupSeparators();
                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets the character to use for the negative sign. </summary>
        /// <remarks>
        ///   By default, this property is set based on the user's system settings, but it will likely be a minus sign. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="AllowNegative" />
        public char NegativeSign
        {
            get
            {
                return m_negativeSign;
            }
            set
            {
                if (m_negativeSign == value)
                    return;

                m_negativeSign = value;
                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets the text to automatically insert in front of the number, such as a currency symbol. </summary>
        /// <remarks>
        ///   By default, this property is set to an empty string. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        public String Prefix
        {
            get
            {
                return m_prefix;
            }
            set
            {
                if (m_prefix == value)
                    return;

                m_prefix = value;
                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets the minimum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to <see cref="Double.MinValue" />, however the range is 
        ///   only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMax" />
        public double RangeMin
        {
            get
            {
                return m_rangeMin;
            }
            set
            {
                m_rangeMin = value;
            }
        }

        /// <summary>
        ///   Gets or sets the maximum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to <see cref="Double.MaxValue" />, however the range is 
        ///   only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMin" />
        public double RangeMax
        {
            get
            {
                return m_rangeMax;
            }
            set
            {
                m_rangeMax = value;
            }
        }

        /// <summary>
        ///   If necessary, adjusts the decimal and group separators so they're not the same. </summary>
        /// <remarks>
        ///   If the decimal and group separators are found to be same, they are adjusted to be different. 
        ///   This prevents potential problems as the user is entering the value. </remarks>	
        protected void AdjustDecimalAndGroupSeparators()
        {
            if (m_decimalPoint == m_groupSeparator)
                m_groupSeparator = (m_decimalPoint == ',' ? '.' : ',');
        }

        /// <summary>
        ///   Gets or sets a mask value representative of this object's properties. </summary>
        /// <remarks>
        ///   This property can be set to configure these other properties: <see cref="MaxWholeDigits" />,
        ///   <see cref="MaxDecimalPlaces" />, <see cref="DigitsInGroup" />, <see cref="Prefix" />, <see cref="DecimalPoint" />, 
        ///   and <see cref="GroupSeparator" />.
        ///   <para>
        ///   For example, <c>"$ #,###.##"</c> means MaxWholeDigits = 4, MaxDecimalPlaces = 2, DigitsInGroup = 3, 
        ///   Prefix = "$ ", DecimalPoint = '.', and GroupSeparator = ','. </para>
        ///   <para>
        ///   The # character is used to denote a digit placeholder. </para> </remarks>
        public string Mask
        {
            get
            {
                StringBuilder mask = new StringBuilder();

                for (int iDigit = 0; iDigit < m_maxWholeDigits; iDigit++)
                    mask.Append('0');

                if (m_maxDecimalPlaces > 0)
                    mask.Append(m_decimalPoint);

                for (int iDigit = 0; iDigit < m_maxDecimalPlaces; iDigit++)
                    mask.Append('0');

                mask = new StringBuilder(GetSeparatedText(mask.ToString()));

                for (int iPos = 0, length = mask.Length; iPos < length; iPos++)
                {
                    if (mask[iPos] == '0')
                        mask[iPos] = '#';
                }

                return mask.ToString();
            }
            set
            {
                int decimalPos = -1;
                int length = value.Length;

                m_maxWholeDigits = 0;
                m_maxDecimalPlaces = 0;
                m_digitsInGroup = 0;
                m_flags = (m_flags & (int)~Flag.CannotBeNegative);  // allow it to be negative
                m_prefix = "";

                for (int iPos = length - 1; iPos >= 0; iPos--)
                {
                    char c = value[iPos];
                    if (c == '#')
                    {
                        if (decimalPos >= 0)
                            m_maxWholeDigits++;
                        else
                            m_maxDecimalPlaces++;
                    }
                    else if ((c == '.' || c == m_decimalPoint) && decimalPos < 0)
                    {
                        decimalPos = iPos;
                        m_decimalPoint = c;
                    }
                    else if (c == ',' || c == m_groupSeparator)
                    {
                        if (m_digitsInGroup == 0)
                        {
                            m_digitsInGroup = (((decimalPos >= 0) ? decimalPos : length) - iPos) - 1;
                            m_groupSeparator = c;
                        }
                    }
                    else
                    {
                        m_prefix = value.Substring(0, iPos + 1);
                        break;
                    }
                }

                if (decimalPos < 0)
                {
                    m_maxWholeDigits = m_maxDecimalPlaces;
                    m_maxDecimalPlaces = 0;
                }

                Debug.Assert(m_maxWholeDigits > 0);	// must have at least one digit on left side of decimal point

                AdjustDecimalAndGroupSeparators();
                UpdateText();
            }
        }

        /// <summary>
        ///   Copies a string while inserting zeros into it. </summary>
        /// <param name="text">
        ///   The text to copy with the zeros inserted. </param>
        /// <param name="startIndex">
        ///   The zero-based position where the zeros should be inserted. 
        ///   If this is less than 0, the zeros are appended. </param>
        /// <param name="count">
        ///   The number of zeros to insert. </param>
        /// <returns>
        ///   The return value is a copy of the text with the zeros inserted. </returns>
        protected string InsertZeros(string text, int startIndex, int count)
        {
            if (startIndex < 0 && count > 0)
                startIndex = text.Length;

            StringBuilder result = new StringBuilder(text);
            for (int iZero = 0; iZero < count; iZero++)
                result.Insert(startIndex, '0');

            return result.ToString();
        }

        /// <summary>
        ///   Checks if the textbox's numeric value is within the allowed range. </summary>
        /// <returns>
        ///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        public override bool IsValid()
        {
            double value = ToDouble(RealNumericText);
            return (value >= m_rangeMin && value <= m_rangeMax);
        }

        /// <summary>
        ///   Gets the error message used to notify the user to enter a valid numeric value 
        ///   within the allowed range. </summary>
        /// <seealso cref="IsValid" />
        public override string ErrorMessage
        {
            get
            {
                if (m_rangeMin > double.MinValue && m_rangeMax < double.MaxValue)
                    return "Please specify a numeric value between " + m_rangeMin.ToString() + " and " + m_rangeMax.ToString() + ".";
                else if (m_rangeMin > double.MinValue)
                    return "Please specify a numeric value greater than or equal to " + m_rangeMin.ToString() + ".";
                else if (m_rangeMax < double.MinValue)
                    return "Please specify a numeric value less than or equal to " + m_rangeMax.ToString() + ".";
                return "Please specify a valid numeric value.";
            }
        }

        /// <summary>
        ///   Adjusts the textbox's value to be within the range of allowed values. </summary>
        protected void AdjustWithinRange()
        {
            // Check if it's already within the range
            if (IsValid())
                return;

            // If it's empty, set it a valid number
            if (m_textBox.Text == "")
                m_textBox.Text = " ";
            else
                UpdateText();

            // Make it fall within the range
            double value = ToDouble(RealNumericText);
            if (value < m_rangeMin)
                m_textBox.Text = m_rangeMin.ToString();
            else if (value > m_rangeMax)
                m_textBox.Text = m_rangeMax.ToString();
        }

        /// <summary>
        ///   Retrieves the textbox's value without any non-numeric characters. </summary>
        /// <seealso cref="RealNumericText" />
        public string NumericText
        {
            get
            {
                return GetNumericText(m_textBox.Text, false);
            }
        }

        /// <summary>
        ///   Retrieves the textbox's value without any non-numeric characters,
        ///   and with a period for the decimal point and a minus for the negative sign. </summary>
        /// <seealso cref="NumericText" />
        public string RealNumericText
        {
            get
            {
                return GetNumericText(m_textBox.Text, true);
            }
        }

        /// <summary>
        ///   Copies a string while removing any non-numeric characters from it. </summary>
        /// <param name="text">
        ///   The text to parse and copy. </param>
        /// <param name="realNumeric">
        ///   If true, the value is returned as a real number 
        ///   (with a period for the decimal point and a minus for the negative sign);
        ///   otherwise, it is returned using the expected symbols. </param>
        /// <returns>
        ///   The return value is a copy of the original text containing only numeric characters. </returns>
        protected string GetNumericText(string text, bool realNumeric)
        {
            StringBuilder numericText = new StringBuilder();
            bool isNegative = false;
            bool hasDecimalPoint = false;

            foreach (char c in text)
            {
                if (Char.IsDigit(c))
                    numericText.Append(c);
                else if (c == m_negativeSign)
                    isNegative = true;
                else if (c == m_decimalPoint && !hasDecimalPoint)
                {
                    hasDecimalPoint = true;
                    numericText.Append(realNumeric ? '.' : m_decimalPoint);
                }
            }

            // Add the negative sign to the front of the number.
            if (isNegative)
                numericText.Insert(0, realNumeric ? '-' : m_negativeSign);

            return numericText.ToString();
        }

        /// <summary>
        ///   Returns the number of group separator characters in the given text. </summary>
        private int GetGroupSeparatorCount(string text)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == m_groupSeparator)
                    count++;
            }
            return count;
        }

        /// <summary>
        ///   Retrieves the textbox's text in valid form. </summary>
        /// <returns>
        ///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
        protected override string GetValidText()
        {
            string text = m_textBox.Text;
            StringBuilder newText = new StringBuilder();
            bool isNegative = false;
            int prefixLength = m_prefix.Length;

            // Remove any invalid characters from the number
            for (int iPos = 0, decimalPos = -1, newLength = 0, length = text.Length; iPos < length; iPos++)
            {
                char c = text[iPos];

                // Check for a negative sign
                if (c == m_negativeSign && AllowNegative)
                    isNegative = true;

                // Check for a digit
                else if (Char.IsDigit(c))
                {
                    // Make sure it doesn't go beyond the limits
                    if (decimalPos < 0 && newLength == m_maxWholeDigits)
                        continue;

                    if (decimalPos >= 0 && newLength > decimalPos + m_maxDecimalPlaces)
                        break;

                    newText.Append(c);
                    newLength++;
                }

                // Check for a decimal point
                else if (c == m_decimalPoint && decimalPos < 0)
                {
                    if (m_maxDecimalPlaces == 0)
                        break;

                    newText.Append(c);
                    decimalPos = newLength;
                    newLength++;
                }
            }

            // Insert the negative sign if it's there
            if (isNegative)
                newText.Insert(0, m_negativeSign);

            return GetSeparatedText(newText.ToString());
        }

        /// <summary>
        ///   Takes a piece of text containing a numeric value and inserts
        ///   group separators in the proper places. </summary>
        /// <param name="text">
        ///   The text to parse. </param>
        /// <returns>
        ///   The return value is a copy of the original text with the group separators inserted. </returns>
        protected string GetSeparatedText(string text)
        {
            string numericText = GetNumericText(text, false);
            string separatedText = numericText;

            // Retrieve the number without the decimal point
            int decimalPos = numericText.IndexOf(m_decimalPoint);
            if (decimalPos >= 0)
                separatedText = separatedText.Substring(0, decimalPos);

            if (m_digitsInGroup > 0)
            {
                int length = separatedText.Length;
                bool isNegative = (separatedText != "" && separatedText[0] == m_negativeSign);

                // Loop in reverse and stick the separator every m_digitsInGroup digits.
                for (int iPos = length - (m_digitsInGroup + 1); iPos >= (isNegative ? 1 : 0); iPos -= m_digitsInGroup)
                    separatedText = separatedText.Substring(0, iPos + 1) + m_groupSeparator + separatedText.Substring(iPos + 1);
            }

            // Prepend the prefix, if the number is not empty.
            if (separatedText != "" || decimalPos >= 0)
            {
                separatedText = m_prefix + separatedText;

                if (decimalPos >= 0)
                    separatedText += numericText.Substring(decimalPos);
            }

            return separatedText;
        }

        /// <summary>
        ///   Handles keyboard presses inside the textbox. </summary>
        /// <param name="sender">
        ///   The object who sent the event. </param>
        /// <param name="e">
        ///   The event data. </param>
        /// <remarks>
        ///   This method is overriden from the Behavior class and it  
        ///   handles the textbox's KeyDown event. </remarks>
        /// <seealso cref="Control.KeyDown" />
        protected override void HandleKeyDown(object sender, KeyEventArgs e)
        {
            TraceLine("NumericBehavior.HandleKeyDown " + e.KeyCode);

            if (e.KeyCode == Keys.Delete)
            {
                int start, end;
                m_selection.Get(out start, out end);

                string text = m_textBox.Text;
                int length = text.Length;

                // If deleting the prefix, don't allow it if there's a number after it.
                int prefixLength = m_prefix.Length;
                if (start < prefixLength && length > prefixLength)
                {
                    if (end != length)
                        e.Handled = true;
                    return;
                }

                m_textChangedByKeystroke = true;

                // If deleting a group separator (comma), move the cursor to the right
                if (start < length && text[start] == m_groupSeparator && start == end)
                    SendKeys.SendWait("{RIGHT}");

                m_previousSeparatorCount = GetGroupSeparatorCount(text);

                // If everything on the right was deleted, put the selection on the right
                if (end == length)
                    SendKeys.Send("{RIGHT}");
            }
        }

        /// <summary>
        ///   Handles keyboard presses inside the textbox. </summary>
        /// <param name="sender">
        ///   The object who sent the event. </param>
        /// <param name="e">
        ///   The event data. </param>
        /// <remarks>
        ///   This method is overriden from the Behavior class and it  
        ///   handles the textbox's KeyPress event. </remarks>
        /// <seealso cref="Control.KeyPress" />
        protected override void HandleKeyPress(object sender, KeyPressEventArgs e)
        {
            TraceLine("NumericBehavior.HandleKeyPress " + e.KeyChar);

            // Check to see if it's read only
            if (m_textBox.ReadOnly)
                return;

            char c = e.KeyChar;
            e.Handled = true;
            m_textChangedByKeystroke = true;

            int start, end;
            m_selection.Get(out start, out end);

            string text = m_textBox.Text;
            m_previousSeparatorCount = -1;

            string numericText = NumericText;
            int decimalPos = text.IndexOf(m_decimalPoint);
            int numericDecimalPos = numericText.IndexOf(m_decimalPoint);
            int length = text.Length;
            int numericLen = numericText.Length;
            int prefixLength = m_prefix.Length;
            int separatorCount = GetGroupSeparatorCount(text);

            // Check if we're in the prefix's location
            if (start < prefixLength && !Char.IsControl(c))
            {
                char cPrefix = m_prefix[start];

                // Check if it's the same character as the prefix.
                if (cPrefix == c)
                {
                    if (length > start)
                    {
                        end = (end == length ? end : (start + 1));
                        m_selection.SetAndReplace(start, end, c.ToString());
                    }
                    else
                        base.HandleKeyPress(sender, e);
                }
                // If it's a part of the number, enter the prefix
                else if (Char.IsDigit(c) || c == m_negativeSign || c == m_decimalPoint)
                {
                    end = (end == length ? end : prefixLength);
                    m_selection.SetAndReplace(start, end, m_prefix.Substring(start));
                    HandleKeyPress(sender, e);
                }

                return;
            }

            // Check if it's a negative sign
            if (c == m_negativeSign && AllowNegative)
            {
                // If it's at the beginning, determine if it should overwritten
                if (start == prefixLength)
                {
                    if (numericText != "" && numericText[0] == m_negativeSign)
                    {
                        end = (end == length ? end : (start + 1));
                        m_selection.SetAndReplace(start, end, m_negativeSign.ToString());
                        return;
                    }
                }
                // If we're not at the beginning, toggle the sign
                else
                {
                    if (numericText[0] == m_negativeSign)
                    {
                        m_selection.SetAndReplace(prefixLength, prefixLength + 1, "");
                        m_selection.Set(start - 1, end - 1);
                    }
                    else
                    {
                        m_selection.SetAndReplace(prefixLength, prefixLength, m_negativeSign.ToString());
                        m_selection.Set(start + 1, end + 1);
                    }

                    return;
                }
            }

            // Check if it's a decimal point (only one is allowed).
            else if (c == m_decimalPoint && m_maxDecimalPlaces > 0)
            {
                if (decimalPos >= 0)
                {
                    // Check if we're replacing the decimal point
                    if (decimalPos >= start && decimalPos < end)
                        m_previousSeparatorCount = separatorCount;
                    else
                    {	// Otherwise, put the caret on it
                        m_selection.Set(decimalPos + 1, decimalPos + 1);
                        return;
                    }
                }
                else
                    m_previousSeparatorCount = separatorCount;
            }

            // Check if it's a digit
            else if (Char.IsDigit(c))
            {
                // Check if we're on the right of the decimal point.
                if (decimalPos >= 0 && decimalPos < start)
                {
                    if (numericText.Substring(numericDecimalPos + 1).Length == m_maxDecimalPlaces)
                    {
                        if (start <= decimalPos + m_maxDecimalPlaces)
                        {
                            end = (end == length ? end : (start + 1));
                            m_selection.SetAndReplace(start, end, c.ToString());
                        }
                        return;
                    }
                }

                // We're on the left side of the decimal point
                else
                {
                    bool isNegative = (numericText.Length != 0 && numericText[0] == m_negativeSign);

                    // Make sure we can still enter digits.
                    if (start == m_maxWholeDigits + separatorCount + prefixLength + (isNegative ? 1 : 0))
                    {
                        if (AddDecimalAfterMaxWholeDigits && m_maxDecimalPlaces > 0)
                        {
                            end = (end == length ? end : (start + 2));
                            m_selection.SetAndReplace(start, end, m_decimalPoint.ToString() + c);
                        }

                        return;
                    }

                    if (numericText.Substring(0, numericDecimalPos >= 0 ? numericDecimalPos : numericLen).Length == m_maxWholeDigits + (isNegative ? 1 : 0))
                    {
                        if (text[start] == m_groupSeparator)
                            start++;

                        end = (end == length ? end : (start + 1));
                        m_selection.SetAndReplace(start, end, c.ToString());
                        return;
                    }

                    m_previousSeparatorCount = separatorCount;
                }
            }

            // Check if it's a non-printable character, such as Backspace or Ctrl+C
            else if (Char.IsControl(c))
                m_previousSeparatorCount = separatorCount;
            else
                return;

            base.HandleKeyPress(sender, e);
        }

        /// <summary>
        ///   Handles changes in the textbox text. </summary>
        /// <param name="sender">
        ///   The object who sent the event. </param>
        /// <param name="e">
        ///   The event data. </param>
        /// <remarks>
        ///   This method is overriden from the Behavior class and it  
        ///   handles the textbox's TextChanged event. 
        ///   Here it is used to adjust the selection if new separators have been added or removed. </remarks>
        /// <seealso cref="Control.TextChanged" />
        // Fires the TextChanged event if the text is valid.
        protected override void HandleTextChanged(object sender, EventArgs e)
        {
            TraceLine("NumericBehavior.HandleTextChanged");

            Selection.Saver savedSelection = new Selection.Saver(m_textBox);  // save the selection before the text changes
            bool textChangedByKeystroke = m_textChangedByKeystroke;
            base.HandleTextChanged(sender, e);
            if (m_textBox.Text.Length == 0)
            {
                m_textBox.Text = m_defaultValue.ToString();
            }

            // Check if the user has changed the number enough to cause
            // one or more separators to be added/removed, in which case
            // the selection may need to be adjusted.
            if (m_previousSeparatorCount >= 0)
            {
                using (savedSelection)
                {
                    int newSeparatorCount = GetGroupSeparatorCount(m_textBox.Text);
                    if (m_previousSeparatorCount != newSeparatorCount && savedSelection.Start > m_prefix.Length)
                        savedSelection.MoveBy(newSeparatorCount - m_previousSeparatorCount);
                }
            }

            // If the text wasn't changed by a keystroke and the UseLostFocusFlagsWhenTextPropertyIsSet flag is set,
            // call the LostFocus handler to adjust the value according to whatever LostFocus flags are set.
            if (HasFlag((int)LostFocusFlag.CallHandlerWhenTextChanges) ||
               (!textChangedByKeystroke && HasFlag((int)LostFocusFlag.CallHandlerWhenTextPropertyIsSet)))
                HandleLostFocus(sender, e);

            m_textChangedByKeystroke = false;
        }

        /// <summary>
        ///   Handles when the control has lost its focus. </summary>
        /// <param name="sender">
        ///   The object who sent the event. </param>
        /// <param name="e">
        ///   The event data. </param>
        /// <remarks>
        ///   This method is overriden from the Behavior class and it  
        ///   handles the textbox's LostFocus event. 
        ///   Here it checks the value's against the allowed range and adds any missing zeros. </remarks>
        /// <seealso cref="Control.LostFocus" />
        protected override void HandleLostFocus(object sender, EventArgs e)
        {
            TraceLine("NumericBehavior.HandleLostFocus");

            if (!HasFlag((int)LostFocusFlag.Max))
                return;

            string originalText = GetNumericText(m_textBox.Text, true);
            string text = originalText;
            int length = text.Length;

            // If desired, remove any extra leading zeros but always leave one in front of the decimal point
            if (HasFlag((int)LostFocusFlag.RemoveExtraLeadingZeros) && length > 0)
            {
                bool isNegative = (text[0] == m_negativeSign);
                if (isNegative)
                    text = text.Substring(1);
                text = text.TrimStart('0');
                if (text == "" || text[0] == m_decimalPoint)
                    text = '0' + text;
                if (isNegative)
                    text = m_negativeSign + text;
            }
            // Check if the value is empty and we don't want to touch it
            else if (length == 0 && HasFlag((int)LostFocusFlag.DontPadWithZerosIfEmpty))
                return;

            int decimalPos = text.IndexOf('.');
            int maxDecimalPlaces = m_maxDecimalPlaces;
            int maxWholeDigits = m_maxWholeDigits;

            // Check if we need to pad the number with zeros after the decimal point
            if (HasFlag((int)LostFocusFlag.PadWithZerosAfterDecimal) && maxDecimalPlaces > 0)
            {
                if (decimalPos < 0)
                {
                    if (length == 0 || text == "-")
                    {
                        text = "0";
                        length = 1;
                    }
                    text += '.';
                    decimalPos = length++;
                }

                text = InsertZeros(text, -1, maxDecimalPlaces - (length - decimalPos - 1));
            }

            // Check if we need to pad the number with zeros before the decimal point
            if (HasFlag((int)LostFocusFlag.PadWithZerosBeforeDecimal) && maxWholeDigits > 0)
            {
                if (decimalPos < 0)
                    decimalPos = length;

                if (length > 0 && text[0] == '-')
                    decimalPos--;

                text = InsertZeros(text, (length > 0 ? (text[0] == '-' ? 1 : 0) : -1), maxWholeDigits - decimalPos);
            }

            if (text != originalText)
            {
                if (decimalPos >= 0 && m_decimalPoint != '.')
                    text = text.Replace('.', m_decimalPoint);

                // remember the current selection 
                using (Selection.Saver savedSelection = new Selection.Saver(m_textBox))
                {
                    m_textBox.Text = text;
                }
            }
        }

        /// <summary>
        ///   Handles when the control's text gets parsed to be converted to the type expected by the 
        ///   object that it's bound to. </summary>
        /// <param name="sender">
        ///   The object who sent the event. </param>
        /// <param name="e">
        ///   The event data. </param>
        /// <remarks>
        ///   This method checks if the control's text is empty and if so sets the value to DBNull.Value;
        ///   otherwise it converts it to a simple numeric value (without any prefix). </remarks>
        /// <seealso cref="Behavior.HandleBindingChanges" />
        /// <seealso cref="Binding.Parse" />
        protected override void HandleBindingParse(object sender, ConvertEventArgs e)
        {
            if (e.Value.ToString() == "")
                e.Value = DBNull.Value;
            else
                e.Value = GetNumericText(e.Value.ToString(), false);
        }
    }

}
