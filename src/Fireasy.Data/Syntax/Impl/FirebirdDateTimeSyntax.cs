// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Syntax
{
    /// <summary>
    /// Firebird日期函数语法解析。
    /// </summary>
    public class FirebirdDateTimeSyntax : DateTimeSyntax
    {
        /// <summary>
        /// 获取当前时间。
        /// </summary>
        /// <returns></returns>
        public override string Now()
        {
            return "CURRENT_TIMESTAMP";
        }

        /// <summary>
        /// 获取当前 UTC 时间。
        /// </summary>
        /// <returns></returns>
        public override string UtcNow()
        {
            return "CURRENT_TIMESTAMP";
        }

        /// <summary>
        /// 获取源表达式中的年份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Year(object sourceExp)
        {
            return $"EXTRACT(YEAR FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的月份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Month(object sourceExp)
        {
            return $"EXTRACT(MONTH FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Day(object sourceExp)
        {
            return $"EXTRACT(DAY FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Hour(object sourceExp)
        {
            return $"EXTRACT(HOUR FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Minute(object sourceExp)
        {
            return $"EXTRACT(MINUTE FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Second(object sourceExp)
        {
            return $"EXTRACT(SECOND FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的毫秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Millisecond(object sourceExp)
        {
            return $"EXTRACT(MILLISECOND FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的本周的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfWeek(object sourceExp)
        {
            return $"EXTRACT(WEEKDAY FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的本年的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfYear(object sourceExp)
        {
            return $"EXTRACT(YEARDAY FROM {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的本年的第几周。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string WeekOfYear(object sourceExp)
        {
            return $"EXTRACT(WEEK FROM {sourceExp})";
        }

        /// <summary>
        /// 源表达式增加年。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="yearExp">年份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddYears(object sourceExp, object yearExp)
        {
            return $"DATEADD(YEAR, {yearExp}, {sourceExp})";
        }

        /// <summary>
        /// 源表达式增加月。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="monthExp">月份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMonths(object sourceExp, object monthExp)
        {
            return $"DATEADD(MONTH, {monthExp}, {sourceExp})";
        }

        /// <summary>
        /// 源表达式增加天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="dayExp">天数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddDays(object sourceExp, object dayExp)
        {
            return $"DATEADD(DAY, {dayExp}, {sourceExp})";
        }

        /// <summary>
        /// 源表达式增加小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="hourExp">小时，可为正可为负。</param>
        /// <returns></returns>
        public override string AddHours(object sourceExp, object hourExp)
        {
            return $"DATEADD(HOUR, {hourExp}, {sourceExp})";
        }

        /// <summary>
        /// 源表达式增加分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="minuteExp">分，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMinutes(object sourceExp, object minuteExp)
        {
            return $"DATEADD(MINUTE, {minuteExp}, {sourceExp})";
        }

        /// <summary>
        /// 源表达式增加秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="secondExp">秒，可为正可为负。</param>
        /// <returns></returns>
        public override string AddSeconds(object sourceExp, object secondExp)
        {
            return $"DATEADD(SECOND, {secondExp}, {sourceExp})";
        }

        /// <summary>
        /// 计算两个表达式相差的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffDays(object sourceExp, object otherExp)
        {
            return $"DATEDIFF(DAY, {sourceExp}, {otherExp})";
        }

        /// <summary>
        /// 计算两个表达式相差的小时数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffHours(object sourceExp, object otherExp)
        {
            return $"DATEDIFF(HOUR, {sourceExp}, {otherExp})";
        }

        /// <summary>
        /// 计算两个表达式相差的分钟数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffMinutes(object sourceExp, object otherExp)
        {
            return $"DATEDIFF(MINUTE, {sourceExp}, {otherExp})";
        }

        /// <summary>
        /// 计算两个表达式相差的秒数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffSeconds(object sourceExp, object otherExp)
        {
            return $"DATEDIFF(SECOND, {sourceExp}, {otherExp})";
        }
    }
}
