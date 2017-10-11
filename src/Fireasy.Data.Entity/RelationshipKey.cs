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
        /// 获取或设置主体主属性名称。
        /// </summary>
        public string ThisKey { get; set; }

        /// <summary>
        /// 获取主体主属性。
        /// </summary>
        public IProperty ThisProperty { get; internal set; }

        /// <summary>
        /// 获取或设置客体主属性名称。
        /// </summary>
        public string OtherKey { get; set; }

        /// <summary>
        /// 获取客体主属性。
        /// </summary>
        public IProperty OtherProperty { get; internal set; }
    }
}
