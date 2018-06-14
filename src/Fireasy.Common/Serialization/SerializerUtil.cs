using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    internal class SerializerUtil
    {
        internal static bool IsNoSerializable(SerializeOption option, PropertyDescriptor property)
        {
            return property.IsDefined<NoTextSerializableAttribute>() || IsNoSerializable(option, property.Name);
        }

        internal static bool IsNoSerializable(SerializeOption option, PropertyInfo property)
        {
            return property.IsDefined<NoTextSerializableAttribute>() ||
                (((option.InclusiveNames != null) &&
                !option.InclusiveNames.Contains(property.Name)) ||
                ((option.ExclusiveNames != null) &&
                option.ExclusiveNames.Contains(property.Name))) ||
                (((option.InclusiveMembers != null) && 
                option.InclusiveMembers.Count(s => s.DeclaringType == property.DeclaringType) != 0 &&
                !option.InclusiveMembers.Contains(property)) ||
                ((option.ExclusiveMembers != null) &&
                option.ExclusiveMembers.Contains(property)));
        }

        internal static bool IsNoSerializable(PropertyDescriptor property, object value)
        {
            var defaultValue = property.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault();
            return defaultValue == null || defaultValue.Value == null || !defaultValue.Value.ToType(property.PropertyType).Equals(value);
        }

        internal static bool IsNoSerializable(PropertyInfo property, object value)
        {
            var defaultValue = property.GetCustomAttributes<DefaultValueAttribute>().FirstOrDefault();
            return defaultValue == null || defaultValue.Value == null || !defaultValue.Value.ToType(property.PropertyType).Equals(value);
        }

        internal static bool IsNoSerializable(SerializeOption option, string propertyName)
        {
            return (((option.InclusiveNames != null) &&
                !option.InclusiveNames.Contains(propertyName)) ||
                ((option.ExclusiveNames != null) &&
                option.ExclusiveNames.Contains(propertyName)));
        }

        internal static string GetPropertyName(PropertyDescriptor property)
        {
            var attr = property.GetCustomAttributes<TextSerializeElementAttribute>().FirstOrDefault();
            return attr == null ? property.Name : attr.Name;
        }

        internal static string GetPropertyName(PropertyInfo property)
        {
            var attr = property.GetCustomAttributes<TextSerializeElementAttribute>().FirstOrDefault();
            return attr == null ? property.Name : attr.Name;
        }

        /// <summary>
        /// 检查延迟加载
        /// </summary>
        /// <param name="lazyMgr"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal static bool CheckLazyValueCreate(ILazyManager lazyMgr, string propertyName)
        {
            if (lazyMgr == null)
            {
                return true;
            }

            return lazyMgr.IsValueCreated(propertyName);
        }

        internal class PropertyEqualityComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                return x.Name.Equals(y.Name);
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}
