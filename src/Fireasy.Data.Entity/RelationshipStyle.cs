// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 描述关系的类型。
    /// </summary>
    public enum RelationshipStyle
    {
        /// <summary>
        /// 一对多的关系。
        /// </summary>
        One2Many,
        /// <summary>
        /// 多对一的关系。
        /// </summary>
        Many2One,
        /// <summary>
        /// 一对一的关系。
        /// </summary>
        One2One
    }

    /// <summary>
    /// 描述关系的来源。
    /// </summary>
    internal enum RelationshipSource
    {
        AssemblyAttribute,
        AutomaticallyAssign,
        MetadataBuild
    }
}
