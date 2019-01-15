// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// 轻量级实体对象的 Json 转换器。
    /// </summary>
    public class LightEntityJsonConverter : JsonConverter
    {
        /// <summary>
        /// 判断指定的类型是否允许转换。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public override bool CanConvert(Type type)
        {
            return typeof(ICompilableEntity).IsAssignableFrom(type) && !typeof(ICompiledEntity).IsAssignableFrom(type);
        }

        /// <summary>
        /// 返回不支持写。
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 将 Json 转换为 <paramref name="dataType"/> 的代理对象。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="reader"><see cref="JsonReader"/> 对象。</param>
        /// <param name="dataType">要反序列化的对象类型。</param>
        /// <returns></returns>
        public override object ReadJson(JsonSerializer serializer, JsonReader reader, Type dataType)
        {
            var proxyType = EntityProxyManager.GetType(dataType);
            var json = reader.ReadRaw();
            return serializer.Deserialize(json, proxyType);
        }
    }
}
