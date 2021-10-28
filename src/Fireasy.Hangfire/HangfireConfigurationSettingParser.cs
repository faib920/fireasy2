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

namespace Fireasy.Hangfire
{
    public class HangfireConfigurationSettingParser : IConfigurationSettingParseHandler
    {
        public IConfigurationSettingItem Parse(XmlNode section)
        {
            var setting = new HangfireConfigurationSetting();
            foreach (XmlNode node in section.SelectNodes("jobs/job"))
            {
                var taskSet = new HangfireJobSetting();
                taskSet.CronExpression = node.GetAttributeValue("cron");

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
            var setting = new HangfireConfigurationSetting();
            foreach (var child in configuration.GetSection("jobs").GetChildren())
            {
                var taskSet = new HangfireJobSetting();
                taskSet.CronExpression = child.GetSection("cron").Value;

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
