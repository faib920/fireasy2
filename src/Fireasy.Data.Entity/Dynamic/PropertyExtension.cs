// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity.Dynamic
{
    /// <summary>
    /// 对 <see cref="IProperty"/> 的扩展定义。
    /// </summary>
    public sealed class PropertyExtension : IProperty
    {
        private readonly List<Expression<Func<Attribute>>> _attrExps = new List<Expression<Func<Attribute>>>();

        /// <summary>
        /// 初始化 <see cref="PropertyExtension"/> 类的新实例。
        /// </summary>
        /// <param name="property"></param>
        public PropertyExtension(IProperty property)
        {
            Property = property;
        }

        /// <summary>
        /// 获取原本的 <see cref="IProperty"/> 对象。
        /// </summary>
        public IProperty Property { get; }

        string IProperty.Name
        {
            get { return Property.Name; }
            set { }
        }

        Type IProperty.Type
        {
            get { return Property.Type; }
            set { }
        }

        Type IProperty.EntityType
        {
            get { return Property.EntityType; }
            set { }
        }

        PropertyMapInfo IProperty.Info
        {
            get { return Property.Info; }
            set { }
        }

        /// <summary>
        /// 获取属性的特性定义表达式。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Expression<Func<Attribute>>> GetCustomAttributes()
        {
            return _attrExps;
        }

        /// <summary>
        /// 为属性指定特性定义表达式。
        /// </summary>
        /// <param name="expression">一个特性表达式，必须为 <see cref="NewExpression"/>。</param>
        public void SetCustomAttribute(Expression<Func<Attribute>> expression)
        {
            _attrExps.Add(expression);
        }
    }
}
