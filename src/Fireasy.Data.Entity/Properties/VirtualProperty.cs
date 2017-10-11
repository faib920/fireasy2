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
    /// 一个虚构的属性，不参与持久化。
    /// </summary>
    public sealed class VirtualProperty : IProperty
    {
        /// <summary>
        /// 初始化 <see cref="VirtualProperty"/> 类的新实例。
        /// </summary>
        /// <param name="propertyName">属性的名称。</param>
        /// <param name="propertyType">属性的类型。</param>
        public VirtualProperty(string propertyName, Type propertyType)
        {
            Name = propertyName;
            Type = propertyType;
        }

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
            return Info == null ? Name : Info.FieldName ?? Name;
        }
    }
}
