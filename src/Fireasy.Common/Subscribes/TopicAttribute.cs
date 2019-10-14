// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;
using System.Linq;

namespace Fireasy.Common.Subscribes
{
    /// <summary>
    /// 用于标识主题类型的名称。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class TopicAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="TopicAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">主题名称。</param>
        public TopicAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 获取或设置主题名称。
        /// </summary>
        public string Name { get; set; }
    }

    public class TopicHelper
    {
        /// <summary>
        /// 获取主题类型的主题名称。
        /// </summary>
        /// <param name="subjectType">主题类型。</param>
        /// <returns></returns>
        public static string GetTopicName(Type subjectType)
        {
            var attr = subjectType.GetCustomAttributes<TopicAttribute>().FirstOrDefault();
            return attr == null ? subjectType.FullName : attr.Name;
        }
    }
}
