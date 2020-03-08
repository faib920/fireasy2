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
using System.Collections.ObjectModel;
using System.Linq;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 枚举器扩展方法。
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// 将一个序列转换为 <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1"/>。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> source)
        {
            var list = source == null ? new List<T>() : new List<T>(source);
            return list.AsReadOnly();
        }

        /// <summary>
        /// 判断序列中是否有元素。
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            if (source == null)
            {
                return true;
            }
            if (source is ICollection collection)
            {
                return collection.Count == 0;
            }
            var enu = source.GetEnumerator();
            var hasElement = enu.MoveNext();
            enu.TryDispose();
            return !hasElement;
        }

        /// <summary>
        /// 枚举序列中的所有元素，并执行指定的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Guard.ArgumentNull(source, nameof(source));
            Guard.ArgumentNull(action, nameof(action));

            foreach (var item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// 枚举序列中的所有元素，并执行指定的方法（方法中带索引参数）。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            Guard.ArgumentNull(source, nameof(source));
            Guard.ArgumentNull(action, nameof(action));

            var index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        /// <summary>
        /// 并行的方式枚举序列中的所有元素，并执行指定的方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="action"></param>
        public static void ForEachParallel<T>(this IEnumerable<T> source, Action<T> action)
        {
            Guard.ArgumentNull(source, nameof(source));
            Guard.ArgumentNull(action, nameof(action));

            source.AsParallel().ForAll(action);
        }

        /// <summary>
        /// 返回一个序列的切片。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, int? start = null, int? stop = null, int? step = null)
        {
            Guard.ArgumentNull(source, nameof(source));
            Guard.Argument(step != 0, nameof(step));

            var sourceCollection = source as IList<T> ?? new List<T>(source);

            if (sourceCollection.Count == 0)
            {
                yield break;
            }

            var stepCount = step ?? 1;
            var startIndex = start ?? ((stepCount > 0) ? 0 : sourceCollection.Count - 1);
            var stopIndex = stop ?? ((stepCount > 0) ? sourceCollection.Count : -1);

            if (start < 0)
            {
                startIndex = sourceCollection.Count + startIndex;
            }

            if (stop < 0)
            {
                stopIndex = sourceCollection.Count + stopIndex;
            }

            startIndex = Math.Max(startIndex, (stepCount > 0) ? 0 : int.MinValue);
            startIndex = Math.Min(startIndex, (stepCount > 0) ? sourceCollection.Count : sourceCollection.Count - 1);
            stopIndex = Math.Max(stopIndex, -1);
            stopIndex = Math.Min(stopIndex, sourceCollection.Count);

            for (var i = startIndex; (stepCount > 0) ? i < stopIndex : i > stopIndex; i += stepCount)
            {
                yield return sourceCollection[i];
            }

            yield break;
        }

        /// <summary>
        /// 构造一个不可变的序列。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IEnumerable<T> MakeImmutable<T>(this IEnumerable<T> source)
        {
            var isReadOnlyCollection = source is ReadOnlyCollection<T>;

            var isMutable = source is T[] || source is IList || source is ICollection<T>;

            if (isReadOnlyCollection || !isMutable)
            {
                return source;
            }
            else
            {
                return CreateImmutableCollection(source);
            }
        }

        /// <summary>
        /// 将一个 <see cref="IEnumerable"/> 枚举成泛型 <typeparamref name="T"/> 的枚举。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static IEnumerable<T> Enumerable<T>(this IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return (T)item;
            }
        }

        public static void Cycle<T>(this IList<T> list, int start, Action<T> action)
        {
            var count = list.Count;
            Guard.OutOfRange(count, start);
            var i = start;
            var reback = false;

            list.ElementAt(1);

            while (true)
            {
                if (i >= count)
                {
                    i = 0;
                    reback = true;
                }

                if (reback && i >= start)
                {
                    break;
                }

                action?.Invoke(list[i]);

                i++;
            }
        }

        private static IEnumerable<T> CreateImmutableCollection<T>(this IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }
        }
    }
}
