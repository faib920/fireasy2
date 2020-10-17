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
    /// 基于内嵌字符串资源的 <see cref="CategoryAttribute"/>。
    /// </summary>
    public class ResCategoryAttribute : CategoryAttribute
    {
        private readonly string _resourceName;

        /// <summary>
        /// 初始化 <see cref="CategoryAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        public ResCategoryAttribute(string name)
            : this("Strings", name)
        {
        }

        /// <summary>
        /// 初始化 <see cref="CategoryAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="resourceName">内嵌资源的名称。</param>
        /// <param name="name">资源名称。</param>
        public ResCategoryAttribute(string resourceName, string name)
            : base(name)
        {
            _resourceName = resourceName;
        }

        /// <summary>
        /// 获取指定类别的本地化名称。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override string GetLocalizedString(string value)
        {
            return StringResource.Create(_resourceName).GetString(value);
        }

    }
}
