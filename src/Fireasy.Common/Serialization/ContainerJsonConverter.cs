// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Ioc;
using System;
using System.Linq;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 从 <see cref="Container"/> 里反转出注册的类型进行 Json 反序列化。
    /// </summary>
    public class ContainerJsonConverter : JsonConverter
    {
        private readonly Container _container;

        /// <summary>
        /// 初始化 <see cref="ContainerJsonConverter"/> 类的新实例。
        /// </summary>
        /// <param name="container"></param>
        public ContainerJsonConverter(Container container)
        {
            _container = container;
        }

        /// <summary>
        /// 不支持序列化。
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 从 <see cref="Container"/> 里判断是否注册指定的类型。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public override bool CanConvert(Type type)
        {
            return _container.IsRegistered(type);
        }

        /// <summary>
        /// 从 Json 中读取一个可由 <see cref="Container"/> 反转的对象。
        /// </summary>
        /// <param name="serializer">当前的 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="reader"><see cref="JsonReader"/> 对象。</param>
        /// <param name="dataType">要读取的对象的类型。</param>
        /// <returns></returns>
        public override object ReadJson(JsonSerializer serializer, JsonReader reader, Type dataType)
        {
            var registration = _container.GetRegistrations(dataType).FirstOrDefault();
            if (registration == null)
            {
                return null;
            }

            var json = reader.ReadRaw();
            return serializer.Deserialize(json, registration.ImplementationType);
        }
    }
}
