using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    #region DateTimeBehavior
    public class DateTimeBehavior : TimeBehavior
    {
        // Fields
        private readonly DateBehavior _dateBehavior;

        /// <summary>
        ///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
        ///   properties of this class. </summary>
        [Flags]
        protected new enum Flag
        {
            /// <summary> Makes this object behave like the Date behavior, where only the date part is shown. </summary>
            /// <seealso cref="DateBehavior" />
            DateOnly = 0x00100000,

            /// <summary> Makes this object behave like the Time behavior, where only the time part is shown. </summary>
            /// <seealso cref="TimeBehavior" />
            TimeOnly = 0x00200000,

            /// <summary> The day is displayed in front of the month. </summary>
            /// <seealso cref="ShowDayBeforeMonth" />
            DayBeforeMonth = 0x00010000,

            /// <summary> The hour is shown in 24-hour format (00 to 23). </summary>
            /// <seealso cref="TimeBehavior.Show24HourFormat" />
            TwentyFourHourFormat = 0x00020000,

            /// <summary> The seconds are also shown. </summary>
            /// <seealso cref="TimeBehavior.ShowSeconds" />
            WithSeconds = 0x00040000
        };

        /// <summary>
        ///   Initializes a new instance of the DateTimeBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor retrieves many of the properties from the user's system. </remarks>
        /// <seealso cref="System.Windows.Forms.TextBoxBase" />	
        public DateTimeBehavior(TextBoxBase textBox)
            :
            base(textBox)
        {
            _dateBehavior = new DateBehavior(textBox, false);  // does not add the event handlers
            _flags |= _dateBehavior.Flags;
            _hourStart = 11;
        }

        /// <summary>
        ///   Initializes a new instance of the DateTimeBehavior class by copying it from 
        ///   another DateTimeBehavior object. </summary>
        /// <param name="behavior">
        ///   The DateTimeBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public DateTimeBehavior(DateTimeBehavior behavior)
            :
            base(behavior)
        {
            _dateBehavior = new DateBehavior(_textBox, false);  // does not add the event handlers
        }

        /// <summary>
        ///   Sets the month, day, year, hour, minute, and second on the textbox. </summary>
        /// <param name="year">
        ///   The year to set. </param>
        /// <param name="month">
        ///   The month to set. </param>
        /// <param name="day">
        ///   The day to set. </param>
        /// <param name="hour">
        ///   The hour to set, between 0 and 23. </param>
        /// <param name="minute">
        ///   The minute to set, between 0 and 59. </param>
        /// <remarks>
        ///   This is a convenience method to set each value individually using a single method. 
        ///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. 
        ///   If the second is being shown on the textbox, it is set to 0.  </remarks>
        public void SetDateTime(int year, int month, int day, int hour, int minute)
        {
            SetDateTime(year, month, day, hour, minute, 0);
        }

        /// <summary>
        ///   Sets the month, day, year, hour, minute, and second on the textbox. </summary>
        /// <param name="year">
        ///   The year to set. </param>
        /// <param name="month">
        ///   The month to set. </param>
        /// <param name="day">
        ///   The day to set. </param>
        /// <param name="hour">
        ///   The hour to set, between 0 and 23. </param>
        /// <param name="minute">
        ///   The minute to set, between 0 and 59. </param>
        /// <param name="second">
        ///   The second to set, between 0 and 59. </param>
        /// <remarks>
        ///   This is a convenience method to set each value individually using a single method. 
        ///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
        public void SetDateTime(int year, int month, int day, int hour, int minute, int second)
        {
            if (HasFlag((int)Flag.DateOnly))
                _dateBehavior.SetDate(year, month, day);
            else if (HasFlag((int)Flag.TimeOnly))
                SetTime(hour, minute, second);
            else
            {
                Debug.Assert(_dateBehavior.IsWithinRange(new DateTime(year, month, day)));
                _textBox.Text = _dateBehavior.GetFormattedDate(year, month, day) + ' ' + GetFormattedTime(hour, minute, second, "");
            }
        }

        /// <summary>
        ///   Sets the month, day, and year on the textbox. </summary>
        /// <param name="year">
        ///   The year to set. </param>
        /// <param name="month">
        ///   The month to set. </param>
        /// <param name="day">
        ///   The day to set. </param>
        /// <remarks>
        ///   This is a convenience method to set each value individually using a single method. 
        ///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
        public void SetDate(int year, int month, int day)
        {
            if (HasFlag((int)Flag.DateOnly) || !HasFlag((int)Flag.TimeOnly))
                _dateBehavior.SetDate(year, month, day);
        }

        /// <summary>
        ///   Sets the hour, minute, and second on the textbox. </summary>
        /// <param name="hour">
        ///   The hour to set, between 0 and 23. </param>
        /// <param name="minute">
        ///   The minute to set, between 0 and 59. </param>
        /// <param name="second">
        ///   The second to set, between 0 and 59. </param>
        /// <remarks>
        ///   This is a convenience method to set each value individually using a single method. 
        ///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. </remarks>
        public new void SetTime(int hour, int minute, int second)
        {
            if (!HasFlag((int)Flag.DateOnly) && HasFlag((int)Flag.TimeOnly))
                base.SetTime(hour, minute, second);
        }

        /// <summary>
        ///   Sets the hour and minute on the textbox. </summary>
        /// <param name="hour">
        ///   The hour to set, between 0 and 23. </param>
        /// <param name="minute">
        ///   The minute to set, between 0 and 59. </param>
        /// <remarks>
        ///   This is a convenience method to set each value individually using a single method. 
        ///   A <see cref="DateTime" /> object is constructed using these parameters, so they must be valid. 
        ///   If the second is being shown on the textbox, it is set to 0.  </remarks>
        public new void SetTime(int hour, int minute)
        {
            SetTime(hour, minute, 0);
        }

        /// <summary>
        ///   Gets or sets the month, day, year, hour, minute, and second on the textbox using a <see cref="DateTime" /> object. </summary>
        /// <remarks>
        ///   This property gets and sets the <see cref="DateTime" /> boxed inside an <c>object</c>.
        ///   This makes it flexible, so that if the textbox does not hold a valid date and time, a <c>null</c> is returned, 
        ///   instead of having to worry about an exception being thrown. </remarks>
        /// <example><code>
        ///   object obj = txtDateTime.Behavior.Value;
        ///   if (obj != null)
        ///   {
        ///     DateTime dtm = (DateTime)obj;
        ///     ...
        ///   } </code></example>
        /// <seealso cref="Month" />
        /// <seealso cref="Day" />
        /// <seealso cref="Year" />
        /// <seealso cref="TimeBehavior.Hour" />
        /// <seealso cref="TimeBehavior.Minute" />
        /// <seealso cref="TimeBehavior.Second" />
        public new object Value
        {
            get
            {
                try
                {
                    if (HasFlag((int)Flag.DateOnly))
                        return _dateBehavior.Value;
                    if (HasFlag((int)Flag.TimeOnly))
                        return base.Value;
                    return new DateTime(Year, Month, Day, Hour, Minute, Second);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                DateTime dt = (DateTime)value;
                SetDateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
        }

        /// <summary>
        ///   Gets or sets the month on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid month. </exception>
        /// <remarks>
        ///   If the month is not being shown or is not valid on the textbox, this property will return 0.
        ///   This property must be set with a month that falls within the allowed range. </remarks>
        /// <seealso cref="Day" />
        /// <seealso cref="Year" />
        public int Month
        {
            get
            {
                if (HasFlag((int)Flag.TimeOnly))
                    return 0;
                return _dateBehavior.Month;
            }
            set
            {
                if (!HasFlag((int)Flag.TimeOnly))
                    _dateBehavior.Month = value;
            }
        }

        /// <summary>
        ///   Gets or sets the day on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid day. </exception>
        /// <remarks>
        ///   If the day is not being shown or is not valid on the textbox, this property will return 0.
        ///   This property must be set with a day that falls within the allowed range. </remarks>
        /// <seealso cref="Month" />
        /// <seealso cref="Year" />
        public int Day
        {
            get
            {
                if (HasFlag((int)Flag.TimeOnly))
                    return 0;
                return _dateBehavior.Day;
            }
            set
            {
                if (!HasFlag((int)Flag.TimeOnly))
                    _dateBehavior.Day = value;
            }
        }

        /// <summary>
        ///   Gets or sets the year on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid year. </exception>
        /// <remarks>
        ///   If the year is not being shown or is not valid on the textbox, this property will return 0.
        ///   This property must be set with a year that falls within the allowed range. </remarks>
        /// <seealso cref="Month" />
        /// <seealso cref="Day" />
        public int Year
        {
            get
            {
                if (HasFlag((int)Flag.TimeOnly))
                    return 0;
                return _dateBehavior.Year;
            }
            set
            {
                if (!HasFlag((int)Flag.TimeOnly))
                    _dateBehavior.Year = value;
            }
        }

        /// <summary>
        ///   Checks if the textbox's date and/or time is valid and falls within the allowed range. </summary>
        /// <returns>
        ///   If the value is valid and falls within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        public override bool IsValid()
        {
            if (HasFlag((int)Flag.DateOnly))
                return _dateBehavior.IsValid();
            if (HasFlag((int)Flag.TimeOnly))
                return base.IsValid();
            return (_dateBehavior.IsValid() && base.IsValid());
        }

        /// <summary>
        ///   Gets or sets the minimum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to DateTime(1900, 1, 1, 0, 0, 0).
        ///   The range is actively checked as the user enters the date but the time is only checked 
        ///   when the control loses focus, if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMax" />
        public new DateTime RangeMin
        {
            get
            {
                if (HasFlag((int)Flag.DateOnly))
                    return _dateBehavior.RangeMin;
                if (HasFlag((int)Flag.TimeOnly))
                    return base.RangeMin;

                DateTime rangeMin = base.RangeMin;
                return new DateTime(_dateBehavior.RangeMin.Year, _dateBehavior.RangeMin.Month, _dateBehavior.RangeMin.Day, rangeMin.Hour, rangeMin.Minute, rangeMin.Second);
            }
            set
            {
                base.RangeMin = value;
                if (HasFlag((int)Flag.DateOnly) || !HasFlag((int)Flag.TimeOnly))
                    _dateBehavior.RangeMin = value;		// updates the control
            }
        }

        /// <summary>
        ///   Gets or sets the maximum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to <see cref="DateTime.MaxValue" />.
        ///   The range is actively checked as the user enters the date but the time is only checked 
        ///   when the control loses focus, if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMin" />
        public new DateTime RangeMax
        {
            get
            {
                if (HasFlag((int)Flag.DateOnly))
                    return _dateBehavior.RangeMax;
                if (HasFlag((int)Flag.TimeOnly))
                    return base.RangeMax;

                DateTime rangeMax = base.RangeMax;
                return new DateTime(_dateBehavior.RangeMax.Year, _dateBehavior.RangeMax.Month, _dateBehavior.RangeMax.Day, rangeMax.Hour, rangeMax.Minute, rangeMax.Second);
            }
            set
            {
                base.RangeMax = value;
                if (HasFlag((int)Flag.DateOnly) || !HasFlag((int)Flag.TimeOnly))
                    _dateBehavior.RangeMax = value;		// updates the control
            }
        }

        /// <summary>
        ///   Checks if a date and time value falls within the allowed range. </summary>
        /// <param name="dt">
        ///   The date and time value to check. </param>
        /// <returns>
        ///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        /// <seealso cref="IsValid" />
        public new bool IsWithinRange(DateTime dt)
        {
            if (HasFlag((int)Flag.DateOnly))
                return _dateBehavior.IsWithinRange(dt);
            if (HasFlag((int)Flag.TimeOnly))
                return base.IsWithinRange(dt);
            return (_dateBehavior.IsWithinRange(dt) && base.IsWithinRange(dt));
        }

        /// <summary>
        ///   Gets or sets the character used to separate the month, day, and year values of the date. </summary>
        /// <remarks>
        ///   By default, this property is set according to the user's system. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        public char DateSeparator
        {
            get
            {
                return _dateBehavior.Separator;
            }
            set
            {
                _dateBehavior.Separator = value;
            }
        }

        /// <summary>
        ///   Gets or sets the character used to separate the hour, minute, and second values of the time. </summary>
        /// <remarks>
        ///   By default, this property is set according to the user's system. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        public char TimeSeparator
        {
            get
            {
                return base.Separator;
            }
            set
            {
                base.Separator = value;
            }
        }

        /// <summary>
        ///   Gets the character used to separate the date or time value. </summary>
        /// <remarks>
        ///   If only the date is being shown, this property retrieves the <see cref="DateSeparator" />.
        ///   If only the time is being shown, this property retrieves the <see cref="TimeSeparator" />.
        ///   If both the date and time are being shown, this property retrieves a space character. </remarks>
        private new char Separator
        {
            get
            {
                if (HasFlag((int)Flag.DateOnly))
                    return _dateBehavior.Separator;
                if (HasFlag((int)Flag.TimeOnly))
                    return base.Separator;
                return ' ';
            }
        }

        /// <summary>
        ///   Gets or sets whether the day should be shown before the month or after it. </summary>
        /// <remarks>
        ///   By default, this property is set according to the user's system. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="Flag.DayBeforeMonth" />
        public bool ShowDayBeforeMonth
        {
            get
            {
                return HasFlag((int)Flag.DayBeforeMonth);
            }
            set
            {
                if (!HasFlag((int)Flag.TimeOnly))
                    ModifyFlags((int)Flag.DayBeforeMonth, value);
            }
        }

        /// <summary>
        ///   Gets or sets the flags associated with this object. </summary>
        /// <remarks>
        ///   This property behaves like the one in the <see cref="Behavior.Flags">base class</see> 
        ///   but is overriden to properly set the start position of the hour, in case the 
        ///   <see cref="Flag.DateOnly" /> or <see cref="Flag.TimeOnly" /> flags are turned on/off. </remarks>
        /// <seealso cref="Behavior.ModifyFlags" />
        public override int Flags
        {
            get
            {
                return _flags;
            }
            set
            {
                if (_flags == value)
                    return;

                _flags = value;
                _hourStart = ((value & (int)Flag.TimeOnly) != 0) ? 0 : 11;

                _dateBehavior.Flags = value;  // should call UpdateText
            }
        }

        /// <summary>
        ///   Retrieves the textbox's text in valid form. </summary>
        /// <returns>
        ///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
        protected override string GetValidText()
        {
            // Check if we're showing the date only
            string date = _dateBehavior.GetValidTextForDateTime();
            if (HasFlag((int)Flag.DateOnly))
                return date;

            // Check if we're showing the time only
            string time = base.GetValidText();
            if (HasFlag((int)Flag.TimeOnly))
                return time;

            string space = (date != "" && time != "" ? " " : "");
            return date + space + time;
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
            TraceLine("DateTimeBehavior.HandleKeyDown " + e.KeyCode);

            // Check if we're showing the time only
            if (HasFlag((int)Flag.TimeOnly))
            {
                base.HandleKeyDown(sender, e);
                return;
            }

            if (e.KeyCode != Keys.Delete)
                _dateBehavior.HandleKeyEvent(sender, e);

            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Delete) && !HasFlag((int)Flag.DateOnly))
                base.HandleKeyDown(sender, e);
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
            TraceLine("DateTimeBehavior.HandleKeyPress " + e.KeyChar);

            // Check to see if it's read only
            if (_textBox.ReadOnly)
                return;

            _noTextChanged = true;

            // Check if we're showing the date or the time only
            if (HasFlag((int)Flag.DateOnly))
            {
                _dateBehavior.HandleKeyEvent(sender, e);
                return;
            }
            if (HasFlag((int)Flag.TimeOnly))
            {
                base.HandleKeyPress(sender, e);
                return;
            }

            char c = e.KeyChar;
            e.Handled = true;

            _selection.Get(out int start, out int end);

            string text = _textBox.Text;
            int length = text.Length;

            if (start >= 0 && start <= 9)
            {
                _dateBehavior.HandleKeyEvent(sender, e);
                ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }
            else if (start == 10)
            {
                _dateBehavior.HandleKeyEvent(sender, e);

                int space = 0;
                if (c == ' ')
                    space = 1;
                else
                    space = (base.IsValidHourDigit(c, 0) || (base.IsValidHourDigit(c, 1) && length <= 11) ? 2 : 0);

                // If we need the space, enter it
                if (space != 0)
                    _selection.SetAndReplace(start, start + 1, " ");

                // If the space is to be preceded by a valid digit, "type" it in.
                if (space == 2)
                    SendKeys.Send(c.ToString());
                else
                    base.ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }
            else
                base.HandleKeyPress(sender, e);
        }

        /// <summary>
        ///   Gets the error message used to notify the user to enter a valid date and time value 
        ///   within the allowed range. </summary>
        /// <seealso cref="IsValid" />
        public override string ErrorMessage
        {
            get
            {
                // Get the message depending on what we're showing
                if (HasFlag((int)Flag.DateOnly))
                    return _dateBehavior.ErrorMessage;
                else if (HasFlag((int)Flag.TimeOnly))
                    return base.ErrorMessage;
                else
                {
                    string minDateTime =
                        _dateBehavior.GetFormattedDate(_dateBehavior.RangeMin.Year, _dateBehavior.RangeMin.Month, _dateBehavior.RangeMin.Day) + ' ' +
                        base.GetFormattedTime(base.RangeMin.Hour, base.RangeMin.Minute, base.RangeMin.Second, "");
                    string maxDateTime =
                        _dateBehavior.GetFormattedDate(_dateBehavior.RangeMax.Year, _dateBehavior.RangeMax.Month, _dateBehavior.RangeMax.Day) + ' ' +
                        base.GetFormattedTime(base.RangeMax.Hour, base.RangeMax.Minute, base.RangeMax.Second, "");

                    return "Please specify a date and time between " + minDateTime + " and " + maxDateTime + '.';
                }
            }
        }
    }
    #endregion
}
