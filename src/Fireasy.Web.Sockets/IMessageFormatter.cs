// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;

namespace Fireasy.Web.Sockets
{
    /// <summary>
    /// 消息格式化器。
    /// </summary>
    public interface IMessageFormatter
    {
        /// <summary>
        /// 格式化 <see cref="InvokeMessage"/> 对象为传输的文本。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string FormatMessage(InvokeMessage message);

        /// <summary>
        /// 从文本中解析出 <see cref="InvokeMessage"/> 对象。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        InvokeMessage ResolveMessage(string content);
    }

    /// <summary>
    /// 缺省的消息格式化器。
    /// </summary>
    public class MessageFormatter : IMessageFormatter
    {
        /// <summary>
        /// 格式化 <see cref="InvokeMessage"/> 对象为传输的文本。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual string FormatMessage(InvokeMessage message)
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            return serializer.Serialize(message);
        }

        /// <summary>
        /// 从文本中解析出 <see cref="InvokeMessage"/> 对象。
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual InvokeMessage ResolveMessage(string content)
        {
            var option = new JsonSerializeOption();
            option.Converters.Add(new FullDateTimeJsonConverter());
            var serializer = new JsonSerializer(option);
            return serializer.Deserialize<InvokeMessage>(content);
        }
    }
}
