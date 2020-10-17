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
using System.Reflection;

namespace Fireasy.Common.Mapper
{
    /// <summary>
    /// 源与目标类型的转换器
    /// </summary>
    public class ConvertMapper
    {
        private readonly Dictionary<MemberInfo, Expression> _expressions = new Dictionary<MemberInfo, Expression>();

        /// <summary>
        /// 获取或设置来源类型。
        /// </summary>
        protected Type SourceType { get; set; }

        /// <summary>
        /// 获取或设置目标类型。
        /// </summary>
        protected Type TargetType { get; set; }

        /// <summary>
        /// 获取成员 <paramref name="member"/> 的映射表达式。
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public Expression GetMapExpression(MemberInfo member)
        {
            _expressions.TryGetValue(member, out Expression exp);
            return exp;
        }

        /// <summary>
        /// 添加映射。
        /// </summary>
        /// <param name="member"></param>
        /// <param name="expression"></param>
        protected void Map(MemberInfo member, Expression expression)
        {
            _expressions.Add(member, expression);
        }
    }

    /// <summary>
    /// 源与目标类型的转换器
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TTarget"></typeparam>
    public class ConvertMapper<TSource, TTarget> : ConvertMapper
    {
        /// <summary>
        /// 初始化 <see cref="ConvertMapper"/> 类的新实例。
        /// </summary>
        public ConvertMapper()
        {
            SourceType = typeof(TSource);
            TargetType = typeof(TTarget);
        }

        /// <summary>
        /// 添加转换映射。
        /// </summary>
        /// <typeparam name="TMember"></typeparam>
        /// <param name="target">目标表达式，为 <see cref="MemberExpression"/>。</param>
        /// <param name="source">来源表达式。</param>
        /// <returns></returns>
        public ConvertMapper<TSource, TTarget> Map<TMember>(Expression<Func<TTarget, TMember>> target, Expression<Func<TSource, TMember>> source)
        {
            if (target.Body is MemberExpression memberExp)
            {
                Map(memberExp.Member, source);
            }

            return this;
        }
    }
}
