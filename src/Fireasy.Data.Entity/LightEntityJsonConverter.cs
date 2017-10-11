// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using Fireasy.Common.Serialization;

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
        /// <param name="serializer"></param>
        /// <param name="dataType"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public override object ReadJson(JsonSerializer serializer, Type dataType, string json)
        {
            var proxyType = EntityProxyManager.GetType(dataType);
            return serializer.Deserialize(json, proxyType);
        }
    }
}
