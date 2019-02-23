// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Common.Serialization
{
    public class XmlSerializeOption : SerializeOption
    {
        public XmlSerializeOption()
        {
            CData = true;
            Declaration = true;
            StartElement = true;
        }

        /// <summary>
        /// 获取或设置是否使用 CData 输出。
        /// </summary>
        public bool CData { get; set; }

        /// <summary>
        /// 获取或设置是否输入 Xml 声明。
        /// </summary>
        public bool Declaration { get; set; }

        /// <summary>
        /// 获取或设置是否输出元素起始，默认为 true。
        /// </summary>
        public bool StartElement { get; set; }

        /// <summary>
        /// 获取或设置是否忽略为 null 的值。默认为 true。
        /// </summary>
        public bool IgnoreNull { get; set; } = true;

        /// <summary>
        /// 获取或设置输出的风格。
        /// </summary>
        public OutputStyle OutputStyle { get; set; }
    }

    public enum OutputStyle
    {
        Element,
        Attribute
    }
}
