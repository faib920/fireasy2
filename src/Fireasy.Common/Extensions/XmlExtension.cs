// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Xml;

namespace Fireasy.Common.Extensions
{
    /// <summary>
    /// Xml的相关扩展方法。
    /// </summary>
    public static class XmlExtension
    {
        /// <summary>
        /// 获取 <see cref="T:System.Xml.XmlAttribute"/> 的值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static T GetValue<T>(this XmlAttribute attribute)
        {
            Guard.ArgumentNull(attribute, nameof(attribute));
            return attribute.Value.To<string, T>();
        }

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetAttributeValue<T>(this XmlNode node, string name)
        {
            Guard.ArgumentNull(node, nameof(node));
            return node.Attributes[name] == null ? (T)typeof(T).GetDefaultValue() : node.Attributes[name].Value.To<string, T>();
        }

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetAttributeValue<T>(this XmlNode node, string name, T defaultValue = default(T))
        {
            Guard.ArgumentNull(node, nameof(node));
            return node.Attributes[name] == null ? defaultValue : node.Attributes[name].Value.To(defaultValue);
        }

        /// <summary>
        /// 获取属性的值。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this XmlNode node, string name)
        {
            Guard.ArgumentNull(node, nameof(node));
            return node.Attributes[name] == null ? string.Empty : node.Attributes[name].Value;
        }

        /// <summary>
        /// 循环节点的子节点列表。
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <param name="action"></param>
        public static void EachChildren(this XmlNode node, string path, Action<XmlNode> action)
        {
            if (action == null)
            {
                return;
            }

            var xmlList = string.IsNullOrEmpty(path) ? node.ChildNodes : node.SelectNodes(path);
            if (xmlList == null)
            {
                return;
            }

            foreach (XmlNode child in xmlList)
            {
                action(child);
            }
        }

        /// <summary>
        /// 将字符串转换为 CDATA。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToCDATA(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return ("<![CDATA[" + value + "]]>");
        }
    }
}
