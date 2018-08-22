// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using System;

namespace Fireasy.Web.EasyUI
{
    /// <summary>
    /// datebox 的参数选项。
    /// </summary>
    public class DateBoxSettings : ComboSettings
    {

        /// <summary>
        /// 获取或设置用户选择了一个日期的时候触发的函数。
        /// </summary>
        [EventFunction]
        public string OnSelect { get; set; }

        /// <summary>
        /// 获取或设置日期格式的值。
        /// </summary>
        public virtual DateTime? DateValue
        {
            get
            {
                if (string.IsNullOrEmpty(Value))
                {
                    return null;
                }

                return Value.To<DateTime?>();
            }
            set
            {
                if (value != null)
                {
                    Value = value.Value.ToString("yyyy-MM-dd");
                }
            }
        }
    }
}
