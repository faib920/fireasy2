using System;
using System.Windows.Forms;
using System.Collections;
using System.Text;

namespace Fireasy.Windows.Forms
{
    #region MaskBehavior
    public class MaskBehavior : Behavior
    {
        // Fields
        private string m_mask;
        private ArrayList m_symbols = new ArrayList();

        /// <summary>
        ///   Initializes a new instance of the MaskedBehavior class by associating it with a TextBoxBase derived object. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <remarks>
        ///   This constructor sets the mask to an empty string, so that anything is allowed. </remarks>
        /// <seealso cref="Mask" />
        public MaskBehavior(TextBoxBase textBox)
            :
            this(textBox, "")
        {
        }

        /// <summary>
        ///   Initializes a new instance of the MaskedBehavior class by associating it with a TextBoxBase derived object and setting its mask. </summary>
        /// <param name="textBox">
        ///   The TextBoxBase object to associate with this behavior.  It must not be null. </param>
        /// <param name="mask">
        ///   The mask string to use for validating and/or formatting the characters entered by the user. 
        ///   By default, the <c>#</c> symbol is configured to represent a digit placeholder on the mask. </param>
        /// <example><c>MaskedBehavior behavior = new MaskedBehavior(txtPhoneNumber, "###-####"); </c></example>
        /// <exception cref="ArgumentNullException">textBox is null. </exception>
        /// <seealso cref="Mask" />
        /// <seealso cref="Symbols" />
        public MaskBehavior(TextBoxBase textBox, string mask)
            :
            base(textBox, true)
        {
            m_mask = mask;

            // Add the default numeric symbol
            m_symbols.Add(new Symbol('#', new Symbol.ValidatorMethod(Char.IsDigit)));
        }

        /// <summary>
        ///   Initializes a new instance of the MaskedBehavior class by copying it from 
        ///   another MaskedBehavior object. </summary>
        /// <param name="behavior">
        ///   The MaskedBehavior object to copied (and then disposed of).  It must not be null. </param>
        /// <exception cref="ArgumentNullException">behavior is null. </exception>
        /// <remarks>
        ///   After the behavior.TextBox object is copied, Dispose is called on the behavior parameter. </remarks>
        public MaskBehavior(MaskBehavior behavior)
            :
            base(behavior)
        {
            m_mask = behavior.m_mask;
            m_symbols = behavior.m_symbols;
        }

        /// <summary>
        ///   Gets or sets the mask. </summary>
        /// <remarks>
        ///   This string is used for validating and/or formatting the characters entered by the user. 
        ///   By default, the <c>#</c> symbol is configured to represent a digit placeholder on the mask. 
        ///   Thus, each '#' symbol in the mask represents a digit, and any other characters between the 
        ///   # symbols are automatically filled-in as the user types digits. 
        ///   <para>
        ///   If this property is changed, <see cref="Behavior.UpdateText" /> is automatically called. </para></remarks>
        /// <seealso cref="Symbols" />
        public string Mask
        {
            get
            {
                return m_mask;
            }
            set
            {
                if (m_mask == value)
                    return;

                m_mask = value;
                UpdateText();
            }
        }

        /// <summary>
        ///   Gets the ArrayList of Symbol objects. </summary>
        /// <remarks>
        ///   This array will initially contain one record: the one for the <c>#</c> symbol, which represents a digit placeholder on the mask. 
        ///   However, more Symbol objects can be easily added to the array to make the mask more powerful. </remarks>
        /// <example><code>
        ///   MaskedBehavior behavior = new MaskedBehavior(txtSerialNumber, "^#^-^##-###");
        ///   
        ///   // Add the ^ symbol to only allow letters and to convert them to upper-case. 
        ///   MaskedBehavior.Symbol.ValidatorMethod validator = new MaskedBehavior.Symbol.ValidatorMethod(Char.IsLetter); 
        ///   MaskedBehavior.Symbol.FormatterMethod formatter = new MaskedBehavior.Symbol.FormatterMethod(Char.ToUpper)));
        ///   behavior.Symbols.Add(new MaskedBehavior.Symbol('^', validator, formatter)); </code></example>
        /// <seealso cref="Mask" />
        /// <seealso cref="Symbol" />
        public ArrayList Symbols
        {
            get
            {
                return m_symbols;
            }
        }

        /// <summary>
        ///   Retrieves the textbox's value without any non-numeric characters. </summary>
        public string NumericText
        {
            get
            {
                string text = m_textBox.Text;
                StringBuilder numericText = new StringBuilder();

                foreach (char c in text)
                {
                    if (Char.IsDigit(c))
                        numericText.Append(c);
                }

                return numericText.ToString();
            }
        }

        /// <summary>
        ///   Represents a character which may be added to the mask and then interpreted by the <see cref="MaskedBehavior" /> class 
        ///   to validate the input from the user and possibly format it to something else. </summary>
        public class Symbol
        {
            /// <summary>
            ///   Definition for the method used to check if the character entered by the user corresponds 
            ///   with this object's symbol. </summary>
            /// <seealso cref="Validator" />
            /// <seealso cref="Validate" />
            /// <seealso cref="FormatterMethod" />
            public delegate bool ValidatorMethod(char c);

