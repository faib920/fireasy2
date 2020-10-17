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
using System.Collections.ObjectModel;

namespace Fireasy.Data.Entity.Metadata
{
    /// <summary>
    /// 用于描述实体关系结构的元数据。无法继承此类。
    /// </summary>
    public sealed class RelationshipMetadata
    {
        /// <summary>
        /// 初始化 <see cref="RelationshipMetadata"/> 类的新实例。
        /// </summary>
        /// <param name="principalType">主要的实体类型。</param>
        /// <param name="dependentType">从属的实体类型。</param>
        /// <param name="style">关系类型。</param>
        /// <param name="source"></param>
        /// <param name="keys"></param>
        internal RelationshipMetadata(Type principalType, Type dependentType, RelationshipStyle style, RelationshipSource source, IEnumerable<RelationshipKey> keys)
        {
            PrincipalType = principalType;
            DependentType = dependentType;
            Style = style;
            Keys = keys.ToReadOnly();
            Source = source;
        }

        internal RelationshipSource Source { get; set; }

        /// <summary>
        /// 获取或设置作为主要实体的类型。
        /// </summary>
        public Type PrincipalType { get; }

        /// <summary>
        /// 获取或设置作为从属实体的类型。
        /// </summary>
        public Type DependentType { get; }

        /// <summary>
        /// 获取关系一组键对。
        /// </summary>
        public ReadOnlyCollection<RelationshipKey> Keys { get; }

        /// <summary>
        /// 获取关系类型。
        /// </summary>
        public RelationshipStyle Style { get; }

        /// <summary>
        /// 修复 Keys 中缺失的 <see cref="IProperty"/>。
        /// </summary>
        /// <returns></returns>
        public RelationshipMetadata Repair()
        {
            foreach (var key in Keys)
            {
                if (key.PrincipalProperty == null)
                {
                    key.PrincipalProperty = PropertyUnity.GetProperty(PrincipalType, key.PrincipalKey);
                }

                if (key.DependentProperty == null)
                {
                    key.DependentProperty = PropertyUnity.GetProperty(DependentType, key.DependentKey);
                }
            }

            return this;
        }
    }
}
