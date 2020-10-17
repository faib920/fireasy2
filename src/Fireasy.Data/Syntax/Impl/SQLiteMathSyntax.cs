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
    /// <summary>
    /// SQLite数学函数语法解析。
    /// </summary>
    public class SQLiteMathSyntax : MathSyntax
    {
        /// <summary>
        /// 对两个表达式进行异或运算。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="otherExp">参与运算的表达式。</param>
        /// <returns></returns>
        public override string ExclusiveOr(object sourceExp, object otherExp)
        {
            return $"({sourceExp} + {otherExp}) - ({sourceExp} & {otherExp}) * 2";
        }

        /// <summary>
        /// 返回随机数生成函数。
        /// </summary>
        /// <returns></returns>
        public override string Random()
        {
            return "RANDOM()";
        }

        /// <summary>
        /// 返回源表达式的整数部份。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Truncate(object sourceExp)
        {
            throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
        }
    }
}
