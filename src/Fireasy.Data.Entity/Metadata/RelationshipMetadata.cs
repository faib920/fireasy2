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
        /// <param name="thisType"></param>
        /// <param name="otherType"></param>
        /// <param name="style"></param>
        /// <param name="source"></param>
        /// <param name="keys"></param>
        internal RelationshipMetadata(Type thisType, Type otherType, RelationshipStyle style, RelationshipSource source, IEnumerable<RelationshipKey> keys)
        {
            ThisType = thisType;
            OtherType = otherType;
            Style = style;
            Keys = keys.ToReadOnly();
            Source = source;
        }

        internal RelationshipSource Source { get; set; }

        /// <summary>
        /// 获取或设置作为主体实体的类型。
        /// </summary>
        public Type ThisType { get; }

        /// <summary>
        /// 获取或设置作为客体实体的类型。
        /// </summary>
        public Type OtherType { get; }

        /// <summary>
        /// 获取关系一组键对。
        /// </summary>
        public ReadOnlyCollection<RelationshipKey> Keys { get; }

        /// <summary>
        /// 获取关系的定义方向。
        /// </summary>
        public RelationshipStyle Style { get; }
    }
}
