// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using Fireasy.Data.Entity.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Translators
{
    /// <summary>
    /// 用于在 ELinq 表达式中搜寻目录表达式，并将找到的表达式替换为指定的表达式。无法继承此类。
    /// </summary>
    public sealed class DbExpressionReplacer : DbExpressionVisitor
    {
        private readonly Expression _searchFor;
        private readonly Expression _replaceWith;
        private readonly Func<Expression, Expression> _replaceWithFunc;

        /// <summary>
        /// 初始化 <see cref="DbExpressionReplacer"/> 类的新实例。
        /// </summary>
        /// <param name="searchFor">搜寻的目标表达式。</param>
        /// <param name="replaceWith">替换的表达式。</param>
        private DbExpressionReplacer(Expression searchFor, Expression replaceWith)
        {
            _searchFor = searchFor;
            _replaceWith = replaceWith;
        }

        /// <summary>
        /// 初始化 <see cref="DbExpressionReplacer"/> 类的新实例。
        /// </summary>
        /// <param name="searchFor">搜寻的目标表达式。</param>
        /// <param name="replaceWithFunc">替换的表达式。</param>
        private DbExpressionReplacer(Expression searchFor, Func<Expression, Expression> replaceWithFunc)
        {
            _searchFor = searchFor;
            _replaceWithFunc = replaceWithFunc;
        }

        /// <summary>
        /// 在表达式中搜寻，如果找到表达式，则进行替换。
        /// </summary>
        /// <param name="expression">要搜寻的表达式。</param>
        /// <param name="searchFor">搜寻的目标表达式。</param>
        /// <param name="replaceWith">替换的表达式。</param>
        /// <returns></returns>
        public static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
        {
            return new DbExpressionReplacer(searchFor, replaceWith).Visit(expression);
        }

        public static Expression Replace(Expression expression, Expression searchFor, Func<Expression, Expression> replaceWith)
        {
            return new DbExpressionReplacer(searchFor, replaceWith).Visit(expression);
        }

        /// <summary>
        /// 替换所有已经找到的表达式。
        /// </summary>
        /// <param name="expression">要搜寻的表达式。</param>
        /// <param name="searchFor">搜寻的目标表达式。</param>
        /// <param name="replaceWith">替换的表达式。</param>
        /// <returns></returns>
        public static Expression ReplaceAll(Expression expression, Expression[] searchFor, Expression[] replaceWith)
        {
            for (int i = 0, n = searchFor.Length; i < n; i++)
            {
                expression = Replace(expression, searchFor[i], replaceWith[i]);
            }

            return expression;
        }

        /// <summary>
        /// 替换所有已经找到的表达式。
        /// </summary>
        /// <param name="expression">要搜寻的表达式。</param>
        /// <param name="searchFor">搜寻的目标表达式。</param>
        /// <param name="replaceWith">替换的表达式。</param>
        /// <returns></returns>
        public static Expression ReplaceAll(Expression expression, IList<Expression> searchFor, IList<Expression> replaceWith)
        {
            for (int i = 0, n = searchFor.Count; i < n; i++)
            {
                expression = Replace(expression, searchFor[i], replaceWith[i]);
            }

            return expression;
        }

        /// <summary>
        /// 访问 <see cref="Expression"/>。
        /// </summary>
        /// <param name="exp">要访问的表达式。</param>
        /// <returns></returns>
        public override Expression Visit(Expression exp)
        {
            if (exp == _searchFor)
            {
                return _replaceWithFunc == null ? _replaceWith : _replaceWithFunc(exp);
            }

            return base.Visit(exp);
        }
    }
}
