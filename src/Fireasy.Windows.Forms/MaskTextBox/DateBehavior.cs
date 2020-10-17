using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    #region DateBehavior
    public class DateBehavior : Behavior
    {
        // Fields
        private DateTime _rangeMin = new DateTime(1900, 1, 1);
        private DateTime _rangeMax = new DateTime(9998, 12, 31);
        private char _separator = '/';

        /// <summary>
        ///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
        ///   properties of this class. </summary>
        [Flags]
        protected enum Flag
        {
            /// <summary> The day is displayed in front of the month. </summary>
            /// <seealso cref="ShowDayBeforeMonth" />
            DayBeforeMonth = 0x00010000,
        };

        /// <summary>
        ///   Initializes a new instance of the DateBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor determines the <see cref="Separator" /> and date format (mm/dd/yyyy or dd/mm/yyyy) from the user's system. </remarks>
        /// <seealso cref="System.Windows.Forms.TextBoxBase" />	
        public DateBehavior(TextBoxBase textBox)
            :
            this(textBox, true)
        {
        }

        /// <summary>
        ///   Initializes a new instance of the DateBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <param name="addEventHandlers">
        ///   If true, the textBox's event handlers are tied to the corresponding methods on this behavior object. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor determines the <see cref="Separator" /> and date format (mm/dd/yyyy or dd/mm/yyyy) from the user's system. 
        ///   It is meant to be used internally by the DateTime behavior class. </remarks>
        /// <seealso cref="System.Windows.Forms.TextBoxBase" />	
        internal DateBehavior(TextBoxBase textBox, bool addEventHandlers)
            :
            base(textBox, addEventHandlers)
        {
            // Get the system's date separator
            _separator = DateTimeFormatInfo.CurrentInfo.DateSeparator[0];

            // Determine if the day should go before the month
            string shortDate = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            for (int iPos = 0; iPos < shortDate.Length; iPos++)
            {
                char c = Char.ToUpper(shortDate[iPos]);
                if (c == 'M')	// see if the month is first
                    break;
                if (c == 'D')	// see if the day is first, and then set the flag
                {
                    _flags |= (int)Flag.DayBeforeMonth;
                    break;
                }
            }
        }

        /// <summary>
        ///   Initializes a new instance of the DateBehavior class by copying it from 
        ///   another DateBehavior object. </summary>
        /// <param name="behavior">
        ///   The DateBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public DateBehavior(DateBehavior behavior)
            :
            base(behavior)
        {
            _rangeMin = behavior._rangeMin;
            _rangeMax = behavior._rangeMax;
            _separator = behavior._separator;
        }

        /// <summary>
        ///   Calls either the <see cref="HandleKeyPress" /> or <see cref="HandleKeyDown" /> method. </summary>
        /// <param name="sender">
        ///   The object who sent the event. </param>
        /// <param name="e">
        ///   The event data. </param>
        /// <remarks>
        ///   This method is designed to be called by the <see cref="DateTimeBehavior" />
        ///   class since it does not have public access to HandleKeyPress or HandleKeyDown.
        ///   The type of EventArgs determines which method is called. </remarks>
        /// <seealso cref="Control.KeyDown" />
        internal void HandleKeyEvent(object sender, EventArgs e)
        {
            if (e is KeyEventArgs)
                HandleKeyDown(sender, (KeyEventArgs)e);
            else if (e is KeyPressEventArgs)
                HandleKeyPress(sender, (KeyPressEventArgs)e);
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
            TraceLine("DateBehavior.HandleKeyDown " + e.KeyCode);

            // Check to see if it's read only
            if (_textBox.ReadOnly)
                return;

            e.Handled = true;

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    {
                        // If deleting make sure it's the last character or that
                        // the selection goes all the way to the end of the text

                        _selection.Get(out int start, out int end);

                        string text = _textBox.Text;
                        int length = text.Length;

                        if (end != length)
                        {
                            if (!(end == start && end == length - 1))
                                return;
                        }

                        _noTextChanged = true;
                        break;
                    }

                case Keys.Up:
                    {
                        // If pressing the UP arrow, increment the corresponding value.

                        _selection.Get(out int start, out int end);

                        if (start >= GetYearStartPosition() && start <= GetYearStartPosition() + 4)
                        {
                            int year = Year;
                            if (year >= _rangeMin.Year && year < _rangeMax.Year)
                                Year = ++year;
                        }

                        else if (start >= GetMonthStartPosition() && start <= GetMonthStartPosition() + 2)
                        {
                            int month = Month;
                            if (month >= GetMinMonth() && month < GetMaxMonth())
                                Month = ++month;
                        }

                        else if (start >= GetDayStartPosition() && start <= GetDayStartPosition() + 2)
                        {
                            int day = Day;
                            if (day >= GetMinDay() && day < GetMaxDay())
                                Day = ++day;
                        }

                        return;
                    }

                case Keys.Down:
                    {
                        // If pressing the DOWN arrow, decrement the corresponding value.

                        _selection.Get(out int start, out int end);

                        if (start >= GetYearStartPosition() && start <= GetYearStartPosition() + 4)
                        {
                            int year = Year;
                            if (year > _rangeMin.Year)
                                Year = --year;
                        }

                        else if (start >= GetMonthStartPosition() && start <= GetMonthStartPosition() + 2)
                        {
                            int month = Month;
                            if (month > GetMinMonth())
                                Month = --month;
                        }

                        else if (start >= GetDayStartPosition() && start <= GetDayStartPosition() + 2)
                        {
                            int day = Day;
                            if (day > GetMinDay())
                                Day = --day;
                        }

                        return;
                    }
            }

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
            TraceLine("DateBehavior.HandleKeyPress " + e.KeyChar);

            // Check to see if it's read only
            if (_textBox.ReadOnly)
                return;

            char c = e.KeyChar;
            e.Handled = true;
            _noTextChanged = true;

            _selection.Get(out int start, out int end);

            string text = _textBox.Text;
            int length = text.Length;

            // Check for a non-printable character (such as Ctrl+C)
            if (Char.IsControl(c))
            {
                if (c == (short)Keys.Back && start != length)
                {
                    SendKeys.Send("{LEFT}");  // move the cursor left
                    return;
                }

                // Allow backspace only if the cursor is all the way to the right
                base.HandleKeyPress(sender, e);
                return;
            }

            // Add the digit depending on its location
            switch (start)
            {
                case 0:		// FIRST DIGIT
                    {
                        if (ShowDayBeforeMonth)
                        {
                            if (IsValidDayDigit(c, 0))
                            {
                                if (length > start)
                                {
                                    _selection.SetAndReplace(start, start + 1, c.ToString());

                                    if (length > start + 1)
                                    {
                                        if (!IsValidDay(Day))
                                        {
                                            _selection.SetAndReplace(start + 1, start + 2, GetMinDayDigit(1).ToString());
                                            _selection.Set(start + 1, start + 2);
                                        }
                                    }
                                }
                                else
                                    base.HandleKeyPress(sender, e);
                            }
                            // Check if we can insert the digit with a leading zero
                            else if (length == start && GetMinDayDigit(0) == '0' && IsValidDayDigit(c, 1))
                                _selection.SetAndReplace(start, start + 2, "0" + c);
                        }
                        else
                        {
                            if (IsValidMonthDigit(c, 0))
                            {
                                if (length > start)
                                {
                                    _selection.SetAndReplace(start, start + 1, c.ToString());

                                    if (length > start + 1)
                                    {
                                        if (!IsValidMonth(Month))
                                        {
                                            _selection.SetAndReplace(start + 1, start + 2, GetMinMonthDigit(1).ToString());
                                            _selection.Set(start + 1, start + 2);
                                        }
                                    }
                                    AdjustMaxDay();
                                }
                                else
                                    base.HandleKeyPress(sender, e);
                            }
                            // Check if we can insert the digit with a leading zero
                            else if (length == start && GetMinMonthDigit(0) == '0' && IsValidMonthDigit(c, 1))
                                _selection.SetAndReplace(start, start + 2, "0" + c);
                        }
                        break;
                    }
                case 1:		// SECOND DIGIT
                    {
                        if (ShowDayBeforeMonth)
                        {
                            if (IsValidDayDigit(c, 1))
                            {
                                if (length > start)
                                    _selection.SetAndReplace(start, start + 1, c.ToString());
                                else
                                    base.HandleKeyPress(sender, e);
                            }
                            // Check if it's a slash and the first digit (preceded by a zero) is a valid month
                            else if (c == _separator && length == start && GetMinDayDigit(0) == '0' && IsValidDay(ToInt("0" + text[0])))
                                _selection.SetAndReplace(0, start, "0" + text[0] + c);
                        }
                        else
                        {
                            if (IsValidMonthDigit(c, 1))
                            {
                                if (length > start)
                                {
                                    _selection.SetAndReplace(start, start + 1, c.ToString());

                                    if (Day > 0 && AdjustMaxDay())
                                        _selection.Set(GetDayStartPosition(), GetDayStartPosition() + 2);
                                }
                                else
                                    base.HandleKeyPress(sender, e);
                            }
                            // Check if it's a slash and the first digit (preceded by a zero) is a valid month
                            else if (c == _separator && length == start && GetMinMonthDigit(0) == '0' && IsValidMonth(ToInt("0" + text[0])))
                                _selection.SetAndReplace(0, start, "0" + text[0] + c);
                        }
                        break;
                    }

                case 2:		// FIRST SLASH
                    {
                        int slash = 0;
                        if (c == _separator)
                            slash = 1;
                        else
                        {
                            if (ShowDayBeforeMonth)
                                slash = (IsValidMonthDigit(c, 0) || (length == start && GetMinMonthDigit(0) == '0' && IsValidMonthDigit(c, 1))) ? 2 : 0;
                            else
                                slash = (IsValidDayDigit(c, 0) || (length == start && GetMinDayDigit(0) == '0' && IsValidDayDigit(c, 1))) ? 2 : 0;
                        }

                        // If we need the slash, enter it
                        if (slash != 0)
                            _selection.SetAndReplace(start, start + 1, _separator.ToString());

                        // If the slash is to be preceded by a valid digit, "type" it in.
                        if (slash == 2)
                            SendKeys.Send(c.ToString());
                        break;
                    }

                case 3:		// THIRD DIGIT
                    {
                        if (ShowDayBeforeMonth)
                        {
                            if (IsValidMonthDigit(c, 0))
                            {
                                if (length > start)
                                {
                                    _selection.SetAndReplace(start, start + 1, c.ToString());

                                    if (length > start + 1)
                                    {
                                        if (!IsValidMonth(Month))
                                        {
                                            _selection.SetAndReplace(start + 1, start + 2, GetMinMonthDigit(1).ToString());
                                            _selection.Set(start + 1, start + 2);
                                        }
                                    }
                                }
                                else
                                    base.HandleKeyPress(sender, e);

                                AdjustMaxDay();
                            }
                            // Check if we can insert the digit with a leading zero
                            else if (length == start && GetMinMonthDigit(0) == '0' && IsValidMonthDigit(c, 1))
                            {
                                _selection.SetAndReplace(start, start + 2, "0" + c);
                                AdjustMaxDay();
                            }
                        }
                        else
                        {
                            if (IsValidDayDigit(c, 0))
                            {
                                if (length > start)
                                {
                                    _selection.SetAndReplace(start, start + 1, c.ToString());

                                    if (length > start + 1)
                                    {
                                        if (!IsValidDay(Day))
                                        {
                                            _selection.SetAndReplace(start + 1, start + 2, GetMinDayDigit(1).ToString());
                                            _selection.Set(start + 1, start + 2);
                                        }
                                    }
                                }
                                else
                                    base.HandleKeyPress(sender, e);
                            }
                            // Check if we can insert the digit with a leading zero
                            else if (length == start && GetMinDayDigit(0) == '0' && IsValidDayDigit(c, 1))
                                _selection.SetAndReplace(start, start + 2, "0" + c);
                        }
                        break;
                    }

                case 4:		// FOURTH DIGIT
                    {
                        if (ShowDayBeforeMonth)
                        {
                            if (IsValidMonthDigit(c, 1))
                            {
                                if (length > start)
                                {
                                    _selection.SetAndReplace(start, start + 1, c.ToString());

                                    if (Day > 0 && AdjustMaxDay())
                                        _selection.Set(GetDayStartPosition(), GetDayStartPosition() + 2);
                                }
                                else
                                {
                                    base.HandleKeyPress(sender, e);
                                    AdjustMaxDay();
                                }
                            }
                            // Check if it's a slash and the first digit (preceded by a zero) is a valid month
                            else if (c == _separator && length == start && GetMinMonthDigit(0) == '0' && IsValidMonth(ToInt("0" + text[3])))
                                _selection.SetAndReplace(3, start, "0" + text[3] + c);
                        }
                        else
                        {
                            if (IsValidDayDigit(c, 1))
                            {
                                if (length > start)
                                    _selection.SetAndReplace(start, start + 1, c.ToString());
                                else
                                    base.HandleKeyPress(sender, e);
                            }
                            // Check if it's a slash and the first digit (preceded by a zero) is a valid month
                            else if (c == _separator && length == start && GetMinDayDigit(0) == '0' && IsValidDay(ToInt("0" + text[3])))
                                _selection.SetAndReplace(3, start, "0" + text[3] + c);
                        }
                        break;
                    }

                case 5:		// SECOND SLASH	(year's first digit)
                    {
                        int slash = 0;
                        if (c == _separator)
                            slash = 1;
                        else
                            slash = (IsValidYearDigit(c, 0) ? 2 : 0);

                        // If we need the slash, enter it
                        if (slash != 0)
                            _selection.SetAndReplace(start, start + 1, _separator.ToString());

                        // If the slash is to be preceded by a valid digit, "type" it in.
                        if (slash == 2)
                            SendKeys.Send(c.ToString());
                        break;
                    }

                case 6:		// YEAR (all 4 digits)
                case 7:
                case 8:
                case 9:
                    {
                        if (IsValidYearDigit(c, start - GetYearStartPosition()))
                        {
                            if (length > start)
                            {
                                _selection.SetAndReplace(start, start + 1, c.ToString());

                                for (; start + 1 < length && start < 9; start++)
                                {
                                    if (!IsValidYearDigit(text[start + 1], start - (GetYearStartPosition() - 1)))
                                    {
                                        _selection.Set(start + 1, 10);
                                        StringBuilder portion = new StringBuilder();
                                        for (int iPos = start + 1; iPos < length && iPos < 10; iPos++)
                                            portion.Append(GetMinYearDigit(iPos - GetYearStartPosition(), false));

                                        _selection.Replace(portion.ToString());
                                        _selection.Set(start + 1, 10);
                                        break;
                                    }
                                }
                            }
                            else
                                base.HandleKeyPress(sender, e);

                            if (IsValidYear(Year))
                            {
                                AdjustMaxDay();			// adjust the day first
                                AdjustMaxMonthAndDay();	// then adjust the month and the day, if necessary
                            }
                        }
                        break;
                    }
            }
        }

        /// <summary>
        ///   Converts an integer value to a 2-digit string (00 - 99). </summary>
        /// <param name="value">
        ///   The number to convert. </param>
        /// <returns>
        ///   The return value is the formatted string. </returns>
        /// <remarks>
        ///   This is convenience method for formatting 2 digit
        ///   values such as the month and day. </remarks>
        protected static string TwoDigits(int value)
        {
            return String.Format("{0,2:00}", value);
        }

        /// <summary>
        ///   Retrieves the zero-based position of the month inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the month. </returns>
        /// <remarks>
        ///   This is based on whether the month is shown before or after the day. </remarks>
        protected int GetMonthStartPosition()
        {
            return ShowDayBeforeMonth ? 3 : 0;
        }

        /// <summary>
        ///   Retrieves the zero-based position of the day inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the day. </returns>
        /// <remarks>
        ///   This is based on whether the day is shown before or after the month. </remarks>
        protected int GetDayStartPosition()
        {
            return ShowDayBeforeMonth ? 0 : 3;
        }

        /// <summary>
        ///   Retrieves the zero-based position of the year inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the year. </returns>
        /// <remarks>
        ///   This is always 6. </remarks>
        protected int GetYearStartPosition()
        {
            return 6;
        }

        /// <summary>
        ///   Retrieves the maximum value for the month based on the year and the allowed range. </summary>
        /// <returns>
        ///   The return value is the maximum value for the month. </returns>
        protected int GetMaxMonth()
        {
            if (GetValidYear() == _rangeMax.Year)
                return _rangeMax.Month;
            return 12;
        }

        /// <summary>
        ///   Retrieves the minimum value for the month based on the year and the allowed range. </summary>
        /// <returns>
        ///   The return value is the minimum value for the month. </returns>
        protected int GetMinMonth()
        {
            if (GetValidYear() == _rangeMin.Year)
                return _rangeMin.Month;
            return 1;
        }

        /// <summary>
        ///   Retrieves the maximum value for the day based on the month, year, and the allowed range. </summary>
        /// <returns>
        ///   The return value is the maximum value for the day. </returns>
        protected int GetMaxDay()
        {
            int year = GetValidYear();
            int month = GetValidMonth();

            if (year == _rangeMax.Year && month == _rangeMax.Month)
                return _rangeMax.Day;

            return GetMaxDayOfMonth(month, year);
        }

        /// <summary>
        ///   Retrieves the minimum value for the day based on the month, year, and the allowed range. </summary>
        /// <returns>
        ///   The return value is the minimum value for the day. </returns>
        protected int GetMinDay()
        {
            int year = GetValidYear();
            int month = GetValidMonth();

            if (year == _rangeMin.Year && month == _rangeMin.Month)
                return _rangeMin.Day;

            return 1;
        }

        /// <summary>
        ///   Retrieves the maximum day for a given month and year. </summary>
        /// <param name="month">
        ///   The month (1 - 12). </param>
        /// <param name="year">
        ///   The year (1900 - 9999). </param>
        /// <returns>
        ///   The return value is the maximum day (1 - 31). </returns>
        protected static int GetMaxDayOfMonth(int month, int year)
        {
            Debug.Assert(month >= 1 && month <= 12);

            switch (month)
            {
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;

                case 2:
                    return DateTime.IsLeapYear(year) ? 29 : 28;
            }
            return 31;
        }

        /// <summary>
        ///   Retrieves the maximum digit that a month value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the month (0 or 1). </param>
        /// <returns>
        ///   The return value is the maximum digit that it can be. </returns>
        protected char GetMaxMonthDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);

            int year = GetValidYear();
            int maxMonth = _rangeMax.Month;
            int maxYear = _rangeMax.Year;

            // First digit
            if (position == 0)
            {
                // If the year is at the max, then use the first digit of the max month
                if (year == maxYear)
                    return TwoDigits(maxMonth)[0];

                // Otherwise, it's always '1'
                return '1';
            }

            // Second digit
            string text = _textBox.Text;
            char firstDigit = (text.Length > GetMonthStartPosition()) ? text[GetMonthStartPosition()] : '0';
            Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

            // If the year is at the max, then check if the first digits match
            if (year == maxYear && (IsValidYear(Year) || maxYear == _rangeMin.Year))
            {
                // If the first digits match, then use the second digit of the max month
                if (TwoDigits(maxMonth)[0] == firstDigit)
                    return TwoDigits(maxMonth)[1];

                // Assuming the logic for the first digit is correct, then it must be '0'
                Debug.Assert(firstDigit == '0');
                return '9';
            }

            // Use the first digit to determine the second digit's max
            return (firstDigit == '1' ? '2' : '9');
        }

        /// <summary>
        ///   Retrieves the minimum digit that a month value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the month (0 or 1). </param>
        /// <returns>
        ///   The return value is the minimum digit that it can be. </returns>
        protected char GetMinMonthDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);

            int year = GetValidYear();
            int minMonth = _rangeMin.Month;
            int minYear = _rangeMin.Year;

            // First digit
            if (position == 0)
            {
                // If the year is at the min, then use the first digit of the min month
                if (year == minYear)
                    return TwoDigits(minMonth)[0];

                // Otherwise, it's always '0'
                return '0';
            }

            // Second digit
            string text = _textBox.Text;
            char firstDigit = (text.Length > GetMonthStartPosition()) ? text[GetMonthStartPosition()] : '0';
            if (firstDigit == 0)
                return '1';

            // If the year is at the max, then check if the first digits match
            if (year == minYear && (IsValidYear(Year) || minYear == _rangeMax.Year))
            {
                // If the first digits match, then use the second digit of the max month
                if (TwoDigits(minMonth)[0] == firstDigit)
                    return TwoDigits(minMonth)[1];

                return '0';
            }

            // Use the first digit to determine the second digit's min
            return (firstDigit == '1' ? '0' : '1');
        }

        /// <summary>
        ///   Checks if a digit is valid for the month at one of its two character positions. </summary>
        /// <param name="c">
        ///   The digit to check. </param>
        /// <param name="position">
        ///   The position of the digit of the month (0 or 1). </param>
        /// <returns>
        ///   If the digit is valid for the month (at the given position), the return value is true; otherwise it is false. </returns>
        protected bool IsValidMonthDigit(char c, int position)
        {
            return (c >= GetMinMonthDigit(position) && c <= GetMaxMonthDigit(position));
        }

        /// <summary>
        ///   Checks if a month is valid -- falls within the allowed range. </summary>
        /// <param name="month">
        ///   The month to check. </param>
        /// <returns>
        ///   If the month falls within the allowed range, the return value is true; otherwise it is false. </returns>
        protected bool IsValidMonth(int month)
        {
            int year = GetValidYear();
            int day = GetValidDay();
            try
            {
                return IsWithinRange(new DateTime(year, month, day));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///   Retrieves the maximum digit that a day value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the day (0 or 1). </param>
        /// <returns>
        ///   The return value is the maximum digit that it can be. </returns>
        protected char GetMaxDayDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);

            int month = GetValidMonth();
            int year = GetValidYear();
            int maxDay = _rangeMax.Day;
            int maxMonth = _rangeMax.Month;
            int maxYear = _rangeMax.Year;

            // First digit
            if (position == 0)
            {
                // If the year and month are at the max, then use the first digit of the max day
                if (year == maxYear && month == maxMonth)
                    return TwoDigits(maxDay)[0];
                return TwoDigits(GetMaxDayOfMonth(month, year))[0];
            }

            // Second digit
            string text = _textBox.Text;
            char firstDigit = (text.Length > GetDayStartPosition()) ? text[GetDayStartPosition()] : '0';
            Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

            // If the year and month are at the max, then use the second digit of the max day
            if (year == maxYear && month == maxMonth && TwoDigits(maxDay)[0] == firstDigit)
                return TwoDigits(maxDay)[1];

            if (firstDigit == '0' ||
                firstDigit == '1' ||
                (firstDigit == '2' && month != 2) ||
                (month == 2 && !IsValidYear(Year)))
                return '9';
            return TwoDigits(GetMaxDayOfMonth(month, year))[1];
        }

        /// <summary>
        ///   Retrieves the minimum digit that a day value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the day (0 or 1). </param>
        /// <returns>
        ///   The return value is the minimum digit that it can be. </returns>
        protected char GetMinDayDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);

            int month = GetValidMonth();
            int year = GetValidYear();
            int minDay = _rangeMin.Day;
            int minMonth = _rangeMin.Month;
            int minYear = _rangeMin.Year;

            // First digit
            if (position == 0)
            {
                // If the year and month are at the min, then use the first digit of the min day
                if (year == minYear && month == minMonth)
                    return TwoDigits(minDay)[0];
                return '0';
            }

            // Second digit
            string text = _textBox.Text;
            char firstDigit = (text.Length > GetDayStartPosition()) ? text[GetDayStartPosition()] : '0';
            if (firstDigit == 0)  // must have a valid first digit at this point
                return '1';

            // If the year and month are at the max, then use the first second of the max day
            if (year == minYear && month == minMonth && TwoDigits(minDay)[0] == firstDigit)
                return TwoDigits(minDay)[1];

            // Use the first digit to determine the second digit's min
            return (firstDigit == '0' ? '1' : '0');
        }

        /// <summary>
        ///   Checks if a digit is valid for the day at one of its two character positions. </summary>
        /// <param name="c">
        ///   The digit to check. </param>
        /// <param name="position">
        ///   The position of the digit of the day (0 or 1). </param>
        /// <returns>
        ///   If the digit is valid for the day (at the given position), the return value is true; otherwise it is false. </returns>
        protected bool IsValidDayDigit(char c, int position)
        {
            return (c >= GetMinDayDigit(position) && c <= GetMaxDayDigit(position));
        }

        /// <summary>
        ///   Checks if a day is valid -- falls within the allowed range. </summary>
        /// <param name="day">
        ///   The day to check. </param>
        /// <returns>
        ///   If the day falls within the allowed range, the return value is true; otherwise it is false. </returns>
        protected bool IsValidDay(int day)
        {
            try
            {
                return IsWithinRange(new DateTime(GetValidYear(), GetValidMonth(), day));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///   Checks if a year is valid -- falls within the allowed range. </summary>
        /// <param name="year">
        ///   The year to check. </param>
        /// <returns>
        ///   If the year falls within the allowed range, the return value is true; otherwise it is false. </returns>
        protected bool IsValidYear(int year)
        {
            return (year >= _rangeMin.Year && year <= _rangeMax.Year);
        }

        /// <summary>
        ///   Adjusts the month (to the minimum) if not valid; otherwise adjusts the day (to the maximum) if not valid. </summary>
        /// <returns>
        ///   If the month and/or day gets adjusted, the return value is true; otherwise it is false. </returns>
        protected bool AdjustMaxMonthAndDay()
        {
            int month = Month;
            if (month != 0 && !IsValidMonth(month))
            {
                Month = GetMinMonth();  // this adjusts the day automatically
                return true;
            }

            return AdjustMaxDay();
        }

        /// <summary>
        ///   Adjusts the day (to the maximum) if not valid. </summary>
        /// <returns>
        ///   If the day gets adjusted, the return value is true; otherwise it is false. </returns>
        protected bool AdjustMaxDay()
        {
            int day = Day;
            if (day != 0 && !IsValidDay(day))
            {
                Day = GetMaxDay();
                return true;
            }

            return false;	// nothing had to be adjusted
        }

        /// <summary>
        ///   Retrieves the maximum digit that a year value can take, at one of its four character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the day (0 to 3). </param>
        /// <returns>
        ///   The return value is the maximum digit that it can be. </returns>
        protected char GetMaxYearDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 3);

            string yearStr = "" + Year;
            string maxYear = "" + _rangeMax.Year;

            if (position == 0 || ToInt(maxYear.Substring(0, position)) <= ToInt(yearStr.Substring(0, position)))
                return maxYear[position];
            return '9';
        }

        /// <summary>
        ///   Retrieves the minimum digit that a year value can take, at one of its four character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the day (0 to 3). </param>
        /// <param name="validYear">
        ///   If true, a valid year is used if the current one is not. </param>
        /// <returns>
        ///   The return value is the minimum digit that it can be. </returns>
        protected char GetMinYearDigit(int position, bool validYear)
        {
            Debug.Assert(position >= 0 && position <= 3);

            int year = Year;
            if (validYear && !IsValidYear(year))
                year = GetValidYear();

            string yearStr = "" + year;
            string minYear = "" + _rangeMin.Year;

            if (position == 0 || ToInt(minYear.Substring(0, position)) >= ToInt(yearStr.Substring(0, position)))
                return minYear[position];
            return '0';
        }

        /// <summary>
        ///   Checks if a digit is valid for the year at one of its four character positions. </summary>
        /// <param name="c">
        ///   The digit to check. </param>
        /// <param name="position">
        ///   The position of the digit of the day (0 to 3). </param>
        /// <returns>
        ///   If the digit is valid for the year (at the given position), the return value is true; otherwise it is false. </returns>
        protected bool IsValidYearDigit(char c, int position)
        {
            return (c >= GetMinYearDigit(position, false) && c <= GetMaxYearDigit(position));
        }

        /// <summary>
        ///   Retrieves the month on the textbox as a valid value. </summary>
        /// <returns>
        ///   The return value is the valid value for the month (1 - 12). </returns>
        /// <remarks>
        ///   The method checks the value of the month on the textbox.  
        ///   If it is within the allowed range, it returns it.  
        ///   If it is less than the minimum allowed, the minimum is returned.
        ///   If it is more than the maximum allowed, the maximum is returned. </remarks>
        protected int GetValidMonth()
        {
            int month = Month;

            // It it's outside the range, fix it
            if (month < GetMinMonth())
                month = GetMinMonth();
            else if (month > GetMaxMonth())
                month = GetMaxMonth();

            return month;
        }

        /// <summary>
        ///   Retrieves the day on the textbox as a valid value. </summary>
        /// <returns>
        ///   The return value is the valid value for the day (1 - 31). </returns>
        /// <remarks>
        ///   The method checks the value of the day on the textbox.  
        ///   If it is within the allowed range, it returns it.  
        ///   If it is less than the minimum allowed, the minimum is returned.
        ///   If it is more than the maximum allowed, the maximum is returned. </remarks>
        protected int GetValidDay()
        {
            int day = Day;

            // It it's outside the range, fix it
            if (day < GetMinDay())
                day = GetMinDay();
            else if (day > GetMaxDay())
                day = GetMaxDay();

            return day;
        }

        /// <summary>
        ///   Retrieves the year on the textbox as a valid value. </summary>
        /// <returns>
        ///   The return value is the valid value for the year. </returns>
        /// <remarks>
        ///   The method checks the value of the year on the textbox.  
        ///   If it is within the allowed range, it returns it.  
        ///   If it is less than the minimum allowed, the minimum is returned.
        ///   If it is more than the maximum allowed, the maximum is returned. </remarks>
        protected int GetValidYear()
        {
            int year = Year;
            if (year < _rangeMin.Year)
            {
                year = DateTime.Today.Year;
                if (year < _rangeMin.Year)
                    year = _rangeMin.Year;
            }
            if (year > _rangeMax.Year)
                year = _rangeMax.Year;

            return year;
        }

        /// <summary>
        ///   Gets or sets the month on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid month. </exception>
        /// <remarks>
        ///   If the month is not valid on the textbox, this property will return 0.
        ///   This property must be set with a month that falls within the allowed range. </remarks>
        /// <seealso cref="Day" />
        /// <seealso cref="Year" />
        public int Month
        {
            get
            {
                string text = _textBox.Text;

                int startPos = GetMonthStartPosition();
                int slash = text.IndexOf(_separator);

                if (startPos != 0 && slash > 0)
                    startPos = slash + 1;

                if (text.Length >= startPos + 2)
                    return ToInt(text.Substring(startPos, 2));
                return 0;
            }
            set
            {
                using (Selection.Saver savedSelection = new Selection.Saver(_textBox)) 	// remember the current selection
                {
                    if (Month > 0)		// see if there's already a month
                        _selection.Set(GetMonthStartPosition(), GetMonthStartPosition() + 3);

                    _selection.Replace(TwoDigits(value) + _separator);	// set the month

                    AdjustMaxDay();	// adjust the day if it's out of range

                    // Verify it's in range
                    if (!IsValidMonth(value))
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///   Gets or sets the day on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid day. </exception>
        /// <remarks>
        ///   If the day is not valid on the textbox, this property will return 0.
        ///   This property must be set with a day that falls within the allowed range. </remarks>
        /// <seealso cref="Month" />
        /// <seealso cref="Year" />
        public int Day
        {
            get
            {
                string text = _textBox.Text;

                int startPos = GetDayStartPosition();
                int slash = text.IndexOf(_separator);

                if (startPos != 0 && slash > 0)
                    startPos = slash + 1;

                if (text.Length >= startPos + 2)
                    return ToInt(text.Substring(startPos, 2));
                return 0;
            }
            set
            {
                // Verify it's in range
                if (!IsValidDay(value))
                    throw new ArgumentOutOfRangeException();

                using (Selection.Saver savedSelection = new Selection.Saver(_textBox))	// remember the current selection
                {
                    if (Day > 0)		// see if there's already a day
                        _selection.Set(GetDayStartPosition(), GetDayStartPosition() + 3);

                    _selection.Replace(TwoDigits(value) + _separator);	// set the day
                }
            }
        }

        /// <summary>
        ///   Gets or sets the year on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid year. </exception>
        /// <remarks>
        ///   If the year is not valid on the textbox, this property will return 0.
        ///   This property must be set with a year that falls within the allowed range. </remarks>
        /// <seealso cref="Month" />
        /// <seealso cref="Day" />
        public int Year
        {
            get
            {
                string text = _textBox.Text;
                int length = text.Length;

                int slash = text.LastIndexOf(_separator);
                if (slash > 0 && slash < length - 1)
                    return ToInt(text.Substring(slash + 1, Math.Min(4, length - slash - 1)));
                return 0;
            }
            set
            {
                // Verify it's in range
                if (!IsValidYear(value))
                    throw new ArgumentOutOfRangeException();

                using (Selection.Saver savedSelection = new Selection.Saver(_textBox))	// remember the current selection
                {
                    if (Year > 0)		// see if there's already a year
                        _selection.Set(GetYearStartPosition(), GetYearStartPosition() + 4);

                    _selection.Replace(String.Format("{0,4:0000}", value));	// set the year

                    AdjustMaxMonthAndDay();	// adjust the month and/or day if they're out of range
                }
            }
        }

        /// <summary>
        ///   Gets or sets the month, day, and year on the textbox using a <see cref="DateTime" /> object. </summary>
        /// <remarks>
        ///   This property gets and sets the <see cref="DateTime" /> boxed inside an <c>object</c>.
        ///   This makes it flexible, so that if the textbox does not hold a valid date, a <c>null</c> is returned, 
        ///   instead of having to worry about an exception being thrown. </remarks>
        /// <example><code>
        ///   object obj = txtDate.Behavior.Value;
        ///   
        ///   if (obj != null)
        ///   {
        ///     DateTime dtm = (DateTime)obj;
        ///     ...
        ///   } </code></example>
        /// <seealso cref="Month" />
        /// <seealso cref="Day" />
        /// <seealso cref="Year" />
        public object Value
        {
            get
            {
                try
                {
                    return new DateTime(Year, Month, Day);
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                DateTime dt = (DateTime)value;
                _textBox.Text = GetFormattedDate(dt.Year, dt.Month, dt.Day);
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
            Value = new DateTime(year, month, day);
        }

        /// <summary>
        ///   Checks if the textbox's date is valid and falls within the allowed range. </summary>
        /// <returns>
        ///   If the value is valid and falls within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        public override bool IsValid()
        {
            try
            {
                return IsWithinRange(new DateTime(Year, Month, Day));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///   Gets the error message used to notify the user to enter a valid date value 
        ///   within the allowed range. </summary>
        /// <seealso cref="IsValid" />
        public override string ErrorMessage
        {
            get
            {
                return "Please specify a date between " + GetFormattedDate(_rangeMin.Year, _rangeMin.Month, _rangeMin.Day) + " and " + GetFormattedDate(_rangeMax.Year, _rangeMax.Month, _rangeMax.Day) + ".";
            }
        }

        /// <summary>
        ///   Gets or sets the minimum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to DateTime(1900, 1, 1).
        ///   The range is actively checked as the user enters the date and 
        ///   when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMax" />
        public DateTime RangeMin
        {
            get
            {
                return _rangeMin;
            }
            set
            {
                if (value < new DateTime(1900, 1, 1))
                    throw new ArgumentOutOfRangeException("RangeMin", value, "Minimum value may not be older than January 1, 1900");

                _rangeMin = value;
                UpdateText();
            }
        }

        /// <summary>
        ///   Gets or sets the maximum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to <see cref="DateTime.MaxValue" />.
        ///   The range is actively checked as the user enters the date and 
        ///   when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMin" />
        public DateTime RangeMax
        {
            get
            {
                return _rangeMax;
            }
            set
            {
                _rangeMax = value;
                UpdateText();
            }
        }

        /// <summary>
        ///   Checks if a date value falls within the allowed range. </summary>
        /// <param name="dt">
        ///   The date value to check. </param>
        /// <returns>
        ///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <remarks>
        ///   Only the date portion is checked; the time is ignored. </remarks>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        /// <seealso cref="IsValid" />
        public bool IsWithinRange(DateTime dt)
        {
            DateTime date = new DateTime(dt.Year, dt.Month, dt.Day);
            return (date >= _rangeMin && date <= _rangeMax);
        }

        /// <summary>
        ///   Gets or sets the character used to separate the month, day, and year values of the date. </summary>
        /// <remarks>
        ///   By default, this property is set according to the user's system. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        public char Separator
        {
            get
            {
                return _separator;
            }
            set
            {
                if (_separator == value)
                    return;

                Debug.Assert(value != 0);
                Debug.Assert(!Char.IsDigit(value));

                _separator = value;
                UpdateText();
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
                ModifyFlags((int)Flag.DayBeforeMonth, value);
            }
        }

        /// <summary>
        ///   Retrieves the textbox's text in valid form. </summary>
        /// <returns>
        ///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
        protected override string GetValidText()
        {
            string text = _textBox.Text;

            if (text == "")
                return text;

            if (IsValid())
                return GetFormattedDate(Year, Month, Day);

            // If the date is empty, try using today
            if (Year == 0 && Month == 0 && Day == 0)
                Value = DateTime.Today;

            int year = GetValidYear();
            int month = GetValidMonth();
            int day = GetValidDay();

            if (!IsWithinRange(new DateTime(year, month, day)))
                month = GetMinMonth();

            if (!IsWithinRange(new DateTime(year, month, day)))
                day = GetMaxDay();

            return GetFormattedDate(year, month, day);
        }

        /// <summary>
        ///   Retrieves the textbox's text in valid form. </summary>
        /// <returns>
        ///   This is just an <c>internal</c> version of <see cref="GetValidText" /> designed to be 
        ///   accessed by the <see cref="DateTimeBehavior" /> class which needs it </returns>
        internal string GetValidTextForDateTime()
        {
            return GetValidText();
        }

        /// <summary>
        ///   Formats a year, month, and day value into a string based on the proper format (mm/dd/yyyy or dd/mm/yyyy). </summary>
        /// <param name="year">
        ///   The year value. </param>
        /// <param name="month">
        ///   The month value. </param>
        /// <param name="day">
        ///   The day value. </param>
        /// <returns>
        ///   The return value is the formatted date value. </returns>
        public string GetFormattedDate(int year, int month, int day)
        {
            if (ShowDayBeforeMonth)
                return String.Format("{0,2:00}{1}{2,2:00}{3}{4,4:0000}", day, _separator, month, _separator, year);
            return String.Format("{0,2:00}{1}{2,2:00}{3}{4,4:0000}", month, _separator, day, _separator, year);
        }
    }
    #endregion
}
