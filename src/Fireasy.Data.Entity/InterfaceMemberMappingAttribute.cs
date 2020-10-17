// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 一个标识实体属性如何与接口属性进行查询映射的特性。
    /// </summary>
    public class InterfaceMemberMappingAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="InterfaceMemberMappingAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="type">接口的类型。</param>
        /// <param name="name">接口属性名称。</param>
        public InterfaceMemberMappingAttribute(Type type, string name)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// 获取或设置接口类型。
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 获取或设置接口属性名称。
        /// </summary>
        public string Name { get; set; }
    }
}
