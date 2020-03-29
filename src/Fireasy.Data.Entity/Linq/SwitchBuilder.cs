// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq
{
    /// <summary>
    /// Switch 表达式构造器。
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class SwitchBuilder<TSource, TValue> where TValue : IComparable
    {
        private readonly TValue value;

        /// <summary>
        /// 初始化 <see cref="SwitchBuilder{TSource, TValue}"/> 类的新实例。
        /// </summary>
        /// <param name="value"></param>
        public SwitchBuilder(TValue value)
        {
            this.value = value;
        }

        /// <summary>
        /// 获取匹配的表达式。
        /// </summary>
        public Expression<Func<TSource, bool>> Expression { get; private set; }

        /// <summary>
        /// 指定在不同 <paramref name="value"/> 时所使用的断言。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public SwitchBuilder<TSource, TValue> When(TValue value, Expression<Func<TSource, bool>> predicate)
        {
            if (Expression == null)
            {
                var equals = value is IComparable<TValue> comparable
                    ? comparable.CompareTo(this.value) == 0
                    : value.CompareTo(this.value) == 0;

                if (equals)
                {
                    Expression = predicate;
                }
            }

            return this;
        }
    }
}
