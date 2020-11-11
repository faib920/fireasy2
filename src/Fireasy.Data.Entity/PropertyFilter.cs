// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 实体属性过滤器抽象类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PropertyFilter<T> : IPropertyFilter
    {
        private readonly List<string> _properties = new List<string>();

        /// <summary>
        /// 获取所有附加的属性名称。
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<string> Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// 初始化一个包含策略的属性过滤器。
        /// </summary>
        /// <returns></returns>
        public static PropertyFilter<T> Inclusive()
        {
            return new InclusivedPropertyFilter();
        }

        /// <summary>
        /// 初始化一个排除策略的属性过滤器。
        /// </summary>
        /// <returns></returns>
        public static PropertyFilter<T> Exclusive()
        {
            return new ExclusivedPropertyFilter();
        }

        /// <summary>
        /// 使用一个表达式来设置要过滤的属性。
        /// </summary>
        /// <typeparam name="TAny"></typeparam>
        /// <param name="exp">一个成员表达式或匿名类型构造函数表达式。</param>
        /// <returns></returns>
        public PropertyFilter<T> With<TAny>(Expression<Func<T, TAny>> exp)
        {
            if (exp == null )
            {
                return this;
            }

            if (exp.Body is MemberExpression mbrExp)
            {
                return With(mbrExp.Member.Name);
            }
            else if (exp.Body is NewExpression newExp)
            {
                foreach (var arg in newExp.Arguments)
                {
                    if (arg is MemberExpression mbrExp1)
                    {
                        _properties.Add(mbrExp1.Member.Name);
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// 使用一个属性名称来设置要过滤的属性。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public PropertyFilter<T> With(string propertyName)
        {
            _properties.Add(propertyName);
            return this;
        }

        /// <summary>
        /// 过滤指定的属性。
        /// </summary>
        /// <param name="property"></param>
        /// <returns>为 true 表示允许持久化该属性。</returns>
        public abstract bool Filter(IProperty property);

        private class InclusivedPropertyFilter : PropertyFilter<T>
        {
            public override bool Filter(IProperty property)
            {
                return Properties.Contains(property.Name);
            }
        }

        private class ExclusivedPropertyFilter : PropertyFilter<T>
        {
            public override bool Filter(IProperty property)
            {
                return !Properties.Contains(property.Name);
            }
        }
    }
}