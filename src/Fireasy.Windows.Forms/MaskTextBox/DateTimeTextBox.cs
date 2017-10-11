using System;
using System.ComponentModel;
using System.Collections;
using System.Text;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the DateTime behavior.")]
    [Designer(typeof(DateTimeTextBox.Designer))]
    public class DateTimeTextBox : TimeTextBox
    {
        public DateTimeTextBox()
            :
            base(null)
        {
            m_behavior = new DateTimeBehavior(this);
        }

        public DateTimeTextBox(DateTimeBehavior behavior)
            :
            base(behavior)
        {
        }

        [Browsable(false)]
        public new DateTimeBehavior Behavior
        {
            get
            {
                return (DateTimeBehavior)m_behavior;
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
        public new object Value
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
        public new DateTime RangeMin
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
        public new DateTime RangeMax
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
        public char DateSeparator
        {
            get
            {
                return Behavior.DateSeparator;
            }
            set
            {
                Behavior.DateSeparator = value;
            }
        }

        [Browsable(false)]
        public char TimeSeparator
        {
            get
            {
                return Behavior.TimeSeparator;
            }
            set
            {
                Behavior.TimeSeparator = value;
            }
        }

        [Browsable(false)]
        private new char Separator
        {
            get
            {
                return Behavior.Separator;
            }
        }

        [Browsable(false)]
        public bool ShowDayBeforeMonth
        {
            get
            {
                return Behavior.ShowDayBeforeMonth;
            }
            set
            {
                Behavior.ShowDayBeforeMonth = value;
            }
        }

        public void SetDate(int year, int month, int day)
        {
            Behavior.SetDate(year, month, day);
        }

        public void SetDateTime(int year, int month, int day, int hour, int minute)
        {
            Behavior.SetDateTime(year, month, day, hour, minute);
        }

        public void SetDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            Behavior.SetDateTime(year, month, day, hour, minute, second);
        }

        internal new class Designer : TimeTextBox.Designer
        {
            protected override void PostFilterProperties(IDictionary properties)
            {
                properties.Remove("Month");
                properties.Remove("Day");
                properties.Remove("Year");
                properties.Remove("DateSeparator");
                properties.Remove("TimeSeparator");
                properties.Remove("ShowDayBeforeMonth");

                base.PostFilterProperties(properties);
            }
        }
    }
}
