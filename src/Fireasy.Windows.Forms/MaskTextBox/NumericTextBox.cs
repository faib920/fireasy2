using System;
using System.Collections;
using System.ComponentModel;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the Numeric behavior.")]
    [Designer(typeof(NumericTextBox.Designer))]
    public class NumericTextBox : MaskBaseTextBox
    {
        public NumericTextBox()
        {
            ImeMode = System.Windows.Forms.ImeMode.Off;
            _behavior = new NumericBehavior(this);
        }

        public NumericTextBox(int maxWholeDigits, int maxDecimalPlaces)
        {
            ImeMode = System.Windows.Forms.ImeMode.Off;
            _behavior = new NumericBehavior(this, maxWholeDigits, maxDecimalPlaces);
        }

        public NumericTextBox(NumericBehavior behavior)
            :
            base(behavior)
        {
            ImeMode = System.Windows.Forms.ImeMode.Off;
        }

        [Browsable(false)]
        public NumericBehavior Behavior
        {
            get
            {
                return (NumericBehavior)_behavior;
            }
        }

        [Category("Behavior")]
        [Description("The default value.")]
        public double DefaultValue
        {
            get
            {
                return Behavior.DefaultValue;
            }
            set
            {
                Behavior.DefaultValue = value;
            }
        }

        [Browsable(false)]
        public double Double
        {
            get
            {
                try
                {
                    return Convert.ToDouble(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
            }
        }

        [Browsable(false)]
        public int Int
        {
            get
            {
                try
                {
                    return Convert.ToInt32(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
            }
        }

        [Browsable(false)]
        public decimal Decimal
        {
            get
            {
                try
                {
                    return Convert.ToDecimal(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
            }
        }
        [Browsable(false)]
        public long Long
        {
            get
            {
                try
                {
                    return Convert.ToInt64(Behavior.NumericText);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                Text = value.ToString();
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

        [Browsable(false)]
        public string RealNumericText
        {
            get
            {
                return Behavior.RealNumericText;
            }
        }

        [Category("Behavior")]
        [Description("The maximum number of digits allowed left of the decimal point.")]
        public int MaxWholeDigits
        {
            get
            {
                return Behavior.MaxWholeDigits;
            }
            set
            {
                Behavior.MaxWholeDigits = value;
            }
        }

        [Category("Behavior")]
        [Description("The maximum number of digits allowed right of the decimal point.")]
        public int MaxDecimalPlaces
        {
            get
            {
                return Behavior.MaxDecimalPlaces;
            }
            set
            {
                Behavior.MaxDecimalPlaces = value;
            }
        }

        [Category("Behavior")]
        [Description("Determines whether the value is allowed to be negative or not.")]
        public bool AllowNegative
        {
            get
            {
                return Behavior.AllowNegative;
            }
            set
            {
                Behavior.AllowNegative = value;
            }
        }

        [Category("Behavior")]
        [Description("The number of digits to place in each group to the left of the decimal point.")]
        public int DigitsInGroup
        {
            get
            {
                return Behavior.DigitsInGroup;
            }
            set
            {
                Behavior.DigitsInGroup = value;
            }
        }

        [Browsable(false)]
        public char DecimalPoint
        {
            get
            {
                return Behavior.DecimalPoint;
            }
            set
            {
                Behavior.DecimalPoint = value;
            }
        }

        [Browsable(false)]
        public char GroupSeparator
        {
            get
            {
                return Behavior.GroupSeparator;
            }
            set
            {
                Behavior.GroupSeparator = value;
            }
        }

        [Browsable(false)]
        public char NegativeSign
        {
            get
            {
                return Behavior.NegativeSign;
            }
            set
            {
                Behavior.NegativeSign = value;
            }
        }

        [Category("Behavior")]
        [Description("The text to automatically insert in front of the number, such as a currency symbol.")]
        public String Prefix
        {
            get
            {
                return Behavior.Prefix;
            }
            set
            {
                Behavior.Prefix = value;
            }
        }

        [Category("Behavior")]
        [Description("The minimum value allowed.")]
        public double RangeMin
        {
            get
            {
                return Behavior.RangeMin;
            }
            set
            {
                Behavior.RangeMin = value;
            }
        }

        [Category("Behavior")]
        [Description("The maximum value allowed.")]
        public double RangeMax
        {
            get
            {
                return Behavior.RangeMax;
            }
            set
            {
                Behavior.RangeMax = value;
            }
        }


        internal new class Designer : MaskBaseTextBox.Designer
        {
            protected override void PostFilterProperties(IDictionary properties)
            {
                properties.Remove("DecimalPoint");
                properties.Remove("GroupSeparator");
                properties.Remove("NegativeSign");
                properties.Remove("Double");
                properties.Remove("Int");
                properties.Remove("Long");

                base.PostFilterProperties(properties);
            }
        }
    }

}
