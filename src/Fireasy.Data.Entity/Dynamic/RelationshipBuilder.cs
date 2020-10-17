// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Emit;
using System;

namespace Fireasy.Data.Entity.Dynamic
{
    /// <summary>
    /// 提供动态构造实体类之间关系的生成器。无法继承此类。
    /// </summary>
    public sealed class RelationshipBuilder
    {
        private readonly DynamicAssemblyBuilder _assemblyBuilder;

        /// <summary>
        /// 初始化 <see cref="RelationshipBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="assemblyBuilder">一个 <see cref="DynamicAssemblyBuilder"/> 容器。</param>
        public RelationshipBuilder(DynamicAssemblyBuilder assemblyBuilder = null)
        {
            _assemblyBuilder = assemblyBuilder ?? new DynamicAssemblyBuilder($"<DynamicRelationship>_{Guid.NewGuid():N}");
        }

        /// <summary>
        /// 定义两个实体类间的关系。
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <param name="keyExpression"></param>
        public void DefineRelation(Type thisType, Type otherType, string keyExpression)
        {
            var relationName = $"{thisType.Name}_{otherType.Name}:{keyExpression}";
            _assemblyBuilder.SetCustomAttribute(() => new RelationshipAttribute(relationName, thisType, otherType, keyExpression));
        }

        /// <summary>
        /// 定义两个实体类间的关系。
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <param name="thisProperty"></param>
        /// <param name="otherProperty"></param>
        /// <param name="style"></param>
        public void DefineRelation(Type thisType, Type otherType, IProperty thisProperty, IProperty otherProperty, RelationshipStyle style)
        {
            var relationName = $"{thisType.Name}:{otherType.Name}";
            _assemblyBuilder.SetCustomAttribute(() => new RelationshipAttribute(relationName, thisType, otherType, GetRelationshipExpression(thisProperty, otherProperty, style)));
        }

        private string GetRelationshipExpression(IProperty thisProperty, IProperty otherProperty, RelationshipStyle style)
        {
            return style switch
            {
                RelationshipStyle.One2One => $"{thisProperty}=={otherProperty}",
                RelationshipStyle.One2Many => $"{thisProperty}=>{otherProperty}",
                RelationshipStyle.Many2One => $"{thisProperty}<={otherProperty}",
                _ => string.Empty,
            };
        }
    }
}