            /// <summary>
            ///   Definition for the method used to format the character entered by the user to a different character, if needed. </summary>
            /// <seealso cref="Formatter" />
            /// <seealso cref="Format" />
            /// <seealso cref="ValidatorMethod" />
            public delegate char FormatterMethod(char c);

            /// <summary>
            ///   Event used to check if the character entered by the user corresponds 
            ///   with this object's symbol. </summary>
            /// <seealso cref="ValidatorMethod" />
            /// <seealso cref="Validate" />
            /// <seealso cref="Formatter" />
            public event ValidatorMethod Validator;

            /// <summary>
            ///   Event used to format the character entered by the user to a different character, if needed. </summary>
            /// <seealso cref="FormatterMethod" />
            /// <seealso cref="Format" />
            /// <seealso cref="Validator" />
            public event FormatterMethod Formatter;

            // The symbol's character
            private char m_symbol;

            /// <summary>
            ///   Initializes a new instance of the Symbol class by associating it with a character. </summary>
            /// <param name="symbol">
            ///   The character that is represented by this object in the mask string. </param>
            /// <remarks>
            ///   This constructor sets the validator and formatter methods to null. </remarks>
            /// <seealso cref="MaskedBehavior" />
            public Symbol(char symbol)
                :
                this(symbol, null, null)
            {
            }

            /// <summary>
            ///   Initializes a new instance of the Symbol class by associating it with a character and 
            ///   a validator method. </summary>
            /// <param name="symbol">
            ///   The character that is represented by this object in the mask string. </param>
            /// <param name="validator">
            ///   The method called to check if the character entered by the user corresponds 
            ///   with this object's symbol. </param>
            /// <remarks>
            ///   This constructor sets the formatter method to null, meaning that the character 
            ///   entered by the user is not formatted. </remarks>
            /// <seealso cref="MaskedBehavior" />
            public Symbol(char symbol, ValidatorMethod validator)
                :
                this(symbol, validator, null)
            {
            }

            /// <summary>
            ///   Initializes a new instance of the Symbol class by associating it with a character and 
            ///   a validator method. </summary>
            /// <param name="symbol">
            ///   The character that is represented by this object in the mask string. </param>
            /// <param name="validator">
            ///   The method called to check if the character entered by the user corresponds 
            ///   with this object's symbol. </param>
            /// <param name="formatter">
            ///   The method called to format the character entered by the user to a different character, if needed. </param>
            /// <seealso cref="MaskedBehavior" />
            public Symbol(char symbol, ValidatorMethod validator, FormatterMethod formatter)
            {
                m_symbol = symbol;
                Validator = validator;
                Formatter = formatter;
            }

            /// <summary>
            ///   Checks if the character entered by the user corresponds with this object's symbol. </summary>
            /// <param name="c">
            ///   The character entered by the user that needs to be checked. </param>
            /// <returns>
            ///   If the character entered by the user is a valid representation of the symbol, 
            ///   the return value is true; otherwise it is false. </returns>
            /// <remarks>
            ///   This method may be overriden by derived classes to provide custom validation logic. </remarks> 
            /// <seealso cref="Format" />
            public virtual bool Validate(char c)
            {
                if (Validator != null)
                {
                    foreach (ValidatorMethod validator in Validator.GetInvocationList())
                    {
                        if (!validator(c))
                            return false;
                    }
                }
                return true;
            }

            /// <summary>
            ///   Formats the character entered by the user to a different character. </summary>
            /// <param name="c">
            ///   The character entered by the user that will be formatted. </param>
            /// <returns>
            ///   The reformatted character, as a string.  This allows derived classes more formatting flexibility if needed. </returns>
            /// <remarks>
            ///   This method may be overriden by derived classes to provide custom formatting logic. 
            ///   If no formatter method was associated with this object, the character is returned intact. </remarks> 
            /// <seealso cref="Validate" />
            public virtual string Format(char c)
            {
                if (Formatter != null)
                    return Formatter(c).ToString();
                return c.ToString();
            }

            /// <summary>
            ///   Gets or sets the character for this symbol. </summary>
            public char Char
            {
                get
                {
                    return m_symbol;
                }
                set
                {
                    m_symbol = value;
                }
            }

            /// <summary>
            ///   Allows converting/casting a Symbol object to its character representation. </summary>
            /// <example><code>
            ///   MaskedBehavior.Symbol s = new MaskedBehavior.Symbol('#');
            ///   char c = s; </code></example>
            /// <seealso cref="Char" />
            public static implicit operator char(Symbol s)
            {
                return s.Char;
            }
        }

