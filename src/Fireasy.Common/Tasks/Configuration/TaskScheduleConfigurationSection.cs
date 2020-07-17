// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using System;
using System.Xml;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Fireasy.Common.Tasks.Configuration
{
    /// <summary>
    /// 提供对任务调度管理器的配置管理。对应的配置节为 fireasy/taskSchedulers(.net framework) 或 fireasy:taskSchedulers(.net core)。
    /// </summary>
    [ConfigurationSectionStorage("fireasy/taskSchedulers")]
    public sealed class TaskScheduleConfigurationSection : ManagableConfigurationSection<TaskScheduleConfigurationSetting>
    {
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="section">对应的配置节点。</param>
        public override void Initialize(XmlNode section)
        {
            InitializeNode(
                section,
                "scheduler",
                func: node => InitializeSetting(new TaskScheduleConfigurationSetting
                {
                    Name = node.GetAttributeValue("name"),
                    SchedulerType = Type.GetType(node.GetAttributeValue("type"), false, true)
                }, node));

            //取默认实例
            DefaultInstanceName = section.GetAttributeValue("default");

            base.Initialize(section);
        }

#if NETSTANDARD
        /// <summary>
        /// 使用配置节点对当前配置进行初始化。
        /// </summary>
        /// <param name="configuration">对应的配置节点。</param>
        public override void Bind(IConfiguration configuration)
        {
            Bind(configuration,
                "settings",
                func: c => InitializeSetting(new TaskScheduleConfigurationSetting
                {
                    Name = c.Key,
                    SchedulerType = Type.GetType(c.GetSection("type").Value, false, true)
                }, c));

            //取默认实例
            DefaultInstanceName = configuration.GetSection("default").Value;

            base.Bind(configuration);
        }
#endif

        private TaskScheduleConfigurationSetting InitializeSetting(TaskScheduleConfigurationSetting setting, XmlNode node)
        {
            var root = node.SelectSingleNode("executors");
            if (root == null)
            {
                return setting;
            }

            foreach (XmlNode child in root.SelectNodes("executors"))
            {
                var executorType = child.GetAttributeValue("type");
                var delay = child.GetAttributeValue("delay").ToTimeSpan();
                var period = child.GetAttributeValue("period").ToTimeSpan(TimeSpan.FromMinutes(10));

                setting.ExecutorSettings.Add(new TaskExecutorSetting
                {
                    ExecutorType = executorType.ParseType(),
                    Delay = delay,
                    Period = period
                });
            }

            return setting;
        }

#if NETSTANDARD
        private TaskScheduleConfigurationSetting InitializeSetting(TaskScheduleConfigurationSetting setting, IConfiguration config)
        {
            foreach (var child in config.GetSection("executors").GetChildren())
            {
                var executorType = child.GetSection("type").Value;
                var delay = child.GetSection("delay").Value.ToTimeSpan();
                var period = child.GetSection("period").Value.ToTimeSpan(TimeSpan.FromMinutes(10));

                setting.ExecutorSettings.Add(new TaskExecutorSetting
                {
                    ExecutorType = executorType.ParseType(),
                    Delay = delay,
                    Period = period
                });
            }

            return setting;
        }
#endif
    }
}
