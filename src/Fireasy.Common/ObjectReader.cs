using System;
// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common
{
    /// <summary>
    /// 定义对象读取器。
    /// </summary>
    public interface IObjectReader
    {
        /// <summary>
        /// 获取可以读取值的属性名称序列。
        /// </summary>
        /// <returns>所有可以读取值的属性名称序列。</returns>
        IEnumerable<string> GetCanReadProperties();

        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="instance">要读取的实例。</param>
        /// <param name="propertyName">要读取的属性的名称。</param>
        /// <returns>指定属性的值。</returns>
        object ReadValue(object instance, string propertyName);
    }

    /// <summary>
    /// 表示使用 <see cref="TypeDescriptor"/> 来读取属性列表。
    /// </summary>
    public interface IObjectDescriptorReader
    {
        /// <summary>
        /// 获取可以读取值的属性名称序列。
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <returns>所有可以读取值的属性名称序列。</returns>
        IEnumerable<string> GetCanReadProperties(object instance);
    }

    /// <summary>
    /// 提供对指定类型的实例进行值读取的方法。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectReader<T> : IObjectReader
    {
        private IEnumerable<PropertyInfo> properties;

        /// <summary>
        /// 读取实例的所有值。
        /// </summary>
        /// <param name="instance">要读取的实例。</param>
        /// <returns>所有属性的值。</returns>
        public virtual IEnumerable<object> ReadValues(T instance)
        {
            return GetPropertyInfos().Select(property => property.GetValue(instance, null));
        }

        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="instance">要读取的实例。</param>
        /// <param name="propertyName">要读取的属性的名称。</param>
        /// <returns>指定属性的值。</returns>
        public virtual object ReadValue(T instance, string propertyName)
        {
            var property = GetPropertyInfos().FirstOrDefault(s => s.Name == propertyName);
            if (property != null)
            {
                return property.GetValue(instance, null);
            }

            return null;
        }

        /// <summary>
        /// 获取可以读取值的属性名称序列。
        /// </summary>
        /// <returns>所有可以读取值的属性名称序列。</returns>
        public virtual IEnumerable<string> GetCanReadProperties()
        {
            return GetPropertyInfos().Select(s => s.Name);
        }

        private IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            return properties ?? (properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(s => s.CanRead));
        }


        object IObjectReader.ReadValue(object instance, string propertyName)
        {
            return ReadValue((T)instance, propertyName);
        }
    }

    /// <summary>
    /// 使用 <see cref="TypeDescriptor"/> 来进行对象的读取。
    /// </summary>
    public class ObjectDescriptorReader : ObjectReader<object>
    {
        private List<PropertyDescriptor> properties = null;

        /// <summary>
        /// 读取实例的所有值。
        /// </summary>
        /// <param name="instance">要读取的实例。</param>
        /// <returns>所有属性的值。</returns>
        public override IEnumerable<object> ReadValues(object instance)
        {
            return GetProperties(instance).Select(property => property.GetValue(instance));
        }

        /// <summary>
        /// 获取指定属性的值。
        /// </summary>
        /// <param name="instance">要读取的实例。</param>
        /// <param name="propertyName">要读取的属性的名称。</param>
        /// <returns>指定属性的值。</returns>
        public override object ReadValue(object instance, string propertyName)
        {
            var property = GetProperties(instance).FirstOrDefault(s => s.Name == propertyName);
            if (property != null)
            {
                return property.GetValue(instance);
            }

            return null;
        }

        /// <summary>
        /// 获取可以读取值的属性名称序列。
        /// </summary>
        /// <returns>所有可以读取值的属性名称序列。</returns>
        public virtual IEnumerable<string> GetCanReadProperties(object instance)
        {
            return GetProperties(instance).Select(s => s.Name);
        }

        /// <summary>
        /// 该方法不受支持。
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetCanReadProperties()
        {
            throw new NotImplementedException();
        }

        private List<PropertyDescriptor> GetProperties(object instance)
        {
            if (properties == null)
            {
                properties = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(instance))
                {
                    properties.Add(pd);
                }
            }

            return properties;
        }
    }
}
