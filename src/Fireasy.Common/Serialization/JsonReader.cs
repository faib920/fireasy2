// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.IO;
using System.Text;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 表示可连续读取json文本的读取器。
    /// </summary>
    public sealed class JsonReader : DisposableBase
    {
        private TextReader _reader;

        /// <summary>
        /// 初始化 <see cref="JsonReader"/> 类的新实例。
        /// </summary>
        /// <param name="reader">一个 <see cref="TextReader"/> 对象。</param>
        public JsonReader(TextReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// 读取一段完整的Json文本。
        /// </summary>
        /// <returns>表示Json的文本。</returns>
        public string ReadRaw()
        {
            var c = Peek();

            //如果不以对象或集合出现，则为值或字符串类型
            if (c != '{' && c != '[')
            {
                var value = ReadValue();
                if (value == null)
                {
                    return null;
                }

                return value.ToString();
            }

            var sb = new StringBuilder();
            var p = c == '{' ? 1 : 2;
            var s = 0;
            while (true)
            {
                c = Peek();
                if (c == '\0')
                {
                    break;
                }

                sb.Append(Read());

                if (p == 1 && c == '{')
                {
                    s++;
                }
                else if (p == 1 && c == '}')
                {
                    s--;
                    if (s == 0)
                    {
                        break;
                    }
                }
                else if (p == 2 && c == '[')
                {
                    s++;
                }
                else if (p == 2 && c == ']')
                {
                    s--;
                    if (s == 0)
                    {
                        break;
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 连续跳过空白字符。
        /// </summary>
        public void SkipWhiteSpaces()
        {
            while (true)
            {
                var c = Peek();
                if (!char.IsWhiteSpace(c) && !c.IsLine())
                {
                    break;
                }

                _reader.Read();
            }
        }

        /// <summary>
        /// 读取除字符串外的一个值，比如布尔和数值型。
        /// </summary>
        /// <returns>字符串、布尔值等。</returns>
        public object ReadValue()
        {
            var c = Peek();
            if (c == JsonTokens.StringDelimiter)
            {
                return ReadAsString();
            }

            if (c == JsonTokens.StartObjectLiteralCharacter ||
                c == JsonTokens.StartArrayCharacter)
            {
                return ReadRaw();
            }

            if (c == 't' || c == 'f')
            {
                return ReadAsBoolean();
            }

            var s = ReadNonString().ToString();
            if (s == "null")
            {
                return null;
            }


            if (int.TryParse(s, out int it))
            {
                return it;
            }

            if (decimal.TryParse(s, out decimal dt))
            {
                return dt;
            }

            if (DateTime.TryParse(s, out DateTime dd))
            {
                return dd;
            }

            return null;
        }

        public long ReadAsInt16()
        {
            var s = ReadNonString().ToString();

            if (short.TryParse(s, out short it))
            {
                return it;
            }

            return 0;
        }

        public int ReadAsInt32()
        {
            var s = ReadNonString().ToString();

            if (int.TryParse(s, out int it))
            {
                return it;
            }

            return 0;
        }

        public long ReadAsInt64()
        {
            var s = ReadNonString().ToString();

            if (long.TryParse(s, out long it))
            {
                return it;
            }

            return 0;
        }

        public decimal ReadAsDecimal()
        {
            var s = ReadNonString().ToString();

            if (decimal.TryParse(s, out decimal dt))
            {
                return dt;
            }

            return 0;
        }

        public DateTime ReadAsDateTime()
        {
            var s = ReadNonString().ToString();

            if (DateTime.TryParse(s, out DateTime dd))
            {
                return dd;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// 读取一个布尔值。
        /// </summary>
        /// <returns>布尔值。</returns>
        public bool ReadAsBoolean()
        {
            var result = false;
            var c = Read();
            if (c == 't')
            {
                result = true;
            }

            while (true)
            {
                c = Peek();
                if (c == '\0' || IsDelimiter(c))
                {
                    break;
                }

                Read();
            }

            return result;
        }

        private object ReadNonString()
        {
            var sb = new StringBuilder(10);
            while (true)
            {
                var c = Peek();
                if (c == '\0' || IsDelimiter(c))
                {
                    break;
                }

                var read = _reader.Read();
                if (read >= '0' && read <= '9')
                {
                    sb.Append(read - '0');
                }
                else
                {
                    sb.Append((char)read);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 表示读出属性名。
        /// </summary>
        /// <returns></returns>
        public string ReadKey()
        {
            var c = Read();
            var quote = c == JsonTokens.StringDelimiter;
            var sb = new StringBuilder(25);
            var isEscaped = false;

            if (!quote)
            {
                sb.Append(c);
            }

            while (true)
            {
                c = Peek();
                if (c == JsonTokens.PairSeparator)
                {
                    break;
                }

                c = Read();

                if (c == '\\' && !isEscaped)
                {
                    isEscaped = true;
                    continue;
                }

                if (isEscaped)
                {
                    var s = FromEscaped(c);
                    sb.Append(s == "u" ? "\\" + s : s);
                    isEscaped = false;
                    continue;
                }

                if ((quote && c == JsonTokens.StringDelimiter) || c == ' ')
                {
                    break;
                }

                sb.Append(c);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        /// <returns>字符串。</returns>
        public string ReadAsString()
        {
            if (IsNull())
            {
                return null;
            }

            AssertAndConsume(JsonTokens.StringDelimiter);
            var sb = new StringBuilder(25);
            var isEscaped = false;

            while (true)
            {
                var c = Read();
                if (c == '\\' && !isEscaped)
                {
                    isEscaped = true;
                    continue;
                }

                if (isEscaped)
                {
                    var s = FromEscaped(c);
                    sb.Append(s == "u" ? "\\" + s : s);
                    isEscaped = false;
                    continue;
                }

                if (c == JsonTokens.StringDelimiter)
                {
                    break;
                }

                sb.Append(c);
            }

            var str = sb.ToString();
            return str == "null" ? null : str;
        }

        /// <summary>
        /// 判断指定的字符是否是界定符。
        /// </summary>
        /// <param name="c">要判断的字符。</param>
        /// <returns>如果字符是界定符，则为 true。</returns>
        public bool IsDelimiter(char c)
        {
            return (c == JsonTokens.EndObjectLiteralCharacter || c == JsonTokens.EndArrayCharacter || c == JsonTokens.ElementSeparator || IsWhiteSpace(c));
        }

        /// <summary>
        /// 判断下一个字符是否与给定的字符相符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsNextCharacter(char c)
        {
            return Peek() == c;
        }

        /// <summary>
        /// 判断指定的字符是否是空白字符。
        /// </summary>
        /// <param name="c">要判断的字符。</param>
        /// <returns>如果字符是空白字符则为 true。</returns>
        public static bool IsWhiteSpace(char c)
        {
            return char.IsWhiteSpace(c);
        }

        /// <summary>
        /// 判断是否为 null。
        /// </summary>
        /// <returns>如果为 null 则为 true。</returns>
        public bool IsNull()
        {
            var isNull = Peek() == 'n';
            if (isNull)
            {
                ReadValue();
            }

            return isNull;
        }

        /// <summary>
        /// 读取一个字符，但是指针不会后移。
        /// </summary>
        /// <returns>读取的字符。</returns>
        public char Peek()
        {
            var c = _reader.Peek();
            return ValidateChar(c);
        }

        /// <summary>
        /// 读取一个字符。
        /// </summary>
        /// <returns>读取的字符。</returns>
        public char Read()
        {
            var c = _reader.Read();
            return ValidateChar(c);
        }

        private static char ValidateChar(int c)
        {
            if (c == -1)
            {
                return '\0';
            }

            return (char)c;
        }

        private static string FromEscaped(char c)
        {
            return c switch
            {
                '"' => "\"",
                '\\' => "\\",
                'b' => "\b",
                'f' => "\f",
                'r' => "\r",
                'n' => "\n",
                't' => "\t",
                'u' => "u",
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 判断下一个字符是否是指定的字符。
        /// </summary>
        /// <param name="character">要判断的字符。</param>
        public void AssertAndConsume(char character)
        {
            SkipWhiteSpaces();

            var c = Read();
            if (c != character)
            {
                throw new SerializationException(SR.GetString(SRKind.JsonExpected, character, c));
            }
        }

        /// <summary>
        /// 判断下一个字符是否是指定结束的字符。
        /// </summary>
        /// <param name="endDelimiter">预期结束的字符。</param>
        /// <returns>如果是结束符号，则为 true。</returns>
        public bool AssertNextIsDelimiterOrSeparator(char endDelimiter)
        {
            SkipWhiteSpaces();

            var delimiter = Read();
            if (delimiter == endDelimiter)
            {
                return true;
            }

            if (delimiter == ',')
            {
                return false;
            }

            throw new SerializationException(SR.GetString(SRKind.JsonExpectedArray, delimiter));
        }

        /// <summary>
        /// 循环读取一个数组块。该块以符号 [ 开始，以符号 ] 结束。
        /// </summary>
        /// <param name="loop">应用数组单位的方法。</param>
        public void LoopReadArray(Action<JsonReader> loop)
        {
            Guard.ArgumentNull(loop, nameof(loop));

            if (IsNull())
            {
                return;
            }

            AssertAndConsume(JsonTokens.StartArrayCharacter);

            while (true)
            {
                if (Peek() == JsonTokens.EndArrayCharacter)
                {
                    AssertAndConsume(JsonTokens.EndArrayCharacter);
                    break;
                }

                loop(this);

                if (AssertNextIsDelimiterOrSeparator(JsonTokens.EndArrayCharacter))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 释放对象所占用的非托管和托管资源。
        /// </summary>
        /// <param name="disposing">为 true 则释放托管资源和非托管资源；为 false 则仅释放非托管资源。</param>
        protected override bool Dispose(bool disposing)
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }

            return base.Dispose(disposing);
        }
    }
}
