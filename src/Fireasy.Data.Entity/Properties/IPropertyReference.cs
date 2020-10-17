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
    /// 表示对其他属性的引用。
    /// </summary>
    public interface IPropertyReference
    {
        IProperty Reference { get; set; }
    }
}
