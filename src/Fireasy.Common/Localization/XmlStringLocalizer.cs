// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Globalization;
using System.Xml;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 使用 XML 字符串本地化。
    /// </summary>
    public class XmlStringLocalizer : IStringLocalizer
    {
        private readonly XmlDocument doc;

        public XmlStringLocalizer(XmlDocument doc, CultureInfo cultureInfo)
        {
            this.doc = doc;
            CultureInfo = cultureInfo;
        }

        public CultureInfo CultureInfo { get; private set; }

        public string this[string name]
        {
            get
            {
                var node = doc.SelectSingleNode($"//strings/{name}");
                if (node == null)
                {
                    return name;
                }

                return node.InnerText;
            }
        }

        public string this[string name, params object[] args]
        {
            get
            {
                var node = doc.SelectSingleNode($"//strings/{name}");
                if (node == null)
                {
                    return name;
                }

                return string.Format(node.InnerText, args);
            }
        }
    }
}
