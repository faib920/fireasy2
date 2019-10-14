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

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// 枚举相关的扩展方法。
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// 根据枚举值获取对应的枚举项对应的显示文本，<br/>
        /// 枚举需要使用 <see cref="EnumDescriptionAttribute"/> 进行描述。
        /// </summary>
        /// <param name="value">枚举项。</param>
        /// <returns>枚举项对应的显示文本。</returns>
        public static string GetDescription(this Enum value)
        {
            var type = value.GetType();
            var fi = type.GetField(value.ToString());
            if (fi == null)
            {
                return string.Empty;
            }

            var attr = fi.GetCustomAttributes<EnumDescriptionAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Description;
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取枚举类型所有的显示文本－值对并填充到IList接口对象中，<br/>
        /// 枚举需要使用 <see cref="EnumDescriptionAttribute"/> 进行描述。
        /// </summary>
        /// <param name="enumType">枚举类型。</param>
        /// <param name="flags">标志位，使用 <see cref="EnumDescriptionAttribute"/> 标识。</param>
        /// <returns>显示文本——值对，可以把返回的内容直接绑定到控件上。</returns>
        public static IDictionary<int, string> GetEnumList(this Type enumType, int flags = 0)
        {
            var dictionary = new SortedDictionary<int, string>();
            foreach (var fieldInfo in enumType.GetFields())
            {
                if (fieldInfo.Name == "value__")
                {
                    continue;
                }

                var attr = fieldInfo.GetCustomAttributes<EnumDescriptionAttribute>().FirstOrDefault();
                if (attr != null)
                {
                    if (flags != 0 && (attr.Flags & flags) != flags)
                    {
                        continue;
                    }

                    dictionary.Add(Enum.Parse(enumType, fieldInfo.Name, true).To<int>(), attr != null ? attr.Description : fieldInfo.Name);
                }
            }

            return dictionary;
        }
    }
}
