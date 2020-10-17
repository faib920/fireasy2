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
    /// 用于描述关系中的主从键对。
    /// </summary>
    public sealed class RelationshipKey
    {
        /// <summary>
        /// 获取或设置主要的属性名称。
        /// </summary>
        public string PrincipalKey { get; set; }

        /// <summary>
        /// 获取主要的属性。
        /// </summary>
        public IProperty PrincipalProperty { get; internal set; }

        /// <summary>
        /// 获取或设置从属的属性名称。
        /// </summary>
        public string DependentKey { get; set; }

        /// <summary>
        /// 获取从属的属性。
        /// </summary>
        public IProperty DependentProperty { get; internal set; }
    }
}
