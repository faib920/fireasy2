// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
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
        private readonly Dictionary<TValue, Expression<Func<TSource, bool>>> _expressions = new Dictionary<TValue, Expression<Func<TSource, bool>>>();
        private Expression<Func<TSource, bool>> _else;

        /// <summary>
        /// 获取匹配的表达式。
        /// </summary>
        /// <param name="value"></param>
        public Expression<Func<TSource, bool>> Build(TValue value)
        {
            if (_expressions.Count > 0 && _expressions.TryGetValue(value, out Expression<Func<TSource, bool>> exp))
            {
                return exp;
            }
            
            return _else;
        }

        /// <summary>
        /// 指定在不同 <paramref name="value"/> 时所使用的断言。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public SwitchBuilder<TSource, TValue> When(TValue value, Expression<Func<TSource, bool>> predicate)
        {
            _expressions.AddOrReplace(value, predicate);
            return this;
        }

        /// <summary>
        /// 指定其他情况所使用的断言。
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public SwitchBuilder<TSource, TValue> Else(Expression<Func<TSource, bool>> predicate)
        {
            _else = predicate;
            return this;
        }
    }
}
