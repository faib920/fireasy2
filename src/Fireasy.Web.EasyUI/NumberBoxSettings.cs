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
    /// numberbox 的参数选项。
    /// </summary>
    public class NumberBoxSettings : TextBoxSettings
    {
        /// <summary>
        /// 获取或设置小数位数。
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// 获取或设置最小值。
        /// </summary>
        public decimal? Min { get; set; }

        /// <summary>
        /// 获取或设置最大值。
        /// </summary>
        public decimal? Max { get; set; }

        /// <summary>
        /// 获取或设置小数点分隔符。
        /// </summary>
        public string DecimalSeparator { get; set; }

        /// <summary>
        /// 获取或设置千分位分隔符。
        /// </summary>
        public string GroupSeparator { get; set; }

        /// <summary>
        /// 获取或设置值。
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 获取或设置前置符号。
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// 获取或设置后置符号。
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// 根据模型类型设置参数选项。
        /// </summary>
        /// <param name="modelType">模型类型。</param>
        /// <param name="propertyName">绑定的属性。</param>
        public override void Bind(Type modelType, string propertyName)
        {
            SettingsBindManager.Bind(modelType, propertyName, this);
        }
    }
}
