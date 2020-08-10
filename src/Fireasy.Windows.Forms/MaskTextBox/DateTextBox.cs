using System;
using System.Collections;
using System.ComponentModel;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the Date behavior.")]
    [Designer(typeof(DateTextBox.Designer))]
    public class DateTextBox : MaskBaseTextBox
    {
        public DateTextBox()
        {
            _behavior = new DateBehavior(this);
        }

        public DateTextBox(DateBehavior behavior)
            :
            base(behavior)
        {
        }

        [Browsable(false)]
        public DateBehavior Behavior
        {
            get
            {
                return (DateBehavior)_behavior;
            }
        }

        [Browsable(false)]
        public int Month
        {
            get
            {
                return Behavior.Month;
            }
            set
            {
                Behavior.Month = value;
            }
        }

        [Browsable(false)]
        public int Day
        {
            get
            {
                return Behavior.Day;
            }
            set
            {
                Behavior.Day = value;
            }
        }

        [Browsable(false)]
        public int Year
        {
            get
            {
                return Behavior.Year;
            }
            set
            {
                Behavior.Year = value;
            }
        }

        [Browsable(false)]
        public object Value
        {
            get
            {
                return Behavior.Value;
            }
            set
            {
                Behavior.Value = value;
            }
        }

        [Category("Behavior")]
        [Description("The minimum value allowed.")]
        public DateTime RangeMin
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
        public DateTime RangeMax
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

        [Browsable(false)]
        public char Separator
        {
            get
            {
                return Behavior.Separator;
            }
            set
            {
                Behavior.Separator = value;
            }
        }

        public void SetDate(int year, int month, int day)
        {
            Behavior.SetDate(year, month, day);
        }

        internal new class Designer : MaskBaseTextBox.Designer
        {

            protected override void PostFilterProperties(IDictionary properties)
            {
                properties.Remove("Month");
                properties.Remove("Day");
                properties.Remove("Year");
                properties.Remove("Value");
                properties.Remove("Separator");
                properties.Remove("ShowDayBeforeMonth");

                base.PostFilterProperties(properties);
            }
        }
    }

}
