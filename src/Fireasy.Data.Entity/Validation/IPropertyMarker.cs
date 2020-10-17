// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 为 <see cref="ValidationAttribute"/> 标注其关联的 <see cref="IProperty"/> 对象。
    /// </summary>
    public interface IPropertyMarker
    {
        /// <summary>
        /// 获取或设置关联的属性对象。
        /// </summary>
        IProperty Property { get; set; }
    }
}
