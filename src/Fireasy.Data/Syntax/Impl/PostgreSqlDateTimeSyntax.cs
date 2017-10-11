
using System.Reflection;
namespace Fireasy.Data.Syntax
{
    public class PostgreSqlDateTimeSyntax : DateTimeSyntax
    {
        /// <summary>
        /// 获取系统日期。
        /// </summary>
        /// <returns></returns>
        public override string SystemTime()
        {
            return "NOW()";
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
            return string.Format("CAST(CAST({0} AS VARCHAR) || '-' || CAST({1} AS VARCHAR) || '-' || CAST({2} AS VARCHAR) AS DATE)",
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
            return string.Format("CAST(CAST({0} AS VARCHAR) || '-' || CAST({1} AS VARCHAR) || '-' || CAST({2} AS VARCHAR) || ' ' || CAST({3} AS VARCHAR) || ':' || CAST({4} AS VARCHAR) || ':' || CAST({5} AS VARCHAR) AS TIMESTAMP)",
                yearExp, monthExp, dayExp, hourExp, minuteExp, secondExp);
        }

        /// <summary>
        /// 获取源表达式中的年份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Year(object sourceExp)
        {
            return string.Format("DATE_PART('YEAR', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的月份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Month(object sourceExp)
        {
            return string.Format("DATE_PART('MONTH', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Day(object sourceExp)
        {
            return string.Format("DATE_PART('DAY', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Hour(object sourceExp)
        {
            return string.Format("DATE_PART('HOUR', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Minute(object sourceExp)
        {
            return string.Format("DATE_PART('MINUTE', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Second(object sourceExp)
        {
            return string.Format("DATE_PART('SECOND', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的毫秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Millisecond(object sourceExp)
        {
            return string.Format("DATE_PART('MILLISECONDS', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本年的第几周。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string WeekOfYear(object sourceExp)
        {
            return string.Format("DATE_PART('WW', {0})", sourceExp);
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
            return string.Format("{0} + INTERVAL '{1} YEARS'", sourceExp, yearExp);
        }

        /// <summary>
        /// 源表达式增加月。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="monthExp">月份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMonths(object sourceExp, object monthExp)
        {
            return string.Format("{0} + INTERVAL '{1} MONTHS'", sourceExp, monthExp);
        }

        /// <summary>
        /// 源表达式增加天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="dayExp">天数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddDays(object sourceExp, object dayExp)
        {
            return string.Format("{0} + INTERVAL '{1} DAYS'", sourceExp, dayExp);
        }
        /// <summary>
        /// 源表达式增加小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="hourExp">小时，可为正可为负。</param>
        /// <returns></returns>
        public override string AddHours(object sourceExp, object hourExp)
        {
            return string.Format("{0} + INTERVAL '{1} HOURS'", sourceExp, hourExp);
        }

        /// <summary>
        /// 源表达式增加分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="minuteExp">分，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMinutes(object sourceExp, object minuteExp)
        {
            return string.Format("{0} + INTERVAL '{1} MINUTES'", sourceExp, minuteExp);
        }

        /// <summary>
        /// 源表达式增加秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="secondExp">秒，可为正可为负。</param>
        /// <returns></returns>
        public override string AddSeconds(object sourceExp, object secondExp)
        {
            return string.Format("{0} + INTERVAL '{1} SECONDS'", sourceExp, secondExp);
        }
    }
}
