// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using System;
using System.Collections.Generic;

namespace Fireasy.QuartzNet
{
    [ConfigurationSettingParseType(typeof(QuartzConfigurationSettingParser))]
    public class QuartzConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取作业项。
        /// </summary>
        public List<QuartzJobSetting> Jobs { get; } = new List<QuartzJobSetting>();
    }

    public class QuartzJobSetting : IQuartzScheduleOptions
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

        /// <summary>
        /// 获取或设置任务类型。
        /// </summary>
        public Type ExecutorType { get; set; }

        /// <summary>
        /// 获取或设置参数字典。
        /// </summary>
        public IDictionary<string, object> Arguments { get; set; }
    }
}
