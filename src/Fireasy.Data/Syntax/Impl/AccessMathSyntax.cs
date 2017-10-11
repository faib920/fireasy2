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
    public class AccessMathSyntax : MathSyntax
    {
        /// <summary>
        /// 返回源表达式的反正切值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Atan(object sourceExp)
        {
            return string.Format("ATN({0})", sourceExp);
        }

        /// <summary>
        /// 返回源表达式的最大整数值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Floor(object sourceExp)
        {
            throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
        }

        /// <summary>
        /// 返回源表达式的指定冪。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="powerExp">冪。</param>
        /// <returns></returns>
        public override string Power(object sourceExp, object powerExp)
        {
            return string.Format("{0} ^ {1}", sourceExp, powerExp);
        }

        /// <summary>
        /// 返回源表达式的二次开方值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Sqrt(object sourceExp)
        {
            return string.Format("SQR({0})", sourceExp);
        }

        /// <summary>
        /// 返回源表达式的整数部份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Truncate(object sourceExp)
        {
            return string.Format("FIX({0})", sourceExp);
        }

        /// <summary>
        /// 对源表达式进行求余运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string Modulo(object sourceExp, object otherExp)
        {
            return string.Format("({0} MOD {1})", sourceExp, otherExp);
        }

        /// <summary>
        /// 返回源表达式左移后的值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="shiftExp">位数。</param>
        /// <returns></returns>
        public override string LeftShift(object sourceExp, object shiftExp)
        {
            return string.Format("{0} * (2 ^ {1})", sourceExp, shiftExp);
        }

        /// <summary>
        /// 返回源表达式右移后的值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="shiftExp">位数。</param>
        /// <returns></returns>
        public override string RightShift(object sourceExp, object shiftExp)
        {
            return string.Format("{0} / (2 ^ {1})", sourceExp, shiftExp);
        }

        /// <summary>
        /// 两个表达式进行与运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string And(object sourceExp, object otherExp)
        {
            return string.Format("{0} BAND {1}", sourceExp, otherExp);
        }

        /// <summary>
        /// 两个表达式进行或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string Or(object sourceExp, object otherExp)
        {
            return string.Format("{0} BOR {1}", sourceExp, otherExp);
        }

        /// <summary>
        /// 对两个表达式进行异或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string ExclusiveOr(object sourceExp, object otherExp)
        {
            return string.Format("{0} XOR {1}", sourceExp, otherExp);
        }

        /// <summary>
        /// 返回源表达式的非值。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Not(object sourceExp)
        {
            return string.Format("NOT {0}", sourceExp);
        }

        /// <summary>
        /// 返回随机数生成函数。
        /// </summary>
        /// <returns></returns>
        public override string Random()
        {
            return "RND()";
        }

    }
}
