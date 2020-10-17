// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections;

namespace Fireasy.Common
{
    /// <summary>
    /// 布隆过滤器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class BloomFilter<T>
    {
        private readonly BitArray _array;

        /// <summary>
        /// 获取布隆数组的大小。
        /// </summary>
        public long BloomArryLength { get; private set; }

        /// <summary>
        /// 获取数据的长度。
        /// </summary>
        public long DataArrayLength { get; private set; }

        /// <summary>
        /// 获取 Hash 数。
        /// </summary>
        public long BitIndexCount { get; private set; }

        /// <summary>
        /// 初始化 <see cref="BloomFilter{T}"/> 类的新实例。
        /// </summary>
        /// <param name="bloomArryLength">布隆数组的大小。</param>
        /// <param name="dataArrayLength">数据的长度。</param>
        /// <param name="bitIndexCount">hash数。</param>
        public BloomFilter(int bloomArryLength, int dataArrayLength, int bitIndexCount)
        {
            _array = new BitArray(bloomArryLength);
            BloomArryLength = bloomArryLength;
            DataArrayLength = dataArrayLength;
            BitIndexCount = bitIndexCount;
        }

        /// <summary>
        /// 向过滤器添加数据。
        /// </summary>
        /// <param name="value"></param>
        public void Add(T value)
        {
            var hashCode = value.GetHashCode();
            Random random = new Random(hashCode);

            for (int i = 0; i < BitIndexCount; i++)
            {
                var c = random.Next((int)(BloomArryLength - 1));
                _array[c] = true;
            }
        }

        /// <summary>
        /// 获取指定的数据是否存在。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsExist(T value)
        {
            var hashCode = value.GetHashCode();
            var random = new Random(hashCode);

            for (var i = 0; i < BitIndexCount; i++)
            {
                if (!_array[random.Next((int)(BloomArryLength - 1))])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取错误概率。
        /// </summary>
        /// <returns></returns>
        public double GetErrorProbability()
        {
            // (1 - e^(-k * n / m)) ^ k
            return Math.Pow(1 - Math.Exp(-BitIndexCount * (double)DataArrayLength / BloomArryLength),
                    BitIndexCount);
        }
    }
}
