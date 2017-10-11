// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Fireasy.Common.Extensions;

namespace Fireasy.Web.EasyUI
{
    public class TimeSpinnerSettings : TextBoxSettings
    {
        /// <summary>
        /// 获取或设置是否显示秒钟信息。
        /// </summary>
        public bool? ShowSeconds { get; set; }


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
                    Value = value.Value.ToString(ShowSeconds == true ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd HH:mm");
                }
            }
        }

    }
}
