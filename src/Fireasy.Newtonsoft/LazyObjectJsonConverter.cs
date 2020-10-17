// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.ComponentModel;
using System;
using Newton = Newtonsoft.Json;

namespace Fireasy.Newtonsoft
{
    /// <summary>
    /// 为 JSON.NET 提供 <see cref="ILazyManager"/> 类型的转换器。
    /// </summary>
    public class LazyObjectJsonConverter : Newton.JsonConverter
    {
        /// <summary>
        /// 判断对象类型是不是实现自 <see cref="ILazyManager"/> 接口。
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ILazyManager).IsAssignableFrom(objectType);
        }

        /// <summary>
        /// 不支持反序列化。
        /// </summary>
        public override bool CanRead => false;

        public override void WriteJson(Newton.JsonWriter writer, object value, Newton.JsonSerializer serializer)
        {
            var lazyMgr = value as ILazyManager;

            writer.WriteStartObject();

            var contract = (Newton.Serialization.JsonObjectContract)serializer.ContractResolver.ResolveContract(value.GetType());

            foreach (var property in contract.Properties)
            {
                if (lazyMgr.IsValueCreated(property.PropertyName))
                {
                    var pValue = property.ValueProvider.GetValue(value);
                    if (pValue == null && serializer.NullValueHandling == Newton.NullValueHandling.Ignore)
                    {
                        continue;
                    }

                    writer.WritePropertyName(property.PropertyName);
                    serializer.Serialize(writer, pValue);
                }
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(Newton.JsonReader reader, Type objectType, object existingValue, Newton.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
