// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;

namespace Fireasy.Data.Entity.Initializers
{
    public class RespositoryChangedEventArgs
    {
        /// <summary>
        /// 获取或设置实体类型。
        /// </summary>
        public Type EntityType { get; set; }

        /// <summary>
        /// 获取或设置是否成功。
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// 获取或设置事件通知类型。
        /// </summary>
        public RespositoryChangeEventType EventType { get; set; }

        /// <summary>
        /// 获取或设置新增的属性。
        /// </summary>
        public ReadOnlyCollection<IProperty> AddedProperties { get; set; }

        /// <summary>
        /// 当更改失败时，返回具体的异常信息。
        /// </summary>
        public Exception Exception { get; set; }
    }

    public enum RespositoryChangeEventType
    {
        /// <summary>
        /// 创建数据表。
        /// </summary>
        Create,
        /// <summary>
        /// 添加新字段。
        /// </summary>
        Modify
    }
}
