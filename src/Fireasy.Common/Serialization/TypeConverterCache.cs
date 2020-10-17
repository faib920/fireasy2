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
using System.Linq;

namespace Fireasy.Common.Serialization
{
    internal class TypeConverterCache<T> where T : ITextConverter
    {
        private readonly Dictionary<Type, T> _typeConverters = new Dictionary<Type, T>();

        internal T GetWritableConverter(Type type, SerializeOption option)
        {
            return _typeConverters.TryGetValue(type, () =>
            {
                TextConverterAttribute attr;
                if ((attr = type.GetCustomAttributes<TextConverterAttribute>().FirstOrDefault()) != null &&
                    typeof(JsonConverter).IsAssignableFrom(attr.ConverterType))
                {
                    return attr.ConverterType.New<T>();
                }
                else
                {
                    return (T)option.Converters.GetWritableConverter(type, new[] { typeof(T) });
                }
            });
        }

        internal T GetReadableConverter(Type type, SerializeOption option)
        {
            return _typeConverters.TryGetValue(type, () =>
            {
                TextConverterAttribute attr;
                if ((attr = type.GetCustomAttributes<TextConverterAttribute>().FirstOrDefault()) != null &&
                    typeof(JsonConverter).IsAssignableFrom(attr.ConverterType))
                {
                    return attr.ConverterType.New<T>();
                }
                else
                {
                    return (T)option.Converters.GetReadableConverter(type, new[] { typeof(T) });
                }
            });
        }
    }
}
