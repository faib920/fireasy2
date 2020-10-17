// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity.Properties
{
    /// <summary>
    /// 提供对属性的延迟加载。
    /// </summary>
    public interface IPropertyLazyLoadder
    {
        /// <summary>
        /// 获取实体对象中指定属性的值。
        /// </summary>
        /// <param name="entity">当前访问的实体对象。</param>
        /// <param name="property">要加载的属性。</param>
        /// <returns></returns>
        PropertyValue GetValue(IEntity entity, IProperty property);
    }
}
