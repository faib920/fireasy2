// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Text;
using System.Text.RegularExpressions;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 字符相关的扩展方法。
    /// </summary>
    public static class CharExtension
    {
        /// <summary>
        /// 判断指定的字符是否是中文字符。
        /// </summary>
        /// <param name="character">要判断的字符。</param>
        /// <returns>如果字符是中文字符，则为 true；否则为 false。</returns>
        public static bool IsChinese(this char character)
        {
            return Regex.IsMatch(character.ToString(), "^[\u4e00-\u9fa5]$");
        }

        /// <summary>
        /// 获取字符的区位码。
        /// </summary>
        /// <param name="character">一个字符。</param>
        /// <returns>字符的区位码。</returns>
        public static int GetAsciiCode(this char character)
        {
            var qwBytes = Encoding.GetEncoding(0).GetBytes(character.ToString());

            if (qwBytes.Length == 1)
            {
                return qwBytes[0];
            }

            return qwBytes[0] * 256 + qwBytes[1] - 65536;
        }

        /// <summary>
        /// 判断是否为回车换行符。
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static bool IsLine(this char character)
        {
            return (character == '\r' || character == '\n');
        }
    }
}
