// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using Fireasy.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
                return 0;
            }
        }

        internal static DateTime? ParseDateTime(string text, CultureInfo culture, DateTimeZoneHandling handling)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            else if (text.Length > 0 && text.StartsWith("/Date(") && text.EndsWith(")/"))
            {
                return ParseMicrosoftDateTime(text, handling);
            }
            else if (DateTime.TryParse(text, culture, DateTimeStyles.RoundtripKind, out DateTime value))
            {
                return handling switch
                {
                    DateTimeZoneHandling.Local => ChangeToLocalTime(value),
                    DateTimeZoneHandling.Utc => ChangeToUtcTime(value),
                    DateTimeZoneHandling.Unspecified => new DateTime(value.Ticks, DateTimeKind.Unspecified),
                    _ => value,
                };
            }

            throw new SerializationException(SR.GetString(SRKind.DeserializeError, text, typeof(DateTime)));
        }

        private static DateTime ChangeToLocalTime(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Unspecified => new DateTime(value.Ticks, DateTimeKind.Local),
                DateTimeKind.Utc => value.ToLocalTime(),
                DateTimeKind.Local => value,
                _ => value,
            };
        }

        private static DateTime ChangeToUtcTime(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Unspecified => new DateTime(value.Ticks, DateTimeKind.Utc),
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => value,
            };
        }

        private static DateTime ParseMicrosoftDateTime(string value, DateTimeZoneHandling handling)
        {
            var regex = new Regex(@"Date\((|-)(\d+)(|\+|-)(|0800)\)");
            var matches = regex.Matches(value);

            var kind = string.IsNullOrEmpty(matches[0].Groups[3].Value) ? DateTimeKind.Utc : DateTimeKind.Local;
            var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var ticks = long.Parse(matches[0].Groups[1].Value + matches[0].Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            var date = new DateTime((ticks * 10000) + time.Ticks, DateTimeKind.Utc);

            if (kind == DateTimeKind.Local)
            {
                date = date.ToLocalTime();
            }

            return EnsureDateTime(date, handling);
        }

        internal static DateTime EnsureDateTime(DateTime date, DateTimeZoneHandling handling)
        {
            return handling switch
            {
                DateTimeZoneHandling.Local => ChangeToLocalTime(date),
                DateTimeZoneHandling.Utc => ChangeToUtcTime(date),
                DateTimeZoneHandling.Unspecified => new DateTime(date.Ticks, DateTimeKind.Unspecified),
                _ => date,
            };
        }
    }
}
