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
    /// Firebird字符串函数语法解析。
    /// </summary>
    public class FirebirdStringSyntax : StringSyntax
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
                string.Format("SUBSTRING({0} FROM {1})", sourceExp, startExp) :
                string.Format("SUBSTRING({0} FROM {1} FOR {2})", sourceExp, startExp, lenExp);
        }

        /// <summary>
        /// 计算源表达式的长度。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public override string Length(object sourceExp)
        {
            return string.Format("CHAR_LENGTH({0})", sourceExp);
        }

        /// <summary>
        /// 判断子串在源表达式中的位置。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="searchExp">要搜寻的字符串。</param>
        /// <param name="startExp">搜索起始位置。</param>
        /// <param name="countExp">要检查的字符位置数</param>
        /// <returns></returns>
        public override string IndexOf(object sourceExp, object searchExp, object startExp = null, object countExp = null)
        {
            if (startExp != null)
            {
                return string.Format("POSITION({0}, {1}, {2})", searchExp, sourceExp, startExp);
            }

            return string.Format("POSITION({0}, {1})", searchExp, sourceExp);
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
    }
}
