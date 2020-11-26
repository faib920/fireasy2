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
    public static class TimeSpanExtension
    {
        /// <summary>
        /// 从字符串中获取 <see cref="TimeSpan"/> 实例。
        /// </summary>
        /// <param name="source">表示时间的格式，如：
        /// 0:20:33 表示 23分33秒、12d 表示 12天、10h 表示 10小时、10m 表示 10分钟、10s 表示 10秒，其余整数表示毫秒。
        /// </param>
        /// <param name="defaultValue">如果 <paramref name="source"/> 为空时，返回些毫秒值。</param>
        /// <returns></returns>
        public static TimeSpan ToTimeSpan(this string source, TimeSpan? defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return defaultValue ?? TimeSpan.Zero;
            }

            if (source.IndexOf(":") != -1)
            {
                if (TimeSpan.TryParse(source, out TimeSpan t))
                {
                    return t;
                }
            }
            else if (source.EndsWith("d", StringComparison.InvariantCultureIgnoreCase))
            {
                var v = source.Left(source.Length - 1).To<int>();
                return TimeSpan.FromDays(v);
            }
            else if (source.EndsWith("h", StringComparison.InvariantCultureIgnoreCase))
            {
                var v = source.Left(source.Length - 1).To<int>();
                return TimeSpan.FromHours(v);
            }
            else if (source.EndsWith("m", StringComparison.InvariantCultureIgnoreCase))
            {
                var v = source.Left(source.Length - 1).To<int>();
                return TimeSpan.FromMinutes(v);
            }
            else if (source.EndsWith("s", StringComparison.InvariantCultureIgnoreCase))
            {
                var v = source.Left(source.Length - 1).To<int>();
                return TimeSpan.FromSeconds(v);
            }

            return TimeSpan.FromMilliseconds(source.To<int>());
        }

        /// <summary>
        /// 将 <see cref="TimeSpan"/> 输出为字符表示。
        /// </summary>
        /// <param name="timeSpan">要输出的时间间隔。</param>
        /// <returns>小时、分钟和秒表示的字符串。</returns>
        public static string ToStringEx(this TimeSpan timeSpan)
        {
            var timeString = new StringBuilder();
            var flag = new AssertFlag();
            if ((timeSpan.Days > 0) || (timeSpan.Hours > 0))
            {
                flag.AssertTrue();
                var hours = ((24 * timeSpan.Days) + timeSpan.Hours);
                timeString.Append(hours + SR.GetString(SRKind.Hour));
            }

            if (timeSpan.Minutes > 0)
            {
                if (!flag.AssertTrue())
                {
                    timeString.Append(" ");
                }

                timeString.Append(timeSpan.Minutes + SR.GetString(SRKind.Minute));
            }

            if (!flag.AssertTrue())
            {
                timeString.Append(" ");
            }

            timeString.Append(timeSpan.Seconds + SR.GetString(SRKind.Second));
            return timeString.ToString();
        }
    }
}
