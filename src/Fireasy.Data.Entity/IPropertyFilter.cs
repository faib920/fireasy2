// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体持久化时的属性过滤器。
    /// </summary>
    public interface IPropertyFilter
    {
        /// <summary>
        /// 过滤指定的属性。
        /// </summary>
        /// <param name="property"></param>
        /// <returns>为 true 表示允许持久化该属性。</returns>
        bool Filter(IProperty property);
    }
}
