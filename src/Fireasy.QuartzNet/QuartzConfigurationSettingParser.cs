// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Configuration;
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using Fireasy.Common.Tasks;
#if NETSTANDARD
using Microsoft.Extensions.Configuration;
#endif
using System;
using System.Collections.Generic;
using System.Xml;

namespace Fireasy.QuartzNet
{
    public class QuartzConfigurationSettingParser : IConfigurationSettingParseHandler
    {
        public IConfigurationSettingItem Parse(XmlNode section)
        {
            var setting = new QuartzConfigurationSetting();
            foreach (XmlNode node in section.SelectNodes("jobs/job"))
            {
                var taskSet = new QuartzJobSetting();
                taskSet.StartTime = node.GetAttributeValue<DateTime?>("startTime");
                taskSet.EndTime = node.GetAttributeValue<DateTime?>("endTime");
                taskSet.CronExpression = node.GetAttributeValue("cron");
                taskSet.Disabled = node.GetAttributeValue<bool>("disabled");

                if (node.SelectSingleNode("arguments")?.InnerText is string strArg && !string.IsNullOrEmpty(strArg))
                {
                    taskSet.Arguments = new JsonSerializer().Deserialize<Dictionary<string, object>>(strArg);
                }

                var executorType = node.GetAttributeValue("type").ParseType();
                if (!ValidExecutorType(executorType))
                {
                    continue;
                }

                taskSet.ExecutorType = executorType;
                setting.Jobs.Add(taskSet);
            }

            return setting;
        }

#if NETSTANDARD
        public IConfigurationSettingItem Parse(IConfiguration configuration)
        {
            var setting = new QuartzConfigurationSetting();
            foreach (var child in configuration.GetSection("jobs").GetChildren())
            {
                var taskSet = new QuartzJobSetting();
                taskSet.StartTime = child.GetSection("startTime").Value.To<DateTime?>();
                taskSet.EndTime = child.GetSection("endTime").Value.To<DateTime?>();
                taskSet.CronExpression = child.GetSection("cron").Value;
                taskSet.Disabled = child.GetSection("disabled").Value.To<bool>();

                if (child.GetSection("arguments").Value is string strArg && !string.IsNullOrEmpty(strArg))
                {
                    taskSet.Arguments = new JsonSerializer().Deserialize<Dictionary<string, object>>(strArg);
                }

                var executorType = child.GetSection("type").Value.ParseType();
                if (!ValidExecutorType(executorType))
                {
                    continue;
                }

                taskSet.ExecutorType = executorType;
                setting.Jobs.Add(taskSet);
            }

            return setting;
        }
#endif

        private bool ValidExecutorType(Type type)
        {
            return type != null && (typeof(ITaskExecutor).IsAssignableFrom(type) || typeof(IAsyncTaskExecutor).IsAssignableFrom(type));
        }
    }
}
