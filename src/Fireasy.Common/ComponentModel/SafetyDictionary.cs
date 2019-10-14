// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 对 <see cref="ConcurrentDictionary"/> 线程安全字典的扩展，使之真正意义上的安全。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SafetyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> dic = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        private LazyThreadSafetyMode mode = LazyThreadSafetyMode.ExecutionAndPublication;

        /// <summary>
        /// 实例化 <see cref="SafetyDictionary"/> 类的新实例。 
        /// </summary>
        public SafetyDictionary()
        {
        }

        /// <summary>
        /// 实例化 <see cref="SafetyDictionary"/> 类的新实例。 
        /// </summary>
        /// <param name="collection"></param>
        public SafetyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            if (collection != null)
            {
                foreach (var kvp in collection)
                {
                    dic.TryAdd(kvp.Key, new Lazy<TValue>(() => kvp.Value, mode));
                }
            }
        }

        /// <summary>
        /// 尝试通过 key 获取值，如果 key 不存在则通过函数生成新值并添加到字典中。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory">新值的函数。</param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey key, Func<TValue> valueFactory)
        {
            var lazy = dic.GetOrAdd(key, k => new Lazy<TValue>(valueFactory, mode));
            return lazy != null && lazy.Value != null ? lazy.Value : default(TValue);
        }

        /// <summary>
        /// 尝试添加新值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(TKey key, TValue value)
        {
            return dic.TryAdd(key, new Lazy<TValue>(() => value, mode));
        }

        /// <summary>
        /// 尝试添加新值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory">新值的函数。</param>
        /// <returns></returns>
        public bool TryAdd(TKey key, Func<TValue> valueFactory)
        {
            return dic.TryAdd(key, new Lazy<TValue>(valueFactory, mode));
        }

        /// <summary>
        /// 尝试获取指定 key 的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (dic.TryGetValue(key, out Lazy<TValue> lazy))
            {
                if (lazy != null)
                {
                    value = lazy.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// 添加或修改指定 key 的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="addOrUpdateFactory"></param>
        /// <returns></returns>
        public TValue AddOrUpdate(TKey key, Func<TValue> addOrUpdateFactory)
        {
            var lazy = dic.AddOrUpdate(key,
                k => new Lazy<TValue>(addOrUpdateFactory, mode),
                (k, v) => new Lazy<TValue>(addOrUpdateFactory, mode));

            if (lazy != null)
            {
                return lazy.Value;
            }

            return default(TValue);
        }

        /// <summary>
        /// 尝试从字典里移除指定 key 的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (dic.TryRemove(key, out Lazy<TValue> lazy))
            {
                if (lazy.IsValueCreated && lazy.Value != null)
                {
                    value = lazy.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// 判断字典里是否存在指定的 key。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return dic.ContainsKey(key);
        }

        /// <summary>
        /// 清空字典。
        /// </summary>
        public void Clear()
        {
            dic.Clear();
        }

        /// <summary>
        /// 获取内部的字典。
        /// </summary>
        public ConcurrentDictionary<TKey, Lazy<TValue>> LazyValues
        {
            get { return dic; }
        }

        public SafetyDictionary<TKey, TValue> TryRemoveDiscreated()
        {
            foreach (var item in dic.Skip(1).Where(s => !s.Value.IsValueCreated))
            {
                dic.TryRemove(item.Key, out Lazy<TValue> value);
            }

            return this;
        }

        /// <summary>
        /// 返回值的个数。
        /// </summary>
        public int Count => dic.Count;

        public TValue this[TKey key] => dic.ContainsKey(key) && dic[key] != null ? dic[key].Value : default(TValue);

        public IEnumerable<TKey> Keys => dic.Keys;

        public IEnumerable<TValue> Values => dic.Select(s => s.Value.Value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new SafetyDictionaryEnumerator<TKey, TValue>(dic.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 枚举器。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        private class SafetyDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private IEnumerator<KeyValuePair<TKey, Lazy<TValue>>> enumerator;

            public SafetyDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, Lazy<TValue>>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(enumerator.Current.Key, enumerator.Current.Value.Value);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }
        }
    }
}
