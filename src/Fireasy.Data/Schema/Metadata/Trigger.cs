
using System;
namespace Fireasy.Data.Schema
{
    /// <summary>
    /// 触发器信息。
    /// </summary>
    public class Trigger : ISchemaMetadata
    {
        /// <summary>
        /// 获取分录名称。
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// 获取架构名称。
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 获取表名称。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取目标表名称。
        /// </summary>
        public string ObjectTable { get; set; }

        /// <summary>
        /// 获取事件处理。
        /// </summary>
        public TriggerManipulation Manipulation { get; set; }

        /// <summary>
        /// 获取动作时间。
        /// </summary>
        public TriggerTiming Timing { get; set; }
    }

    [Flags]
    public enum TriggerManipulation
    {
        Update = 1,
        Insert = 2,
        Delete = 4
    }

    public enum TriggerTiming
    {
        Before,
        After
    }
}
