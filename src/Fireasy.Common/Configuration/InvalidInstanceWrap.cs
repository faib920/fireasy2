// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Configuration
{
    /// <summary>
    /// 无效配置项的包装体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class InvalidInstanceWrap<T> where T : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置实例。
        /// </summary>
        public T Instance { get; set; }
    }
}
