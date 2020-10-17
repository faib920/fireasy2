// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.Caching
{
    public sealed class CacheTenancyInfo
    {
        public static readonly CacheTenancyInfo Default = new CacheTenancyInfo("$default");

        /// <summary>
        /// 初始化 <see cref="CacheTenancyInfo"/> 类的新实例。
        /// </summary>
        /// <param name="key">key。</param>
        public CacheTenancyInfo(string key)
        {
            Key = key;
        }

        /// <summary>
        /// 获取 key。
        /// </summary>
        public string Key { get; }
    }
}
