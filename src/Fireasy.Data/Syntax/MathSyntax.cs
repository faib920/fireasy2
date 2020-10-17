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
    /// 基本的数学函数。
    /// </summary>
    public class MathSyntax
    {
        /// <summary>
        /// 两个表达式进行与运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public virtual string And(object sourceExp, object otherExp)
        {
            return $"({sourceExp} & {otherExp})";
        }

        /// <summary>
        /// 两个表达式进行或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public virtual string Or(object sourceExp, object otherExp)
        {
            return $"({sourceExp} | {otherExp})";
        }

        /// <summary>
        /// 返回源表达式的非值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Not(object sourceExp)
        {
            return $"~{sourceExp}";
        }

        /// <summary>
        /// 对源表达式进行求余运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public virtual string Modulo(object sourceExp, object otherExp)
        {
            return $"({sourceExp} % {otherExp})";
        }

        /// <summary>
        /// 对两个表达式进行异或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public virtual string ExclusiveOr(object sourceExp, object otherExp)
        {
            return $"({sourceExp} ^ {otherExp})";
        }

        /// <summary>
        /// 返回源表达式的最小整数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Ceiling(object sourceExp)
        {
            return $"CEILING({sourceExp})";
        }

        /// <summary>
        /// 将源表达式的小数位舍入。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="digitExp">小数位数。</param>
        /// <returns></returns>
        public virtual string Round(object sourceExp, object digitExp = null)
        {
            return $"ROUND({sourceExp}, {digitExp ?? 0})";
        }

        /// <summary>
        /// 返回源表达式的整数部份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Truncate(object sourceExp)
        {
            return $"ROUND({sourceExp}, 0)";
        }

        /// <summary>
        /// 返回源表达式的最大整数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Floor(object sourceExp)
        {
            return $"FLOOR({sourceExp})";
        }

        /// <summary>
        /// 返回以e为底的对数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Log(object sourceExp)
        {
            return $"LOG({sourceExp})";
        }

        /// <summary>
        /// 返回以10为底的对数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Log10(object sourceExp)
        {
            return $"LOG10({sourceExp})";
        }

        /// <summary>
        /// 返回e的指定次冪。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Exp(object sourceExp)
        {
            return $"EXP({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的绝对值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Abs(object sourceExp)
        {
            return $"ABS({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的反值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Negate(object sourceExp)
        {
            return $"-{sourceExp}";
        }

        /// <summary>
        /// 返回源表达式的指定冪。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="powerExp">冪。</param>
        /// <returns></returns>
        public virtual string Power(object sourceExp, object powerExp)
        {
            return $"POWER({sourceExp}, {powerExp})";
        }

        /// <summary>
        /// 返回源表达式的二次开方值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Sqrt(object sourceExp)
        {
            return $"SQRT({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的正弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Sin(object sourceExp)
        {
            return $"SIN({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的余弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Cos(object sourceExp)
        {
            return $"COS({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Tan(object sourceExp)
        {
            return $"TAN({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的反正弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Asin(object sourceExp)
        {
            return $"ASIN({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的反余弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Acos(object sourceExp)
        {
            return $"ACOS({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的反正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Atan(object sourceExp)
        {
            return $"ATAN({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式的符号。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Sign(object sourceExp)
        {
            return $"SIGN({sourceExp})";
        }

        /// <summary>
        /// 返回源表达式左移后的值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="shiftExp">位数。</param>
        /// <returns></returns>
        public virtual string LeftShift(object sourceExp, object shiftExp)
        {
            return $"{sourceExp} * POWER(2, {shiftExp})";
        }

        /// <summary>
        /// 返回源表达式右移后的值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="shiftExp">位数。</param>
        /// <returns></returns>
        public virtual string RightShift(object sourceExp, object shiftExp)
        {
            return $"{sourceExp} / POWER(2, {shiftExp})";
        }

        /// <summary>
        /// 返回随机数生成函数。
        /// </summary>
        /// <returns></returns>
        public virtual string Random()
        {
            return "RAND()";
        }
    }
}
