using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    #region TimeBehavior
    public class TimeBehavior : Behavior
    {
        // Fields
        private DateTime _rangeMin = new DateTime(1900, 1, 1, 0, 0, 0);
        private DateTime _rangeMax = new DateTime(1900, 1, 1, 23, 59, 59);
        private char _separator = ':';
        private string _am = "AM";
        private string _pm = "PM";
        private int _ampmLength = 2;

        /// <summary>
        ///   The starting zero-based position of the hour on the texbox. </summary>
        /// <remarks>
        ///   This is 0 by default, however it may be changed to allow 
        ///   another value to be placed in front of the time, such as a date. </remarks>
        protected int _hourStart = 0;

        /// <summary>
        ///   Internal values that are added/removed to the <see cref="Behavior.Flags" /> property by other
        ///   properties of this class. </summary>
        [Flags]
        protected enum Flag
        {
            /// <summary> The hour is shown in 24-hour format (00 to 23). </summary>
            /// <seealso cref="Show24HourFormat" />
            TwentyFourHourFormat = 0x00020000,

            /// <summary> The seconds are also shown. </summary>
            /// <seealso cref="ShowSeconds" />
            WithSeconds = 0x00040000,
        };

        /// <summary>
        ///   Initializes a new instance of the TimeBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor determines the <see cref="Separator" /> and time format from the user's system. </remarks>
        /// <seealso cref="System.Windows.Forms.TextBoxBase" />	
        public TimeBehavior(TextBoxBase textBox)
            :
            base(textBox, true)
        {
            // Get the system's time separator
            _separator = DateTimeFormatInfo.CurrentInfo.TimeSeparator[0];

            // Determine if it's in 24-hour format
            string shortTime = DateTimeFormatInfo.CurrentInfo.ShortTimePattern;
            if (shortTime.IndexOf('H') >= 0)
                _flags |= (int)Flag.TwentyFourHourFormat;

            // Get the AM and PM symbols
            _am = DateTimeFormatInfo.CurrentInfo.AMDesignator;
            _pm = DateTimeFormatInfo.CurrentInfo.PMDesignator;
            _ampmLength = _am.Length;

            // Verify the lengths are the same; otherwise use the default
            if (_ampmLength == 0 || _ampmLength != _pm.Length)
            {
                _am = "AM";
                _pm = "PM";
                _ampmLength = 2;
            }
        }

        /// <summary>
        ///   Initializes a new instance of the TimeBehavior class by copying it from 
        ///   another TimeBehavior object. </summary>
        /// <param name="behavior">
        ///   The TimeBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public TimeBehavior(TimeBehavior behavior)
            :
            base(behavior)
        {
            _rangeMin = behavior._rangeMin;
            _rangeMax = behavior._rangeMax;
            _separator = behavior._separator;
            _am = behavior._am;
            _pm = behavior._pm;
            _ampmLength = behavior._ampmLength;
            _hourStart = behavior._hourStart;
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
            TraceLine("TimeBehavior.HandleKeyDown " + e.KeyCode);

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

                        if (start >= GetHourStartPosition() && start <= GetHourStartPosition() + 2)
                        {
                            int hour = Hour;
                            if (hour >= GetMinHour(false))
                            {
                                // Handle moving up through the noon hour
                                string ampm = AMPM;
                                if (IsValidAMPM(ampm))
                                {
                                    if (hour == 11)
                                    {
                                        if (ampm == _pm)  // stop at midnight
                                            return;
                                        SetAMPM(false);
                                    }
                                    else if (hour == 12)
                                        hour = 0;
                                }

                                if (hour < GetMaxHour(false))
                                    Hour = ++hour;
                            }
                        }
                        else if (start >= GetMinuteStartPosition() && start <= GetMinuteStartPosition() + 2)
                        {
                            int minute = Minute;
                            if (minute >= GetMinMinute() && minute < GetMaxMinute())
                                Minute = ++minute;
                        }
                        else if (start >= GetAMPMStartPosition() && start <= GetAMPMStartPosition() + _ampmLength)
                        {
                            string ampm = AMPM;
                            SetAMPM(!IsValidAMPM(ampm) || ampm == _pm);
                        }
                        else if (start >= GetSecondStartPosition() && start <= GetSecondStartPosition() + 2)
                        {
                            int second = Second;
                            if (second >= GetMinSecond() && second < GetMaxSecond())
                                Second = ++second;
                        }

                        return;
                    }

                case Keys.Down:
                    {
                        // If pressing the DOWN arrow, decrement the corresponding value.

                        _selection.Get(out int start, out int end);

                        if (start >= GetHourStartPosition() && start <= GetHourStartPosition() + 2)
                        {
                            int hour = Hour;
                            if (hour <= GetMaxHour(false))
                            {
                                // Handle moving up through the noon hour
                                string ampm = AMPM;
                                if (IsValidAMPM(ampm))
                                {
                                    if (hour == 12)
                                    {
                                        if (ampm == _am)	// stop at midnight
                                            return;
                                        SetAMPM(true);
                                    }
                                    else if (hour == 1)
                                        hour = 13;
                                }

                                if (hour > GetMinHour(false))
                                    Hour = --hour;
                            }
                        }
                        else if (start >= GetMinuteStartPosition() && start <= GetMinuteStartPosition() + 2)
                        {
                            int minute = Minute;
                            if (minute > GetMinMinute() && minute <= GetMaxMinute())
                                Minute = --minute;
                        }
                        else if (start >= GetAMPMStartPosition() && start <= GetAMPMStartPosition() + _ampmLength)
                        {
                            string ampm = AMPM;
                            SetAMPM(!IsValidAMPM(ampm) || ampm == _pm);
                        }
                        else if (start >= GetSecondStartPosition() && start <= GetSecondStartPosition() + 2)
                        {
                            int second = Second;
                            if (second > GetMinSecond() && second <= GetMaxSecond())
                                Second = --second;
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
            TraceLine("TimeBehavior.HandleKeyPress " + e.KeyChar);

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
            if (start == _hourStart)		// FIRST DIGIT
            {
                if (IsValidHourDigit(c, 0))
                {
                    if (length > start)
                    {
                        _selection.SetAndReplace(start, start + 1, c.ToString());

                        if (length > start + 1)
                        {
                            // If the second digit is no longer valid, correct and select it
                            if (!IsValidHour(Hour, false))
                            {
                                _selection.SetAndReplace(start + 1, start + 2, GetMinHourDigit(1).ToString());
                                _selection.Set(start + 1, start + 2);
                            }
                        }
                    }
                    else
                        base.HandleKeyPress(sender, e);
                }
                else if (length == start && IsValidHourDigit(c, 1))
                    _selection.SetAndReplace(start, start + 2, "0" + c);
                else
                    ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 1)	// SECOND DIGIT
            {
                if (IsValidHourDigit(c, 1))
                {
                    if (length > start)
                        _selection.SetAndReplace(start, start + 1, c.ToString());
                    else
                        base.HandleKeyPress(sender, e);
                }
                else if (c == _separator && length == start && IsValidHour(ToInt("0" + text[_hourStart]), false))
                    _selection.SetAndReplace(_hourStart, start, "0" + text[_hourStart] + c);
                else
                    ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 2)	// FIRST COLON
            {
                int colon = 0;
                if (c == _separator)
                    colon = 1;
                else
                    colon = (IsValidMinuteDigit(c, 0) ? 2 : 0);

                // If we need the colon, enter it
                if (colon != 0)
                    _selection.SetAndReplace(start, start + 1, _separator.ToString());

                // If the colon is to be preceded by a valid digit, "type" it in.
                if (colon == 2)
                    SendKeys.Send(c.ToString());
                else
                    ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 3)	// THIRD DIGIT
            {
                if (IsValidMinuteDigit(c, 0))
                {
                    if (length > start)
                    {
                        _selection.SetAndReplace(start, start + 1, c.ToString());

                        if (length > start + 1)
                        {
                            if (!IsValidMinute(Minute))
                            {
                                _selection.SetAndReplace(start + 1, start + 2, GetMinMinuteDigit(1).ToString());
                                _selection.Set(start + 1, start + 2);
                            }
                        }
                    }
                    else
                        base.HandleKeyPress(sender, e);
                }
                else
                    ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 4)	// FOURTH DIGIT
            {
                if (IsValidMinuteDigit(c, 1))
                {
                    if (length > start)
                        _selection.SetAndReplace(start, start + 1, c.ToString());
                    else
                        base.HandleKeyPress(sender, e);

                    // Show the AM/PM symbol if we're not showing seconds
                    if (!ShowSeconds)
                        ShowAMPM();
                }
                else
                    ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 5)	// SECOND COLON	OR FIRST SPACE (seconds' first digit or AM/PM)
            {
                if (ShowSeconds)
                {
                    int colon = 0;
                    if (c == _separator)
                        colon = 1;
                    else
                        colon = (IsValidSecondDigit(c, 0) ? 2 : 0);

                    // If we need the slash, enter it
                    if (colon != 0)
                    {
                        int replace = (start < length && text[start] != ' ') ? 1 : 0;
                        _selection.SetAndReplace(start, start + replace, _separator.ToString());
                    }

                    // If the colon is to be preceded by a valid digit, "type" it in.
                    if (colon == 2)
                        SendKeys.Send(c.ToString());
                }
                else if (!Show24HourFormat)
                {
                    if (c == ' ')
                        _selection.SetAndReplace(start, start + 1, c.ToString());
                    ShowAMPM();
                }

                ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 6)	// FIFTH DIGIT - first digit of seconds or AM/PM
            {
                if (ShowSeconds)
                {
                    if (IsValidSecondDigit(c, 0))
                    {
                        if (length > start)
                        {
                            int replace = (start < length && text[start] != ' ') ? 1 : 0;
                            _selection.SetAndReplace(start, start + replace, c.ToString());
                        }
                        else
                            base.HandleKeyPress(sender, e);
                    }
                }

                ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 7)	// SIXTH DIGIT - second digit of seconds or AM/PM
            {
                if (ShowSeconds)
                {
                    if (IsValidSecondDigit(c, 1))
                    {
                        if (length > start)
                        {
                            int replace = (start < length && text[start] != ' ') ? 1 : 0;
                            _selection.SetAndReplace(start, start + replace, c.ToString());
                        }
                        else
                            base.HandleKeyPress(sender, e);

                        // Show the AM/PM symbol if we're not in 24-hour format
                        ShowAMPM();
                    }
                }

                ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else if (start == _hourStart + 8)	// FIRST SPACE (with seconds showing)
            {
                if (ShowSeconds && !Show24HourFormat)
                {
                    if (c == ' ')
                    {
                        _selection.SetAndReplace(start, start + 1, c.ToString());
                        ShowAMPM();
                    }
                }

                ChangeAMPM(c);	// allow changing AM/PM (if it's being shown) by pressing A or P
            }

            else 		// AM/PM
                ChangeAMPM(c);
        }

        /// <summary>
        ///   Converts an integer value to a 2-digit string (00 - 99). </summary>
        /// <param name="value">
        ///   The number to convert. </param>
        /// <returns>
        ///   The return value is the formatted string. </returns>
        /// <remarks>
        ///   This is convenience method for formatting 2 digit
        ///   values such as the hour and minute. </remarks>
        protected static string TwoDigits(int value)
        {
            return String.Format("{0,2:00}", value);
        }

        /// <summary>
        ///   Retrieves the zero-based position of the hour inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the hour. </returns>
        protected int GetHourStartPosition()
        {
            return _hourStart;
        }

        /// <summary>
        ///   Retrieves the zero-based position of the minute inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the minute. </returns>
        protected int GetMinuteStartPosition()
        {
            return _hourStart + 3;
        }

        /// <summary>
        ///   Retrieves the zero-based position of the second inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the second. </returns>
        protected int GetSecondStartPosition()
        {
            return _hourStart + 6;
        }

        /// <summary>
        ///   Retrieves the zero-based position of the AM/PM inside the texbox. </summary>
        /// <returns>
        ///   The return value is the starting position of the AM/PM. </returns>
        /// <remarks>
        ///   This is based on whether the seconds are being shown or not. </remarks>
        protected int GetAMPMStartPosition()
        {
            return _hourStart + (ShowSeconds ? 9 : 6);
        }

        /// <summary>
        ///   Retrieves the maximum value for the hour. </summary>
        /// <param name="force24HourFormat">
        ///   If true, the maximum is 23, regardless of the <see cref="Show24HourFormat" /> property; 
        ///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
        /// <returns>
        ///   The return value is the maximum value for the hour (23 or 12). </returns>
        /// <remarks>
        ///   Note: This value is not based on <see cref="RangeMax" />. </remarks>
        protected int GetMaxHour(bool force24HourFormat)
        {
            return (force24HourFormat || Show24HourFormat) ? 23 : 12;
        }

        /// <summary>
        ///   Retrieves the minimum value for the hour. </summary>
        /// <param name="force24HourFormat">
        ///   If true, the minimum is 0, regardless of the <see cref="Show24HourFormat" /> property; 
        ///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
        /// <returns>
        ///   The return value is the minimum value for the hour (0 or 1). </returns>
        /// <remarks>
        ///   Note: This value is not based on <see cref="RangeMin" />. </remarks>
        protected int GetMinHour(bool force24HourFormat)
        {
            return (force24HourFormat || Show24HourFormat) ? 0 : 1;
        }

        /// <summary>
        ///   Retrieves the maximum value for the minute: 59. </summary>
        /// <returns>
        ///   The return value is always 59. </returns>
        /// <remarks>
        ///   Note: This value is not based on <see cref="RangeMax" />. </remarks>
        protected int GetMaxMinute()
        {
            return 59;
        }

        /// <summary>
        ///   Retrieves the minimum value for the minute: 0. </summary>
        /// <returns>
        ///   The return value is always 0. </returns>
        /// <remarks>
        ///   Note: This value is not based on <see cref="RangeMin" />. </remarks>
        protected int GetMinMinute()
        {
            return 0;
        }

        /// <summary>
        ///   Retrieves the maximum value for the second: 59. </summary>
        /// <returns>
        ///   The return value is always 59. </returns>
        /// <remarks>
        ///   Note: This value is not based on <see cref="RangeMax" />. </remarks>
        protected int GetMaxSecond()
        {
            return 59;
        }

        /// <summary>
        ///   Retrieves the minimum value for the second: 0. </summary>
        /// <returns>
        ///   The return value is always 0. </returns>
        /// <remarks>
        ///   Note: This value is not based on <see cref="RangeMin" />. </remarks>
        protected int GetMinSecond()
        {
            return 0;
        }

        /// <summary>
        ///   Retrieves the maximum digit that an hour value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the hour (0 or 1). </param>
        /// <returns>
        ///   The return value is the maximum digit that it can be. </returns>
        protected char GetMaxHourDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);

            // First digit
            if (position == 0)
                return Show24HourFormat ? '2' : '1';

            // Second digit
            string text = _textBox.Text;
            char firstDigit = (text.Length > GetHourStartPosition()) ? text[GetHourStartPosition()] : '0';
            Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

            // Use the first digit to determine the second digit's max
            if (firstDigit == '2')
                return '3';
            if (firstDigit == '1' && !Show24HourFormat)
                return '2';
            return '9';
        }

        /// <summary>
        ///   Retrieves the minimum digit that an hour value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the hour (0 or 1). </param>
        /// <returns>
        ///   The return value is the minimum digit that it can be. </returns>
        protected char GetMinHourDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);

            // First digit
            if (position == 0)
                return '0';

            // Second digit
            string text = _textBox.Text;
            char firstDigit = (text.Length > GetHourStartPosition()) ? text[GetHourStartPosition()] : '0';
            Debug.Assert(firstDigit != 0);  // must have a valid first digit at this point

            // If the first digit is a 0 and we're not in 24-hour format, don't allow 0
            if (firstDigit == '0' && !Show24HourFormat)
                return '1';

            // For all other cases it's always 0
            return '0';
        }

        /// <summary>
        ///   Checks if a digit is valid for the hour at one of its two character positions. </summary>
        /// <param name="c">
        ///   The digit to check. </param>
        /// <param name="position">
        ///   The position of the digit of the hour (0 or 1). </param>
        /// <returns>
        ///   If the digit is valid for the hour (at the given position), the return value is true; otherwise it is false. </returns>
        protected bool IsValidHourDigit(char c, int position)
        {
            return (c >= GetMinHourDigit(position) && c <= GetMaxHourDigit(position));
        }

        /// <summary>
        ///   Checks if a value represents a valid hour. </summary>
        /// <param name="hour">
        ///   The value to check. </param>
        /// <param name="force24HourFormat">
        ///   If true, the range is based on a 24-hour format, regardless of the <see cref="Show24HourFormat" /> property; 
        ///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
        /// <returns>
        ///   If the value is a valid hour, the return value is true; otherwise it is false. </returns>
        protected bool IsValidHour(int hour, bool force24HourFormat)
        {
            return (hour >= GetMinHour(force24HourFormat) && hour <= GetMaxHour(force24HourFormat));
        }

        /// <summary>
        ///   Retrieves the maximum digit that a minute value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the minute (0 or 1). </param>
        /// <returns>
        ///   The return value is the maximum digit that it can be. </returns>
        protected char GetMaxMinuteDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);
            return (position == 0 ? '5' : '9');
        }

        /// <summary>
        ///   Retrieves the minimum digit that a minute value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the minute (0 or 1). </param>
        /// <returns>
        ///   The return value is the minimum digit that it can be. </returns>
        protected char GetMinMinuteDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);
            return '0';
        }

        /// <summary>
        ///   Checks if a digit is valid for the minute at one of its two character positions. </summary>
        /// <param name="c">
        ///   The digit to check. </param>
        /// <param name="position">
        ///   The position of the digit of the minute (0 or 1). </param>
        /// <returns>
        ///   If the digit is valid for the minute (at the given position), the return value is true; otherwise it is false. </returns>
        protected bool IsValidMinuteDigit(char c, int position)
        {
            return (c >= GetMinMinuteDigit(position) && c <= GetMaxMinuteDigit(position));
        }

        /// <summary>
        ///   Checks if a value represents a valid minute. </summary>
        /// <param name="minute">
        ///   The value to check. </param>
        /// <returns>
        ///   If the value is a valid minute, the return value is true; otherwise it is false. </returns>
        protected bool IsValidMinute(int minute)
        {
            return (minute >= GetMinMinute() && minute <= GetMaxMinute());
        }

        /// <summary>
        ///   Retrieves the maximum digit that a "second" value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the second (0 or 1). </param>
        /// <returns>
        ///   The return value is the maximum digit that it can be. </returns>
        protected char GetMaxSecondDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);
            return (position == 0 ? '5' : '9');
        }

        /// <summary>
        ///   Retrieves the minimum digit that a "second" value can take, at one of its two character positions. </summary>
        /// <param name="position">
        ///   The position of the digit of the second (0 or 1). </param>
        /// <returns>
        ///   The return value is the minimum digit that it can be. </returns>
        protected char GetMinSecondDigit(int position)
        {
            Debug.Assert(position >= 0 && position <= 1);
            return '0';
        }

        /// <summary>
        ///   Checks if a digit is valid for the "second" at one of its two character positions. </summary>
        /// <param name="c">
        ///   The digit to check. </param>
        /// <param name="position">
        ///   The position of the digit of the second (0 or 1). </param>
        /// <returns>
        ///   If the digit is valid for the second (at the given position), the return value is true; otherwise it is false. </returns>
        protected bool IsValidSecondDigit(char c, int position)
        {
            return (c >= GetMinSecondDigit(position) && c <= GetMaxSecondDigit(position));
        }

        /// <summary>
        ///   Checks if a value represents a valid second. </summary>
        /// <param name="second">
        ///   The value to check. </param>
        /// <returns>
        ///   If the value is a valid second, the return value is true; otherwise it is false. </returns>
        protected bool IsValidSecond(int second)
        {
            return (second >= GetMinSecond() && second <= GetMaxSecond());
        }

        /// <summary>
        ///   Shows the AM symbol if not in 24-hour format and it's not already shown. </summary>
        protected void ShowAMPM()
        {
            if (!Show24HourFormat && !IsValidAMPM(AMPM))
                SetAMPM(true);
        }

        /// <summary>
        ///   Sets the AM or PM symbol if not in 24-hour format. </summary>
        /// <param name="am">
        ///   If true, sets the AM symbol; otherwise it sets the PM symbol. </param>
        /// <seealso cref="AMPM" />
        public void SetAMPM(bool am)
        {
            if (Show24HourFormat)
                return;

            using (Selection.Saver savedSelection = new Selection.Saver(_textBox))	// remember the current selection
            {
                _selection.Set(GetAMPMStartPosition() - 1, GetAMPMStartPosition() + _ampmLength);
                _selection.Replace(" " + (am ? _am : _pm));	// set the AM/PM
            }
        }

        /// <summary>
        ///   Changes the AM/PM symbol based on a character entered by the user. </summary>
        /// <param name="c">
        ///   The character entered by the user, such as 'a' or 'p'. </param>
        /// <returns>
        ///   If the AM/PM symbol is changed, the return value is true; otherwise it is false. </returns>
        protected bool ChangeAMPM(char c)
        {
            if (Show24HourFormat)
                return false;

            string text = _textBox.Text;
            int length = text.Length;

            int position = GetAMPMPosition(text);
            if (position == 0)
                return false;

            _selection.Get(out int start, out int end);

            char cUpper = Char.ToUpper(c);

            switch (cUpper)
            {
                case 'A':
                case 'P':
                    SetAMPM(cUpper == 'A');

                    if (cUpper == Char.ToUpper(_am[0]) || cUpper == Char.ToUpper(_pm[0]))
                    {
                        // Move the cursor right, if we're in front of the AM/PM symbols
                        if (start == position)
                            SendKeys.Send("{RIGHT}");

                        // Move the cursor right twice, if we're in front of the space in front of the AM/PM symbols
                        if (start + 1 == position)
                        {
                            SendKeys.Send("{RIGHT}");
                            SendKeys.Send("{RIGHT}");
                        }
                    }
                    return true;

                default:
                    // Handle entries after the first character of the AM/PM symbol -- allow the user to enter each character
                    if (start > position)
                    {
                        // Check if we're adding a character of the AM/PM symbol (after the first one)
                        if ((length == start && !IsValidAMPM(AMPM)) || (length == end && end != start))
                        {
                            string ampmToUse = Char.ToUpper(text[position]) == Char.ToUpper(_am[0]) ? _am : _pm;
                            if (cUpper == Char.ToUpper(ampmToUse[start - position]))
                            {
                                _selection.Replace(ampmToUse.Substring(start - position));	// set the rest of the AM/PM
                                _selection.Set(start, start);  // Reset the selection so that the cursor can be moved
                                return ChangeAMPM(c); // move the cursor (below)
                            }
                        }

                        // Check if the AM/PM symbol is OK and we just need to move over one
                        if (length > start && end == start && cUpper == Char.ToUpper(text[start]))
                        {
                            SendKeys.Send("{RIGHT}");
                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        ///   Retrieves the zero-based position of the AM/PM symbol on a given text. </summary>
        /// <param name="text">
        ///   The text to parse and find the position of the AM/PM symbol. </param>
        /// <returns>
        ///   The return value is the zero-based position of the AM/PM symbol. </returns>
        private int GetAMPMPosition(string text)
        {
            int position = text.IndexOf(' ' + _am);
            return ((position < 0) ? text.IndexOf(' ' + _pm) : position) + 1;
        }

        /// <summary>
        ///   Checks if a string is a valid AM or PM symbol. </summary>
        /// <param name="ampm">
        ///   The value to check. </param>
        /// <returns>
        ///   If the value is a valid AM or PM symbol, the return value is true; otherwise it is false. </returns>
        protected bool IsValidAMPM(string ampm)
        {
            return (ampm == _am || ampm == _pm);
        }

        /// <summary>
        ///   Retrieves the hour on the textbox as a valid value. </summary>
        /// <param name="force24HourFormat">
        ///   If true, the value is validated based on a 24-hour format, regardless of the <see cref="Show24HourFormat" /> property; 
        ///   otherwise it is based on the <see cref="Show24HourFormat" /> property. </param>
        /// <returns>
        ///   The return value is the valid value for the hour. </returns>
        /// <remarks>
        ///   The method checks the value of the hour on the textbox.  
        ///   If it is a valid hour, it returns it.  
        ///   If it is less than it should be, the minimum is returned.
        ///   If it is more than it should be, the maximum is returned. </remarks>
        protected int GetValidHour(bool force24HourFormat)
        {
            int hour = Hour;

            // It it's outside the range, fix it
            if (hour < GetMinHour(force24HourFormat))
                hour = GetMinHour(force24HourFormat);
            else if (hour > GetMaxHour(force24HourFormat))
                hour = GetMaxHour(force24HourFormat);

            return hour;
        }

        /// <summary>
        ///   Retrieves the minute on the textbox as a valid value. </summary>
        /// <returns>
        ///   The return value is the valid value for the minute. </returns>
        /// <remarks>
        ///   The method checks the value of the minute on the textbox.  
        ///   If it is a valid minute, it returns it.  
        ///   If it is less than it should be, the minimum is returned.
        ///   If it is more than it should be, the maximum is returned. </remarks>
        protected int GetValidMinute()
        {
            int minute = Minute;

            // It it's outside the range, fix it
            if (minute < GetMinMinute())
                minute = GetMinMinute();
            else if (minute > GetMaxMinute())
                minute = GetMaxMinute();

            return minute;
        }

        /// <summary>
        ///   Retrieves the second on the textbox as a valid value. </summary>
        /// <returns>
        ///   The return value is the valid value for the second. </returns>
        /// <remarks>
        ///   The method checks the value of the second on the textbox.  
        ///   If it is a valid second, it returns it.  
        ///   If it is less than it should be, the minimum is returned.
        ///   If it is more than it should be, the maximum is returned. </remarks>
        protected int GetValidSecond()
        {
            int second = Second;
            if (second < GetMinSecond())
                second = GetMinSecond();
            else if (second > GetMaxSecond())
                second = GetMaxSecond();

            return second;
        }

        /// <summary>
        ///   Retrieves the AM/PM symbol on the textbox as a valid value. </summary>
        /// <returns>
        ///   The return value is the valid value for the AM/PM symbol. </returns>
        /// <remarks>
        ///   The method checks the value of the AM/PM symbol on the textbox.  
        ///   If it is a valid AM/PM symbol, it returns it; otherwise it returns the AM symbol. </remarks>
        protected string GetValidAMPM()
        {
            string ampm = AMPM;
            if (!IsValidAMPM(ampm))
                return _am;

            return ampm;
        }

        /// <summary>
        ///   Gets or sets the hour on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid hour. </exception>
        /// <remarks>
        ///   If the hour is not valid on the textbox, this property will return -1.
        ///   This property must be set with a valid hour -- between 0 and 23. </remarks>
        /// <seealso cref="Minute" />
        /// <seealso cref="Second" />
        public int Hour
        {
            get
            {
                string text = _textBox.Text;

                int startPos = GetHourStartPosition();

                // If there's already a separator, extract the value in front of it
                int sepPos = text.IndexOf(_separator);
                if (sepPos > 0)
                {
                    startPos = sepPos - 2;
                    if (startPos < 0)
                        startPos = 0;
                }

                if (text.Length >= startPos + 1)
                    return ToInt(text.Substring(startPos, 2).Trim());

                return -1;
            }
            set
            {
                if (!IsValidHour(value, false))
                    throw new ArgumentOutOfRangeException();

                using (Selection.Saver savedSelection = new Selection.Saver(_textBox))	// remember the current selection
                {
                    if (Hour >= 0)		// see if there's already an hour
                        _selection.Set(GetHourStartPosition(), GetHourStartPosition() + 3);

                    // Convert it to AM/PM hour if necessary
                    string ampm = "";
                    if (!Show24HourFormat && value > 12)
                        value = ConvertToAMPMHour(value, out ampm);

                    _selection.Replace(TwoDigits(value) + _separator);	// set the hour

                    // Change the AM/PM if it's present
                    if (ampm != "" && IsValidAMPM(AMPM))
                        SetAMPM(ampm == _am);
                }
            }
        }

        /// <summary>
        ///   Gets or sets the minute on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid minute. </exception>
        /// <remarks>
        ///   If the minute is not valid on the textbox, this property will return -1.
        ///   This property must be set with a valid minute -- between 0 and 59. </remarks>
        /// <seealso cref="Hour" />
        /// <seealso cref="Second" />
        public int Minute
        {
            get
            {
                string text = _textBox.Text;
                int startPos = text.IndexOf(_separator, _hourStart) + 1;

                if (startPos > 0 && text.Length >= startPos + 2)
                    return ToInt(text.Substring(startPos, 2));

                return -1;
            }
            set
            {
                if (!IsValidMinute(value))
                    throw new ArgumentOutOfRangeException();

                using (Selection.Saver savedSelection = new Selection.Saver(_textBox))	// remember the current selection
                {
                    if (Minute >= 0)		// see if there's already a minute
                        _selection.Set(GetMinuteStartPosition(), GetMinuteStartPosition() + 2 + (ShowSeconds ? 1 : 0));

                    string text = TwoDigits(value);
                    if (ShowSeconds)
                        text += _separator;

                    _selection.Replace(text);	// set the minute

                    // Append the AM/PM if no seconds come after and it's not in 24-hour format
                    if (!ShowSeconds)
                        ShowAMPM();
                }
            }
        }

        /// <summary>
        ///   Gets or sets the second on the textbox. </summary>
        /// <exception cref="ArgumentOutOfRangeException">Setting this property with an invalid second. </exception>
        /// <remarks>
        ///   If the second is not valid on the textbox, this property will return -1.
        ///   This property must be set with a valid second -- between 0 and 59. </remarks>
        /// <seealso cref="Hour" />
        /// <seealso cref="Minute" />
        public int Second
        {
            get
            {
                string text = _textBox.Text;
                int startPos = text.IndexOf(_separator, _hourStart);
                if (startPos > 0)
                {
                    startPos = text.IndexOf(_separator, startPos + 1) + 1;
                    if (startPos == 0)
                        return -1;
                }

                if (text.Length >= startPos + 2 && Char.IsDigit(text[startPos]) && Char.IsDigit(text[startPos + 1]))
                    return ToInt(text.Substring(startPos, 2));

                return -1;
            }
            set
            {
                if (!IsValidSecond(value))
                    throw new ArgumentOutOfRangeException();

                if (!ShowSeconds)
                    return;

                using (Selection.Saver savedSelection = new Selection.Saver(_textBox))	// remember the current selection
                {
                    if (Second >= 0)		// see if there's already a second
                        _selection.Set(GetSecondStartPosition(), GetSecondStartPosition() + 2);

                    _selection.Replace(TwoDigits(value));	// set the second

                    // Append the AM/PM if it's not in 24-hour format
                    ShowAMPM();
                }
            }
        }

        /// <summary>
        ///   Gets AM/PM symbol on the textbox. </summary>
        /// <remarks>
        ///   If the AM/PM symbol is not valid or is not being shown on the textbox, this property will return an empty string. </remarks>
        /// <seealso cref="Hour" />
        /// <seealso cref="Minute" />
        /// <seealso cref="Second" />
        public string AMPM
        {
            get
            {
                string text = _textBox.Text;
                int position = GetAMPMPosition(text);
                if (position > 0)
                    return text.Substring(position);
                return "";
            }
        }

        /// <summary>
        ///   Converts an hour in 12-hour format and its AM/PM symbol to its 
        ///   24-hour equivalent. </summary>
        /// <param name="hour">
        ///   The hour value to convert, in 12-hour format (1 to 12). </param>
        /// <param name="ampm">
        ///   The AM/PM symbol which denotes if the hour is between 0 and 11, or 12 and 23. </param>
        /// <returns>
        ///   The return value is the hour converted to 24-hour format (0 to 23). </returns>
        /// <seealso cref="ConvertToAMPMHour" />
        protected int ConvertTo24Hour(int hour, string ampm)
        {
            if (ampm == _pm && hour >= 1 && hour <= 11)
                hour += 12;
            else if (ampm == _am && hour == 12)
                hour = 0;
            return hour;
        }

        /// <summary>
        ///   Converts an hour in 24-hour format to its 12-hour equivalent. </summary>
        /// <param name="hour">
        ///   The hour value to convert, in 24-hour format (0 to 23). </param>
        /// <param name="ampm">
        ///   The returned AM/PM symbol, used to denote if the hour was between 0 and 11, or 12 and 23. </param>
        /// <returns>
        ///   The return value is the hour converted to 12-hour format (1 to 12). </returns>
        /// <seealso cref="ConvertTo24Hour" />
        protected int ConvertToAMPMHour(int hour, out string ampm)
        {
            ampm = _am;

            if (hour >= 12)
            {
                hour -= 12;
                ampm = _pm;
            }
            if (hour == 0)
                hour = 12;

            return hour;
        }

        /// <summary>
        ///   Gets or sets the hour, minute, and second on the textbox using a <see cref="DateTime" /> object. </summary>
        /// <remarks>
        ///   This property gets and sets the <see cref="DateTime" /> boxed inside an <c>object</c>.
        ///   This makes it flexible, so that if the textbox does not hold a valid time, a <c>null</c> is returned, 
        ///   instead of having to worry about an exception being thrown. </remarks>
        /// <example><code>
        ///   object obj = txtTime.Behavior.Value;
        ///   if (obj != null)
        ///   {
        ///     DateTime dtm = (DateTime)obj;
        ///     ...
        ///   } </code></example>
        /// <seealso cref="Hour" />
        /// <seealso cref="Minute" />
        /// <seealso cref="Second" />
        public object Value
        {
            get
            {
                try
                {
                    if (Show24HourFormat)
                        return new DateTime(1900, 1, 1, Hour, Minute, GetValidSecond());
                    return new DateTime(1900, 1, 1, ConvertTo24Hour(Hour, AMPM), Minute, GetValidSecond());
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                DateTime dt = (DateTime)value;
                _textBox.Text = GetFormattedTime(dt.Hour, dt.Minute, dt.Second, "");
            }
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
        public void SetTime(int hour, int minute, int second)
        {
            Value = new DateTime(1900, 1, 1, hour, minute, second);
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
        public void SetTime(int hour, int minute)
        {
            SetTime(hour, minute, 0);
        }

        /// <summary>
        ///   Checks if the textbox's time is valid and falls within the allowed range. </summary>
        /// <returns>
        ///   If the value is valid and falls within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        public override bool IsValid()
        {
            return IsValid(true);
        }

        /// <summary>
        ///   Checks if the textbox's time is valid and optionally if it falls within the allowed range. </summary>
        /// <param name="checkRangeAlso">
        ///   If true, the time is also checked that it falls within allowed range. </param>
        /// <returns>
        ///   If the value is valid, the return value is true; otherwise it is false. </returns>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        public bool IsValid(bool checkRangeAlso)
        {
            // Check that we have a valid hour and minute
            int hour = Hour;
            int minute = Minute;
            if (hour < 0 || minute < 0)
                return false;

            // Check that the seconds are valid if being shown
            int second = Second;
            bool showingSeconds = ShowSeconds;
            if (showingSeconds != (second >= 0))
                return false;

            // Check the AM/PM portion
            string ampm = AMPM;
            bool force24HourFormat = Show24HourFormat;
            if ((force24HourFormat && ampm != "") ||
                (!force24HourFormat && (ampm != _am && ampm != _pm)))
                return false;

            if (!force24HourFormat && ampm == _pm)
            {
                hour += 12;
                if (hour == 24)
                    hour = 0;
            }
            if (!showingSeconds)
                second = _rangeMin.Second; // avoids possible problem when checking range below

            // Check the range if desired
            if (checkRangeAlso)
                return IsWithinRange(new DateTime(1900, 1, 1, hour, minute, second));
            return true;
        }

        /// <summary>
        ///   Gets the error message used to notify the user to enter a valid time value 
        ///   within the allowed range. </summary>
        /// <seealso cref="IsValid" />
        public override string ErrorMessage
        {
            get
            {
                return "Please specify a time between " + GetFormattedTime(_rangeMin.Hour, _rangeMin.Minute, _rangeMin.Second, "") + " and " + GetFormattedTime(_rangeMax.Hour, _rangeMax.Minute, _rangeMax.Second, "") + ".";
            }
        }

        /// <summary>
        ///   Gets or sets the minimum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to DateTime(1900, 1, 1, 0, 0, 0),
        ///   however the range is only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMax" />
        public DateTime RangeMin
        {
            get
            {
                return _rangeMin;
            }
            set
            {
                _rangeMin = new DateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);
            }
        }

        /// <summary>
        ///   Gets or sets the maximum value allowed. </summary>
        /// <remarks>
        ///   By default, this property is set to DateTime(1900, 1, 1, 23, 59, 59),
        ///   however the range is only checked when the control loses focus if one of the <see cref="ValidatingFlag" /> flags is set. </remarks>	
        /// <seealso cref="RangeMin" />
        public DateTime RangeMax
        {
            get
            {
                return _rangeMax;
            }
            set
            {
                _rangeMax = new DateTime(1900, 1, 1, value.Hour, value.Minute, value.Second);
            }
        }

        /// <summary>
        ///   Checks if a time value falls within the allowed range. </summary>
        /// <param name="dt">
        ///   The time value to check. </param>
        /// <returns>
        ///   If the value is within the allowed range, the return value is true; otherwise it is false. </returns>
        /// <remarks>
        ///   Only the time portion is checked; the date is ignored. </remarks>
        /// <seealso cref="RangeMin" />
        /// <seealso cref="RangeMax" />
        /// <seealso cref="IsValid" />
        public bool IsWithinRange(DateTime dt)
        {
            DateTime time = new DateTime(1900, 1, 1, dt.Hour, dt.Minute, dt.Second);
            return (time >= _rangeMin && time <= _rangeMax);
        }

        /// <summary>
        ///   Gets or sets the character used to separate the hour, minute, and second values of the time. </summary>
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
        ///   Gets or sets whether the hour should be shown in 24-hour format. </summary>
        /// <remarks>
        ///   By default, this property is set according to the user's system. 
        ///   If the format is set to 12-hour format the AM/PM symbols are also shown; otherwise they are are not shown.
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="Flag.TwentyFourHourFormat" />
        public bool Show24HourFormat
        {
            get
            {
                return HasFlag((int)Flag.TwentyFourHourFormat);
            }
            set
            {
                ModifyFlags((int)Flag.TwentyFourHourFormat, value);
            }
        }

        /// <summary>
        ///   Gets or sets whether the seconds should be shown (after the minutes). </summary>
        /// <remarks>
        ///   By default, this property is set to false, so that the seconds are not shown. 
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="Flag.WithSeconds" />
        public bool ShowSeconds
        {
            get
            {
                return HasFlag((int)Flag.WithSeconds);
            }
            set
            {
                ModifyFlags((int)Flag.WithSeconds, value);
            }
        }

        /// <summary>
        ///   Sets the symbols to use for AM and PM. </summary>
        /// <param name="am">
        ///   The symbol to use for AM. </param>
        /// <param name="pm">
        ///   The symbol to use for PM. </param>
        /// <exception cref="ArgumentException">The lengths of the parameters are not the same. </exception>
        /// <remarks>
        ///   By default, the symbols are set according to the user's system. 
        ///   This method allows them to be changed, however, they must both be identical in length.
        ///   If the symbols are changed, <see cref="Behavior.UpdateText" /> is automatically called. </remarks>
        /// <seealso cref="GetAMPMSymbols" />
        public void SetAMPMSymbols(string am, string pm)
        {
            if (_am == am && _pm == pm)
                return;

            // Make sure they're the same length
            if (am.Length != pm.Length)
                throw new ArgumentException("The length of the AM and PM symbols must be identical.");

            _am = am;
            _pm = pm;

            if (_am == "")
                _am = "AM";
            if (_pm == "")
                _pm = "PM";

            _ampmLength = _am.Length;
            UpdateText();
        }

        /// <summary>
        ///   Gets the symbols used for AM and PM. </summary>
        /// <param name="am">
        ///   The symbol used for AM. </param>
        /// <param name="pm">
        ///   The symbol used for PM. </param>
        /// <seealso cref="SetAMPMSymbols" />
        public void GetAMPMSymbols(out string am, out string pm)
        {
            am = _am;
            pm = _pm;
        }

        /// <summary>
        ///   Retrieves the textbox's text in valid form. </summary>
        /// <returns>
        ///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
        protected override string GetValidText()
        {
            string text = _textBox.Text;

            // If it's empty or has a valid time, return it
            if (text == "")
                return text;

            if (IsValid(false))
                return GetFormattedTime(Hour, Minute, Second, AMPM);

            // If the hour, minute, and second are invalid, set it to the current time
            if (Hour < 0 && Minute < 0 && Second < 0)
            {
                DateTime dt = DateTime.Now;
                return GetFormattedTime(dt.Hour, dt.Minute, dt.Second, "");
            }

            // Otherwise retrieve the validated time
            return GetFormattedTime(GetValidHour(true), GetValidMinute(), GetValidSecond(), AMPM);
        }

        /// <summary>
        ///   Formats an hour, minute, second, and AM/PM value into a string based on the proper format. </summary>
        /// <param name="hour">
        ///   The hour value. </param>
        /// <param name="minute">
        ///   The minute value. </param>
        /// <param name="second">
        ///   The second value. </param>
        /// <param name="ampm">
        ///   The AM/PM value, which may be empty if the hour is in 24-hour format. </param>
        /// <returns>
        ///   The return value is the formatted time value. </returns>
        public string GetFormattedTime(int hour, int minute, int second, string ampm)
        {
            if (Show24HourFormat)
            {
                // Handle switching from AM/PM to 24-hour format
                if (IsValidAMPM(ampm))
                    hour = ConvertTo24Hour(hour, ampm);
            }
            else
            {
                // Handle switching from 24-hour format to AM/PM
                if (!IsValidAMPM(ampm))
                    hour = ConvertToAMPMHour(hour, out ampm);
            }

            if (ShowSeconds)
            {
                if (Show24HourFormat)
                    return String.Format("{0,2:00}{1}{2,2:00}{3}{4,2:00}", hour, _separator, minute, _separator, second);
                return String.Format("{0,2:00}{1}{2,2:00}{3}{4,2:00} {5}", hour, _separator, minute, _separator, second, ampm);
            }

            if (Show24HourFormat)
                return String.Format("{0,2:00}{1}{2,2:00}", hour, _separator, minute);
            return String.Format("{0,2:00}{1}{2,2:00} {3}", hour, _separator, minute, ampm);
        }

        /// <summary>
        ///   Adjusts the textbox's value to be within the range of allowed values. </summary>
        protected void AdjustWithinRange()
        {
            // Check if it's already within the range
            if (IsValid())
                return;

            // If it's empty, set it to the current time
            if (_textBox.Text == "")
                _textBox.Text = " ";
            else
                UpdateText();

            // Make it fall within the range
            DateTime date = (DateTime)Value;
            if (date < _rangeMin)
                Value = _rangeMin;
            else if (date > _rangeMax)
                Value = _rangeMax;
        }
    }
    #endregion

}
