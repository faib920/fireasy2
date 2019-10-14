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
    /// 基本的字符串函数。
    /// </summary>
    public class StringSyntax
    {
        /// <summary>
        /// 取源表达式的子串。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="startExp">子串的起始字符位置。</param>
        /// <param name="lenExp">子串中的字符数。</param>
        /// <returns></returns>
        public virtual string Substring(object sourceExp, object startExp, object lenExp = null)
        {
            if (lenExp == null)
            {
                lenExp = Length(sourceExp);
            }
            return string.Format("SUBSTRING({0}, {1}, {2})", sourceExp, startExp, lenExp);
        }

        /// <summary>
        /// 计算源表达式的长度。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Length(object sourceExp)
        {
            return string.Format("LENGTH({0})", sourceExp);
        }

        /// <summary>
        /// 判断子串在源表达式中的位置。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="searchExp">要搜寻的字符串。</param>
        /// <param name="startExp">搜索起始位置。</param>
        /// <param name="countExp">要检查的字符位置数</param>
        /// <returns></returns>
        public virtual string IndexOf(object sourceExp, object searchExp, object startExp = null, object countExp = null)
        {
            return startExp == null ?
                string.Format("CHARINDEX({1}, {0})", sourceExp, searchExp) :
                string.Format("CHARINDEX({1}, {0}, {2})", sourceExp, searchExp, startExp);
        }

        /// <summary>
        /// 将源表达式转换为小写格式。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string ToLower(object sourceExp)
        {
            return string.Format("LOWER({0})", sourceExp);
        }

        /// <summary>
        /// 将源表达式转换为大写格式。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string ToUpper(object sourceExp)
        {
            return string.Format("UPPER({0})", sourceExp);
        }

        /// <summary>
        /// 截掉源表达式的左边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string TrimStart(object sourceExp)
        {
            return string.Format("LTRIM({0})", sourceExp);
        }

        /// <summary>
        /// 截掉源表达式的右边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string TrimEnd(object sourceExp)
        {
            return string.Format("RTRIM({0})", sourceExp);
        }

        /// <summary>
        /// 截掉源表达式的两边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Trim(object sourceExp)
        {
            return string.Format("LTRIM(RTRIM({0}))", sourceExp);
        }

        /// <summary>
        /// 将源表达式中的部份字符替换为新的字符。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="searchExp">要搜寻的字符串。</param>
        /// <param name="replaceExp">要替换的字符串。</param>
        /// <returns></returns>
        public virtual string Replace(object sourceExp, object searchExp, object replaceExp)
        {
            return string.Format("REPLACE({0}, {1}, {2})", sourceExp, searchExp, replaceExp);
        }

        /// <summary>
        /// 将一组字符串连接为新的字符串。
        /// </summary>
        /// <param name="strExps">要连接的字符串数组。</param>
        /// <returns></returns>
        public virtual string Concat(params object[] strExps)
        {
            return string.Join(" + ", strExps);
        }

        /// <summary>
        /// 反转源表达式。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        public virtual string Reverse(object sourceExp)
        {
            return string.Format("REVERSE({0})", sourceExp);
        }

        /// <summary>
        /// 正则表达式匹配。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="regexExp">正则表达式。</param>
        /// <returns></returns>
        public virtual string IsMatch(object sourceExp, object regexExp)
        {
            throw new SyntaxParseException(MethodInfo.GetCurrentMethod().Name);
        }
    }
}
