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
    /// 字符串函数解析接口。
    /// </summary>
    public interface IStringSyntax
    {
        /// <summary>
        /// 取源表达式的子串。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="startExp">子串的起始字符位置。</param>
        /// <param name="lenExp">子串中的字符数。</param>
        /// <returns></returns>
        string Substring(object sourceExp, object startExp, object lenExp = null);

        /// <summary>
        /// 计算源表达式的长度。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Length(object sourceExp);

        /// <summary>
        /// 判断子串在源表达式中的位置。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="searchExp">要搜寻的字符串。</param>
        /// <param name="startExp">搜索起始位置。</param>
        /// <param name="countExp">要检查的字符位置数</param>
        /// <returns></returns>
        string IndexOf(object sourceExp, object searchExp, object startExp = null, object countExp = null);

        /// <summary>
        /// 将源表达式转换为小写格式。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string ToLower(object sourceExp);

        /// <summary>
        /// 将源表达式转换为大写格式。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string ToUpper(object sourceExp);

        /// <summary>
        /// 截掉源表达式的左边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string TrimStart(object sourceExp);

        /// <summary>
        /// 截掉源表达式的右边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string TrimEnd(object sourceExp);

        /// <summary>
        /// 截掉源表达式的两边所有空格。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Trim(object sourceExp);

        /// <summary>
        /// 将源表达式中的部份字符替换为新的字符。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="searchExp">要搜寻的字符串。</param>
        /// <param name="replaceExp">要替换的字符串。</param>
        /// <returns></returns>
        string Replace(object sourceExp, object searchExp, object replaceExp);

        /// <summary>
        /// 将一组字符串连接为新的字符串。
        /// </summary>
        /// <param name="strExps">要连接的字符串数组。</param>
        /// <returns></returns>
        string Concat(params object[] strExps);

        /// <summary>
        /// 反转源表达式。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <returns></returns>
        string Reverse(object sourceExp);

        /// <summary>
        /// 删除字符串表达式中的字符。
        /// </summary>
        /// <param name="sourceExp">源表达式。</param>
        /// <param name="startExp">起始字符位置。</param>
        /// <param name="lenExp">要删除的长度。</param>
        /// <returns></returns>
        string Remove(object sourceExp, object startExp, object lenExp);
    }
}
