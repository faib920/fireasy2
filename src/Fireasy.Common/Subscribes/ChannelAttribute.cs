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
    /// 用于标识主题类型的通道名称。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ChannelAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ChannelAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">通道名称。</param>
        public ChannelAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// 获取或设置通道名称。
        /// </summary>
        public string Name { get; set; }
    }

    public class ChannelHelper
    {
        /// <summary>
        /// 获取主题类型的通道名称。
        /// </summary>
        /// <param name="subjectType">主题类型。</param>
        /// <returns></returns>
        public static string GetChannelName(Type subjectType)
        {
            var attr = subjectType.GetCustomAttributes<ChannelAttribute>().FirstOrDefault();
            return attr == null ? subjectType.FullName : attr.Name;
        }
    }
}
