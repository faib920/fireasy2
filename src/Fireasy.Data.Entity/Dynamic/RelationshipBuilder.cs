// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Fireasy.Common.Emit;

namespace Fireasy.Data.Entity.Dynamic
{
    /// <summary>
    /// 提供动态构造实体类之间关系的生成器。无法继承此类。
    /// </summary>
    public sealed class RelationshipBuilder
    {
        private readonly DynamicAssemblyBuilder assemblyBuilder;

        /// <summary>
        /// 初始化 <see cref="RelationshipBuilder"/> 类的新实例。
        /// </summary>
        /// <param name="assemblyBuilder">一个 <see cref="DynamicAssemblyBuilder"/> 容器。</param>
        public RelationshipBuilder(DynamicAssemblyBuilder assemblyBuilder = null)
        {
            this.assemblyBuilder = assemblyBuilder ?? new DynamicAssemblyBuilder("<DynamicRelationship>_" + Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// 定义两个实体类间的关系。
        /// </summary>
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <param name="keyExpression"></param>
        public void DefineRelation(Type thisType, Type otherType, string keyExpression)
        {
            var relationName = string.Format("{0}_{1}:{2}", thisType.Name, otherType.Name, keyExpression);
            assemblyBuilder.SetCustomAttribute(() => new RelationshipAttribute(relationName, thisType, otherType, keyExpression));
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
            var relationName = thisType.Name + ":" + otherType.Name;
            assemblyBuilder.SetCustomAttribute(() => new RelationshipAttribute(relationName, thisType, otherType, GetRelationshipExpression(thisProperty, otherProperty, style)));
        }

        private string GetRelationshipExpression(IProperty thisProperty, IProperty otherProperty, RelationshipStyle style)
        {
            switch (style)
            {
                case RelationshipStyle.One2One: return string.Format("{0}=={1}", thisProperty, otherProperty);
                case RelationshipStyle.One2Many: return string.Format("{0}=>{1}", thisProperty, otherProperty);
                case RelationshipStyle.Many2One: return string.Format("{0}<={1}", thisProperty, otherProperty);
                default: return string.Empty;
            }
        }
    }
}
