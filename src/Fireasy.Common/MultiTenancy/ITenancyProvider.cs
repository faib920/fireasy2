// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.MultiTenancy
{
    /// <summary>
    /// 多租户信息提供者。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITenancyProvider<T> where T : class
    {
        /// <summary>
        /// 解析租户信息。
        /// </summary>
        /// <returns></returns>
        T Resolve(T tenancy);
    }
}
