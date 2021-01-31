// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Emit;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Fireasy.Common.Reflection
{
    /// <summary>
    /// 属性的读写器。
    /// </summary>
    public interface IPropertyAccessor
    {
        /// <summary>
        /// 获取给定对象的属性的值。
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        object GetValue(object instance);

        /// <summary>
        /// 设置给定对象的属性的值。
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="value"></param>
        void SetValue(object instance, object value);
    }

    /// <summary>
    /// 包装 <see cref="PropertyInfo"/> 对象，创建一个委托来提升属性的读写。
    /// </summary>
    public class PropertyAccessor : IPropertyAccessor
    {
        private readonly Func<object, object> _getter;
        private readonly Action<object, object> _setter;

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
            _getter = CreateGetterDelegate(propertyInfo);
            _setter = CreateSetterDelegate(propertyInfo);
        }

        private Func<object, object> CreateGetterDelegate(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return null;
            }

            var getMethod = propertyInfo.GetGetMethod(true);

            var dm = new DynamicMethod("PropertyGetter", typeof(object),
                new Type[] { typeof(object) }, propertyInfo.DeclaringType, true);

            var emiter = new EmitHelper(dm.GetILGenerator());
            emiter.Assert(!getMethod.IsStatic, e => e.ldarg_0.end())
                .Assert(!getMethod.IsStatic && !propertyInfo.DeclaringType.IsValueType,
                    e => e.callvirt(getMethod), e => e.call(getMethod))
                .Assert(propertyInfo.PropertyType.IsValueType,
                    e => e.box(propertyInfo.PropertyType))
                .ret();

            return (Func<object, object>)dm.CreateDelegate(typeof(Func<object, object>));
        }

        private Action<object, object> CreateSetterDelegate(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            var setMethod = propertyInfo.GetSetMethod(true);
            var dm = new DynamicMethod("PropertySetter", null,
                new Type[] { typeof(object), typeof(object) }, propertyInfo.DeclaringType, true);

            var emiter = new EmitHelper(dm.GetILGenerator());
            emiter.Assert(!setMethod.IsStatic, e => e.ldarg_0.end())
                .ldarg_1
                .Assert(propertyInfo.PropertyType.IsValueType,
                    e => e.unbox_any(propertyInfo.PropertyType), e => e.castclass(propertyInfo.PropertyType))
                .Assert(!setMethod.IsStatic && !propertyInfo.DeclaringType.IsValueType,
                    e => e.callvirt(setMethod), e => e.call(setMethod))
                .ret();

            return (Action<object, object>)dm.CreateDelegate(typeof(Action<object, object>));
        }

        /// <summary>
        /// 获取给定对象的属性的值。
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            if (_getter == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate, PropertyInfo.Name));
            }

            return _getter(instance);
        }

        /// <summary>
        /// 设置给定对象的属性的值。
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <param name="value">属性的值。</param>
        public void SetValue(object instance, object value)
        {
            if (_setter == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate, PropertyInfo.Name));
            }

            _setter(instance, value);
        }
    }

    /// <summary>
    /// 包装 <see cref="PropertyInfo"/> 对象，创建一个委托来提升属性的读写。
    /// </summary>
    public class PropertyAccessor<T> : IPropertyAccessor
    {
        private readonly Func<object, T> _getter;
        private readonly Action<object, T> _setter;

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
            _getter = CreateGetterDelegate(propertyInfo);
            _setter = CreateSetterDelegate(propertyInfo);
        }

        private Func<object, T> CreateGetterDelegate(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return null;
            }

            var getMethod = propertyInfo.GetGetMethod(true);

            var dm = new DynamicMethod("PropertyGetter", typeof(T),
                new Type[] { typeof(object) }, propertyInfo.DeclaringType, true);

            var emiter = new EmitHelper(dm.GetILGenerator());
            emiter.Assert(!getMethod.IsStatic, e => e.ldarg_0.end())
                .Assert(!getMethod.IsStatic && !propertyInfo.DeclaringType.IsValueType,
                    e => e.callvirt(getMethod), e => e.call(getMethod))
                .ret();

            return (Func<object, T>)dm.CreateDelegate(typeof(Func<object, T>));
        }

        private Action<object, T> CreateSetterDelegate(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            var setMethod = propertyInfo.GetSetMethod(true);
            var dm = new DynamicMethod("PropertySetter", null,
                new Type[] { typeof(object), typeof(T) }, propertyInfo.DeclaringType, true);

            var emiter = new EmitHelper(dm.GetILGenerator());
            emiter.Assert(!setMethod.IsStatic, e => e.ldarg_0.end())
                .ldarg_1
                .Assert(!setMethod.IsStatic && !propertyInfo.DeclaringType.IsValueType,
                    e => e.callvirt(setMethod), e => e.call(setMethod))
                .ret();

            return (Action<object, T>)dm.CreateDelegate(typeof(Action<object, T>));
        }

        public T GetValue(object instance)
        {
            if (_getter == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate));
            }

            return _getter(instance);
        }

        public void SetValue(object instance, T value)
        {
            if (_setter == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate));
            }

            _setter(instance, value);
        }

        object IPropertyAccessor.GetValue(object instance)
        {
            return GetValue(instance);
        }

        void IPropertyAccessor.SetValue(object instance, object value)
        {
            SetValue(instance, (T)value);
        }
    }
}
