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
    /// 数学函数解析接口。
    /// </summary>
    public interface IMathSyntax
    {
        /// <summary>
        /// 两个表达式进行与运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        string And(object sourceExp, object otherExp);

        /// <summary>
        /// 两个表达式进行或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        string Or(object sourceExp, object otherExp);

        /// <summary>
        /// 对源表达式进行求余运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        string Modulo(object sourceExp, object otherExp);

        /// <summary>
        /// 对两个表达式进行异或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        string ExclusiveOr(object sourceExp, object otherExp);

        /// <summary>
        /// 返回源表达式的最小整数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Ceiling(object sourceExp);

        /// <summary>
        /// 将源表达式的小数位舍入。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="digitExp">小数位数。</param>
        /// <returns></returns>
        string Round(object sourceExp, object digitExp = null);

        /// <summary>
        /// 返回源表达式的整数部份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Truncate(object sourceExp);

        /// <summary>
        /// 返回源表达式的最大整数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Floor(object sourceExp);

        /// <summary>
        /// 返回以e为底的对数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Log(object sourceExp);

        /// <summary>
        /// 返回以10为底的对数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Log10(object sourceExp);

        /// <summary>
        /// 返回e的指定次冪。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Exp(object sourceExp);

        /// <summary>
        /// 返回源表达式的绝对值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Abs(object sourceExp);

        /// <summary>
        /// 返回源表达式的反值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Negate(object sourceExp);

        /// <summary>
        /// 返回源表达式的指定冪。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="powerExp">冪。</param>
        /// <returns></returns>
        string Power(object sourceExp, object powerExp);

        /// <summary>
        /// 返回源表达式的二次开方值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Sqrt(object sourceExp);

        /// <summary>
        /// 返回源表达式的正弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Sin(object sourceExp);

        /// <summary>
        /// 返回源表达式的余弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Cos(object sourceExp);

        /// <summary>
        /// 返回源表达式的正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Tan(object sourceExp);

        /// <summary>
        /// 返回源表达式的反正弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Asin(object sourceExp);

        /// <summary>
        /// 返回源表达式的反余弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Acos(object sourceExp);

        /// <summary>
        /// 返回源表达式的反正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Atan(object sourceExp);

        /// <summary>
        /// 返回源表达式的符号。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Sign(object sourceExp);

        /// <summary>
        /// 返回源表达式左移后的值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="shiftExp">位数。</param>
        /// <returns></returns>
        string LeftShift(object sourceExp, object shiftExp);

        /// <summary>
        /// 返回源表达式右移后的值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="shiftExp">位数。</param>
        /// <returns></returns>
        string RightShift(object sourceExp, object shiftExp);
    }
}
