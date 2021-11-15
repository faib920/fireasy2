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
    /// 对 <see cref="ConcurrentDictionary{TKey, TValue}"/> 线程安全字典的扩展，使之真正意义上的安全。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SafetyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly LazyThreadSafetyMode _mode = LazyThreadSafetyMode.ExecutionAndPublication;

        /// <summary>
        /// 实例化 <see cref="SafetyDictionary{TKey, TValue}"/> 类的新实例。 
        /// </summary>
        public SafetyDictionary()
        {
            LazyValues = new ConcurrentDictionary<TKey, Lazy<TValue>>();
        }

        /// <summary>
        /// 实例化 <see cref="SafetyDictionary{TKey, TValue}"/> 类的新实例。 
        /// </summary>
        /// <param name="comparer"></param>
        public SafetyDictionary(IEqualityComparer<TKey> comparer)
        {
            LazyValues = new ConcurrentDictionary<TKey, Lazy<TValue>>(comparer);
        }

        /// <summary>
        /// 实例化 <see cref="SafetyDictionary{TKey, TValue}"/> 类的新实例。 
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="comparer"></param>
        public SafetyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
            : this(comparer)
        {
            if (collection != null)
            {
                foreach (var kvp in collection)
                {
                    LazyValues.TryAdd(kvp.Key, new Lazy<TValue>(() => kvp.Value, _mode));
                }
            }
        }

        /// <summary>
        /// 实例化 <see cref="SafetyDictionary{TKey, TValue}"/> 类的新实例。 
        /// </summary>
        /// <param name="collection"></param>
        public SafetyDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this()
        {
            if (collection != null)
            {
                foreach (var kvp in collection)
                {
                    LazyValues.TryAdd(kvp.Key, new Lazy<TValue>(() => kvp.Value, _mode));
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
            var lazy = LazyValues.GetOrAdd(key, k => new Lazy<TValue>(valueFactory, _mode));
            return TryGetLazyValue(key, lazy);
        }

        /// <summary>
        /// 尝试通过 key 获取值，如果 key 不存在则通过函数生成新值并添加到字典中。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory">新值的函数。</param>
        /// <returns></returns>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var lazy = LazyValues.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(key), _mode));
            return TryGetLazyValue(key, lazy);
        }

        /// <summary>
        /// 尝试通过 key 获取值，如果 key 不存在则通过函数生成新值并添加到字典中。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arg1">参数1。</param>
        /// <param name="valueFactory">新值的函数。</param>
        /// <returns></returns>
        public TValue GetOrAdd<TArg1>(TKey key, TArg1 arg1, Func<TKey, TArg1, TValue> valueFactory)
        {
            var lazy = LazyValues.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(key, arg1), _mode));
            return TryGetLazyValue(key, lazy);
        }

        /// <summary>
        /// 尝试通过 key 获取值，如果 key 不存在则通过函数生成新值并添加到字典中。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="arg1">参数1。</param>
        /// <param name="arg2">参数2。</param>
        /// <param name="valueFactory">新值的函数。</param>
        /// <returns></returns>
        public TValue GetOrAdd<TArg1, TArg2>(TKey key, TArg1 arg1, TArg2 arg2, Func<TKey, TArg1, TArg2, TValue> valueFactory)
        {
            var lazy = LazyValues.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(key, arg1, arg2), _mode));
            return TryGetLazyValue(key, lazy);
        }

        /// <summary>
        /// 尝试添加新值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(TKey key, TValue value)
        {
            return LazyValues.TryAdd(key, new Lazy<TValue>(() => value, _mode));
        }

        /// <summary>
        /// 尝试添加新值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="valueFactory">新值的函数。</param>
        /// <returns></returns>
        public bool TryAdd(TKey key, Func<TValue> valueFactory)
        {
            return LazyValues.TryAdd(key, new Lazy<TValue>(valueFactory, _mode));
        }

        /// <summary>
        /// 尝试获取指定 key 的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (LazyValues.TryGetValue(key, out Lazy<TValue> lazy))
            {
                if (lazy != null)
                {
                    value = lazy.Value;
                    return true;
                }
            }

            value = default;
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
            var lazy = LazyValues.AddOrUpdate(key,
                k => new Lazy<TValue>(addOrUpdateFactory, _mode),
                (k, v) => new Lazy<TValue>(addOrUpdateFactory, _mode));

            return TryGetLazyValue(key, lazy);
        }

        /// <summary>
        /// 添加或修改指定 key 的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="addOrUpdateFactory"></param>
        /// <returns></returns>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addOrUpdateFactory)
        {
            var lazy = LazyValues.AddOrUpdate(key,
                k => new Lazy<TValue>(() => addOrUpdateFactory(key), _mode),
                (k, v) => new Lazy<TValue>(() => addOrUpdateFactory(key), _mode));

            return TryGetLazyValue(key, lazy);
        }

        /// <summary>
        /// 尝试从字典里移除指定 key 的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (LazyValues.TryRemove(key, out Lazy<TValue> lazy))
            {
                if (lazy.IsValueCreated && lazy.Value != null)
                {
                    value = lazy.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 判断字典里是否存在指定的 key。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return LazyValues.ContainsKey(key);
        }

        /// <summary>
        /// 清空字典。
        /// </summary>
        public void Clear()
        {
            LazyValues.Clear();
        }

        /// <summary>
        /// 获取内部的字典。
        /// </summary>
        public ConcurrentDictionary<TKey, Lazy<TValue>> LazyValues { get; private set; }

        /// <summary>
        /// 尝试移除没有创建值的项。
        /// </summary>
        /// <returns></returns>
        public SafetyDictionary<TKey, TValue> TryRemoveDiscreated()
        {
            foreach (var item in LazyValues.Skip(1).Where(s => !s.Value.IsValueCreated))
            {
                LazyValues.TryRemove(item.Key, out Lazy<TValue> value);
            }

            return this;
        }

        /// <summary>
        /// 返回值的个数。
        /// </summary>
        public int Count => LazyValues.Count;

        public TValue this[TKey key] => LazyValues.ContainsKey(key) && LazyValues[key] != null ? LazyValues[key].Value : default;

        public IEnumerable<TKey> Keys => LazyValues.Keys;

        public IEnumerable<TValue> Values => LazyValues.Select(s => s.Value.Value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new SafetyDictionaryEnumerator(LazyValues.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        private TValue TryGetLazyValue(TKey key, Lazy<TValue> lazy)
        {
            try
            {
                return lazy != null && lazy.Value != null ? lazy.Value : default;
            }
            catch
            {
                LazyValues.TryRemove(key, out _);
                throw;
            }
        }

        /// <summary>
        /// 枚举器。
        /// </summary>
        private class SafetyDictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly IEnumerator<KeyValuePair<TKey, Lazy<TValue>>> _enumerator;

            public SafetyDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, Lazy<TValue>>> enumerator)
            {
                _enumerator = enumerator;
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_enumerator.Current.Key, _enumerator.Current.Value.Value);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                _enumerator.Reset();
            }
        }
    }
}
