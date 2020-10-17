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
    /// SQLite字符串函数语法解析。
    /// </summary>
    public class SQLiteStringSyntax : StringSyntax
    {
        /// <summary>
        /// 取源表达式的子串。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="startExp">子串的起始字符位置。</param>
        /// <param name="lenExp">子串中的字符数。</param>
        /// <returns></returns>
        public override string Substring(object sourceExp, object startExp, object lenExp = null)
        {
            return lenExp == null ?
                $"SUBSTR({sourceExp}, {startExp})" :
                $"SUBSTR({sourceExp}, {startExp}, {lenExp})";
        }

        /// <summary>
        /// 截掉源表达式的两边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Trim(object sourceExp)
        {
            return $"TRIM({sourceExp})";
        }

        /// <summary>
        /// 将一组字符串连接为新的字符串。
        /// </summary>
        /// <param name="strExps">要连接的字符串数组。</param>
        /// <returns></returns>
        public override string Concat(params object[] strExps)
        {
            return string.Join(" || ", strExps);
        }

        /// <summary>
        /// 正则表达式匹配。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="regexExp">正则表达式。</param>
        /// <returns></returns>
        public override string IsMatch(object sourceExp, object regexExp)
        {
            new SQLiteFunctionBuilder().RegisterRegexFunction();
            return $"REGEXP({sourceExp}, {regexExp})";
        }
    }
}
