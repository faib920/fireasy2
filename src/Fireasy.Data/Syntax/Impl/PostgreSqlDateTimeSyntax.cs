// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Reflection;
namespace Fireasy.Data.Syntax
{
    public class PostgreSqlDateTimeSyntax : DateTimeSyntax
    {
        /// <summary>
        /// 获取当前时间。
        /// </summary>
        /// <returns></returns>
        public override string Now()
        {
            return "NOW()";
        }

        /// <summary>
        /// 获取当前 UTC 时间。
        /// </summary>
        /// <returns></returns>
        public override string UtcNow()
        {
            return "(NOW() AT TIME ZONE 'UTC')";
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
            return $"CAST(CAST({yearExp} AS VARCHAR) || '-' || CAST({monthExp} AS VARCHAR) || '-' || CAST({dayExp} AS VARCHAR) AS DATE)";
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
            return $"CAST(CAST({yearExp} AS VARCHAR) || '-' || CAST({monthExp} AS VARCHAR) || '-' || CAST({dayExp} AS VARCHAR) || ' ' || CAST({hourExp} AS VARCHAR) || ':' || CAST({minuteExp} AS VARCHAR) || ':' || CAST({secondExp} AS VARCHAR) AS TIMESTAMP)";
        }

        /// <summary>
        /// 获取源表达式中的年份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Year(object sourceExp)
        {
            return $"DATE_PART('YEAR', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的月份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Month(object sourceExp)
        {
            return $"DATE_PART('MONTH', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Day(object sourceExp)
        {
            return $"DATE_PART('DAY', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Hour(object sourceExp)
        {
            return $"DATE_PART('HOUR', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Minute(object sourceExp)
        {
            return $"DATE_PART('MINUTE', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Second(object sourceExp)
        {
            return $"DATE_PART('SECOND', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的毫秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Millisecond(object sourceExp)
        {
            return $"DATE_PART('MILLISECONDS', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的本年的第几周。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string WeekOfYear(object sourceExp)
        {
            return $"DATE_PART('WW', {sourceExp})";
        }

        /// <summary>
        /// 获取源表达式中的本周的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfWeek(object sourceExp)
        {
            throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
        }
        /// <summary>
        /// 源表达式增加年。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="yearExp">年份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddYears(object sourceExp, object yearExp)
        {
            return $"{sourceExp} + INTERVAL '{yearExp} YEARS'";
        }

        /// <summary>
        /// 源表达式增加月。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="monthExp">月份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMonths(object sourceExp, object monthExp)
        {
            return $"{sourceExp} + INTERVAL '{monthExp} MONTHS'";
        }

        /// <summary>
        /// 源表达式增加天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="dayExp">天数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddDays(object sourceExp, object dayExp)
        {
            return $"{sourceExp} + INTERVAL '{dayExp} DAYS'";
        }
        /// <summary>
        /// 源表达式增加小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="hourExp">小时，可为正可为负。</param>
        /// <returns></returns>
        public override string AddHours(object sourceExp, object hourExp)
        {
            return $"{sourceExp} + INTERVAL '{hourExp} HOURS'";
        }

        /// <summary>
        /// 源表达式增加分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="minuteExp">分，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMinutes(object sourceExp, object minuteExp)
        {
            return $"{sourceExp} + INTERVAL '{minuteExp} MINUTES'";
        }

        /// <summary>
        /// 源表达式增加秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="secondExp">秒，可为正可为负。</param>
        /// <returns></returns>
        public override string AddSeconds(object sourceExp, object secondExp)
        {
            return $"{sourceExp} + INTERVAL '{secondExp} SECONDS'";
        }
    }
}
