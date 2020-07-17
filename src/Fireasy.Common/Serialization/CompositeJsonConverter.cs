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
using System.Linq.Expressions;
using System.Reflection;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 组合的 Json 转换器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CompositeJsonConverter<T> : JsonConverter<T>
    {
        private readonly Dictionary<PropertyInfo, ITextConverter> _converters = new Dictionary<PropertyInfo, ITextConverter>();

        /// <summary>
        /// 不支持反序列化。
        /// </summary>
        public override bool CanRead => false;

        /// <summary>
        /// 为匹配的成员表达式添加转换器。
        /// </summary>
        public CompositeJsonConverter<T> AddConverter<V>(Expression<Func<T, V>> expression, ITextConverter converter)
        {
            if (expression is LambdaExpression lambda)
            {
                if (lambda.Body is MemberExpression mbrExp && mbrExp.Member.MemberType == MemberTypes.Property)
                {
                    _converters.Add(mbrExp.Member as PropertyInfo, converter);
                }
            }

            return this;
        }

        /// <summary>
        /// 将一个对象转换为 Json 文本。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="writer"><see cref="JsonWriter"/>对象。</param>
        /// <param name="obj">要序列化对象。</param>
        public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
        {
            var lazyMgr = obj.As<ILazyManager>();
            var flag = new AssertFlag();
            var type = obj.GetType();

            writer.WriteStartObject();
            var context = SerializeContext.Current;
            var option = context.Option as JsonSerializeOption;

            foreach (var acc in context.GetProperties(type, () => option.ContractResolver.GetProperties(type)))
            {
                if (acc.Filter(acc.PropertyInfo, lazyMgr))
                {
                    continue;
                }

                var value = acc.Getter.Invoke(obj);
                if (option.NullValueHandling == NullValueHandling.Ignore && value == null)
                {
                    continue;
                }

                if (!flag.AssertTrue())
                {
                    writer.WriteComma();
                }

                writer.WriteKey(SerializeName(acc.PropertyName, option));
                if (_converters.TryGetValue(acc.PropertyInfo, out ITextConverter converter))
                {
                    writer.WriteRaw(converter.WriteObject(serializer, value));
                }
                else
                {
                    writer.WriteRaw(serializer.Serialize(value));
                }
            }

            writer.WriteEndObject();
        }

        private string SerializeName(string name, JsonSerializeOption option)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return string.Concat(JsonTokens.StringDelimiter, name, JsonTokens.StringDelimiter);
        }
    }
}
