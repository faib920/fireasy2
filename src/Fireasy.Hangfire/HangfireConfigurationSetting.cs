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

namespace Fireasy.Hangfire
{
    [ConfigurationSettingParseType(typeof(HangfireConfigurationSettingParser))]
    public class HangfireConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取作业项。
        /// </summary>
        public List<HangfireJobSetting> Jobs { get; } = new List<HangfireJobSetting>();
    }

    public class HangfireJobSetting
    {
        /// <summary>
        /// 获取或设置 Cron 表达式。
        /// </summary>
        public string CronExpression { get; set; }

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
