// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 日期相关的扩展方法。
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// 转换为日期的起始时刻。
        /// </summary>
        /// <param name="time">当前的日期。</param>
        /// <returns>日期在0点0分0秒的时刻。</returns>
        public static DateTime StartOfDay(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
        }

        /// <summary>
        ///转换为日期的终止时刻。
        /// </summary>
        /// <param name="time">当前的日期。</param>
        /// <returns>日期在23点59分59秒的时刻。</returns>
        public static DateTime EndOfDay(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
        }

        /// <summary>
        /// 获取当前日期中本月的第一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本月的第一天。</returns>
        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, date.Hour, date.Minute, date.Second);
        }

        /// <summary>
        /// 获取当前日期中本月的最后一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本月的最后一天。</returns>
        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.AddMonths(1).StartOfMonth().AddDays(-1);
        }

        /// <summary>
        /// 获取当前日期中本周的第一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本周的第一天。</returns>
        public static DateTime StartOfWeek(this DateTime date)
        {
            var startDayOfWeek = DayOfWeek.Monday;
            if (date.DayOfWeek != startDayOfWeek)
            {
                var d = startDayOfWeek - date.DayOfWeek;
                return startDayOfWeek <= date.DayOfWeek ? date.AddDays(d) :
                    date.AddDays(-7 + d);
            }

            return date;
        }

        /// <summary>
        /// 获取当前日期中本周的最后一天。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>指定日期中本周的最后一天。</returns>
        public static DateTime EndOfWeek(this DateTime date)
        {
            var startDayOfWeek = DayOfWeek.Monday;
            var endDayOfWeek = startDayOfWeek - 1;
            if (endDayOfWeek < 0)
            {
                endDayOfWeek = DayOfWeek.Saturday;
            }

            if (date.DayOfWeek != endDayOfWeek)
            {
                if (endDayOfWeek == date.DayOfWeek)
                {
                    return date.AddDays(6);
                }

                if (endDayOfWeek < date.DayOfWeek)
                {
                    return date.AddDays(7 - (date.DayOfWeek - endDayOfWeek));
                }

                return date.AddDays(endDayOfWeek - date.DayOfWeek);
            }

            return date;
        }

        /// <summary>
        /// 判断该日期中的年份是否为闰年。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>如果指定日期的年份是闰年，则为 true，否则为 false。</returns>
        public static bool IsLeapYear(this DateTime date)
        {
            return date.Year % 4 == 0 && (date.Year % 100 != 0 || date.Year % 400 == 0);
        }

        /// <summary>
        /// 设置该日期中的年份。
        /// </summary>
        /// <param name="time">当前的时间。</param>
        /// <param name="year">要设置的年份。</param>
        /// <returns>指定年份后的日期。</returns>
        public static DateTime SetYear(this DateTime time, int year)
        {
            return new DateTime(year, time.Month, time.Day);
        }

        /// <summary>
        /// 设置该日期中的月份。
        /// </summary>
        /// <param name="time">当前的时间</param>
        /// <param name="month">要设置的月份。</param>
        /// <returns>指定月份后的日期。</returns>
        public static DateTime SetMonth(this DateTime time, int month)
        {
            return new DateTime(time.Year, month, time.Day);
        }

        /// <summary>
        /// 设置该日期中的天数。
        /// </summary>
        /// <param name="time">当前的时间</param>
        /// <param name="day">要设置的天数。</param>
        /// <returns>指定天数后的日期。</returns>
        public static DateTime SetDay(this DateTime time, int day)
        {
            return new DateTime(time.Year, time.Month, day);
        }

        /// <summary>
        /// 判断当前日期是否是周末。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <returns>如果指定的日期是周末（星期六和星期天），则为 true，否则为 false。</returns>
        public static bool IsWeekend(this DateTime date)
        {
            return (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday);
        }

        /// <summary>
        /// 获取本月中的第一个 <paramref name="dayOfWeek"/>。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <param name="dayOfWeek">周几。</param>
        /// <returns>指定日期中的第一个周几（周一或周六等）。</returns>
        public static DateTime FirstWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            var first = date.StartOfMonth();
            if (first.DayOfWeek != dayOfWeek)
            {
                first = first.NextWeek(dayOfWeek);
            }

            return first;
        }

        /// <summary>
        /// 获取本月中的最后一个 <paramref name="dayOfWeek"/>。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <param name="dayOfWeek">周几。</param>
        /// <returns>指定日期中的最后一个周几（周一或周六等）。</returns>
        public static DateTime LastWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            var last = date.EndOfMonth();
            var day = dayOfWeek <= last.DayOfWeek ? last.DayOfWeek - dayOfWeek :
                7 - Math.Abs(dayOfWeek - last.DayOfWeek);

            return last.AddDays(day * -1);
        }

        /// <summary>
        /// 获取下一个 <paramref name="dayOfWeek"/> 的日期。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <param name="dayOfWeek">下一个周几。</param>
        /// <returns>指定日期的下一个周几（周一或周六等）。</returns>
        public static DateTime NextWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            return date.NextWeek(dayOfWeek, 1);
        }

        /// <summary>
        /// 获取下 <paramref name="week"/> 个周后 <paramref name="dayOfWeek"/> 的日期。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <param name="dayOfWeek">周几。</param>
        /// <param name="week">往后的第几个周。</param>
        /// <returns>指定日期的下几个周几（周一或周六等）。</returns>
        public static DateTime NextWeek(this DateTime date, DayOfWeek dayOfWeek, int week)
        {
            var offsetDays = dayOfWeek - date.DayOfWeek;
            if (offsetDays <= 0)
            {
                offsetDays += 7;
            }

            if (week > 1)
            {
                offsetDays += (week - 1) * 7;
            }

            return date.AddDays(offsetDays);
        }

        /// <summary>
        /// 获取上一个 <paramref name="dayOfWeek"/> 的日期。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <param name="dayOfWeek">上一个周几。</param>
        /// <returns>指定日期的上一个周几（周一或周六等）。</returns>
        public static DateTime PreviousWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            return date.PreviousWeek(dayOfWeek, 1);
        }

        /// <summary>
        /// 获取上 <paramref name="week"/> 个周后 <paramref name="dayOfWeek"/> 的日期。
        /// </summary>
        /// <param name="date">当前的日期。</param>
        /// <param name="dayOfWeek">周几。</param>
        /// <param name="week">往前的第几个周。</param>
        /// <returns>指定日期的上几个周几（周一或周六等）。</returns>
        public static DateTime PreviousWeek(this DateTime date, DayOfWeek dayOfWeek, int week)
        {
            var offsetDays = dayOfWeek - date.DayOfWeek;
            if (offsetDays >= 0)
            {
                offsetDays -= 7;
            }

            if (week > 1)
            {
                offsetDays -= (week - 1) * 7;
            }

            return date.AddDays(offsetDays);
        }

        /// <summary>
        /// 获取本月中的第一个周周一的日期。
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime FirstWeek(this DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            return new DateTime(date.Year, date.Month, 1).AddDays(0 - dayOfWeek + 1);
        }

        /// <summary>
        /// 判断 <paramref name="other"/> 日期是否在 <paramref name="source"/> 之前。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsBefore(this DateTime source, DateTime other)
        {
            Guard.ArgumentNull(source, nameof(source));
            Guard.ArgumentNull(other, nameof(other));

            return source.CompareTo(other) < 0;
        }

        /// <summary>
        /// 判断 <paramref name="other"/> 日期是否在 <paramref name="source"/> 之后。
        /// </summary>
        /// <param name="source"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAfter(this DateTime source, DateTime other)
        {
            Guard.ArgumentNull(source, nameof(source));
            Guard.ArgumentNull(other, nameof(other));

            return source.CompareTo(other) > 0;
        }

        /// <summary>
        /// 将日期转换为 <see cref="DateTimeOffset"/>。
        /// </summary>
        /// <param name="localDateTime"></param>
        /// <param name="localTimeZone"></param>
        /// <returns></returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime localDateTime, TimeZoneInfo localTimeZone = null)
        {
            localTimeZone = (localTimeZone ?? TimeZoneInfo.Local);

            if (localDateTime.Kind != DateTimeKind.Unspecified)
            {
                localDateTime = new DateTime(localDateTime.Ticks, DateTimeKind.Unspecified);
            }

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);
        }

        /// <summary>
        /// 将时间转换为时间戳。
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static long ToTimeStamp(this DateTime datetime)
        {
            return (datetime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        /// <summary>
        /// 将时间戳转换为时间。
        /// </summary>
        /// <param name="timeStamp">时间戳。</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timeStamp)
        {
            var ticks = timeStamp * 10000000 + 621355968000000000;
            var date = new DateTime(ticks);
            var offset = TimeZone.CurrentTimeZone.GetUtcOffset(date).TotalSeconds;
            return date.AddSeconds(offset);
        }
    }
}
