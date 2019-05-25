// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 用于保存实体的属性值的字典。
    /// </summary>
    [Serializable]
    internal sealed class EntityEntryDictionary : IEnumerable<KeyValuePair<string, EntityEntry>>
    {
        private readonly Dictionary<string, EntityEntry> dicEntry = new Dictionary<string, EntityEntry>();

        /// <summary>
        /// 重置所有属性的修改状态。
        /// </summary>
        internal void Reset()
        {
            foreach (var kvp in dicEntry)
            {
                kvp.Value.Reset();
            }
        }

        /// <summary>
        /// 获取被修改过的所有属性的名称。
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<string> GetModifiedProperties()
        {
            return from kvp in dicEntry where kvp.Value.IsModified select kvp.Key;
        }

        /// <summary>
        /// 判断指定的属性是否有值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <returns></returns>
        internal bool Has(string propertyName)
        {
            return dicEntry.ContainsKey(propertyName);
        }

        /// <summary>
        /// 初始化属性的值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <param name="value">要设置的值。</param>
        /// <param name="addAction">如果在此之前没有设置值，则通过此方法回调。</param>
        internal void Initializate(string propertyName, PropertyValue value, Action addAction = null)
        {
            if (Has(propertyName))
            {
                dicEntry[propertyName].Initializate(value);
            }
            else
            {
                if (addAction != null)
                {
                    addAction();
                }

                dicEntry.Add(propertyName, EntityEntry.InitByOldValue(value));
            }
        }

        /// <summary>
        /// 修改属性的值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <param name="value">要设置的值。</param>
        /// <param name="addAction">如果在此之前没有设置值，则通过此方法回调。</param>
        internal void Modify(string propertyName, PropertyValue value, Action addAction = null)
        {
            if (Has(propertyName))
            {
                dicEntry[propertyName].Modify(value);
            }
            else
            {
                if (addAction != null)
                {
                    addAction();
                }

                dicEntry.Add(propertyName, EntityEntry.InitByNewValue(value));
            }
        }

        internal void Modify(string propertyName, bool modified = true)
        {
            if (!modified && Has(propertyName))
            {
                dicEntry[propertyName].Reset();
            }
            else
            {
                if (Has(propertyName))
                {
                    dicEntry[propertyName].Modify();
                }
                else
                {
                    dicEntry.Add(propertyName, EntityEntry.Modified());
                }
            }
        }

        /// <summary>
        /// 索引器，返回指定属性名的属性值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <returns></returns>
        internal EntityEntry this[string propertyName]
        {
            get { return dicEntry[propertyName]; }
        }

        /// <summary>
        /// 返回循环返回此字典的枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, EntityEntry>> GetEnumerator()
        {
            return dicEntry.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
