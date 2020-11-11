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
            return $"LEN({sourceExp})";
        }

        /// <summary>
        /// 将分组后的某字段连接为新的字符串。
        /// </summary>
        /// <param name="sourceExp"></param>
        /// <param name="separator">分隔符。</param>
        /// <returns></returns>
        public override string GroupConcat(object sourceExp, object separator)
        {
            return $"STRING_AGG({sourceExp}, {separator})";
        }
    }
}
