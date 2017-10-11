
namespace Fireasy.Data.Entity.Subscribes
{
    /// <summary>
    /// 实体持久化的事件类型。
    /// </summary>
    public enum EntityPersistentEventType
    {
        /// <summary>
        /// 创建实体之前。
        /// </summary>
        BeforeCreate,
        /// <summary>
        /// 创建实体之后。
        /// </summary>
        AfterCreate,
        /// <summary>
        /// 更新实体之前。
        /// </summary>
        BeforeUpdate,
        /// <summary>
        /// 更新实体之后。
        /// </summary>
        AfterUpdate,
        /// <summary>
        /// 移除实体之前。
        /// </summary>
        BeforeRemove,
        /// <summary>
        /// 移除实体之后。
        /// </summary>
        AfterRemove,
    }
}
