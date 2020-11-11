// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace Fireasy.QuartzNet
{
    public interface IQuartzScheduleOptions
    {
        /// <summary>
        /// 获取或设置 Cron 表达式。
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 获取或设置开始时间。
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 获取或设置终止时间。
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
