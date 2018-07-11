// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Emit
{
    /// <summary>
    /// 访问性修饰符。
    /// </summary>
    public enum CallingDecoration
    {
        /// <summary>
        /// 基本的。
        /// </summary>
        Standard,

        /// <summary>
        /// 抽象的。
        /// </summary>
        Abstract,

        /// <summary>
        /// 密封的。
        /// </summary>
        Sealed,

        /// <summary>
        /// 多态的。
        /// </summary>
        Virtual,

        /// <summary>
        /// 静态的。
        /// </summary>
        Static,

        /// <summary>
        /// 显式实现的。
        /// </summary>
        ExplicitImpl,
    }
}
