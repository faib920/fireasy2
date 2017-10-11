// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// 提供对 LINQ 表达式的分组查询。无法继承此类。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    public sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly TKey m_key;
        private readonly IEnumerable<TElement> m_elements;

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.Linq.Grouping`2"/> 类的新实例。
        /// </summary>
        /// <param name="key">表示分组中的键。</param>
        /// <param name="elements">一个元素的序列。</param>
        public Grouping(TKey key, IEnumerable<TElement> elements)
        {
            m_key = key;
            m_elements = elements;
        }

        /// <summary>
        /// 获取该序列的键。
        /// </summary>
        public TKey Key
        {
            get { return m_key; }
        }

        /// <summary>
        /// 返回枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TElement> GetEnumerator()
        {
            return m_elements == null ? null : m_elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_elements.GetEnumerator();
        }
    }
}
