// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.IO;
using System.Xml;

namespace Fireasy.Data.Entity.Validation
{
    /// <summary>
    /// 一个抽象类，继承此类的验证类可将正则表达式模式放在 Validation-Regulars.Config 里进行配置。
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
            var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Validation-Regulars.Config");
            if (!File.Exists(fileName))
            {
                return defaultPattern;
            }

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
    }
}
