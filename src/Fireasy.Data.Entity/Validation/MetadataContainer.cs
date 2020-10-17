using Fireasy.Common.Extensions;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 提供元验证数据的初始化。
    /// </summary>
    public interface IMetadataContainer
    {
        /// <summary>
        /// 初始化规则字典。
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<ValidationAttribute>> InitializeRules();
    }

    /// <summary>
    /// 元数据转换器。无法继承此类。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public sealed class MetadataConverter<TEntity> where TEntity : IEntity
    {
        private readonly Dictionary<string, List<ValidationAttribute>> _attrCache = new Dictionary<string, List<ValidationAttribute>>();

        /// <summary>
        /// 使用表达式来添加一个规则。
        /// </summary>
        /// <param name="expression">使用表达式来限定添加验证的属性。</param>
        /// <param name="attribute">一个 <see cref="ValidationAttribute"/> 实例。</param>
        /// <returns></returns>
        public MetadataConverter<TEntity> Add(Expression<Func<TEntity, object>> expression, ValidationAttribute attribute)
        {
            var memberName = MemberVisitor.Find(expression);
            if (!string.IsNullOrEmpty(memberName))
            {
                var attrs = _attrCache.TryGetValue(memberName, () => new List<ValidationAttribute>());
                attrs.Add(attribute);
            }

            return this;
        }

        /// <summary>
        /// 将定义的规则输出为一个字典。
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<ValidationAttribute>> ToDictionary()
        {
            return _attrCache;
        }

        private class MemberVisitor : Fireasy.Common.Linq.Expressions.ExpressionVisitor
        {
            private string _memberName;

            public static string Find(Expression expression)
            {
                var visitor = new MemberVisitor();
                visitor.Visit(expression);
                return visitor._memberName;
            }

            public override Expression Visit(Expression expression)
            {
                if (expression is LambdaExpression labda)
                {
                    if (labda.Body is MemberExpression member && member.Member.DeclaringType == typeof(TEntity))
                    {
                        _memberName = member.Member.Name;
                    }
                }

                return expression;
            }
        }
    }
}
