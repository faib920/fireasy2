// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 判断定义实体相等比较的方法。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EqualityComparer<T> : IEqualityComparer<T> where T : IEntity
    {
        /// <summary>
        /// 指定的 x 是否等于 y。
        /// </summary>
        /// <param name="entityX">实体对象 x。</param>
        /// <param name="entityY">实体对象 y。</param>
        /// <returns></returns>
        public bool Equals(T entityX, T entityY)
        {
            return entityX != null && entityX.Equals(entityY);
        }

        /// <summary>
        /// 获得实体对象的哈希值。
        /// </summary>
        /// <param name="entity">实体对象。</param>
        /// <returns></returns>
        public int GetHashCode(T entity)
        {
            return 0;
        }
    }
}
