// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 相关数学函数的扩展方法。
    /// </summary>
    public static class MathExtension
    {
        /// <summary>
        /// 将小数按照指定的小数位数舍入。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <param name="roundType"></param>
        /// <returns></returns>
        public static double Round(this double value, int decimals = 2, RoundType roundType = RoundType.FourFive)
        {
            var strArray = value.ToString().Split(new[] { '.' });
            if (((decimals <= 0) || (strArray.Length < 2)) || (strArray[1].Length <= decimals))
            {
                return value;
            }

            var str2 = strArray[1].Substring(0, decimals);
            var num = Int32.Parse(strArray[1].Substring(decimals, 1));
            var num2 = Convert.ToDouble(strArray[0] + "." + str2);
            if ((roundType != RoundType.None) && (num >= (int)roundType))
            {
                var s = "0." + new string('0', decimals - 1) + "1";
                num2 += Convert.ToDouble(s);
            }

            return num2;
        }

        /// <summary>
        /// 计算一组数值的方差。
        /// </summary>
        /// <param name="array">要计算的数组。</param>
        /// <param name="weightFunc">加权函数。第一个参数为当前的数值，第二个参数 double 为数组的平均值。</param>
        /// <returns></returns>
        public static double Variance(this int[] array, Func<double, double, double> weightFunc = null)
        {
            return Variance(array.Select(s => (double)s).ToArray(), weightFunc);
        }

        /// <summary>
        /// 计算一组数值的方差。
        /// </summary>
        /// <param name="array">要计算的数组。</param>
        /// <param name="weightFunc">加权函数。第一个参数为当前的数值，第二个参数 double 为数组的平均值。</param>
        /// <returns></returns>
        public static double Variance(this decimal[] array, Func<double, double, double> weightFunc = null)
        {
            return Variance(array.Select(s => (double)s).ToArray(), weightFunc);
        }

        /// <summary>
        /// 计算一组数值的方差。
        /// </summary>
        /// <param name="array">要计算的数组。</param>
        /// <param name="weightFunc">加权函数。第一个参数为当前的数值，第二个参数 double 为数组的平均值。</param>
        /// <returns></returns>
        public static double Variance(this float[] array, Func<double, double, double> weightFunc = null)
        {
            return Variance(array.Select(s => (double)s).ToArray(), weightFunc);
        }

        /// <summary>
        /// 计算一组数值的方差。
        /// </summary>
        /// <param name="array">要计算的数组。</param>
        /// <param name="weightFunc">加权函数。第一个参数为当前的数值，第二个参数 double 为数组的平均值。</param>
        /// <returns></returns>
        public static double Variance(this double[] array, Func<double, double, double> weightFunc = null)
        {
            Guard.ArgumentNull(array, nameof(array));

            if (array.Length == 0)
            {
                return 0;
            }

            var sum = 0.0;
            var average = array.Average(s => s);
            foreach (var t in array)
            {
                var d = t - average;
                if (weightFunc != null)
                {
                    d += weightFunc(t, average);
                }

                sum += Math.Pow(d, 2);
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// 求一组数值的中位数。
        /// </summary>
        /// <param name="array">要计算的数组。</param>
        /// <returns></returns>
        public static double Median<T>(this T[] array) where T : IComparable<T>, IConvertible
        {
            Guard.ArgumentNull(array, nameof(array));

            if (array.Length == 0)
            {
                return 0;
            }

            var median = array.Length / 2;
            var array1 = array.OrderBy(s => s).ToArray();
            if (array.Length % 2 == 0)
            {
                return (array1[median - 1].ToDouble(null) +
                    array1[median].ToDouble(null)) / 2;
            }

            return array1[median].ToDouble(null);
        }
    }

    /// <summary>
    /// 小数位舍入的方式。
    /// </summary>
    public enum RoundType
    {
        /// <summary>
        /// 八舍九入。
        /// </summary>
        EightNine = 9,
        /// <summary>
        /// 五舍六入。
        /// </summary>
        FiveSix = 6,
        /// <summary>
        /// 四舍五入。
        /// </summary>
        FourFive = 5,
        /// <summary>
        /// 九舍不入。
        /// </summary>
        None = 0,
        /// <summary>
        /// 一舍二入。
        /// </summary>
        OneTow = 2,
        /// <summary>
        /// 七舍八入。
        /// </summary>
        SevenEight = 8,
        /// <summary>
        /// 六舍七入。
        /// </summary>
        SixSeven = 7,
        /// <summary>
        /// 三舍四入。
        /// </summary>
        ThreeFour = 4,
        /// <summary>
        /// 二舍三入。
        /// </summary>
        TowThree = 3,
        /// <summary>
        /// 零舍一入。
        /// </summary>
        ZeroOne = 1
    }

}
