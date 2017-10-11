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
    /// 定义两个实体之间的关系，通过由父实体与子实体构成一对多的关系。父实体应包含一个子实体集合属性，同时子实体应包含一个父实体的引用实体属性。无法继承此类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RelationshipAttribute : Attribute
    {
        /// <summary>
        /// 获取或设置关系名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置父实体类型。
        /// </summary>
        public Type ThisType { get; set; }

        /// <summary>
        /// 获取或设置子实体的类型。
        /// </summary>
        public Type OtherType { get; set; }

        /// <summary>
        /// 获取或设置两者间的关系表达式。
        /// </summary>
        public string KeyExpression { get; set; }

        /// <summary>
        /// 初始化 <see cref="T:Fireasy.Data.Entity.RelationshipAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">关系名称。</param>
        /// <param name="thisType">父实体类型。</param>
        /// <param name="otherType">子实体的类型。</param>
        /// <param name="keyExpression">键对表达式，如 "父键1=>子键1,父键2=>子键2"。</param>
        public RelationshipAttribute(string name, Type thisType, Type otherType, string keyExpression)
        {
            Name = name;
            ThisType = thisType;
            OtherType = otherType;
            KeyExpression = keyExpression;
        }
    }
}
