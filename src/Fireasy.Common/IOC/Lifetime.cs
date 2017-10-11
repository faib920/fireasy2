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
        /// 实例化。
        /// </summary>
        Instance,
        /// <summary>
        /// 单例。
        /// </summary>
        Singleton
    }
}
