// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Web.EasyUI.Binders;
using System;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// ValidateBox 的参数选项。
    /// </summary>
    public class ValidateBoxSettings : SettingsBase, ISettingsBindable
    {
        /// <summary>
        /// 获取或设置是否为必填项。
        /// </summary>
        public bool? Required { get; set; }

        /// <summary>
        /// 获取或设置验证器的数组。
        /// </summary>
        public string[] ValidType { set; get; }

        /// <summary>
        /// 获取或设置为空时的提示信息。
        /// </summary>
        public string MissingMessage { get; set; }

        /// <summary>
        /// 获取或设置验证无效时的提示信息。
        /// </summary>
        public string InvalidMessage { get; set; }

        /// <summary>
        /// 获取或设置是否关闭验证。
        /// </summary>
        public bool? Novalidate { get; set; }

        /// <summary>
        /// 根据模型类型设置参数选项。
        /// </summary>
        /// <param name="modelType">模型类型。</param>
        /// <param name="propertyName">绑定的属性。</param>
        public virtual void Bind(Type modelType, string propertyName)
        {
            SettingsBindManager.Bind(modelType, propertyName, this);
        }
    }
}
