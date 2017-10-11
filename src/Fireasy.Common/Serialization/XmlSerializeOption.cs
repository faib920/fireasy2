using System.Collections.Generic;

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
    }
}
