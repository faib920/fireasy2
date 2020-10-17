// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Caching
{
    /// <summary>
    /// 基于内存缓存的策略。
    /// </summary>
    public interface IMemoryCacheStrategy
    {
        /// <summary>
        /// 返回是否使用独立存储。
        /// </summary>
        bool UseStandaloneStorage { get; }
    }
}
