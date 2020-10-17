// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity.Properties
{
    /// <summary>
    /// 一般的属性。
    /// </summary>
    [Serializable]
    public class GeneralProperty : IProperty, ISavedProperty, ILoadedProperty
    {
        /// <summary>
        /// 获取或设置属性的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置属性的类型。
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 获取或设置实体类型。
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 获取或设置属性的映射信息。
        /// </summary>
        public PropertyMapInfo Info { get; set; }

        /// <summary>
        /// 输出列名称。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Info == null ? Name : Info.ColumnName ?? Name;
        }
    }
}
