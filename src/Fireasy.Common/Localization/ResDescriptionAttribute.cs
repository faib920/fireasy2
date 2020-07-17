// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;

namespace Fireasy.Common.Localization
{
    /// <summary>
    /// 基于内嵌字符串资源的 <see cref="DescriptionAttribute"/>。
    /// </summary>
    public class ResDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _name;
        private readonly string _resourceName;

        /// <summary>
        /// 初始化 <see cref="ResDescriptionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        public ResDescriptionAttribute(string name)
            : this("Strings", name)
        {
            _name = name;
        }

        /// <summary>
        /// 初始化 <see cref="ResDescriptionAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="resourceName">内嵌资源的名称。</param>
        /// <param name="name">资源名称。</param>
        public ResDescriptionAttribute(string resourceName, string name)
            : base(name)
        {
            _resourceName = resourceName;
        }

        /// <summary>
        /// 获取存储在此特性中的说明。
        /// </summary>
        public override string Description
        {
            get
            {
                return StringResource.Create(_resourceName).GetString(_name);
            }
        }
    }
}
