// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Linq.Expressions;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// Lambda 表达式树的转换器。
    /// </summary>
    public class ExpressionJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return typeof(Expression).IsAssignableFrom(type);
        }

        /// <summary>
        /// 将 <see cref="LambdaExpression"/> 值写为 Json 文本。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="writer"><see cref="JsonWriter"/>对象。</param>
        /// <param name="obj">要序列化的 <see cref="LambdaExpression"/> 对象。</param>
        public override void WriteJson(JsonSerializer serializer, JsonWriter writer, object obj)
        {
            var expression = obj as Expression;
            writer.WriteRaw(new ExpressionJsonWriter(serializer, expression).ToString());
        }

        /// <summary>
        /// 将 Json 转换为 <see cref="LambdaExpression"/> 的代理对象。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="reader"><see cref="JsonReader"/> 对象。</param>
        /// <param name="dataType">要反序列化的对象类型。</param>
        /// <returns></returns>
        public override object ReadJson(JsonSerializer serializer, JsonReader reader, Type dataType)
        {
            var json = reader.ReadRaw();
            return new ExpressionJsonReader(serializer, json).GetExpression();
        }
    }
}
