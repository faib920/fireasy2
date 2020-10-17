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
    /// 提供对象延迟加载的管理。
    /// </summary>
    public interface ILazyManager
    {
        /// <summary>
        /// 判断指定的属性的值是否已创建。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <returns></returns>
        bool IsValueCreated(string propertyName);
    }
}
