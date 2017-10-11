// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Ioc;
using System;

namespace Fireasy.Common.Serialization
{
    /// <summary>
    /// 从 <see cref="Container"/> 里反转出注册的类型进行 Json 反序列化。
    /// </summary>
    public class ContainerJsonConverter : JsonConverter
    {
        private Container container;

        /// <summary>
        /// 初始化 <see cref="ContainerJsonConverter"/> 类的新实例。
        /// </summary>
        /// <param name="container"></param>
        public ContainerJsonConverter(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// 不支付序列化。
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// 从 <see cref="Container"/> 里判断是否注册指定的类型。
        /// </summary>
        /// <param name="type">要判断的类型。</param>
        /// <returns>可以转换则为 true。</returns>
        public override bool CanConvert(Type type)
        {
            return container.IsRegistered(type);
        }

        /// <summary>
        /// 从 Json 中读取一个可由 <see cref="Container"/> 反转的对象。
        /// </summary>
        /// <param name="serializer">一个 <see cref="JsonSerializer"/> 对象。</param>
        /// <param name="dataType">要读取的对象的类型。</param>
        /// <param name="json">表示对象的 Json 文本。</param>
        /// <returns></returns>
        public override object ReadJson(JsonSerializer serializer, Type dataType, string json)
        {
            var registration = container.GetRegistration(dataType);
            if (registration == null)
            {
                return null;
            }

            var obj = registration.Resolve();
            if (obj == null)
            {
                return null;
            }

            return serializer.Deserialize(json, obj.GetType());
        }

    }
}
