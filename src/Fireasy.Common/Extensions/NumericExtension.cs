// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Text;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 数字相关的扩展方法。
    /// </summary>
    public static class NumericExtension
    {
        /// <summary>
        /// 获取数值的整数部份。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIntegerPart(this decimal value)
        {
            return GetIntegerPart((double)value);
        }

        /// <summary>
        /// 获取数值的整数部份。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int GetIntegerPart(this double value)
        {
            var str = value.ToString();
            if (!str.IsNumeric())
            {
                return 0;
            }
            var index = str.IndexOf(".");
            if (index == -1)
            {
                return Convert.ToInt32(value);
            }
            return Convert.ToInt32(str.Substring(0, index));
        }

        /// <summary>
        /// 获取数值的整数部份。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal GetDecimalPart(this decimal value)
        {
            return GetDecimalPart((double)value);
        }

        /// <summary>
        /// 获取数值的整数部份。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal GetDecimalPart(this double value)
        {
            var str = value.ToString();
            if (!str.IsNumeric())
            {
                return 0;
            }
            var index = str.IndexOf(".");
            if (index == -1)
            {
                return 0;
            }
            return Convert.ToDecimal(str.Substring(index));
        }

        /// <summary>
        /// 将数字转换为用人民币大写表示的字符串。
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static string ToUpper(this decimal money)
        {
            return ToUpper((double)money);
        }

        /// <summary>
        /// 将数字转换为用人民币大写表示的字符串。
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public static string ToUpper(this double money)
        {
            if (!money.ToString().IsNumeric())
            {
                return string.Empty;
            }

            long money100;
            try
            {
                money100 = (long)(money.To<double>(0.0) * 100);
            }
            catch
            {
                throw new OverflowException();
            }
            if (money100 == long.MinValue)
            {
                throw new OverflowException();
            }
            if (money100 == 0)
            {
                return "零元";
            }

            const string digit = "零壹贰叁肆伍陆柒捌玖";
            var isAllZero = true;
            var isPreZero = true;
            var output = new StringBuilder();
            var unit = new[] { "元", "万", "亿", "万", "亿亿" };
            var value = Math.Abs(money100);

            ParseMoneySection(output, true, digit, ref value, ref isAllZero, ref isPreZero);
            for (var i = 0; i < unit.Length && value > 0; i++)
            {
                if (isPreZero && !isAllZero)
                {
                    output.Append(digit[0]);
                }
                if (i == 4 && output.ToString().EndsWith(unit[2]))
                {
                    output.Remove(output.Length - unit[2].Length, unit[2].Length);
                }
                output.Append(unit[i]);

                ParseMoneySection(output, false, digit, ref value, ref isAllZero, ref isPreZero);

                if ((i % 2) == 1 && isAllZero)
                {
                    output.Remove(output.Length - unit[i].Length, unit[i].Length);
                }
            }
            if (money100 < 0)
            {
                output.Append("负");
            }
            return ReverseMoneyString(output);
        }

        private static void ParseMoneySection(StringBuilder output, bool isJiaoFen, string digit, ref long value, ref bool isAllZero, ref bool isPreZero)
        {
            var unit1 = isJiaoFen ? new[] { "分", "角" } : new[] { "", "拾", "佰", "仟" };
            isAllZero = true;
            for (var i = 0; i < unit1.Length && value > 0; i++)
            {
                var d = (int)(value % 10);
                if (d != 0)
                {
                    if (isPreZero && !isAllZero)
                    {
                        output.Append(digit[0]);
                    }
                    output.AppendFormat("{0}{1}", unit1[i], digit[d]);
                    isAllZero = false;
                }
                isPreZero = (d == 0);
                value /= 10;
            }
        }

        private static string ReverseMoneyString(StringBuilder output)
        {
            var reversed = new StringBuilder();
            for (var i = output.Length - 1; i >= 0; i--)
            {
                reversed.Append(output[i]);
            }
            return reversed.ToString();
        }
    }
}
