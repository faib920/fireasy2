// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace Fireasy.Common.ComponentModel
{
    /// <summary>
    /// 为 <see cref="ExpandoObject"/> 类型提供信息补充。无法继承此类。
    /// </summary>
    public sealed class DynamicTypeDescriptor : ICustomTypeDescriptor
    {
        private readonly IDynamicMetaObjectProvider instance;

        /// <summary>
        /// 初始化 <see cref="DynamicTypeDescriptor"/> 类的新实例。
        /// </summary>
        /// <param name="instance"></param>
        public DynamicTypeDescriptor(IDynamicMetaObjectProvider instance)
        {
            this.instance = instance;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return instance;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(
                ((IDictionary<string, object>)instance).Keys
                .Select(x => new DynamicPropertyDescriptor(instance, x))
                .ToArray());
        }

        private class DynamicPropertyDescriptor : PropertyDescriptor
        {
            private readonly IDynamicMetaObjectProvider instance;
            private readonly string name;

            public DynamicPropertyDescriptor(IDynamicMetaObjectProvider instance, string name)
                : base(name, null)
            {
                this.instance = instance;
                this.name = name;
            }

            public override Type PropertyType
            {
                get 
                {
                    object value;
                    instance.TryGetMember(name, out value);
                    return value == null ? null : value.GetType();
                }
            }

            public override void SetValue(object component, object value)
            {
                instance.TrySetMember(name, value);
            }

            public override object GetValue(object component)
            {
                object value;
                instance.TryGetMember(name, out value);
                return value;
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public override Type ComponentType
            {
                get { return null; }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override string Category
            {
                get { return string.Empty; }
            }

            public override string Description
            {
                get { return string.Empty; }
            }
        }
    }
}
