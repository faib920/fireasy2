// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;

namespace Fireasy.Common.Subscribes
{
    public sealed class SubscribeNotificationContext
    {
        /// <summary>
        /// 初始化 <see cref="SubscribeNotificationContext"/> 类的新实例。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="body"></param>
        /// <param name="exception"></param>
        /// <param name="serializer"></param>
        /// <param name="hasNext"></param>
        public SubscribeNotificationContext(IServiceProvider serviceProvider, string name, byte[] body, Exception exception, ISerializer serializer, bool hasNext)
        {
            ServiceProvider = serviceProvider;
            Name = name;
            Body = body;
            Exception = exception;
            Serializer = serializer;
            HasNext = hasNext;
        }

        /// <summary>
        /// 获取 <see cref="IServiceProvider"/> 实例。
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// 获取主题名称。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 获取消息内容。
        /// </summary>
        public byte[] Body { get; private set; }

        /// <summary>
        /// 获取异常信息。
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// 获取或设置是否重试。默认为 true。
        /// </summary>
        public bool CanRetry { get; set; } = true;

        /// <summary>
        /// 获取 <see cref="ISerializer"/> 实例。
        /// </summary>
        public ISerializer Serializer { get; }

        /// <summary>
        /// 获取是否还有下一次。
        /// </summary>
        public bool HasNext { get; }
    }
}
