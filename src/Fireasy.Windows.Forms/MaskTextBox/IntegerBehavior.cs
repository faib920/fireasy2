using System;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    #region IntegerBehavior
    public class IntegerBehavior : NumericBehavior
    {
        /// <summary>
        ///   Initializes a new instance of the IntegerBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets <see cref="NumericBehavior.MaxWholeDigits" /> = 9, <see cref="NumericBehavior.MaxDecimalPlaces" /> = 0, <see cref="NumericBehavior.DigitsInGroup" /> = 0, 
        ///   <see cref="NumericBehavior.Prefix" /> = "", <see cref="NumericBehavior.AllowNegative" /> = true, and the rest of the properties according to user's system. </remarks>
        public IntegerBehavior(TextBoxBase textBox)
            :
            base(textBox, 9, 0)
        {
            SetDefaultRange();
        }

        /// <summary>
        ///   Initializes a new instance of the IntegerBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <param name="maxWholeDigits">
        ///   The maximum number of digits allowed to the left of the decimal point. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets <see cref="NumericBehavior.MaxDecimalPlaces" /> = 0, <see cref="NumericBehavior.DigitsInGroup" /> = 0, <see cref="NumericBehavior.Prefix" /> = "", <see cref="NumericBehavior.AllowNegative" /> = true, 
        ///   and the rest of the properties according to user's system. </remarks>
        public IntegerBehavior(TextBoxBase textBox, int maxWholeDigits)
            :
            base(textBox, maxWholeDigits, 0)
        {
            SetDefaultRange();
        }

        /// <summary>
        ///   Initializes a new instance of the IntegerBehavior class by copying it from 
        ///   another IntegerBehavior object. </summary>
        /// <param name="behavior">
        ///   The IntegerBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public IntegerBehavior(IntegerBehavior behavior)
            :
            base(behavior)
        {
            SetDefaultRange();
        }

        /// <summary>
        ///   Changes the default min and max values to 32-bit integer ranges.</summary>
        private void SetDefaultRange()
        {
            RangeMin = Int32.MinValue;
            RangeMax = Int32.MaxValue;
        }

        /// <summary>
        ///   Gets the maximum number of digits allowed right of the decimal point, which is always 0. </summary>
        /// <seealso cref="NumericBehavior.MaxWholeDigits" />
        public new int MaxDecimalPlaces
        {
            get
            {
                return base.MaxDecimalPlaces;
            }
        }

        /// <summary>
        ///   Gets or sets a mask value representative of this object's properties. </summary>
        /// <remarks>
        ///   This property behaves like <see cref="NumericBehavior.Mask" /> except that  
        ///   <see cref="NumericBehavior.MaxDecimalPlaces" /> is maintained with a value of 0. </remarks>
        public new string Mask
        {
            get
            {
                return base.Mask;
            }
            set
            {
                base.Mask = value;
                if (base.MaxDecimalPlaces > 0)
                    base.MaxDecimalPlaces = 0;
            }
        }
    }
    #endregion

}
