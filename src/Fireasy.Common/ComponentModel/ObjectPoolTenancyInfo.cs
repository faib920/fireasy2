// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 定义缓冲池中多租户的信息。
    /// </summary>
    public sealed class ObjectPoolTenancyInfo
    {
        public static readonly ObjectPoolTenancyInfo Default = new ObjectPoolTenancyInfo("$default");

        /// <summary>
        /// 初始化 <see cref="ObjectPoolTenancyInfo"/> 类的新实例。
        /// </summary>
        /// <param name="key">Key。</param>
        public ObjectPoolTenancyInfo(string key)
        {
            Key = key;
        }

        /// <summary>
        /// 获取 Key。
        /// </summary>
        public string Key { get; }
    }
}