        /// <summary>
        ///   Retrieves the textbox's text in valid form. </summary>
        /// <returns>
        ///   If the textbox's text is valid, it is returned; otherwise a valid version of it is returned. </returns>
        protected override string GetValidText()
        {
            string text = m_textBox.Text;
            int maskLength = m_mask.Length;

            // If the mask is empty, allow anything
            if (maskLength == 0)
                return text;

            StringBuilder validText = new StringBuilder();
            int symbolCount = m_symbols.Count;

            // Accomodate the text to the mask as much as possible
            for (int iPos = 0, iMaskPos = 0, length = text.Length; iPos < length; iPos++, iMaskPos++)
            {
                char c = text[iPos];
                char cMask = (iMaskPos < maskLength ? m_mask[iMaskPos] : (char)0);

                // If we've reached the end of the mask, break
                if (cMask == 0)
                    break;

                int iSymbol = 0;

                // Match the character to any of the symbols
                for (; iSymbol < symbolCount; iSymbol++)
                {
                    Symbol symbol = (Symbol)m_symbols[iSymbol];

                    // Find the symbol that applies for the given character
                    if (!symbol.Validate(c))
                        continue;

                    // Try to add matching characters in the mask until a different symbol is reached
                    for (; iMaskPos < maskLength; iMaskPos++)
                    {
                        cMask = m_mask[iMaskPos];
                        if (cMask == (char)symbol)
                        {
                            validText.Append(symbol.Format(c));
                            break;
                        }
                        else
                        {
                            int iSymbol2 = 0;
                            for (; iSymbol2 < symbolCount; iSymbol2++)
                            {
                                Symbol symbol2 = (Symbol)m_symbols[iSymbol2];
                                if (cMask == (char)symbol2)
                                {
                                    validText.Append(symbol.Format(c));
                                    break;
                                }
                            }

                            if (iSymbol2 < symbolCount)
                                break;

                            validText.Append(cMask);
                        }
                    }

                    break;
                }

                // If the character was not matched to a symbol, stop
                if (iSymbol == symbolCount)
                {
                    if (c == cMask)
                    {
                        // Match the character to any of the symbols
                        for (iSymbol = 0; iSymbol < symbolCount; iSymbol++)
                        {
                            Symbol symbol = (Symbol)m_symbols[iSymbol];
                            if (cMask == (char)symbol)
                                break;
                        }

                        if (iSymbol == symbolCount)
                        {
                            validText.Append(c);
                            continue;
                        }
                    }

                    break;
                }
            }

            return validText.ToString();
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
            TraceLine("MaskedBehavior.HandleKeyDown " + e.KeyCode);

            if (e.KeyCode == Keys.Delete)
            {
                // If deleting make sure it's the last character or that
                // the selection goes all the way to the end of the text

                int start, end;
                m_selection.Get(out start, out end);

                string text = m_textBox.Text;
                int length = text.Length;

                if (end != length)
                {
                    if (!(end == start && end == length - 1))
                        e.Handled = true;
                }
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
            TraceLine("MaskedBehavior.HandleKeyPress " + e.KeyChar);

            // Check to see if it's read only
            if (m_textBox.ReadOnly)
                return;

            char c = e.KeyChar;
            e.Handled = true;

            // If the mask is empty, allow anything
            int maskLength = m_mask.Length;
            if (maskLength == 0)
            {
                base.HandleKeyPress(sender, e);
                return;
            }

            int start, end;
            m_selection.Get(out start, out end);

            // Check that we haven't gone past the mask's length
            if (start >= maskLength && c != (short)Keys.Back)
                return;

            string text = m_textBox.Text;
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

            char cMask = m_mask[start];

            // Check if the mask's character matches with any of the symbols in the array.
            foreach (Symbol symbol in m_symbols)
            {
                if (cMask == (char)symbol)
                {
                    if (symbol.Validate(c))
                    {
                        end = (end == length ? end : (start + 1));
                        m_selection.SetAndReplace(start, end, symbol.Format(c));
                    }
                    return;
                }
            }

            // Check if it's the same character as the mask.
            if (cMask == c)
            {
                end = (end == length ? end : (start + 1));
                m_selection.SetAndReplace(start, end, c.ToString());
                return;
            }

            // Concatenate all the mask symbols
            StringBuilder concatenatedSymbols = new StringBuilder();
            foreach (Symbol symbol in m_symbols)
                concatenatedSymbols.Append((char)symbol);

            char[] symbolChars = concatenatedSymbols.ToString().ToCharArray();

            // If it's a valid character, find the next symbol on the mask and add any non-mask characters in between.
            foreach (Symbol symbol in m_symbols)
            {
                // See if the character is valid for any other symbols
                if (!symbol.Validate(c))
                    continue;

                string maskPortion = m_mask.Substring(start);
                int maskPos = maskPortion.IndexOfAny(symbolChars);

                // Enter the character if there isn't another symbol before it
                if (maskPos >= 0 && maskPortion[maskPos] == (char)symbol)
                {
                    m_selection.SetAndReplace(start, start + maskPos, maskPortion.Substring(0, maskPos));
                    HandleKeyPress(sender, e);
                    return;
                }
            }
        }
    }
    #endregion
}
