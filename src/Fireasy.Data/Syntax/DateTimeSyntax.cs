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
    /// 基本的日期函数。
    /// </summary>
    public class DateTimeSyntax
    {
        /// <summary>
        /// 获取系统日期。
        /// </summary>
        /// <returns></returns>
        public virtual string SystemTime()
        {
            return "GETDATE()";
        }

        /// <summary>
        /// 初始化日期。
        /// </summary>
        /// <param name="yearExp">年表达式。</param>
        /// <param name="monthExp">月表达式。</param>
        /// <param name="dayExp">日表达式。</param>
        /// <returns></returns>
        public virtual string New(object yearExp, object monthExp, object dayExp)
        {
            return string.Format("CAST(CAST({0} AS VARCHAR) + '/' + CAST({1} AS VARCHAR) + '/' + CAST({2} AS VARCHAR) AS DATETIME)",
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
        public virtual string New(object yearExp, object monthExp, object dayExp, object hourExp, object minuteExp, object secondExp)
        {
            return string.Format("CAST(CAST({0} AS VARCHAR) + '/' + CAST({1} AS VARCHAR) + '/' + CAST({2} AS VARCHAR) + ' ' + CAST({3} AS VARCHAR) + ':' + CAST({4} AS VARCHAR) + ':' + CAST({5} AS VARCHAR) AS DATETIME)", 
                yearExp, monthExp, dayExp, hourExp, minuteExp, secondExp);
        }

        /// <summary>
        /// 获取源表达式中的年份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Year(object sourceExp)
        {
            return string.Format("YEAR({0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的月份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Month(object sourceExp)
        {
            return string.Format("MONTH({0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Day(object sourceExp)
        {
            return string.Format("DAY({0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Hour(object sourceExp)
        {
            return string.Format("DATEPART(HH, {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Minute(object sourceExp)
        {
            return string.Format("DATEPART(MI, {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Second(object sourceExp)
        {
            return string.Format("DATEPART(SS, {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的毫秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Millisecond(object sourceExp)
        {
            return string.Format("DATEPART(MS, {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的短时间部份。
        /// </summary>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        public virtual string ShortTime(object sourceExp)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取源表达式中的短日期部份。
        /// </summary>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        public virtual string ShortDate(object sourceExp)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取源表达式中的本周的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string DayOfWeek(object sourceExp)
        {
            return string.Format("DATEPART(WEEKDAY, {0}) - 1", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本年的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string DayOfYear(object sourceExp)
        {
            return string.Format("DATEPART(DAYOFYEAR, {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本年的第几周。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string WeekOfYear(object sourceExp)
        {
            return string.Format("DATEPART(WW, {0}) - 1", sourceExp);
        }

        /// <summary>
        /// 源表达式增加年。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="yearExp">年份数，可为正可为负。</param>
        /// <returns></returns>
        public virtual string AddYears(object sourceExp, object yearExp)
        {
            return string.Format("DATEADD(YYYY, {1}, {0})", sourceExp, yearExp);
        }

        /// <summary>
        /// 源表达式增加月。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="monthExp">月份数，可为正可为负。</param>
        /// <returns></returns>
        public virtual string AddMonths(object sourceExp, object monthExp)
        {
            return string.Format("DATEADD(MM, {1}, {0})", sourceExp, monthExp);
        }

        /// <summary>
        /// 源表达式增加天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="dayExp">天数，可为正可为负。</param>
        /// <returns></returns>
        public virtual string AddDays(object sourceExp, object dayExp)
        {
            return string.Format("DATEADD(DD, {1}, {0})", sourceExp, dayExp);
        }

        /// <summary>
        /// 源表达式增加小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="hourExp">小时，可为正可为负。</param>
        /// <returns></returns>
        public virtual string AddHours(object sourceExp, object hourExp)
        {
            return string.Format("DATEADD(HH, {1}, {0})", sourceExp, hourExp);
        }

        /// <summary>
        /// 源表达式增加分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="minuteExp">分，可为正可为负。</param>
        /// <returns></returns>
        public virtual string AddMinutes(object sourceExp, object minuteExp)
        {
            return string.Format("DATEADD(MI, {1}, {0})", sourceExp, minuteExp);
        }

        /// <summary>
        /// 源表达式增加秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="secondExp">秒，可为正可为负。</param>
        /// <returns></returns>
        public virtual string AddSeconds(object sourceExp, object secondExp)
        {
            return string.Format("DATEADD(SS, {1}, {0})", sourceExp, secondExp);
        }

        /// <summary>
        /// 计算两个表达式相差的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public virtual string DiffDays(object sourceExp, object otherExp)
        {
            return string.Format("DATEDIFF(DD, {0}, {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 计算两个表达式相差的小时数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public virtual string DiffHours(object sourceExp, object otherExp)
        {
            return string.Format("DATEDIFF(HH, {0}, {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 计算两个表达式相差的分钟数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public virtual string DiffMinutes(object sourceExp, object otherExp)
        {
            return string.Format("DATEDIFF(MI, {0}, {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 计算两个表达式相差的秒数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">结束日期。</param>
        /// <returns></returns>
        public virtual string DiffSeconds(object sourceExp, object otherExp)
        {
            return string.Format("DATEDIFF(SS, {0}, {1})", sourceExp, otherExp);
        }
    }
}
