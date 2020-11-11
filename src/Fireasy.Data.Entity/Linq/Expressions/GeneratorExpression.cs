// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Linq.Expressions;
namespace Fireasy.Data.Entity.Linq.Expressions
{
    /// <summary>
    /// 表示属性使用 <see cref="IGeneratorProvider"/> 作为生成的表达式。
    /// </summary>
    public class GeneratorExpression : DbExpression
    {
        public GeneratorExpression(TableExpression table, Expression entity, IProperty property)
            : base(DbExpressionType.Generator, typeof(int))
        {
            Table = table;
            Entity = entity;
            RelationProperty = property;
        }

        /// <summary>
        /// 获取关联的 <see cref="TableExpression"/> 对象。
        /// </summary>
        public TableExpression Table { get; }

        /// <summary>
        /// 获取相关的实体表达式。
        /// </summary>
        public Expression Entity { get; }

        /// <summary>
        /// 获取相关的属性。
        /// </summary>
        public IProperty RelationProperty { get; set; }

        public override string ToString()
        {
            return $"Generate({Entity}.{RelationProperty.Name})";
        }
    }
}
