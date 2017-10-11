using System;
using System.ComponentModel;
using System.Collections;
using System.Text;

namespace Fireasy.Windows.Forms
{
    [ToolboxItem(true),
    DesignTimeVisible(true)]
    [Description("TextBox control which supports the Time behavior.")]
    [Designer(typeof(TimeTextBox.Designer))]
    public class TimeTextBox : MaskBaseTextBox
    {
        public TimeTextBox()
        {
            m_behavior = new TimeBehavior(this);
        }

        public TimeTextBox(TimeBehavior behavior)
            :
            base(behavior)
        {
        }

        [Browsable(false)]
        public TimeBehavior Behavior
        {
            get
            {
                return (TimeBehavior)m_behavior;
            }
        }

        [Browsable(false)]
        public int Hour
        {
            get
            {
                return Behavior.Hour;
            }
            set
            {
                Behavior.Hour = value;
            }
        }

        [Browsable(false)]
        public int Minute
        {
            get
            {
                return Behavior.Minute;
            }
            set
            {
                Behavior.Minute = value;
            }
        }

        [Browsable(false)]
        public int Second
        {
            get
            {
                return Behavior.Second;
            }
            set
            {
                Behavior.Second = value;
            }
        }

        [Browsable(false)]
        public string AMPM
        {
            get
            {
                return Behavior.AMPM;
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

        [Browsable(false)]
        public bool Show24HourFormat
        {
            get
            {
                return Behavior.Show24HourFormat;
            }
            set
            {
                Behavior.Show24HourFormat = value;
            }
        }

        [Category("Behavior")]
        [Description("Determines whether the seconds should be shown (after the minutes).")]
        public bool ShowSeconds
        {
            get
            {
                return Behavior.ShowSeconds;
            }
            set
            {
                Behavior.ShowSeconds = value;
            }
        }

        public void SetTime(int hour, int minute, int second)
        {
            Behavior.SetTime(hour, minute, second);
        }

        public void SetTime(int hour, int minute)
        {
            Behavior.SetTime(hour, minute);
        }

        internal new class Designer : MaskBaseTextBox.Designer
        {
            protected override void PostFilterProperties(IDictionary properties)
            {
                properties.Remove("Hour");
                properties.Remove("Minute");
                properties.Remove("Second");
                properties.Remove("Value");
                properties.Remove("Separator");
                properties.Remove("Show24HourFormat");

                base.PostFilterProperties(properties);
            }
        }
    }

}
