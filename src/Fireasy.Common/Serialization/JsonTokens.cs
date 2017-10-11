// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Json 令牌类型。
    /// </summary>
    public sealed class JsonTokens
    {
        /// <summary>
        /// 表示对象开始的字符。
        /// </summary>
        public const char StartObjectLiteralCharacter = '{';

        /// <summary>
        /// 表示对象结束的字符。
        /// </summary>
        public const char EndObjectLiteralCharacter = '}';

        /// <summary>
        /// 表示数组开始的字符。
        /// </summary>
        public const char StartArrayCharacter = '[';

        /// <summary>
        /// 表示数组结束的字符。
        /// </summary>
        public const char EndArrayCharacter = ']';

        /// <summary>
        /// 表示字符串的引用符。
        /// </summary>
        public const char StringDelimiter = '"';

        /// <summary>
        /// 表示元素的分隔符。
        /// </summary>
        public const char ElementSeparator = ',';

        /// <summary>
        /// 表示属性的命名符。
        /// </summary>
        public const char PairSeparator = ':';
    }
}
