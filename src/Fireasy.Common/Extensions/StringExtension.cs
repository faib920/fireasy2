// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.VisualBasic;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 字符串相关的扩展方法。
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// 截取字符串左边的n个字符。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string source, int length)
        {
            if ((source != null) && (source.Length > length))
            {
                return source.Substring(0, length);
            }
            return source;
        }

        /// <summary>
        /// 截取字符串右边的n个字符。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Right(this string source, int length)
        {
            if ((source != null) && (source.Length > length))
            {
                return source.Substring(source.Length - length);
            }
            return source;
        }

        /// <summary>
        /// 判断字符串是否满足通配符。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static bool Like(this string source, string pattern)
        {
            return Regex.IsMatch(source, pattern);
        }

        /// <summary>
        /// 获取非 Unicode 下的字符串长度，即一个汉字占两个长度。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int GetAnsiLength(this string source)
        {
            var count = Regex.Matches(source, "([\u4e00-\u9fa5])").Count;
            return source.Length - count + count * 2;
        }

        /// <summary>
        /// 将字符串转换为全角编码 SBC。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToSBC(this string source)
        {
            var c = source.ToCharArray();
            for (var i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                {
                    c[i] = (char)(c[i] + 65248);
                }
            }
            return new string(c);
        }

        /// <summary>
        /// 将字符串转换为半角编码 DBC。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToDBC(this string source)
        {
            var c = source.ToCharArray();
            for (var i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                {
                    c[i] = (char)(c[i] - 65248);
                }
            }
            return new string(c);
        }

        /// <summary>
        /// 单词变成单数形式。
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToSingular(this string word)
        {
            if (IsSample(word))
            {
                return word;
            }

            var plural1 = new Regex("(?<keep>[^aeiou])ies$");
            var plural2 = new Regex("(?<keep>[aeiou]y)s$");
            var plural3 = new Regex("(?<keep>[sxzh])es$");
            var plural4 = new Regex("(?<keep>[^sxzhyu])s$");

            if (plural1.IsMatch(word))
            {
                return plural1.Replace(word, "${keep}y");
            }
            else if (plural2.IsMatch(word))
            {
                return plural2.Replace(word, "${keep}");
            }
            else if (plural3.IsMatch(word))
            {
                return plural3.Replace(word, "${keep}");
            }
            else if (plural4.IsMatch(word))
            {
                return plural4.Replace(word, "${keep}");
            }

            return word;
        }

        /// <summary>
        /// 单词变成复数形式。
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static string ToPlural(this string word)
        {
            if (IsSample(word))
            {
                return word;
            }

            var plural1 = new Regex("(?<keep>[^aeiou])y$");
            var plural2 = new Regex("(?<keep>[aeiou]y)$");
            var plural3 = new Regex("(?<keep>[sxzh])$");
            var plural4 = new Regex("(?<keep>[^sxzhy])$");

            if (plural1.IsMatch(word))
            {
                return plural1.Replace(word, "${keep}ies");
            }
            else if (plural2.IsMatch(word))
            {
                return plural2.Replace(word, "${keep}s");
            }
            else if (plural3.IsMatch(word))
            {
                return plural3.Replace(word, "${keep}es");
            }
            else if (plural4.IsMatch(word))
            {
                return plural4.Replace(word, "${keep}s");
            }

            return word;
        }

        /// <summary>
        /// 判断指定的字符是否是数字。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string source)
        {
            return Regex.IsMatch(source, @"^([+-]?)\d*\.?\d+$");
        }

        /// <summary>
        /// 判断指定的字符是否是整数。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsInteger(this string source)
        {
            return Regex.IsMatch(source, @"^([+-]?)\d+$");
        }

        /// <summary>
        /// 将形如 \u3232 类似的字符串转换为字符串显示。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string DeUnicode(this string source)
        {
            Match m;
            var r = new Regex("(?<code>\\\\u[[0-9a-fA-F]{4})", RegexOptions.IgnoreCase);
            for (m = r.Match(source); m.Success; m = m.NextMatch())
            {
                var strValue = m.Result("${code}");   //代码
#if NETSTANDARD && !NETSTANDARD2_0
                var charNum = Int32.Parse(strValue.AsSpan(2, 4), System.Globalization.NumberStyles.HexNumber);
#else
                var charNum = Int32.Parse(strValue.Substring(2, 4), System.Globalization.NumberStyles.HexNumber);
#endif
                var ch = string.Format("{0}", (char)charNum);
                source = source.Replace(strValue, ch);
            }
            return source;
        }

        /// <summary>
        /// 判断指定的字符串是否与表达式匹配。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <param name="ignorCase"></param>
        /// <returns></returns>
        public static bool IsMatch(this string source, string pattern, bool ignorCase = false)
        {
            return Regex.IsMatch(source, pattern, ignorCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        /// <summary>
        /// 将字符串转换为简体中文表示。
        /// </summary>
        /// <param name="source">要转换的字符串。</param>
        /// <returns></returns>
        public static string ToSimplified(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

#if !NETSTANDARD
            return Strings.StrConv(source, VbStrConv.SimplifiedChinese);
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// 将字符串转换为繁体中文表示。
        /// </summary>
        /// <param name="source">要转换的字符串。</param>
        /// <returns></returns>
        public static string ToTraditional(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

#if !NETSTANDARD
            return Strings.StrConv(source, VbStrConv.TraditionalChinese);
#else
            throw new NotSupportedException();
#endif
        }

        /// <summary>
        /// 将中文字符转换为汉语拼音的首字母。
        /// </summary>
        /// <param name="source">将要转换的字符串。</param>
        /// <returns>转换后的字符串。</returns>
        public static string ToPinyin(this string source)
        {
            return ChineseSpellHelper.GetPinyinFirstLetter(source);
        }

        /// <summary>
        /// 将中文字符转换为汉语拼音的全拼。
        /// </summary>
        /// <param name="source">将要转换的字符串。</param>
        /// <returns>转换后的字符串。</returns>
        public static string ToFullPinyin(this string source)
        {
            return ChineseSpellHelper.GetPinyin(source);
        }

        /// <summary>
        /// 获取字符串中换行的个数。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int GetLines(this string source)
        {
            var regex = new Regex("(\r\n)");
            return regex.Matches(source).Count + 1;
        }

        private static bool IsSample(string word)
        {
            var words = new[] { "people", "deer", "sheep" };
            return words.Contains(word.ToLower());
        }
    }
}
