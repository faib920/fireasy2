// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Reflection
{
    /// <summary>
    /// 包装 <see cref="PropertyInfo"/> 对象，创建一个委托来提升属性的读写。
    /// </summary>
    public class PropertyAccessor
    {
        private Func<object, object> getter;
        private MethodInvoker setMethodInvoker;

        /// <summary>
        /// 获取要包装的 <see cref="PropertyInfo"/> 对象。
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// 初始化 <see cref="PropertyAccessor"/> 类的新实例。
        /// </summary>
        /// <param name="propertyInfo">要包装的 <see cref="PropertyInfo"/> 对象。</param>
        public PropertyAccessor(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            InitializeGet(propertyInfo);
            InitializeSet(propertyInfo);
        }

        private void InitializeGet(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return;
            }

            var instance = Expression.Parameter(typeof(object), "s");

            var instanceCast = propertyInfo.GetGetMethod(true).IsStatic ? null :
                Expression.Convert(instance, propertyInfo.ReflectedType);

            var propertyAccess = Expression.Property(instanceCast, propertyInfo);
            var castPropertyValue = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castPropertyValue, instance);

            getter = lambda.Compile();
        }

        private void InitializeSet(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return;
            }

            setMethodInvoker = new MethodInvoker(propertyInfo.GetSetMethod(true));
        }

        /// <summary>
        /// 获取给定对象的属性的值。
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            if (getter == null)
            {
                throw new NotSupportedException("Get method is not defined for this property.");
            }

            return getter(instance);
        }

        /// <summary>
        /// 设置给定对象的属性的值
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <param name="value">属性的值。</param>
        public void SetValue(object instance, object value)
        {
            if (setMethodInvoker == null)
            {
                throw new NotSupportedException("Set method is not defined for this property.");
            }

            setMethodInvoker.Invoke(instance, new object[] { value });
        }
    }
}
