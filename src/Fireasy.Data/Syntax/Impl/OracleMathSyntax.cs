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
    /// Oracle数学函数语法解析。
    /// </summary>
    public class OracleMathSyntax : MathSyntax
    {
        /// <summary>
        /// 两个表达式进行与运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string And(object sourceExp, object otherExp)
        {
            return string.Format("BITAND({0}, {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 两个表达式进行或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string Or(object sourceExp, object otherExp)
        {
            return string.Format("({0} + {1}) - BITAND({0}, {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 返回源表达式的非值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Not(object sourceExp)
        {
            return string.Format("(-1 - {0})", sourceExp);
        }

        /// <summary>
        /// 对源表达式进行求余运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string Modulo(object sourceExp, object otherExp)
        {
            return string.Format("MOD({0}, {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 对两个表达式进行异或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string ExclusiveOr(object sourceExp, object otherExp)
        {
            return string.Format("({0} + {1}) - BITAND({0}, {1}) * 2", sourceExp, otherExp);
        }

        /// <summary>
        /// 返回源表达式的最小整数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Ceiling(object sourceExp)
        {
            return string.Format("CEIL({0})", sourceExp);
        }

        /// <summary>
        /// 返回源表达式的整数部份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Truncate(object sourceExp)
        {
            return string.Format("TRUNC({0})", sourceExp);
        }

        /// <summary>
        /// 返回以e为底的对数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Log(object sourceExp)
        {
            return string.Format("ROUND(LN({0}), 9)", sourceExp);
        }

        /// <summary>
        /// 返回以10为底的对数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Log10(object sourceExp)
        {
            return string.Format("ROUND(LOG(10, {0}), 9)", sourceExp);
        }

        /// <summary>
        /// 返回源表达式的反值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Negate(object sourceExp)
        {
            return string.Format("(0 - {0})", sourceExp);
        }

        /// <summary>
        /// 返回e的指定次冪。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Exp(object sourceExp)
        {
            return Round(base.Exp(sourceExp), 9);
        }

        /// <summary>
        /// 返回随机数生成函数。
        /// </summary>
        /// <returns></returns>
        public override string Random()
        {
            return "DBMS_RANDOM.RANDOM()";
        }

        /// <summary>
        /// 返回源表达式的正弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Sin(object sourceExp)
        {
            return Round(base.Sin(sourceExp), 9);
        }

        /// <summary>
        /// 返回源表达式的余弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Cos(object sourceExp)
        {
            return Round(base.Cos(sourceExp), 9);
        }

        /// <summary>
        /// 返回源表达式的正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Tan(object sourceExp)
        {
            return Round(base.Tan(sourceExp), 9);
        }

        /// <summary>
        /// 返回源表达式的反正弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Asin(object sourceExp)
        {
            return Round(base.Asin(sourceExp), 9);
        }

        /// <summary>
        /// 返回源表达式的反余弦值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Acos(object sourceExp)
        {
            return Round(base.Acos(sourceExp), 9);
        }

        /// <summary>
        /// 返回源表达式的反正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Atan(object sourceExp)
        {
            return Round(base.Atan(sourceExp), 9);
        }

        /// <summary>
        /// 返回源表达式的符号。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Sign(object sourceExp)
        {
            return Round(base.Sign(sourceExp), 9);
        }
    }
}

