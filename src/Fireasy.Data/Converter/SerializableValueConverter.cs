// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using System.Data;

namespace Fireasy.Data.Converter
{
    /// <summary>
    /// 通过使用文本序列化的数据转换器的抽象类。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SerializableValueConverter<T> : IValueConverter
    {
        protected readonly ITextSerializer serializer;

        public SerializableValueConverter()
        {
            serializer = CreateSerializer();
        }

        protected virtual ITextSerializer CreateSerializer()
        {
            return SerializerFactory.CreateSerializer() as ITextSerializer ?? new JsonSerializer();
        }

        public virtual T ConvertFrom(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default;
            }

            return serializer.Deserialize<T>(value);
        }

        public virtual string ConvertTo(T value)
        {
            return serializer.Serialize(value);
        }

        object IValueConverter.ConvertFrom(object value, DbType dbType)
        {
            return ConvertFrom(value.ToStringSafely());
        }

        object IValueConverter.ConvertTo(object value, DbType dbType)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return ConvertTo((T)value);
        }
    }
}
