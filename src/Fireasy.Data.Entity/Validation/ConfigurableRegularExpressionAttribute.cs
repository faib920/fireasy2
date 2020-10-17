// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 一个抽象类，继承此类的验证类可将正则表达式模式放在 validation-regulars.xml 或 validation-regulars.json 里进行配置。
    /// </summary>
    public abstract class ConfigurableRegularExpressionAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
    {
        /// <summary>
        /// 初始化 <see cref="ConfigurableRegularExpressionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="key">存放正则表达式的键名。</param>
        /// <param name="defaultPattern">缺省的正则表达式。</param>
        public ConfigurableRegularExpressionAttribute(string key, string defaultPattern)
            : base(GetPattern(key, defaultPattern))
        {
        }

        /// <summary>
        /// 从配置文件里获取正则表达式。如果读取失败，返回默认的正则表达式。
        /// </summary>
        /// <param name="key">存放正则表达式的键名。</param>
        /// <param name="defaultPattern">缺省的正则表达式。</param>
        /// <returns></returns>
        private static string GetPattern(string key, string defaultPattern)
        {
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "validation-regulars.xml");
            if (File.Exists(fileName))
            {
                return ReadFromXml(fileName, key, defaultPattern);
            }

            fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "validation-regulars.json");
            if (File.Exists(fileName))
            {
                return ReadFromJson(fileName, key, defaultPattern);
            }

            return defaultPattern;
        }

        /// <summary>
        /// 从 Xml 文件里读正则表达式。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="key"></param>
        /// <param name="defaultPattern"></param>
        /// <returns></returns>
        private static string ReadFromXml(string fileName, string key, string defaultPattern)
        {
            var doc = new XmlDocument();
            doc.Load(fileName);

            var node = doc.SelectSingleNode("//config/patterns/pattern[@key='" + key + "']");
            if (node == null)
            {
                return defaultPattern;
            }

            var pattern = node.InnerText;
            if (!string.IsNullOrEmpty(pattern))
            {
                return pattern;
            }

            return defaultPattern;
        }

        /// <summary>
        /// 从 Json 文件里读正则表达式。
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="key"></param>
        /// <param name="defaultPattern"></param>
        /// <returns></returns>
        private static string ReadFromJson(string fileName, string key, string defaultPattern)
        {
            var content = File.ReadAllText(fileName);
            var dict = new JsonSerializer().Deserialize<Dictionary<string, string>>(content);
            if (dict.TryGetValue(key, out string pattern))
            {
                return pattern;
            }

            return defaultPattern;
        }
    }
}
