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
    /// MySql日期函数语法解析。
    /// </summary>
    public class MySqlDateTimeSyntax : DateTimeSyntax
    {
        /// <summary>
        /// 获取系统日期。
        /// </summary>
        /// <returns></returns>
        public override string SystemTime()
        {
            return "SYSDATE()";
        }

        /// <summary>
        /// 初始化日期。
        /// </summary>
        /// <param name="yearExp">年表达式。</param>
        /// <param name="monthExp">月表达式。</param>
        /// <param name="dayExp">日表达式。</param>
        /// <returns></returns>
        public override string New(object yearExp, object monthExp, object dayExp)
        {
            return string.Format("STR_TO_DATE(CONCAT({0}, '/', {1}, '/', {2}),'%Y/%m/%d')",
                yearExp, monthExp, dayExp);
        }

        /// <summary>
        /// 初始化日期时间。
        /// </summary>
        /// <param name="yearExp">年表达式。</param>
        /// <param name="monthExp">月表达式。</param>
        /// <param name="dayExp">日表达式。</param>
        /// <param name="hourExp">时表达式。</param>
        /// <param name="minuteExp">分表达式。</param>
        /// <param name="secondExp">秒表达式。</param>
        /// <returns></returns>
        public override string New(object yearExp, object monthExp, object dayExp, object hourExp, object minuteExp, object secondExp)
        {
            return string.Format("STR_TO_DATE(CONCAT({0}, '/', {1}, '/', {2}, ' ', {3}, ':', {4}, ':', {5}),'%Y/%m/%d %H:%i:%s')",
                yearExp, monthExp, dayExp, hourExp, minuteExp, secondExp);
        }

        /// <summary>
        /// 获取源表达式中的年份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Year(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%Y')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的月份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Month(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%m')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Day(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%d')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Hour(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%H')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Minute(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%i')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Second(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%s')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的毫秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Millisecond(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%f')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本周的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfWeek(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%w')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本年的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfYear(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%j')", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本年的第几周。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string WeekOfYear(object sourceExp)
        {
            return string.Format("DATE_FORMAT({0}, '%U')", sourceExp);
        }

        /// <summary>
        /// 源表达式增加年。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="yearExp">年份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddYears(object sourceExp, object yearExp)
        {
            return string.Format("DATE_ADD({0}, INTERVAL {1} YEAR)", sourceExp, yearExp);
        }

        /// <summary>
        /// 源表达式增加月。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="monthExp">月份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMonths(object sourceExp, object monthExp)
        {
            return string.Format("DATE_ADD({0}, INTERVAL {1} MONTH)", sourceExp, monthExp);
        }

        /// <summary>
        /// 源表达式增加天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="dayExp">天数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddDays(object sourceExp, object dayExp)
        {
            return string.Format("DATE_ADD({0}, INTERVAL {1} DAY)", sourceExp, dayExp);
        }

        /// <summary>
        /// 源表达式增加小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="hourExp">小时，可为正可为负。</param>
        /// <returns></returns>
        public override string AddHours(object sourceExp, object hourExp)
        {
            return string.Format("DATE_ADD({0}, INTERVAL {1} HOUR)", sourceExp, hourExp);
        }

        /// <summary>
        /// 源表达式增加分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="minuteExp">分，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMinutes(object sourceExp, object minuteExp)
        {
            return string.Format("DATE_ADD({0}, INTERVAL {1} MINUTE)", sourceExp, minuteExp);
        }

        /// <summary>
        /// 源表达式增加秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="secondExp">秒，可为正可为负。</param>
        /// <returns></returns>
        public override string AddSeconds(object sourceExp, object secondExp)
        {
            return string.Format("DATE_ADD({0}, INTERVAL {1} SECOND)", sourceExp, secondExp);
        }

        /// <summary>
        /// 计算两个表达式相差的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffDays(object sourceExp, object otherExp)
        {
            return string.Format("DATEDIFF({1}, {0})", sourceExp, otherExp);
        }

        /// <summary>
        /// 计算两个表达式相差的小时数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffHours(object sourceExp, object otherExp)
        {
            return string.Format("PERIOD_DIFF(DATE_FORMAT({1}, '%Y%m%d%H'), DATE_FORMAT({0}, '%Y%m%d%H'))", sourceExp, otherExp);
        }

        /// <summary>
        /// 计算两个表达式相差的分钟数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffMinutes(object sourceExp, object otherExp)
        {
            return string.Format("PERIOD_DIFF(DATE_FORMAT({1}, '%Y%m%d%H%i'), DATE_FORMAT({0}, '%Y%m%d%H%i'))", sourceExp, otherExp);
        }

        /// <summary>
        /// 计算两个表达式相差的秒数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public override string DiffSeconds(object sourceExp, object otherExp)
        {
            return string.Format("PERIOD_DIFF(DATE_FORMAT({1}, '%Y%m%d%H%i%s'), DATE_FORMAT({0}, '%Y%m%d%H%i%s'))", sourceExp, otherExp);
        }
    }
}
