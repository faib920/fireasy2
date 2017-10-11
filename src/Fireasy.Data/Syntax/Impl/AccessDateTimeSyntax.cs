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
    public class AccessDateTimeSyntax : DateTimeSyntax
    {

        /// <summary>
        /// 初始化日期。
        /// </summary>
        /// <param name="yearExp"></param>
        /// <param name="monthExp"></param>
        /// <param name="dayExp"></param>
        /// <returns></returns>
        public override string New(object yearExp, object monthExp, object dayExp)
        {
            return string.Format("CDATE({0} & '/' & {1} & '/' & {2})",
                yearExp, monthExp, dayExp);
        }

        /// <summary>
        /// 初始化日期时间。
        /// </summary>
        /// <param name="yearExp"></param>
        /// <param name="monthExp"></param>
        /// <param name="dayExp"></param>
        /// <param name="hourExp"></param>
        /// <param name="minuteExp"></param>
        /// <param name="secondExp"></param>
        /// <returns></returns>
        public override string New(object yearExp, object monthExp, object dayExp, object hourExp, object minuteExp, object secondExp)
        {
            return string.Format("CDATE({0} & '/' & {1} & '/' & {2} & ' ' & {3} & ':' & {4} & ':' & {5})",
                yearExp, monthExp, dayExp, hourExp, minuteExp, secondExp);
        }

        /// <summary>
        /// 获取源表达式中的年份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Year(object sourceExp)
        {
            return string.Format("DATEPART('yyyy', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的月份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Month(object sourceExp)
        {
            return string.Format("DATEPART('m', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的天数。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Day(object sourceExp)
        {
            return string.Format("DATEPART('d', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Hour(object sourceExp)
        {
            return string.Format("DATEPART('h', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Minute(object sourceExp)
        {
            return string.Format("DATEPART('n', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Second(object sourceExp)
        {
            return string.Format("DATEPART('s', {0})", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的毫秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Millisecond(object sourceExp)
        {
            throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// 获取源表达式中的本周的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfWeek(object sourceExp)
        {
            return string.Format("DATEPART('w', {0}) - 1", sourceExp);
        }

        /// <summary>
        /// 获取源表达式中的本年的第几天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string DayOfYear(object sourceExp)
        {
            return string.Format("DATEPART('y', {0})", sourceExp);
        }

        /// <summary>
        /// 源表达式增加年。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="yearExp">年份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddYears(object sourceExp, object yearExp)
        {
            return string.Format("DATEADD('yyyy', {1}, {0})", sourceExp, yearExp);
        }

        /// <summary>
        /// 源表达式增加月。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="monthExp">月份数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMonths(object sourceExp, object monthExp)
        {
            return string.Format("DATEADD('m', {1}, {0})", sourceExp, monthExp);
        }

        /// <summary>
        /// 源表达式增加天。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="dayExp">天数，可为正可为负。</param>
        /// <returns></returns>
        public override string AddDays(object sourceExp, object dayExp)
        {
            return string.Format("DATEADD('d', {1}, {0})", sourceExp, dayExp);
        }
        /// <summary>
        /// 源表达式增加小时。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="hourExp">小时，可为正可为负。</param>
        /// <returns></returns>
        public override string AddHours(object sourceExp, object hourExp)
        {
            return string.Format("DATEADD('h', {1}, {0})", sourceExp, hourExp);
        }

        /// <summary>
        /// 源表达式增加分。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="minuteExp">分，可为正可为负。</param>
        /// <returns></returns>
        public override string AddMinutes(object sourceExp, object minuteExp)
        {
            return string.Format("DATEADD('n', {1}, {0})", sourceExp, minuteExp);
        }

        /// <summary>
        /// 源表达式增加秒。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="secondExp">秒，可为正可为负。</param>
        /// <returns></returns>
        public override string AddSeconds(object sourceExp, object secondExp)
        {
            return string.Format("DATEADD('s', {1}, {0})", sourceExp, secondExp);
        }
    }
}
