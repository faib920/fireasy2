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
    /// 提供实体间的关联状态支持。
    /// </summary>
    public interface IEntityRelation
    {
        /// <summary>
        /// 获取或设置所有者。
        /// </summary>
        EntityOwner Owner { get; set; }

        /// <summary>
        /// 通知实体已经修改。
        /// </summary>
        /// <param name="propertyName">属性名称。</param>
        void NotifyModified(string propertyName);
    }
}
