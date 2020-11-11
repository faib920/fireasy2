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

namespace Fireasy.Common.Tasks.Configuration
{
    public class TaskScheduleConfigurationSetting : IConfigurationSettingItem
    {
        /// <summary>
        /// 获取或设置配置的名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置任务调度管理器的类型。
        /// </summary>
        public Type SchedulerType { get; set; }
    }
}
