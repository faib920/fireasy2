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
    /// MsSql字符串函数语法解析。
    /// </summary>
    public class MsSqlStringSyntax : StringSyntax
    {
        /// <summary>
        /// 计算源表达式的长度。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Length(object sourceExp)
        {
            return string.Format("LEN({0})", sourceExp);
        }
    }
}
