// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.Common.Subscribes.Persistance
{
    /// <summary>
    /// 存储在本地的主题数据。无法继承此类。
    /// </summary>
    [Serializable]
    public sealed class StoredSubject
    {
        /// <summary>
        /// 初始化 <see cref="StoredSubject"/> 类的新实例。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="body"></param>
        public StoredSubject(string name, byte[] body)
            : this(Guid.NewGuid().ToString(), name, body)
        {
        }

        /// <summary>
        /// 初始化 <see cref="StoredSubject"/> 类的新实例。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="body"></param>
        public StoredSubject(string key, string name, byte[] body)
        {
            Key = key;
            Name = name;
            Body = body;
            ExpiresAt = DateTime.Now.AddMinutes(30);
        }

        /// <summary>
        /// 获取或设置主题的唯一标识。
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 获取或设置主题名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置存储的数据。
        /// </summary>
        public byte[] Body { get; set; }

        /// <summary>
        /// 获取或设置发布重试次数。
        /// </summary>
        public int PublishRetries { get; set; }

        /// <summary>
        /// 获取或设置接收重试次数。
        /// </summary>
        public int AcceptRetries { get; set; }

        /// <summary>
        /// 获取或设置过期时间。
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
