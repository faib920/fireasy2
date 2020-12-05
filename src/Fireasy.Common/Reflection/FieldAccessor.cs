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
    /// 字段的读写器。
    /// </summary>
    public interface IFieldAccessor
    {
        /// <summary>
        /// 获取给定对象的属性的值。
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        object GetValue(object instance);
    }

    /// <summary>
    /// 包装 <see cref="FieldInfo"/> 对象，创建一个委托来提升字段的读写。
    /// </summary>
    public class FieldAccessor : IFieldAccessor
    {
        private readonly Func<object, object> _getter;

        /// <summary>
        /// 获取要包装的 <see cref="FieldInfo"/> 对象。
        /// </summary>
        public FieldInfo FieldInfo { get; private set; }

        /// <summary>
        /// 初始化 <see cref="FieldAccessor"/> 类的新实例。
        /// </summary>
        /// <param name="fieldInfo">要包装的 <see cref="FieldInfo"/> 对象。</param>
        public FieldAccessor(FieldInfo fieldInfo)
        {
            FieldInfo = fieldInfo;
            _getter = GetDelegate(fieldInfo);
        }

        private Func<object, object> GetDelegate(FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(object), "s");

            var instanceCast = fieldInfo.IsStatic ? null :
                Expression.Convert(instance, fieldInfo.ReflectedType);

            var fieldAccess = Expression.Field(instanceCast, fieldInfo);
            var castFieldValue = Expression.Convert(fieldAccess, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castFieldValue, instance);

            return lambda.Compile();
        }

        /// <summary>
        /// 获取给定对象的字段的值。
        /// </summary>
        /// <param name="instance">实例对象。</param>
        /// <returns></returns>
        public object GetValue(object instance)
        {
            if (_getter == null)
            {
                throw new NotSupportedException(SR.GetString(SRKind.UnableCreateCachedDelegate, FieldInfo.Name));
            }

            return _getter(instance);
        }
    }
}
