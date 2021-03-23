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
        public static readonly XmlSerializeOption Default = new XmlSerializeOption();

        /// <summary>
        /// 初始化 <see cref="XmlSerializeOption"/> 类的新实例。
        /// </summary>
        public XmlSerializeOption()
        {
            CData = true;
            Declaration = true;
            StartElement = true;
            Converters.AddRange(GlobalConverters.GetConverters(typeof(XmlConverter)));
        }

        /// <summary>
        /// 初始化 <see cref="XmlSerializeOption"/> 类的新实例。
        /// </summary>
        /// <param name="option">参照的实例。</param>
        public XmlSerializeOption(XmlSerializeOption option)
        {
            Reference(option);
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
        /// 获取或设置输出节点的风格。
        /// </summary>
        public XmlNodeStyle NodeStyle { get; set; }

        /// <summary>
        /// 引用另一个选项的设置属性。
        /// </summary>
        /// <param name="other"></param>
        public override void Reference(SerializeOption other)
        {
            base.Reference(other);

            if (other is XmlSerializeOption xoption)
            {
                CData = xoption.CData;
                Declaration = xoption.Declaration;
                StartElement = xoption.StartElement;
                IgnoreNull = xoption.IgnoreNull;
                NodeStyle = xoption.NodeStyle;
            }
        }
    }

    public enum XmlNodeStyle
    {
        Element,
        Attribute
    }
}
