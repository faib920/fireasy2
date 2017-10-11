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
    /// 基于内嵌字符串资源的 <see cref="DisplayNameAttribute"/>。
    /// </summary>
    public class ResDisplayNameAttribute : DisplayNameAttribute
    {
        private readonly string name;
        private readonly string resourceName;

        /// <summary>
        /// 初始化 <see cref="ResDisplayNameAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="name">资源名称。</param>
        public ResDisplayNameAttribute(string name)
            : this ("Strings", name)
        {
            this.name = name;
        }

        /// <summary>
        /// 初始化 <see cref="ResDisplayNameAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="resourceName">内嵌资源的名称。</param>
        /// <param name="name">资源名称。</param>
        public ResDisplayNameAttribute(string resourceName, string name)
            : base (name)
        {
            this.resourceName = resourceName;
        }

        /// <summary>
        /// 获取属性、事件或不采用此特性中存储的任何参数的公共 void 方法的显示名称。
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return StringResource.Create(resourceName).GetString(name);
            }
        }
    }
}
