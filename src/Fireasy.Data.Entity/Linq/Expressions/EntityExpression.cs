// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Data.Entity.Metadata;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示数据实体的表达式。
    /// </summary>
    public sealed class EntityExpression : DbExpression
    {
        /// <summary>
        /// 初始化 <see cref="EntityExpression"/> 类的新实例。
        /// </summary>
        /// <param name="meta">实体元数据对象。</param>
        /// <param name="expression">定义的 Linq 表达式。</param>
        /// <param name="isNoTracking"
        public EntityExpression(EntityMetadata meta, Expression expression)
            : base(DbExpressionType.Entity, expression.Type)
        {
            Metadata = meta;
            Expression = expression;
        }

        /// <summary>
        /// 获取实体元数据对象。
        /// </summary>
        public EntityMetadata Metadata { get; }

        /// <summary>
        /// 获取定义的 Linq 表达式。
        /// </summary>
        public Expression Expression { get; }

        /// <summary>
        /// 更新 <see cref="EntityExpression"/> 对象。
        /// </summary>
        /// <param name="expression">定义的 Linq 表达式。</param>
        /// <returns></returns>
        public EntityExpression Update(Expression expression)
        {
            return expression != Expression ? new EntityExpression(Metadata, expression) : this;
        }
    }
}
