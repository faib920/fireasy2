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
        private readonly Dictionary<string, EntityEntry> _entries = new Dictionary<string, EntityEntry>();

        /// <summary>
        /// 重置所有属性的修改状态。
        /// </summary>
        internal void Reset()
        {
            foreach (var kvp in _entries)
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
            return from kvp in _entries where kvp.Value.IsModified select kvp.Key;
        }

        /// <summary>
        /// 判断指定的属性是否有值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <returns></returns>
        internal bool Has(string propertyName)
        {
            return _entries.ContainsKey(propertyName);
        }

        /// <summary>
        /// 初始化属性的值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <param name="value">要设置的值。</param>
        internal void Initializate(string propertyName, PropertyValue value)
        {
            _entries[propertyName] = EntityEntry.InitByOldValue(value);
        }

        /// <summary>
        /// 修改属性的值。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <param name="value">要设置的值。</param>
        internal void Modify(string propertyName, PropertyValue value)
        {
            if (Has(propertyName))
            {
                _entries[propertyName].Modify(value);
            }
            else
            {
                _entries.Add(propertyName, EntityEntry.InitByNewValue(value));
            }
        }

        internal void Add(string propertyName, EntityEntry entry)
        {
            _entries.Add(propertyName, entry);
        }

        internal void Modify(string propertyName, bool modified = true)
        {
            if (!modified && Has(propertyName))
            {
                _entries[propertyName].Reset();
            }
            else
            {
                if (Has(propertyName))
                {
                    _entries[propertyName].Modify();
                }
                else
                {
                    _entries.Add(propertyName, EntityEntry.Modified());
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
            get { return _entries[propertyName]; }
        }

        /// <summary>
        /// 返回循环返回此字典的枚举器。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, EntityEntry>> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
