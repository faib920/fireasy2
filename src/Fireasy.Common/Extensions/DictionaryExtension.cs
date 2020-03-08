// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 字典的扩展方法。
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// 尝试添加键和值，如果已经存在该键则忽略。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            Guard.ArgumentNull(dictionary, nameof(dictionary));
            Guard.ArgumentNull(valueFactory, nameof(valueFactory));

            if (dictionary.ContainsKey(key) == false)
            {
                dictionary.Add(key, valueFactory());
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试添加键和值，如果已经存在该键则忽略。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            Guard.ArgumentNull(dictionary, nameof(dictionary));

            if (dictionary.ContainsKey(key) == false)
            {
                dictionary.Add(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 尝试根据键值从字典中获取对应的值，如果键不存在时，则添加使用函数返回的值到字典中。
        /// </summary>
        /// <typeparam name="TKey">字典中的键的类型。</typeparam>
        /// <typeparam name="TValue">字典中的值的类型。</typeparam>
        /// <param name="dictionary">当前操作的字典。</param>
        /// <param name="key">元素的键。</param>
        /// <param name="func">一个函数，返回一个元素的值，并添加到字典中。</param>
        /// <returns>如果字典内查找到键，则为键对应的元素的值，否则为函数返回的值。</returns>
        public static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> func)
        {
            if (!dictionary.TryGetValue(key, out TValue value))
            {
                Guard.ArgumentNull(func, nameof(func));
                value = func();
                dictionary.Add(key, value);
            }

            return value;
        }

        /// <summary>
        /// 添加键和值，如果键已存在，则使用新值替换。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            Guard.ArgumentNull(dictionary, nameof(dictionary));

            dictionary[key] = value;
            return dictionary;
        }

        /// <summary>
        /// 获取字典的值。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            Guard.ArgumentNull(dictionary, nameof(dictionary));
            return dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
        }

        /// <summary>
        /// 对字典进行排序。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static SortedDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            Guard.ArgumentNull(dictionary, nameof(dictionary));
            return new SortedDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// 使用一个 <see cref="System.Collections.Generic.IComparer{T}"/> 对字典进行排序。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public static SortedDictionary<TKey, TValue> Sort<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            Guard.ArgumentNull(dictionary, nameof(dictionary));
            return new SortedDictionary<TKey, TValue>(dictionary, comparer);
        }
    }
}
