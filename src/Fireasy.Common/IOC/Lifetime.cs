// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Ioc
{
    /// <summary>
    /// 实例的生命周期。
    /// </summary>
    public enum Lifetime
    {
        /// <summary>
        /// 瞬时。
        /// </summary>
        Transient,
        /// <summary>
        /// 单例。
        /// </summary>
        Singleton,
        /// <summary>
        /// 作用域。
        /// </summary>
        Scoped
    }
}
